using System.Collections;
using System.Collections.Generic;
using GameOff.Audio;
using GameOff.Core;
using GameOff.Day;
using UnityEngine;

namespace GameOff.Station
{
    /// <summary>
    /// P107: two to four small ships stand on the hangar pads, different ones every day; one of them comes in through the barrier each
    /// morning. Which ones follows from the day number, so a replayed day looks the same.
    /// </summary>
    public class HangarShips : MonoBehaviour
    {
        [SerializeField] private DayManager day;
        [SerializeField] private GameObject[] ships;
        [SerializeField] private Transform[] pads;
        [Tooltip("Where the morning arrival starts: outside the energy barrier.")]
        [SerializeField] private Vector3 approachFrom = new Vector3(0f, 7f, -48f);
        [SerializeField] private float landingSeconds = 7f;
        [Tooltip("P155: the hangar's energy barrier (station prefab); flashes when the morning ship flies through it.")]
        [SerializeField] private EnergyBarrier barrier;

        private readonly List<GameObject> _placed = new List<GameObject>();

        /// <summary>P14: the morning's ship has touched down on this pad.</summary>
        public event System.Action<Transform> Landed;

        private void OnEnable() => day.DayStarted += OnDayStarted;

        private void OnDisable() => day.DayStarted -= OnDayStarted;

        private void OnDayStarted(int number)
        {
            StopAllCoroutines();
            foreach (var ship in _placed) if (ship != null) Destroy(ship);
            _placed.Clear();
            if (ships.Length == 0 || pads.Length == 0) return;
            var rng = new System.Random(number * 7919);
            int count = Mathf.Min(rng.Next(2, 5), pads.Length);
            int firstPad = rng.Next(pads.Length);
            for (int i = 0; i < count; i++)
            {
                var pad = pads[(firstPad + i) % pads.Length];
                var ship = Instantiate(ships[rng.Next(ships.Length)], pad.position, pad.rotation, transform);
                _placed.Add(ship);
                var body = FitCollider(ship);
                body.enabled = i != 0; // the one coming in gets solid when it is down
                if (i == 0) StartCoroutine(Land(ship.transform, pad, body));
            }
        }

        /// <summary>One box around the whole ship, in its own space.</summary>
        private static BoxCollider FitCollider(GameObject ship)
        {
            var toLocal = ship.transform.worldToLocalMatrix;
            var bounds = new Bounds();
            bool any = false;
            foreach (var filter in ship.GetComponentsInChildren<MeshFilter>())
            {
                if (filter.sharedMesh == null) continue;
                var b = filter.sharedMesh.bounds;
                var matrix = toLocal * filter.transform.localToWorldMatrix;
                for (int c = 0; c < 8; c++)
                {
                    var corner = matrix.MultiplyPoint3x4(b.center + Vector3.Scale(b.extents, new Vector3((c & 1) == 0 ? -1f : 1f, (c & 2) == 0 ? -1f : 1f, (c & 4) == 0 ? -1f : 1f)));
                    if (any) bounds.Encapsulate(corner);
                    else { bounds = new Bounds(corner, Vector3.zero); any = true; }
                }
            }
            var box = ship.AddComponent<BoxCollider>();
            box.center = bounds.center;
            box.size = bounds.size;
            return box;
        }

        private static bool PlayerInside(BoxCollider box)
        {
            var t = box.transform;
            foreach (var hit in Physics.OverlapBox(t.TransformPoint(box.center), Vector3.Scale(box.size * 0.5f, t.lossyScale), t.rotation, ~0, QueryTriggerInteraction.Ignore))
                if (hit is CharacterController) return true;
            return false;
        }

        private IEnumerator Land(Transform ship, Transform pad, BoxCollider body)
        {
            AudioManager.PlayAt(s => s.shipLand, pad.position, 60f);
            var previous = approachFrom;
            for (float t = 0f; t < landingSeconds; t += Time.deltaTime)
            {
                float k = Mathf.SmoothStep(0f, 1f, t / landingSeconds);
                ship.position = Vector3.Lerp(approachFrom, pad.position, k);
                if (barrier != null) barrier.PulseIfPassed(previous, ship.position);
                previous = ship.position;
                yield return null;
            }
            ship.position = pad.position;
            Landed?.Invoke(pad);
            // Someone standing on the pad must not end up inside a solid box.
            while (PlayerInside(body)) yield return new WaitForSeconds(0.5f);
            body.enabled = true;
        }
    }
}
