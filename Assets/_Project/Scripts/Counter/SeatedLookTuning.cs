using UnityEngine;

namespace GameOff.Counter
{
    /// <summary>Looking around while seated at the counter (P41).</summary>
    [CreateAssetMenu(menuName = "GameOff/Counter/Seated Look Tuning", fileName = "SeatedLookTuning")]
    public class SeatedLookTuning : ScriptableObject
    {
        [Tooltip("How far the view turns to either side (degrees).")]
        public float yawLimit = 70f;
        [Tooltip("Degrees per second.")]
        public float turnSpeed = 90f;
        [Tooltip("Camera pitch at the limit (degrees, 0 = level). The desk view's own pitch applies when looking straight ahead.")]
        public float pitchAtLimit = 0f;
    }
}
