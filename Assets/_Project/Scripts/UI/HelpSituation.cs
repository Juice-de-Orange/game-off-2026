namespace GameOff.UI
{
    /// <summary>P140: where the player is right now, for the key list behind [H]. None: no help (menu, ending, a dialog, paused).</summary>
    public enum HelpSituation
    {
        None,
        Walking,
        Counter,
        ReadingPapers,
        Terminal,
        PinBoard,
        RestSeat,
        BenchEvening,
        CloseLook,
        Telescope,
        Flight,
        Inventory,
        Trade,
    }
}
