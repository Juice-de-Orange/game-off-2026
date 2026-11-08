using UnityEngine;

namespace GameOff.Shops
{
    /// <summary>How much a figure without a skeleton moves while idle (P96).</summary>
    [CreateAssetMenu(menuName = "GameOff/Idle Breathing Tuning", fileName = "IdleBreathingTuning")]
    public class IdleBreathingTuning : ScriptableObject
    {
        public float breathAmount = 0.012f;
        public float breathSeconds = 3.6f;
        public float swayDegrees = 1f;
        public float swaySeconds = 7.5f;
        public float turnDegrees = 9f;
        public Vector2 secondsBetweenTurns = new Vector2(4f, 10f);
        public float turnSmoothing = 0.7f;
    }
}
