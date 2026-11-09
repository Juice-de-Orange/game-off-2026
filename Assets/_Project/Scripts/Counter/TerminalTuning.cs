using UnityEngine;

namespace GameOff.Counter
{
    /// <summary>P34: timings of the counter terminal.</summary>
    [CreateAssetMenu(menuName = "GameOff/Counter/Terminal Tuning", fileName = "TerminalTuning")]
    public class TerminalTuning : ScriptableObject
    {
        [Tooltip("\"SEARCHING...\" before a result shows (seconds, real time).")]
        public float lookupSeconds = 0.6f;
        [Tooltip("How fast a result types out (characters per second).")]
        public float charsPerSecond = 220f;
    }
}
