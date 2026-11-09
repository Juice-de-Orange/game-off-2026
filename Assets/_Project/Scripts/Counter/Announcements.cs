using UnityEngine;

namespace GameOff.Counter
{
    /// <summary>Loudspeaker lines at the counter (P41). The "new rule" line comes from the day (DayEntry.newToday).</summary>
    [CreateAssetMenu(menuName = "GameOff/Counter/Announcements", fileName = "Announcements")]
    public class Announcements : ScriptableObject
    {
        [TextArea] public string shiftStart = "Attention, officer. Your counter is open. Today's bulletin is on the board to your left.";
        public AudioClip shiftStartClip;
        [TextArea] public string warning = "Officer, that decision was wrong. A citation has been issued.";
        public AudioClip warningClip;
        public const string DefaultCurfew = "Curfew. All shops are closed. Return to your quarters.";
        public const string DefaultCurfewHint = "It's 23:00. Go back to room 103 and get some sleep.";
        public const string DefaultCurfewClosed = "Closed for the night. Curfew.";
        [Tooltip("P109: the station loudspeaker at 23:00 (shown as a hint anywhere in the station).")]
        [TextArea] public string curfew = DefaultCurfew;
        [Tooltip("P109: the hint after the curfew line.")]
        [TextArea] public string curfewHint = DefaultCurfewHint;
        [Tooltip("P109: what shops, bar, drone, vending and ticket machine say after 23:00.")]
        [TextArea] public string curfewClosed = DefaultCurfewClosed;
        [Tooltip("How long a subtitle stays at least (s); longer if its clip is longer.")]
        public float secondsPerLine = 4f;
    }
}
