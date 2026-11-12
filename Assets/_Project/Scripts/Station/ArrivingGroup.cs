using System.Collections;
using System.Collections.Generic;
using GameOff.Travelers;
using UnityEngine;

namespace GameOff.Station
{
    /// <summary>
    /// P14: up to three of today's travelers come out of the ship that landed this morning and queue in the barrier lane; when the
    /// shift starts they walk on to the inspection door and disappear. Fixed routes, no NavMesh; they are gone during the shift, so
    /// the skinned figures cost nothing while the counter runs.
    /// P136: they queue in counter order. The first one steps out first and the others follow one behind the other on the same way
    /// (ArrivalQueue), so nobody walks through anybody; at shift start the front leaves first.
    /// </summary>
    public class ArrivingGroup : MonoBehaviour
    {
        private const int MaxFigures = 3;
        private const float StepOut = 3f; // metres from the pad's centre toward the lane: the ship's side
        private static readonly int Idle = Animator.StringToHash("Idle");
        private static readonly int Walk = Animator.StringToHash("Walk");

        [SerializeField] private HangarShips ships;
        [SerializeField] private CounterShift shift;
        [SerializeField] private TravelerLooks looks;
        [SerializeField] private float walkSpeed = 1.5f;
        [Tooltip("South end of the barrier lane.")]
        [SerializeField] private Vector3 laneEntry = new Vector3(2f, 0f, -22.5f);
        [Tooltip("In front of the inspection door; the figures vanish here.")]
        [SerializeField] private Vector3 laneEnd = new Vector3(2f, 0f, -13.6f);
        [Tooltip("Where they wait in the lane (world Z); the place nearest the lane's end is the front of the queue.")]
        [SerializeField] private float[] queueZ = { -16f, -18f, -20f };
        [Tooltip("Seconds between two of them walking on at shift start, front first.")]
        [SerializeField] private float startSpacingSeconds = 1.5f;
        [Tooltip("P136: metres between two of them walking in, one behind the other.")]
        [SerializeField] private float followGap = 1.8f;

        private readonly List<GameObject> _figures = new List<GameObject>();
        private bool _shiftStarted;

        private void OnEnable()
        {
            ships.Landed += OnLanded;
            shift.DayBegun += Clear;
            shift.ShiftStarted += OnShiftStarted;
        }

        private void OnDisable()
        {
            ships.Landed -= OnLanded;
            shift.DayBegun -= Clear;
            shift.ShiftStarted -= OnShiftStarted;
        }

        private void OnShiftStarted() => _shiftStarted = true;

        private void Clear()
        {
            StopAllCoroutines();
            foreach (var figure in _figures) if (figure != null) Destroy(figure);
            _figures.Clear();
            _shiftStarted = false;
        }

        private void OnLanded(Transform pad)
        {
            if (shift.Day == null || _shiftStarted) return;
            var places = ArrivalQueue.PlacesFrontFirst(queueZ, laneEnd.z);
            Vector3 exit = ArrivalQueue.ExitPoint(pad.position, laneEntry, StepOut);
            int placed = 0;
            foreach (var traveler in shift.Day.Travelers) // counter order: the first one here is the front of the queue
            {
                if (placed >= Mathf.Min(MaxFigures, places.Length)) break;
                if (traveler == null || traveler.Model == null) continue;
                StartCoroutine(Route(traveler, ArrivalQueue.WayIn(exit, laneEntry, places[placed]), placed));
                placed++;
            }
        }

        private IEnumerator Route(TravelerData traveler, Vector3[] wayIn, int index)
        {
            float stepOut = ArrivalQueue.StepOutDelay(index, followGap, walkSpeed);
            if (stepOut > 0f) yield return new WaitForSeconds(stepOut);
            if (_shiftStarted) yield break; // the counter opened before this one stepped out: it doesn't come at all
            var figure = Instantiate(traveler.Model, wayIn[0], Facing(wayIn[1] - wayIn[0]), transform);
            if (looks != null) looks.Apply(figure, traveler.DisplayName, traveler.Species);
            var animator = figure.GetComponent<Animator>();
            // Not CullCompletely: a state change made while off-screen would be lost, and they would keep walking on the spot.
            if (animator != null) animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
            _figures.Add(figure);
            var body = figure.transform;

            Play(animator, Walk);
            yield return Follow(body, wayIn);
            Play(animator, Idle);
            body.rotation = Facing(laneEnd - body.position);
            while (!_shiftStarted) yield return null;
            yield return new WaitForSeconds(ArrivalQueue.LeaveDelay(index, startSpacingSeconds));
            Play(animator, Walk);
            yield return Follow(body, new[] { body.position, laneEnd });
            _figures.Remove(figure);
            Destroy(figure);
        }

        /// <summary>Walks the path at walkSpeed; all of them on the same way at the same speed keep their distance.</summary>
        private IEnumerator Follow(Transform figure, Vector3[] path)
        {
            float length = ArrivalQueue.Length(path);
            float start = walkSpeed > 0f ? 0f : length; // standing still would never arrive
            for (float walked = start; ; walked += walkSpeed * Time.deltaTime)
            {
                figure.position = ArrivalQueue.Along(path, walked, out var heading);
                heading.y = 0f;
                if (heading.sqrMagnitude > 0.0001f) figure.rotation = Quaternion.LookRotation(heading);
                if (walked >= length) yield break;
                yield return null;
            }
        }

        private static Quaternion Facing(Vector3 direction)
        {
            direction.y = 0f;
            return direction.sqrMagnitude > 0.0001f ? Quaternion.LookRotation(direction) : Quaternion.identity;
        }

        private static void Play(Animator animator, int state)
        {
            if (animator != null && animator.isActiveAndEnabled && animator.HasState(0, state)) animator.Play(state);
        }
    }
}
