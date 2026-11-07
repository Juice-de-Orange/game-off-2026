using UnityEngine;

namespace GameOff.Day
{
    /// <summary>
    /// How the station's situation (P36) shows: crisis events of the market raise it, it calms down slowly, and the worse it is,
    /// the more the lamps flicker, the neon signs drop out and the power fails.
    /// </summary>
    [CreateAssetMenu(menuName = "GameOff/Day/Station Mood Tuning", fileName = "StationMoodTuning")]
    public class StationMoodTuning : ScriptableObject
    {
        [Header("Situation")]
        [Tooltip("Share of yesterday's situation that is still there today (0.5 = it halves every calm day).")]
        [Range(0f, 1f)] public float carryOver = 0.5f;

        [Header("Flicker lamps (pause between bursts, seconds)")]
        public Vector2 calmPause = new Vector2(15f, 30f);
        public Vector2 worstPause = new Vector2(1f, 3f);

        [Header("Neon signs drop out")]
        [Tooltip("Situation from which the neon signs start to drop out.")]
        [Range(0f, 1f)] public float neonFrom = 0.2f;
        [Tooltip("Pause between dropouts at neonFrom.")]
        public Vector2 neonCalmPause = new Vector2(30f, 60f);
        [Tooltip("Pause between dropouts at situation 1.")]
        public Vector2 neonWorstPause = new Vector2(4f, 10f);
        public Vector2 neonDropSeconds = new Vector2(0.2f, 0.8f);

        [Header("Blackouts")]
        [Tooltip("Situation from which short blackouts happen.")]
        [Range(0f, 1f)] public float blackoutFrom = 0.5f;
        public Vector2 blackoutPause = new Vector2(45f, 90f);
        public Vector2 blackoutSeconds = new Vector2(2f, 4f);
    }
}
