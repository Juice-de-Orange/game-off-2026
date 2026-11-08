using UnityEngine;

namespace GameOff.Shops
{
    /// <summary>
    /// P96: keeps a figure without a skeleton (the delivered vendors) from standing dead still: it breathes, sways a little and
    /// now and then turns slightly. Works around the object's pose at start, and every figure runs on its own rhythm.
    /// </summary>
    public class IdleBreathing : MonoBehaviour
    {
        [SerializeField] private IdleBreathingTuning tuning;

        private Vector3 _restScale;
        private Quaternion _restRotation;
        private float _phase, _turn, _turnTarget, _turnVelocity, _nextTurn;

        private void Awake()
        {
            _restScale = transform.localScale;
            _restRotation = transform.localRotation;
            _phase = Random.value * 100f;
            _nextTurn = Time.time + Random.Range(tuning.secondsBetweenTurns.x, tuning.secondsBetweenTurns.y);
        }

        private void Update()
        {
            if (Time.deltaTime <= 0f) return; // paused: SmoothDamp would divide by zero
            if (Time.time >= _nextTurn)
            {
                _turnTarget = Random.value < 0.4f ? 0f : Random.Range(-tuning.turnDegrees, tuning.turnDegrees);
                _nextTurn = Time.time + Random.Range(tuning.secondsBetweenTurns.x, tuning.secondsBetweenTurns.y);
            }
            _turn = Mathf.SmoothDamp(_turn, _turnTarget, ref _turnVelocity, tuning.turnSmoothing);

            float t = Time.time + _phase;
            float breath = Mathf.Sin(t * Mathf.PI * 2f / tuning.breathSeconds) * tuning.breathAmount;
            float sway = Mathf.Sin(t * Mathf.PI * 2f / tuning.swaySeconds) * tuning.swayDegrees;
            transform.localScale = Vector3.Scale(_restScale, new Vector3(1f - breath * 0.5f, 1f + breath, 1f - breath * 0.5f));
            transform.localRotation = _restRotation * Quaternion.Euler(sway * 0.5f, _turn, sway);
        }

        private void OnDisable()
        {
            transform.localScale = _restScale;
            transform.localRotation = _restRotation;
        }
    }
}
