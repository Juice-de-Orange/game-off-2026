using UnityEngine;

namespace GameOff.Core
{
    /// <summary>
    /// Sun, planets and moon around the station (P24). SetDay(1..3) turns the whole sky a bit further each day and lets the
    /// sun sink — the station's slow rotation, a visible "shift" between the days. The sun is a realtime directional light,
    /// so its light falls through the windows from a different angle every day. Sets _GameOffSunDir for the sky shaders.
    /// </summary>
    [ExecuteAlways]
    public class SkyRig : MonoBehaviour
    {
        private static readonly int SunDirId = Shader.PropertyToID("_GameOffSunDir");

        [SerializeField] private Transform sunGlow;
        [SerializeField] private Light sunLight;
        [Tooltip("Rotation of the whole sky per day (degrees around Y). Index 0 = day 1.")]
        [SerializeField] private float[] dayYaw = { 0f, 18f, 36f };
        [Tooltip("Sun height above the station plane per day (degrees).")]
        [SerializeField] private float[] daySunElevation = { 16f, 9f, 3f };
        [Tooltip("Sun direction before the daily rotation (degrees from north, negative = west).")]
        [SerializeField] private float sunAzimuth = -38f;
        [SerializeField] private float sunDistance = 900f;
        [Range(1, 3)] [SerializeField] private int previewDay = 1;

        private int _day = 1;

        /// <summary>Sets the sky for day 1..3 (called by the day flow, P09).</summary>
        public void SetDay(int day)
        {
            _day = Mathf.Clamp(day, 1, Mathf.Min(dayYaw.Length, daySunElevation.Length));
            Apply();
        }

        private void OnEnable()
        {
            // In play mode keep the day set by SetDay (re-enabling must not jump back to the preview day).
            if (!Application.isPlaying) _day = previewDay;
            Apply();
        }

        private void OnValidate()
        {
            if (!Application.isPlaying) { _day = previewDay; Apply(); }
        }

        // Global shader values are lost on domain reload/scene load, so keep them set.
        private void Update() => Shader.SetGlobalVector(SunDirId, SunDirection());

        private Vector3 SunDirection()
        {
            int i = Mathf.Clamp(_day - 1, 0, Mathf.Min(dayYaw.Length, daySunElevation.Length) - 1);
            if (i < 0) return Vector3.up;
            return transform.rotation * (Quaternion.Euler(-daySunElevation[i], sunAzimuth, 0f) * Vector3.forward);
        }

        private void Apply()
        {
            int i = Mathf.Clamp(_day - 1, 0, dayYaw.Length - 1);
            if (dayYaw.Length > 0) transform.rotation = Quaternion.Euler(0f, dayYaw[i], 0f);
            Vector3 dir = SunDirection();
            if (sunGlow != null) sunGlow.position = transform.position + dir * sunDistance;
            if (sunLight != null) sunLight.transform.rotation = Quaternion.LookRotation(-dir);
            Shader.SetGlobalVector(SunDirId, dir);
        }
    }
}
