using UnityEngine;

namespace GameOff.Core
{
    /// <summary>A tired neon tube: mostly steady, now and then a short burst of flicker. Needs a realtime Light.</summary>
    [RequireComponent(typeof(Light))]
    public class LightFlicker : MonoBehaviour
    {
        [SerializeField] private Vector2 pauseSeconds = new Vector2(2f, 7f);
        [SerializeField] private Vector2 burstSeconds = new Vector2(0.15f, 0.6f);
        [Range(0f, 1f)] [SerializeField] private float minIntensityFactor = 0.05f;

        private Light _light;
        private float _baseIntensity;
        private float _nextBurst;
        private float _burstEnd;

        private void Awake()
        {
            _light = GetComponent<Light>();
            _baseIntensity = _light.intensity;
        }

        private void OnEnable() => _nextBurst = Time.time + Random.Range(pauseSeconds.x, pauseSeconds.y);

        private void OnDisable()
        {
            if (_light != null) _light.intensity = _baseIntensity;
        }

        private void Update()
        {
            float now = Time.time;
            if (now >= _nextBurst && now >= _burstEnd)
            {
                _burstEnd = now + Random.Range(burstSeconds.x, burstSeconds.y);
                _nextBurst = _burstEnd + Random.Range(pauseSeconds.x, pauseSeconds.y);
            }

            _light.intensity = now < _burstEnd
                ? _baseIntensity * (Random.value < 0.5f ? minIntensityFactor : 1f)
                : _baseIntensity;
        }
    }
}
