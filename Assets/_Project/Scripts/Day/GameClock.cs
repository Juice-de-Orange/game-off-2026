namespace GameOff.Day
{
    /// <summary>
    /// P97: the one time of day every clock in the station shows. Morning 06:00, the shift runs 08:00 to 18:00 with the travelers done,
    /// the evening runs up to 23:00. It only shows the day passing; nothing depends on it.
    /// </summary>
    public static class GameClock
    {
        public const int MorningMinutes = 6 * 60, ShiftStartMinutes = 8 * 60, ShiftEndMinutes = 18 * 60, NightMinutes = 23 * 60;

        public static int Minutes { get; private set; } = MorningMinutes;
        public static string Text => Format(Minutes);

        public static void Set(int minutes) => Minutes = minutes;

        public static string Format(int minutes) => $"{minutes / 60:00}:{minutes % 60:00}";

        /// <param name="shiftProgress">0 to 1: share of today's travelers already dealt with.</param>
        /// <param name="eveningProgress">0 to 1: how far the evening has run.</param>
        public static int Compute(DayManager.Phase phase, float shiftProgress, float eveningProgress)
        {
            switch (phase)
            {
                case DayManager.Phase.Morning: return MorningMinutes;
                case DayManager.Phase.Shift: return Lerp(ShiftStartMinutes, ShiftEndMinutes, shiftProgress);
                case DayManager.Phase.Evening: return Lerp(ShiftEndMinutes, NightMinutes, eveningProgress);
                default: return NightMinutes;
            }
        }

        private static int Lerp(int from, int to, float t)
        {
            if (t < 0f) t = 0f;
            if (t > 1f) t = 1f;
            return from + (int)((to - from) * t);
        }
    }
}
