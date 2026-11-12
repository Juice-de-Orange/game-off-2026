using System.Collections.Generic;
using GameOff.Counter;
using GameOff.Day;
using GameOff.Economy;
using GameOff.Player;
using GameOff.Rest;
using UnityEngine;

namespace GameOff.Travelers
{
    /// <summary>
    /// P101: whoever the player let through today stands around the station after the shift, at fixed spots, and says a line when spoken
    /// to. They do not walk there, and they are gone in the morning. At most six at a time (skinned figures are expensive in the browser).
    /// P113: after an airlock that day they talk about that instead.
    /// P134: every spot has a reason (a bar table, a window, the kiosk, the departure board, the hangar barrier), a facing and a pose;
    /// the idle starts at a random point so they don't breathe in step.
    /// P145: some spots are seats. A_NPC_Sit is made on the human skeleton for a 0.45 m seat, and retargeted the pose scales with the
    /// figure's legs, so every guest is measured once in it: lifted onto higher seats (the feet dangle, never in the floor), small ones
    /// slide forward until their knees clear the seat's edge, and a guest whose legs are too long for a seat takes the next spot instead.
    /// A player's seat a guest sits on (a bar stool) is closed until the morning; the player's place in the middle of a hall bench stays free.
    /// </summary>
    public class EveningCrowd : MonoBehaviour
    {
        private const int MaxFigures = 6;
        /// <summary>P145: A_NPC_Sit as delivered: hips 0.586 m over the floor, the seat 0.45 m high, the knees 0.42 m in front of the hips.</summary>
        public const float ClipHipsHeight = 0.586f, ClipSeatHeight = 0.45f, ClipKneeReach = 0.42f;
        /// <summary>P145: legs a little too long for a seat let the guest hover this much over it; any longer and they sit elsewhere.</summary>
        public const float MaxHover = 0.04f;
        /// <summary>P145: how far the knees come past a seat's front edge, so the shins hang in front of it.</summary>
        public const float KneeOverhang = 0.03f;
        private static readonly int Idle = Animator.StringToHash("Idle");
        private static readonly int Sit = Animator.StringToHash("Sit");

        [SerializeField] private DayManager day;
        [SerializeField] private CounterShift shift;
        [SerializeField] private DialogueTuning tuning;
        [SerializeField] private TravelerLooks looks;
        [Tooltip("Filled in this order: the most recently admitted traveler gets the first spot that fits them.")]
        [SerializeField] private Transform[] spots;
        [Tooltip("P134: pose per spot, same order as the spots; missing entries stand.")]
        [SerializeField] private CrowdPose[] poses = new CrowdPose[0];
        [Tooltip("P145: per sitting spot, how high the seat is above the spot's floor; same order as the spots.")]
        [SerializeField] private float[] seatHeights = new float[0];
        [Tooltip("P145: per sitting spot, how far the seat's front edge is in front of the spot (the guest's hips).")]
        [SerializeField] private float[] seatFronts = new float[0];

        private readonly List<TravelerData> _admitted = new List<TravelerData>();
        private readonly List<GameObject> _figures = new List<GameObject>();
        private readonly List<Interactable> _takenSeats = new List<Interactable>();

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
            var taken = new bool[spots.Length];
            int room = Mathf.Min(MaxFigures, spots.Length);
            for (int i = _admitted.Count - 1; i >= 0 && _figures.Count < room; i--)
            {
                var figure = Place(_admitted[i], taken);
                if (figure != null) _figures.Add(figure);
            }
        }

        /// <summary>The pose for spot <paramref name="index"/>; spots without an entry stand.</summary>
        public static CrowdPose PoseAt(CrowdPose[] poses, int index) =>
            poses != null && index >= 0 && index < poses.Length ? poses[index] : CrowdPose.Stand;

        /// <summary>
        /// P145: the first free spot that fits a guest: a standing spot takes anyone, a seat only a guest who can sit (size is not NaN)
        /// and whose legs are not too long for it. -1 if none is left.
        /// </summary>
        public static int PickSpot(CrowdPose[] poses, float[] seatHeights, bool[] taken, float size)
        {
            for (int i = 0; i < taken.Length; i++)
            {
                if (taken[i]) continue;
                if (PoseAt(poses, i) == CrowdPose.Stand) return i;
                if (!float.IsNaN(size) && ValueAt(seatHeights, i) - ClipSeatHeight * size >= -MaxHover) return i;
            }
            return -1;
        }

        /// <summary>P145: how far a guest of this size goes up so the seat carries them; never down, the feet stay out of the floor.</summary>
        public static float SeatLift(float seatHeight, float size) => Mathf.Max(0f, seatHeight - ClipSeatHeight * size);

        /// <summary>P145: how far a small guest slides forward on the seat so the knees clear its front edge and the shins hang in front.</summary>
        public static float SeatForward(float seatFront, float size) => Mathf.Max(0f, seatFront + KneeOverhang - ClipKneeReach * size);

        /// <summary>P145: a player's seat trigger centred where the guest sits is theirs (a bar stool); one centred elsewhere is a bench with room.</summary>
        public static bool TakesSeat(Vector3 triggerCentre, Vector3 seat) =>
            new Vector2(triggerCentre.x - seat.x, triggerCentre.z - seat.z).magnitude < 0.3f;

        private static float ValueAt(float[] values, int index) => values != null && index >= 0 && index < values.Length ? values[index] : 0f;

        private GameObject Place(TravelerData traveler, bool[] taken)
        {
            var figure = Instantiate(traveler.Model, transform);
            var animator = figure.GetComponent<Animator>();
            float size = SitSize(animator);
            int index = PickSpot(poses, seatHeights, taken, size);
            if (index < 0)
            {
                Destroy(figure);
                return null;
            }
            taken[index] = true;
            var spot = spots[index];
            bool sits = PoseAt(poses, index) == CrowdPose.Sit;
            var position = spot.position;
            if (sits)
            {
                float seat = ValueAt(seatHeights, index);
                position += Vector3.up * SeatLift(seat, size) + spot.forward * SeatForward(ValueAt(seatFronts, index), size);
                TakeSeat(spot.position + Vector3.up * seat);
            }
            figure.transform.SetPositionAndRotation(position, spot.rotation);
            if (looks != null) looks.Apply(figure, traveler.DisplayName, traveler.Species);
            if (animator != null)
            {
                // A random start keeps them out of step; Update(0) shows the pose at once (measuring left everyone sitting).
                animator.Play(sits ? Sit : Idle, 0, Random.value);
                animator.Update(0f);
                animator.cullingMode = AnimatorCullingMode.CullCompletely;
            }
            var body = figure.AddComponent<CapsuleCollider>();
            if (sits)
            {
                body.center = new Vector3(0f, 0.65f, 0.1f) * size;
                body.height = 1.3f * size;
            }
            else
            {
                body.center = Vector3.up * 0.9f;
                body.height = 1.8f;
            }
            body.radius = 0.32f;
            var talk = figure.AddComponent<Interactable>();
            talk.SetPrompt("Talk to " + traveler.DisplayName);
            string line = Confiscation.Instance != null ? Confiscation.Instance.MoraleCrowdLine(Random.value) : null;
            if (string.IsNullOrEmpty(line)) line = TravelerDialogue.EveningLine(traveler.Species, tuning, Random.value);
            talk.OnInteract.AddListener(() => day.UI.ShowHint($"{traveler.DisplayName}: \"{line}\"", 5f));
            return figure;
        }

        /// <summary>
        /// P145: the figure's size against the body A_NPC_Sit was made on, measured at its hips in the pose (retargeting scales the
        /// pose with the legs). NaN if it cannot sit: no Sit state, not humanoid, or the pose did not come out lower than standing.
        /// </summary>
        private static float SitSize(Animator animator)
        {
            if (animator == null || !animator.isHuman || !animator.HasState(0, Sit) || !animator.HasState(0, Idle)) return float.NaN;
            var hips = animator.GetBoneTransform(HumanBodyBones.Hips);
            if (hips == null) return float.NaN;
            float standing = HipsHeight(animator, hips, Idle);
            float sitting = HipsHeight(animator, hips, Sit);
            return sitting > 0.05f && sitting < 0.8f * standing ? sitting / ClipHipsHeight : float.NaN;
        }

        private static float HipsHeight(Animator animator, Transform hips, int state)
        {
            animator.Play(state, 0, 0f);
            animator.Update(0f);
            return hips.position.y - animator.transform.position.y;
        }

        /// <summary>P145: a player's seat that would put the camera where this guest sits (a bar stool) is closed until the morning.</summary>
        private void TakeSeat(Vector3 seat)
        {
            foreach (var hit in Physics.OverlapSphere(seat, 0.05f, ~0, QueryTriggerInteraction.Collide))
            {
                var use = hit.GetComponent<Interactable>();
                if (use == null || !use.InteractionEnabled || hit.GetComponentInParent<SeatView>() == null) continue;
                if (!TakesSeat(hit.bounds.center, seat)) continue;
                use.SetInteractionEnabled(false);
                _takenSeats.Add(use);
            }
        }

        private void Disperse()
        {
            foreach (var figure in _figures) if (figure != null) Destroy(figure);
            _figures.Clear();
            _admitted.Clear();
            foreach (var seat in _takenSeats) if (seat != null) seat.SetInteractionEnabled(true);
            _takenSeats.Clear();
        }
    }
}
