using UnityEngine;

namespace GameOff.Counter
{
    /// <summary>Timing of the traveler in the inspection lock and their speech bubble (P40).</summary>
    [CreateAssetMenu(menuName = "GameOff/Counter/Dialogue Tuning", fileName = "DialogueTuning")]
    public class DialogueTuning : ScriptableObject
    {
        [Tooltip("Pause between the traveler appearing and the greeting (s).")]
        public float greetingDelay = 0.6f;
        [Tooltip("Alien speech: how long the translation wave takes per text line (s).")]
        public float translateSecondsPerLine = 0.4f;
        [Tooltip("Translated characters still shown in cyan at the wave's edge.")]
        public int translateEdge = 3;
        [Tooltip("A bubble stays at least this long after the text is readable (s).")]
        public float minHoldSeconds = 2.5f;
        [Tooltip("Extra time per character of text (s).")]
        public float holdSecondsPerCharacter = 0.04f;
        [Tooltip("P91: the door opens this long before someone walks through it (s).")]
        public float doorLeadSeconds = 0.4f;
        [Tooltip("P91: how far behind the lock doors a traveler starts and ends, out of sight (m).")]
        public float offstageMeters = 0.9f;
        [Tooltip("P91: time to turn toward the counter after arriving (s).")]
        public float turnSeconds = 0.3f;
        [Tooltip("P91: how long the shutter takes to fall (s).")]
        public float shutterDropSeconds = 0.25f;
        [Tooltip("P91: how long the shutter takes to rise again (s).")]
        public float shutterRiseSeconds = 1.1f;
        [Tooltip("P91: arrest and airlock: how long the shutter stays down (s).")]
        public float shutterHoldSeconds = 3f;
        [Tooltip("Crossfade between Idle, Talk and Walk (s).")]
        public float animationBlendSeconds = 0.2f;
        [Tooltip("Walking speed through the lock (m/s).")]
        public float walkSpeed = 1.4f;
    }
}
