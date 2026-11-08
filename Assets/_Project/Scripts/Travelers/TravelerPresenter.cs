using System.Collections;
using System.Collections.Generic;
using GameOff.Counter;
using GameOff.Audio;
using GameOff.Documents;
using GameOff.Station;
using UnityEngine;

namespace GameOff.Travelers
{
    /// <summary>
    /// The traveler in the inspection lock (P40, P91): comes in through the hangar door, stands at the marking behind the glass, greets
    /// the officer and answers questions about the fields on their papers. Admitted: leaves through the hall door; denied: back through
    /// the hangar door; arrest: a guard comes, then the shutter (P104); airlock: the hatch opens and the figure is torn out (P105). Figures are reused per model. Idle, Talk and Walk are states of the
    /// model's Animator.
    /// </summary>
    public class TravelerPresenter : MonoBehaviour
    {
        private static readonly int Idle = Animator.StringToHash("Idle");
        private static readonly int Talk = Animator.StringToHash("Talk");
        private static readonly int Walk = Animator.StringToHash("Walk");
        private static readonly int Grab = Animator.StringToHash("Grab");

        [SerializeField] private CounterShift shift;
        [SerializeField] private DocumentDesk desk;
        [SerializeField] private SpeechBubble bubble;
        [SerializeField] private DialogueTuning tuning;
        [Tooltip("The marking in the lock; its forward looks at the counter.")]
        [SerializeField] private Transform standSpot;
        [SerializeField] private Transform hallDoor;
        [SerializeField] private Transform hangarDoor;
        [SerializeField] private SecurityShutter shutter;
        [Tooltip("P104: comes through the hall door when someone is arrested. Prefab with Idle and Walk.")]
        [SerializeField] private GameObject guardModel;
        [Tooltip("P105: flashes while the airlock is open; off otherwise.")]
        [SerializeField] private Light alarmLight;

        private readonly Dictionary<GameObject, GameObject> _figures = new Dictionary<GameObject, GameObject>();
        private GameObject _figure;
        private GameObject _figureModel;
        private bool _figureIsRagdoll;
        private GameObject _guard;
        private Animator _guardAnimator;
        private Animator _animator;
        private Transform _head;
        private Coroutine _routine;
        private bool _walking;
        private Coroutine _lamp;

        private void OnEnable()
        {
            shift.TravelerArrived += OnArrived;
            shift.DecisionMade += OnDecision;
            shift.DayBegun += Clear;
            desk.FieldClicked += OnFieldClicked;
            bubble.Finished += OnBubbleFinished;
            shift.ArrivalDelay = ArrivalSeconds;
            shift.NextDelay = LeaveSeconds;
        }

        private void OnDisable()
        {
            shift.TravelerArrived -= OnArrived;
            shift.DecisionMade -= OnDecision;
            shift.DayBegun -= Clear;
            desk.FieldClicked -= OnFieldClicked;
            bubble.Finished -= OnBubbleFinished;
            shift.ArrivalDelay = null;
            shift.NextDelay = null;
            _routine = null; // the coroutine stops with the component
            CloseDoors();
        }

        /// <summary>P33: the lock's warning lamp: yellow when someone walks in, red for an arrest and the airlock.</summary>
        private void FlashLamp(Color color, float seconds)
        {
            if (alarmLight == null) return;
            if (_lamp != null) StopCoroutine(_lamp);
            _lamp = StartCoroutine(Flash(color, seconds));
        }

        private IEnumerator Flash(Color color, float seconds)
        {
            alarmLight.color = color;
            for (float t = 0f; t < seconds; t += Time.deltaTime)
            {
                alarmLight.enabled = Mathf.Repeat(t * 4f, 1f) < 0.5f;
                yield return null;
            }
            alarmLight.enabled = false;
            _lamp = null;
        }

        private void OnArrived(TravelerData traveler)
        {
            FlashLamp(new Color(1f, 0.75f, 0.2f), 1.2f);
            StopRoutine();
            CloseDoors(); // in case the last traveler's walk was cut short
            bubble.Hide();
            Show(traveler);
            string greeting = GreetingFor != null ? GreetingFor(traveler) : traveler.Greeting;
            _routine = StartCoroutine(Arrive(traveler, greeting));
        }

        /// <summary>P75: recurring characters greet by what happened last time; null = the traveler's own greeting.</summary>
        public System.Func<TravelerData, string> GreetingFor;

        private IEnumerator Arrive(TravelerData traveler, string greeting)
        {
            if (_figure != null) yield return WalkIn(); // a traveler without a model still greets
            if (!string.IsNullOrWhiteSpace(greeting))
            {
                yield return new WaitForSeconds(tuning.greetingDelay);
                Say(traveler, greeting);
            }
            _routine = null;
        }

        private IEnumerator WalkIn()
        {
            var door = SlidingDoor.ForRole(SlidingDoor.Role.LockHangar);
            if (door != null) door.Open();
            yield return new WaitForSeconds(tuning.doorLeadSeconds);
            yield return WalkTo(standSpot.position);
            if (door != null) door.Close();
            var figure = _figure.transform;
            var from = figure.rotation;
            for (float t = 0f; t < tuning.turnSeconds; t += Time.deltaTime)
            {
                figure.rotation = Quaternion.Slerp(from, standSpot.rotation, t / tuning.turnSeconds);
                yield return null;
            }
            figure.rotation = standSpot.rotation;
            Play(Idle);
        }

        private IEnumerator WalkTo(Vector3 target)
        {
            var figure = _figure.transform;
            var heading = Vector3.ProjectOnPlane(target - figure.position, Vector3.up);
            if (heading.sqrMagnitude > 0.001f) figure.rotation = Quaternion.LookRotation(heading);
            Play(Walk);
            _walking = true;
            while ((figure.position - target).sqrMagnitude > 0.0001f)
            {
                figure.position = Vector3.MoveTowards(figure.position, target, tuning.walkSpeed * Time.deltaTime);
                yield return null;
            }
            _walking = false;
        }

        /// <summary>A spot out of sight behind a lock door, seen from the marking.</summary>
        private Vector3 Offstage(Transform door) => door.position + (door.position - standSpot.position).normalized * tuning.offstageMeters;

        private float WalkSeconds(Transform door) => Vector3.Distance(standSpot.position, Offstage(door)) / tuning.walkSpeed;

        /// <summary>P91: from the door opening until the traveler stands at the window, when the papers can come.</summary>
        private float ArrivalSeconds() => tuning.doorLeadSeconds + WalkSeconds(hangarDoor) + tuning.turnSeconds;

        /// <summary>P91: how long the lock is occupied after a decision.</summary>
        private float LeaveSeconds(Verdict chosen) => chosen == Verdict.Airlock
            ? tuning.hatchSeconds + tuning.airlockBlowSeconds + tuning.hatchSeconds
            : chosen == Verdict.Arrest
            ? GuardSeconds() + tuning.shutterDropSeconds + tuning.shutterHoldSeconds + tuning.shutterRiseSeconds
            : tuning.doorLeadSeconds + WalkSeconds(chosen == Verdict.Admit ? hallDoor : hangarDoor) + DoorCloseSeconds(chosen);

        private Vector3 GuardSpot()
        {
            var fromDoor = Offstage(hallDoor) - standSpot.position;
            return standSpot.position + fromDoor.normalized * tuning.guardStopMeters;
        }

        private float GuardSeconds() => guardModel == null ? 0f
            : tuning.doorLeadSeconds + Vector3.Distance(Offstage(hallDoor), GuardSpot()) / tuning.guardWalkSpeed;

        private static float DoorCloseSeconds(Verdict chosen)
        {
            var door = SlidingDoor.ForRole(chosen == Verdict.Admit ? SlidingDoor.Role.LockHall : SlidingDoor.Role.LockHangar);
            return door != null ? door.CloseSeconds : 0f;
        }

        private static void CloseDoors()
        {
            foreach (var role in new[] { SlidingDoor.Role.LockHall, SlidingDoor.Role.LockHangar })
            {
                var door = SlidingDoor.ForRole(role);
                if (door != null) door.Close();
            }
        }

        private void OnFieldClicked(DocumentData document, string fieldKey)
        {
            var traveler = shift.Current;
            if (traveler == null || !Carries(traveler, document)) return; // e.g. papers left over from someone else
            StopRoutine(); // a question interrupts a greeting that hasn't started yet
            Say(traveler, TravelerDialogue.Answer(traveler, document, fieldKey));
        }

        /// <summary>P87: the traveler at the counter says a line (e.g. the answer to the player's choice).</summary>
        public void SayLine(string line)
        {
            var traveler = shift.Current;
            if (traveler == null || string.IsNullOrWhiteSpace(line)) return;
            StopRoutine();
            Say(traveler, line);
        }

        private static bool Carries(TravelerData traveler, DocumentData document)
        {
            foreach (var paper in traveler.Documents)
                if (paper == document) return true;
            return false;
        }

        private void Say(TravelerData traveler, string line)
        {
            bubble.Say(line, traveler.IsAlien, _head != null ? _head : standSpot);
            Play(Talk);
        }

        private void OnBubbleFinished()
        {
            if (!_walking) Play(Idle); // a reaction can end while they are already leaving
        }

        private void OnDecision(TravelerData traveler, Verdict chosen, Judgement judgement)
        {
            StopRoutine();
            bubble.Hide();
            CloseDoors();
            string reaction = TravelerDialogue.Reaction(traveler, chosen, tuning, Random.value); // P100: said on the way out, costs no time
            if (!string.IsNullOrWhiteSpace(reaction)) Say(traveler, reaction);
            if (chosen == Verdict.Arrest) _routine = StartCoroutine(Arrest());
            else if (chosen == Verdict.Airlock) _routine = StartCoroutine(Airlock());
            else if (_figure != null) _routine = StartCoroutine(WalkOff(chosen == Verdict.Admit ? SlidingDoor.Role.LockHall : SlidingDoor.Role.LockHangar,
                chosen == Verdict.Admit ? hallDoor : hangarDoor));
        }

        private IEnumerator WalkOff(SlidingDoor.Role role, Transform marker)
        {
            var door = SlidingDoor.ForRole(role);
            if (door != null) door.Open();
            yield return new WaitForSeconds(tuning.doorLeadSeconds);
            yield return WalkTo(Offstage(marker));
            if (door != null)
            {
                door.Close();
                yield return new WaitForSeconds(door.CloseSeconds); // gone only once nobody can see it
            }
            _routine = null;
            HideFigure();
        }

        /// <summary>P104: a guard walks in and takes the traveler; then the shutter falls and the rest is only heard (P91).</summary>
        private IEnumerator Arrest()
        {
            FlashLamp(new Color(1f, 0.12f, 0.08f), GuardSeconds() + tuning.shutterDropSeconds);
            var door = SlidingDoor.ForRole(SlidingDoor.Role.LockHall);
            if (guardModel != null)
            {
                if (_guard == null)
                {
                    _guard = Instantiate(guardModel, transform);
                    _guardAnimator = _guard.GetComponent<Animator>();
                }
                _guard.transform.SetPositionAndRotation(Offstage(hallDoor), Quaternion.LookRotation(Vector3.ProjectOnPlane(standSpot.position - hallDoor.position, Vector3.up)));
                _guard.SetActive(true);
                if (door != null) door.Open();
                yield return new WaitForSeconds(tuning.doorLeadSeconds);
                if (_guardAnimator != null) _guardAnimator.CrossFade(Walk, tuning.animationBlendSeconds);
                var guard = _guard.transform;
                var spot = GuardSpot();
                while ((guard.position - spot).sqrMagnitude > 0.0001f)
                {
                    guard.position = Vector3.MoveTowards(guard.position, spot, tuning.guardWalkSpeed * Time.deltaTime);
                    yield return null;
                }
                if (_guardAnimator != null)
                    _guardAnimator.CrossFade(_guardAnimator.HasState(0, Grab) ? Grab : Idle, tuning.animationBlendSeconds); // a stand-in has no Grab
            }
            yield return BehindShutter();
        }

        /// <summary>P105: no shutter. The outer hatch swings open, the alarm flashes and the traveler is torn out into space.</summary>
        private IEnumerator Airlock()
        {
            var hatch = AirlockHatch.Instance;
            if (hatch != null) hatch.Open(tuning.hatchSeconds);
            shift.Station.Shake(tuning.hatchSeconds + tuning.airlockBlowSeconds, 0.02f);
            FlashLamp(new Color(1f, 0.12f, 0.08f), tuning.hatchSeconds + tuning.airlockBlowSeconds);
            bubble.Hide();
            yield return new WaitForSeconds(tuning.hatchSeconds * 0.5f);
            var outward = Vector3.ProjectOnPlane(-standSpot.forward, Vector3.up).normalized;
            var bodies = _figure != null ? Ragdoll.Build(_figure) : null;
            _figureIsRagdoll = bodies != null;
            for (float t = 0f; t < tuning.airlockBlowSeconds; t += Time.fixedDeltaTime)
            {
                if (bodies != null)
                    foreach (var body in bodies)
                        body.AddForce(outward * tuning.airlockSuction, ForceMode.Acceleration);
                yield return new WaitForFixedUpdate();
            }
            DestroyFigure(); // its bones have joints now: the next one of this model is a fresh copy
            if (hatch != null) hatch.Close(tuning.hatchSeconds);
            yield return new WaitForSeconds(tuning.hatchSeconds);
            _routine = null;
        }

        private void DestroyFigure()
        {
            _figureIsRagdoll = false;
            if (_figure != null)
            {
                _figures.Remove(_figureModel);
                Destroy(_figure);
            }
            _figure = null;
            _animator = null;
            _head = null;
        }

        /// <summary>P91: the shutter falls, the figure is gone when it rises. What happened is only heard.</summary>
        private IEnumerator BehindShutter()
        {
            AudioManager.Play(s => s.shutterDrop);
            shutter.Drop(tuning.shutterDropSeconds);
            yield return new WaitForSeconds(tuning.shutterDropSeconds);
            HideFigure();
            HideGuard();
            CloseDoors();
            AudioManager.Play(s => s.arrestScuffle);
            yield return new WaitForSeconds(tuning.shutterHoldSeconds);
            AudioManager.Play(s => s.shutterRise);
            shutter.Raise(tuning.shutterRiseSeconds);
            _routine = null;
        }

        private void Show(TravelerData traveler)
        {
            HideFigure();
            var model = traveler.Model;
            _figureModel = model;
            if (model == null) return;
            if (!_figures.TryGetValue(model, out _figure) || _figure == null)
            {
                _figure = Instantiate(model, transform);
                _figures[model] = _figure;
            }
            _figure.transform.SetPositionAndRotation(Offstage(hangarDoor), standSpot.rotation);
            _animator = _figure.GetComponent<Animator>();
            _figure.SetActive(true);
            _head = _animator != null && _animator.isHuman ? _animator.GetBoneTransform(HumanBodyBones.Head) : null;
            Play(Idle);
        }

        /// <summary>A new day: nobody is left standing in the lock.</summary>
        private void Clear()
        {
            StopRoutine();
            bubble.Hide();
            HideFigure();
            HideGuard();
            if (_lamp != null) StopCoroutine(_lamp);
            _lamp = null;
            if (alarmLight != null) alarmLight.enabled = false;
            if (AirlockHatch.Instance != null) AirlockHatch.Instance.Close(0.01f);
            CloseDoors();
            shutter.ResetOpen();
        }

        private void HideGuard()
        {
            if (_guard != null) _guard.SetActive(false);
        }

        private void HideFigure()
        {
            if (_figureIsRagdoll)
            {
                DestroyFigure(); // never back into the cache
                return;
            }
            if (_figure != null) _figure.SetActive(false);
            _figure = null;
            _animator = null;
            _head = null;
        }

        private void StopRoutine()
        {
            if (_routine != null) StopCoroutine(_routine);
            _routine = null;
            _walking = false;
        }

        private void Play(int state)
        {
            if (_animator != null && _animator.isActiveAndEnabled) _animator.CrossFade(state, tuning.animationBlendSeconds);
        }
    }
}
