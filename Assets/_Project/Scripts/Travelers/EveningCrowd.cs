using System.Collections.Generic;
using GameOff.Counter;
using GameOff.Day;
using GameOff.Player;
using UnityEngine;

namespace GameOff.Travelers
{
    /// <summary>
    /// P101: whoever the player let through today stands around the station after the shift, at fixed spots, and says a line when spoken
    /// to. They do not walk there, and they are gone in the morning. At most six at a time (skinned figures are expensive in the browser).
    /// </summary>
    public class EveningCrowd : MonoBehaviour
    {
        private const int MaxFigures = 6;
        private static readonly int Idle = Animator.StringToHash("Idle");

        [SerializeField] private DayManager day;
        [SerializeField] private CounterShift shift;
        [SerializeField] private DialogueTuning tuning;
        [SerializeField] private Transform[] spots;

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
                _figures.Add(Place(_admitted[_admitted.Count - 1 - i], spots[i]));
        }

        private GameObject Place(TravelerData traveler, Transform spot)
        {
            var figure = Instantiate(traveler.Model, spot.position, spot.rotation, transform);
            var animator = figure.GetComponent<Animator>();
            if (animator != null)
            {
                animator.cullingMode = AnimatorCullingMode.CullCompletely;
                animator.Play(Idle);
            }
            var body = figure.AddComponent<CapsuleCollider>();
            body.center = Vector3.up * 0.9f;
            body.height = 1.8f;
            body.radius = 0.32f;
            var talk = figure.AddComponent<Interactable>();
            talk.SetPrompt("Talk to " + traveler.DisplayName);
            string line = TravelerDialogue.EveningLine(traveler.Species, tuning, Random.value);
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
