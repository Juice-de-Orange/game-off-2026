namespace GameOff.Station
{
    /// <summary>Who may open a sliding door (P27).</summary>
    public enum DoorAccess
    {
        /// <summary>Opens for the player and for travelers.</summary>
        Public,
        /// <summary>Staff doors and the player's own room: opens only for the player.</summary>
        PlayerOnly,
        /// <summary>Never opens (rooms 101/102); approaching plays the locked sound.</summary>
        Locked,
        /// <summary>Opened and closed only by game code, e.g. the inspection lock doors (P08).</summary>
        Scripted
    }
}
