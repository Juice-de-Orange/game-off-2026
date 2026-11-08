using System.Collections;
using System.Collections.Generic;
using GameOff.Audio;
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

        private readonly List<GameObject> _placed = new List<GameObject>();

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
                if (i == 0) StartCoroutine(Land(ship.transform, pad));
            }
        }

        private IEnumerator Land(Transform ship, Transform pad)
        {
            AudioManager.PlayAt(s => s.shipLand, pad.position, 60f);
            for (float t = 0f; t < landingSeconds; t += Time.deltaTime)
            {
                float k = Mathf.SmoothStep(0f, 1f, t / landingSeconds);
                ship.position = Vector3.Lerp(approachFrom, pad.position, k);
                yield return null;
            }
            ship.position = pad.position;
        }
    }
}
