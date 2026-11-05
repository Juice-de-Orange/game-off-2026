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
        [Tooltip("How long the traveler walks toward the door after a decision before disappearing (s). Keep below CounterShift's pause before the next traveler.")]
        public float leaveSeconds = 1.2f;
        [Tooltip("Crossfade between Idle, Talk and Walk (s).")]
        public float animationBlendSeconds = 0.2f;
        [Tooltip("Walking speed while leaving (m/s).")]
        public float walkSpeed = 0.85f;
    }
}
