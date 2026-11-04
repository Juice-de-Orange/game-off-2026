using UnityEngine;

namespace GameOff.Counter
{
    /// <summary>Tuning values for picking up and using the stamps at the counter (P38).</summary>
    [CreateAssetMenu(menuName = "GameOff/Stamp Tuning", fileName = "StampTuning")]
    public class StampTuning : ScriptableObject
    {
        [Tooltip("How high the held stamp floats above the surface under the mouse (m).")]
        public float hoverHeight = 0.06f;
        [Tooltip("Duration of one stamp press, down and up (s).")]
        public float pressDuration = 0.16f;
        [Tooltip("Edge length of the ink mark on the document (m).")]
        public float markSize = 0.11f;
        [Tooltip("Random tilt of the ink mark in degrees (±), so stamps look hand-made.")]
        public float markTilt = 8f;
    }
}
