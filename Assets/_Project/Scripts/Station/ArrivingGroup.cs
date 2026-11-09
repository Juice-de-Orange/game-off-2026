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
    /// </summary>
    public class ArrivingGroup : MonoBehaviour
    {
        private const int MaxFigures = 3;
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
        [Tooltip("Where they wait in the lane, front of the queue first (world Z).")]
        [SerializeField] private float[] queueZ = { -16f, -18f, -20f };
        [SerializeField] private float startSpacingSeconds = 1.5f;

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
            int placed = 0;
            foreach (var traveler in shift.Day.Travelers)
            {
                if (placed >= Mathf.Min(MaxFigures, queueZ.Length)) break;
                if (traveler == null || traveler.Model == null) continue;
                Vector3 toLane = (laneEntry - pad.position).normalized;
                Vector3 start = pad.position + toLane * 3f + Vector3.Cross(toLane, Vector3.up) * (placed - 1f) * 0.8f;
                var figure = Instantiate(traveler.Model, start, Quaternion.LookRotation(toLane), transform);
                if (looks != null) looks.Apply(figure, traveler.DisplayName, traveler.Species);
                var animator = figure.GetComponent<Animator>();
                if (animator != null) animator.cullingMode = AnimatorCullingMode.CullCompletely;
                _figures.Add(figure);
                StartCoroutine(Route(figure.transform, animator, placed));
                placed++;
            }
        }

        private IEnumerator Route(Transform figure, Animator animator, int index)
        {
            Play(animator, Walk);
            yield return WalkTo(figure, laneEntry);
            yield return WalkTo(figure, new Vector3(laneEntry.x, laneEntry.y, queueZ[index]));
            Play(animator, Idle);
            figure.rotation = Quaternion.LookRotation(Vector3.forward);
            while (!_shiftStarted) yield return null;
            yield return new WaitForSeconds(index * startSpacingSeconds);
            Play(animator, Walk);
            yield return WalkTo(figure, laneEnd);
            _figures.Remove(figure.gameObject);
            Destroy(figure.gameObject);
        }

        private IEnumerator WalkTo(Transform figure, Vector3 target)
        {
            Vector3 direction = target - figure.position;
            if (direction.sqrMagnitude > 0.001f) figure.rotation = Quaternion.LookRotation(direction);
            while (figure.position != target)
            {
                figure.position = Vector3.MoveTowards(figure.position, target, walkSpeed * Time.deltaTime);
                yield return null;
            }
        }

        private static void Play(Animator animator, int state)
        {
            if (animator != null && animator.isActiveAndEnabled && animator.HasState(0, state)) animator.Play(state);
        }
    }
}
