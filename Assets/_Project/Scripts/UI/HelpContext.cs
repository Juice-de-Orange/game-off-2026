namespace GameOff.UI
{
    /// <summary>P140: what the help overlay sees of the game this frame; HelpKeys.SituationFor turns it into a HelpSituation.</summary>
    public struct HelpContext
    {
        public enum SeatKind { None, LookAround, LookAroundEvening, CloseLook }

        /// <summary>The main menu or its flight to room 103 is still running.</summary>
        public bool InMenu;
        /// <summary>The night, or the run is over (end screen).</summary>
        public bool Ending;
        /// <summary>A dialog, letter or report window over everything.</summary>
        public bool PanelOpen;
        public bool Paused;
        public bool Flying;
        public bool Telescope;
        public bool PinBoard;
        public bool Trade;
        public bool Inventory;
        public bool Terminal;
        public bool AtCounter;
        public bool ReadingPapers;
        public SeatKind Seat;
    }
}
