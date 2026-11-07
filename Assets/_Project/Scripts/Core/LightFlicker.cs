using UnityEngine;

namespace GameOff.Core
{
    /// <summary>
    /// A tired neon tube: mostly steady, now and then a short burst of flicker. Needs a realtime Light.
    /// P36: StationMood sets the pause between bursts from the station's situation and switches the light off in a blackout.
    /// </summary>
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
        private bool _off;

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

        /// <summary>New pause between bursts (seconds, min..max); a longer wait already running is cut short.</summary>
        public void SetPause(Vector2 seconds)
        {
            pauseSeconds = seconds;
            _nextBurst = Mathf.Min(_nextBurst, Time.time + Random.Range(seconds.x, seconds.y));
        }

        /// <summary>Blackout: the light stays dark until switched on again.</summary>
        public void SetOff(bool off) => _off = off;

        private void Update()
        {
            if (_off)
            {
                _light.intensity = 0f;
                return;
            }
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
