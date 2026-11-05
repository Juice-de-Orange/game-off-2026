using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameOff.Core
{
    /// <summary>
    /// Ships flying past on fixed lanes with random gaps (P47 menu backdrop; reusable for P25 in Main).
    /// Cheap on purpose: every ship model is created once and parked when its flight ends (no spawning per flight),
    /// no lights, no shadows, no colliders, and never more than tuning.maxActive ships at once. Uses unscaled time.
    /// </summary>
    public class ShipTraffic : MonoBehaviour
    {
        [SerializeField] private ShipTrafficTuning tuning;

        private readonly Dictionary<GameObject, List<Transform>> _pool = new Dictionary<GameObject, List<Transform>>();
        private int _active;

        /// <summary>Time-lapse factor for the traffic dev tool (P56); 1 = normal.</summary>
        public float SpeedFactor { get; set; } = 1f;

        public ShipTrafficTuning Tuning => tuning;

        /// <summary>Sends one ship down the lane right away (dev tool), ignoring maxActive and the interval.</summary>
        public void LaunchNow(TrafficLane lane)
        {
            if (isActiveAndEnabled && lane.ships.Count > 0) StartCoroutine(Fly(lane));
        }

        private void OnEnable()
        {
            if (tuning == null) return;
            foreach (var lane in tuning.lanes) StartCoroutine(RunLane(lane));
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            _active = 0;
            foreach (var list in _pool.Values)
                foreach (var ship in list) ship.gameObject.SetActive(false);
        }

        private IEnumerator RunLane(TrafficLane lane)
        {
            if (lane.ships.Count == 0) yield break;
            yield return Wait(Random.Range(lane.firstDelay.x, lane.firstDelay.y));
            while (true)
            {
                if (_active < tuning.maxActive) yield return Fly(lane);
                yield return Wait(Random.Range(lane.interval.x, lane.interval.y));
            }
        }

        private IEnumerator Fly(TrafficLane lane)
        {
            var model = lane.ships[Random.Range(0, lane.ships.Count)];
            if (model == null) yield break;
            var ship = Take(model);
            bool reverse = Random.value < lane.reverseChance;
            var offset = Vector3.Scale(lane.randomOffset, new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)));
            var start = (reverse ? lane.to : lane.from) + offset;
            var end = (reverse ? lane.from : lane.to) + offset;
            float speed = Random.Range(lane.speed.x, lane.speed.y);
            float duration = Vector3.Distance(start, end) / Mathf.Max(0.1f, speed);

            ship.localScale = Vector3.one * lane.scale;
            ship.rotation = Quaternion.LookRotation(end - start, Vector3.up) * Quaternion.Euler(0f, tuning.modelYaw, 0f);
            ship.gameObject.SetActive(true);
            _active++;
            for (float t = 0f; t < duration; t += Time.unscaledDeltaTime * SpeedFactor)
            {
                ship.position = Vector3.Lerp(start, end, t / duration);
                yield return null;
            }
            ship.gameObject.SetActive(false);
            _active--;
        }

        /// <summary>A parked copy of the model, or a new one (lights/shadows/colliders stripped).</summary>
        private Transform Take(GameObject model)
        {
            if (!_pool.TryGetValue(model, out var list)) _pool[model] = list = new List<Transform>();
            foreach (var parked in list)
                if (!parked.gameObject.activeSelf) return parked;
            var copy = Instantiate(model, transform);
            foreach (var c in copy.GetComponentsInChildren<Collider>(true)) Destroy(c);
            foreach (var l in copy.GetComponentsInChildren<Light>(true)) Destroy(l);
            foreach (var r in copy.GetComponentsInChildren<Renderer>(true))
            {
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                r.receiveShadows = false;
            }
            list.Add(copy.transform);
            return copy.transform;
        }

        private IEnumerator Wait(float seconds)
        {
            for (float t = 0f; t < seconds; t += Time.unscaledDeltaTime * SpeedFactor) yield return null;
        }
    }
}
