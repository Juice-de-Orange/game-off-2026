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
    /// the hangar door; arrest and airlock happen behind the shutter. Figures are reused per model. Idle, Talk and Walk are states of the
    /// model's Animator.
    /// </summary>
    public class TravelerPresenter : MonoBehaviour
    {
        private static readonly int Idle = Animator.StringToHash("Idle");
        private static readonly int Talk = Animator.StringToHash("Talk");
        private static readonly int Walk = Animator.StringToHash("Walk");

        [SerializeField] private CounterShift shift;
        [SerializeField] private DocumentDesk desk;
        [SerializeField] private SpeechBubble bubble;
        [SerializeField] private DialogueTuning tuning;
        [Tooltip("The marking in the lock; its forward looks at the counter.")]
        [SerializeField] private Transform standSpot;
        [SerializeField] private Transform hallDoor;
        [SerializeField] private Transform hangarDoor;
        [SerializeField] private SecurityShutter shutter;

        private readonly Dictionary<GameObject, GameObject> _figures = new Dictionary<GameObject, GameObject>();
        private GameObject _figure;
        private Animator _animator;
        private Transform _head;
        private Coroutine _routine;

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

        private void OnArrived(TravelerData traveler)
        {
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
            while ((figure.position - target).sqrMagnitude > 0.0001f)
            {
                figure.position = Vector3.MoveTowards(figure.position, target, tuning.walkSpeed * Time.deltaTime);
                yield return null;
            }
        }

        /// <summary>A spot out of sight behind a lock door, seen from the marking.</summary>
        private Vector3 Offstage(Transform door) => door.position + (door.position - standSpot.position).normalized * tuning.offstageMeters;

        private float WalkSeconds(Transform door) => Vector3.Distance(standSpot.position, Offstage(door)) / tuning.walkSpeed;

        /// <summary>P91: from the door opening until the traveler stands at the window, when the papers can come.</summary>
        private float ArrivalSeconds() => tuning.doorLeadSeconds + WalkSeconds(hangarDoor) + tuning.turnSeconds;

        /// <summary>P91: how long the lock is occupied after a decision.</summary>
        private float LeaveSeconds(Verdict chosen) => chosen == Verdict.Arrest || chosen == Verdict.Airlock
            ? tuning.shutterDropSeconds + tuning.shutterHoldSeconds + tuning.shutterRiseSeconds
            : tuning.doorLeadSeconds + WalkSeconds(chosen == Verdict.Admit ? hallDoor : hangarDoor) + DoorCloseSeconds(chosen);

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

        private void OnBubbleFinished() => Play(Idle);

        private void OnDecision(TravelerData traveler, Verdict chosen, Judgement judgement)
        {
            StopRoutine();
            bubble.Hide();
            CloseDoors();
            if (chosen == Verdict.Arrest || chosen == Verdict.Airlock) _routine = StartCoroutine(BehindShutter(chosen));
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

        /// <summary>P91: the shutter falls, the figure is gone when it rises. What happened is only heard.</summary>
        private IEnumerator BehindShutter(Verdict chosen)
        {
            AudioManager.Play(s => s.shutterDrop);
            shutter.Drop(tuning.shutterDropSeconds);
            yield return new WaitForSeconds(tuning.shutterDropSeconds);
            HideFigure();
            if (chosen == Verdict.Arrest) AudioManager.Play(s => s.arrestScuffle); // the airlock's own sound already plays
            yield return new WaitForSeconds(tuning.shutterHoldSeconds);
            AudioManager.Play(s => s.shutterRise);
            shutter.Raise(tuning.shutterRiseSeconds);
            _routine = null;
        }

        private void Show(TravelerData traveler)
        {
            HideFigure();
            var model = traveler.Model;
            if (model == null) return;
            if (!_figures.TryGetValue(model, out _figure) || _figure == null)
            {
                _figure = Instantiate(model, transform);
                _figures[model] = _figure;
            }
            _figure.transform.SetPositionAndRotation(Offstage(hangarDoor), standSpot.rotation);
            _animator = _figure.GetComponent<Animator>();
            _head = _animator != null && _animator.isHuman ? _animator.GetBoneTransform(HumanBodyBones.Head) : null;
            _figure.SetActive(true);
            Play(Idle);
        }

        /// <summary>A new day: nobody is left standing in the lock.</summary>
        private void Clear()
        {
            StopRoutine();
            bubble.Hide();
            HideFigure();
            CloseDoors();
            shutter.ResetOpen();
        }

        private void HideFigure()
        {
            if (_figure != null) _figure.SetActive(false);
            _figure = null;
            _animator = null;
            _head = null;
        }

        private void StopRoutine()
        {
            if (_routine != null) StopCoroutine(_routine);
            _routine = null;
        }

        private void Play(int state)
        {
            if (_animator != null && _animator.isActiveAndEnabled) _animator.CrossFade(state, tuning.animationBlendSeconds);
        }
    }
}
