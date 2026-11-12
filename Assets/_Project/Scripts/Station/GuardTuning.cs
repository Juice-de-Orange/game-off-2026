using UnityEngine;

namespace GameOff.Station
{
    /// <summary>P148: the guards walking their rounds through the station: pace, pauses, and what they say when spoken to.</summary>
    [CreateAssetMenu(menuName = "GameOff/Station/Guard Tuning", fileName = "GuardTuning")]
    public class GuardTuning : ScriptableObject
    {
        [Tooltip("Walking pace on the round (m/s). The arrest guard (P104) hurries; these don't.")]
        public float walkSpeed = 1.3f;
        [Tooltip("How long they stand at a waypoint and look around (s, random between x and y).")]
        public Vector2 pauseSeconds = new Vector2(2f, 5f);
        [Tooltip("While standing they turn this far to either side of the waypoint's facing (degrees).")]
        public float lookAroundDegrees = 45f;
        [Tooltip("Turning speed while looking around or toward the player (degrees per second).")]
        public float turnDegreesPerSecond = 120f;
        [Tooltip("A guard stops while the player stands this close in front of him (m), instead of walking through.")]
        public float giveWayMeters = 1.2f;
        [Tooltip("No progress for this long while walking: he gives up this leg and stands as if he had arrived (s).")]
        public float stuckSeconds = 4f;
        [Tooltip("Spoken to: he stands still and faces the player this long (s).")]
        public float talkSeconds = 3f;
        [Tooltip("How long his line stays in the hint bar (s).")]
        public float lineSeconds = 5f;
        [Tooltip("Crossfade between Idle and Walk (s).")]
        public float animationBlendSeconds = 0.2f;

        [Header("Talking")]
        [Tooltip("[E] prompt on a guard.")]
        public string prompt = "Talk to the guard";
        [Tooltip("Who says the line in the hint bar.")]
        public string speakerName = "Guard";
        [Tooltip("One of these when spoken to.")]
        public string[] lines =
        {
            "Move along, officer. Nothing to see here.",
            "Quiet so far. Let's keep it that way.",
            "Eyes open. Somebody always tries something.",
            "You stamp them, we carry them. Fair split.",
            "Just walking my round. Don't mind me.",
            "Keep your papers straight and we'll get along.",
            "Station rules, officer. Not mine.",
            "Long queue today. Long night, too.",
        };
        [Tooltip("P109: after 23:00 one of these instead.")]
        public string[] curfewLines =
        {
            "Curfew's on, officer. Off to your room.",
            "Twenty-three hundred. The station sleeps now. So should you.",
            "Late for a walk. Get some sleep.",
        };
    }
}
