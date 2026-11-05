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
        [Tooltip("How long a subtitle stays at least (s); longer if its clip is longer.")]
        public float secondsPerLine = 4f;
    }
}
