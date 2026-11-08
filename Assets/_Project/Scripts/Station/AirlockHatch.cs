using System.Collections;
using UnityEngine;

namespace GameOff.Station
{
    /// <summary>P105: the outer hatch of the inspection lock. Hinged at its pivot, swings out into space and back.</summary>
    public class AirlockHatch : MonoBehaviour
    {
        [SerializeField] private float openAngle = -90f;

        /// <summary>The hatch of the station (there is one).</summary>
        public static AirlockHatch Instance { get; private set; }

        private Quaternion _closed;
        private Coroutine _moving;

        private void Awake()
        {
            Instance = this;
            _closed = transform.localRotation;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void Open(float seconds) => Swing(_closed * Quaternion.Euler(0f, openAngle, 0f), seconds);

        public void Close(float seconds) => Swing(_closed, seconds);

        private void Swing(Quaternion target, float seconds)
        {
            if (_moving != null) StopCoroutine(_moving);
            _moving = StartCoroutine(Move(target, seconds));
        }

        private IEnumerator Move(Quaternion target, float seconds)
        {
            var from = transform.localRotation;
            for (float t = 0f; t < seconds; t += Time.deltaTime)
            {
                transform.localRotation = Quaternion.Slerp(from, target, t / seconds);
                yield return null;
            }
            transform.localRotation = target;
            _moving = null;
        }
    }
}
