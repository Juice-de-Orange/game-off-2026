using UnityEngine;

namespace GameOff.Core
{
    /// <summary>Navigation light: switches a glowing bulb (and optionally a Light) on for a short moment every period.</summary>
    public class BlinkLight : MonoBehaviour
    {
        [SerializeField] private Renderer bulb;
        [SerializeField] private Light lightSource;
        [SerializeField] private float period = 1.6f;
        [SerializeField] private float onTime = 0.25f;
        [Tooltip("0..1, so neighbouring lights don't blink in sync.")]
        [SerializeField] private float phase;

        private void Update()
        {
            bool on = Mathf.Repeat(Time.time / period + phase, 1f) < onTime / period;
            if (bulb != null && bulb.enabled != on) bulb.enabled = on;
            if (lightSource != null && lightSource.enabled != on) lightSource.enabled = on;
        }
    }
}
