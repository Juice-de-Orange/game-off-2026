using UnityEngine;

namespace GameOff.UI
{
    /// <summary>
    /// Menu background: the camera swings slowly back and forth on an arc around a point (the station seen from space)
    /// and keeps looking at it, with a slight vertical bob. Unscaled time, so it also runs if timeScale is 0.
    /// </summary>
    public class MenuCameraDrift : MonoBehaviour
    {
        [SerializeField] private Vector3 target = new Vector3(0f, 10f, 0f);
        [Tooltip("Extra offset of the look point, e.g. to keep the station right of the menu panel.")]
        [SerializeField] private Vector3 lookOffset = Vector3.zero;
        [SerializeField] private float radius = 120f;
        [SerializeField] private float height = 30f;
        [Tooltip("Center of the arc in degrees around Y (180 = camera south of the target, looking north).")]
        [SerializeField] private float centerAngle = 200f;
        [Tooltip("How far the camera swings to each side (degrees).")]
        [SerializeField] private float swingAngle = 18f;
        [Tooltip("Seconds for one full swing back and forth.")]
        [SerializeField] private float swingPeriod = 90f;
        [SerializeField] private float bobHeight = 3f;

        /// <summary>P48: set on the player's camera at runtime.</summary>
        public void Configure(Vector3 orbitTarget, Vector3 look)
        {
            target = orbitTarget;
            lookOffset = look;
        }

        private void LateUpdate() => Apply(Time.unscaledTime);

        private void OnValidate() => Apply(0f);

        private void Apply(float time)
        {
            float phase = time / Mathf.Max(1f, swingPeriod) * Mathf.PI * 2f;
            float angle = (centerAngle + Mathf.Sin(phase) * swingAngle) * Mathf.Deg2Rad;
            var offset = new Vector3(Mathf.Sin(angle), 0f, Mathf.Cos(angle)) * radius;
            offset.y = height + Mathf.Sin(phase * 0.5f) * bobHeight;
            transform.position = target + offset;
            transform.rotation = Quaternion.LookRotation(target + lookOffset - transform.position, Vector3.up);
        }
    }
}
