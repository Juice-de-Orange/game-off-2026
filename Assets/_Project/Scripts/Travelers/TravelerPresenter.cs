using System.Collections;
using System.Collections.Generic;
using GameOff.Counter;
using GameOff.Documents;
using UnityEngine;

namespace GameOff.Travelers
{
    /// <summary>
    /// The traveler in the inspection lock (P40): stands at the marking behind the glass, greets the officer, answers questions
    /// about the fields on their papers, and walks a few steps toward the matching door after the decision before disappearing
    /// (real walking comes with P14). Figures are reused per model. Idle, Talk and Walk are states of the model's Animator.
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
        [SerializeField] private Transform outerDoor;

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
        }

        private void OnDisable()
        {
            shift.TravelerArrived -= OnArrived;
            shift.DecisionMade -= OnDecision;
            shift.DayBegun -= Clear;
            desk.FieldClicked -= OnFieldClicked;
            bubble.Finished -= OnBubbleFinished;
            _routine = null; // the coroutine stops with the component
        }

        private void OnArrived(TravelerData traveler)
        {
            StopRoutine();
            bubble.Hide();
            Show(traveler);
            if (!string.IsNullOrWhiteSpace(traveler.Greeting)) _routine = StartCoroutine(Greet(traveler));
        }

        private IEnumerator Greet(TravelerData traveler)
        {
            yield return new WaitForSeconds(tuning.greetingDelay);
            _routine = null;
            Say(traveler, traveler.Greeting);
        }

        private void OnFieldClicked(DocumentData document, string fieldKey)
        {
            var traveler = shift.Current;
            if (traveler == null || !Carries(traveler, document)) return; // e.g. papers left over from someone else
            StopRoutine(); // a question interrupts a greeting that hasn't started yet
            Say(traveler, TravelerDialogue.Answer(traveler, document, fieldKey));
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
            if (_figure != null) _routine = StartCoroutine(WalkOff(DoorFor(chosen)));
        }

        private Transform DoorFor(Verdict verdict) => verdict switch
        {
            Verdict.Admit => hallDoor,
            Verdict.Arrest => hallDoor, // the guard comes through the hall (STATION.md)
            Verdict.Airlock => outerDoor,
            _ => hangarDoor,
        };

        private IEnumerator WalkOff(Transform door)
        {
            var figure = _figure.transform;
            var toDoor = Vector3.ProjectOnPlane(door.position - figure.position, Vector3.up);
            if (toDoor.sqrMagnitude > 0.001f) figure.rotation = Quaternion.LookRotation(toDoor);
            Play(Walk);
            for (float t = 0f; t < tuning.leaveSeconds; t += Time.deltaTime)
            {
                figure.position = Vector3.MoveTowards(figure.position, door.position, tuning.walkSpeed * Time.deltaTime);
                yield return null;
            }
            _routine = null;
            HideFigure();
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
            _figure.transform.SetPositionAndRotation(standSpot.position, standSpot.rotation);
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
