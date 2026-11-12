using System.Collections.Generic;
using GameOff.Counter;
using GameOff.Day;
using GameOff.Economy;
using GameOff.Player;
using UnityEngine;

namespace GameOff.Travelers
{
    /// <summary>
    /// P101: whoever the player let through today stands around the station after the shift, at fixed spots, and says a line when spoken
    /// to. They do not walk there, and they are gone in the morning. At most six at a time (skinned figures are expensive in the browser).
    /// P113: after an airlock that day they talk about that instead.
    /// P134: every spot has a reason (a bar table, a window, the kiosk, the departure board, the hangar barrier), a facing and a pose;
    /// the idle starts at a random point so they don't breathe in step.
    /// </summary>
    public class EveningCrowd : MonoBehaviour
    {
        private const int MaxFigures = 6;
        private static readonly int Idle = Animator.StringToHash("Idle");
        private static readonly int Sit = Animator.StringToHash("Sit");

        [SerializeField] private DayManager day;
        [SerializeField] private CounterShift shift;
        [SerializeField] private DialogueTuning tuning;
        [SerializeField] private TravelerLooks looks;
        [Tooltip("Filled in this order: the most recently admitted traveler gets the first spot.")]
        [SerializeField] private Transform[] spots;
        [Tooltip("P134: pose per spot, same order as the spots; missing entries stand.")]
        [SerializeField] private CrowdPose[] poses = new CrowdPose[0];

        private readonly List<TravelerData> _admitted = new List<TravelerData>();
        private readonly List<GameObject> _figures = new List<GameObject>();

        private void OnEnable()
        {
            shift.DecisionMade += OnDecision;
            shift.ShiftEnded += Gather;
            shift.DayBegun += Disperse;
        }

        private void OnDisable()
        {
            shift.DecisionMade -= OnDecision;
            shift.ShiftEnded -= Gather;
            shift.DayBegun -= Disperse;
        }

        private void OnDecision(TravelerData traveler, Verdict chosen, Judgement judgement)
        {
            if (chosen == Verdict.Admit && traveler.Model != null) _admitted.Add(traveler);
        }

        private void Gather()
        {
            int count = Mathf.Min(_admitted.Count, Mathf.Min(MaxFigures, spots.Length));
            for (int i = 0; i < count; i++)
                _figures.Add(Place(_admitted[_admitted.Count - 1 - i], spots[i], PoseAt(poses, i)));
        }

        /// <summary>The pose for spot <paramref name="index"/>; spots without an entry stand.</summary>
        public static CrowdPose PoseAt(CrowdPose[] poses, int index) =>
            poses != null && index >= 0 && index < poses.Length ? poses[index] : CrowdPose.Stand;

        private GameObject Place(TravelerData traveler, Transform spot, CrowdPose pose)
        {
            var figure = Instantiate(traveler.Model, spot.position, spot.rotation, transform);
            if (looks != null) looks.Apply(figure, traveler.DisplayName, traveler.Species);
            var animator = figure.GetComponent<Animator>();
            if (animator != null)
            {
                animator.cullingMode = AnimatorCullingMode.CullCompletely;
                // Sitting needs the clip in the controller; without it they stand. A random start keeps them out of step.
                int state = pose == CrowdPose.Sit && animator.HasState(0, Sit) ? Sit : Idle;
                animator.Play(state, 0, Random.value);
            }
            var body = figure.AddComponent<CapsuleCollider>();
            body.center = Vector3.up * 0.9f;
            body.height = 1.8f;
            body.radius = 0.32f;
            var talk = figure.AddComponent<Interactable>();
            talk.SetPrompt("Talk to " + traveler.DisplayName);
            string line = Confiscation.Instance != null ? Confiscation.Instance.MoraleCrowdLine(Random.value) : null;
            if (string.IsNullOrEmpty(line)) line = TravelerDialogue.EveningLine(traveler.Species, tuning, Random.value);
            talk.OnInteract.AddListener(() => day.UI.ShowHint($"{traveler.DisplayName}: \"{line}\"", 5f));
            return figure;
        }

        private void Disperse()
        {
            foreach (var figure in _figures) if (figure != null) Destroy(figure);
            _figures.Clear();
            _admitted.Clear();
        }
    }
}
