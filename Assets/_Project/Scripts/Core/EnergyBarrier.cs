using UnityEngine;

namespace GameOff.Core
{
    /// <summary>
    /// Drives the hangar energy barrier material (shader GameOff/EnergyBarrier): idle hum + Pulse() when something passes.
    /// Uses a MaterialPropertyBlock, so the material asset is never modified at runtime.
    /// </summary>
    [RequireComponent(typeof(Renderer))]
    public class EnergyBarrier : MonoBehaviour
    {
        private static readonly int PulseId = Shader.PropertyToID("_Pulse");

        [Tooltip("How fast a pulse fades out (1/s).")]
        [SerializeField] private float pulseDecay = 1.5f;
        [Tooltip("Small random flickers while idle, per second.")]
        [SerializeField] private float idleFlickerRate = 0.2f;

        private Renderer _renderer;
        private MaterialPropertyBlock _block;
        private float _pulse;

        private void Awake()
        {
            _renderer = GetComponent<Renderer>();
            _block = new MaterialPropertyBlock();
        }

        /// <summary>Flash the field, e.g. when a ship flies through it (P25).</summary>
        public void Pulse(float strength = 1f) => _pulse = Mathf.Max(_pulse, Mathf.Clamp01(strength));

        /// <summary>
        /// P155: flashes the field if the step from → to (world space) goes through it: the morning ship flying into the hangar
        /// (HangarShips), the shuttle home if the player flies it through the hangar mouth (ShuttleFlight). The field is the unit
        /// quad of this object, so in its own space it spans -0.5..0.5 in x and y at z = 0.
        /// </summary>
        public void PulseIfPassed(Vector3 from, Vector3 to, float strength = 1f)
        {
            var a = transform.InverseTransformPoint(from);
            var b = transform.InverseTransformPoint(to);
            if ((a.z < 0f) == (b.z < 0f) || Mathf.Approximately(a.z, b.z)) return; // still on the same side
            var hit = Vector3.Lerp(a, b, a.z / (a.z - b.z));
            if (Mathf.Abs(hit.x) <= 0.5f && Mathf.Abs(hit.y) <= 0.5f) Pulse(strength);
        }

        private void Update()
        {
            if (Random.value < idleFlickerRate * Time.deltaTime) _pulse = Mathf.Max(_pulse, 0.25f);
            _pulse = Mathf.MoveTowards(_pulse, 0f, pulseDecay * Time.deltaTime);
            _renderer.GetPropertyBlock(_block);
            _block.SetFloat(PulseId, _pulse);
            _renderer.SetPropertyBlock(_block);
        }
    }
}
