namespace GameOff.Counter
{
    /// <summary>
    /// P86, without Unity (EditMode-tested): what Inspector Korr asks for next. Always the first step not done yet, so a player who
    /// runs ahead simply skips steps and nothing is ever blocked. A step counts as done once the player has done it.
    /// P143: the board ends with the decision. What he used to add afterwards (the ARREST and AIRLOCK buttons) comes with the rules
    /// step, where the orders are read; his last words are his reaction in the speech bubble (StoryDayContent "Korr_1").
    /// </summary>
    public class TutorialProgress
    {
        public enum Step { HoldPaper, AskField, ReadRules, Stamp, Drawer }

        private const string Buttons = "Smugglers and wanted names: ARREST. Quarantine and terminate orders: AIRLOCK.";

        private bool _paperHeld, _fieldAsked, _rulesRead;
        private bool? _lastStampApproved;

        public void PaperHeld(bool isNote)
        {
            if (isNote) _rulesRead = true;
            else _paperHeld = true;
        }

        public void FieldAsked() => _fieldAsked = true;

        /// <summary>The last stamp counts, as it does at the drawer.</summary>
        public void Stamped(bool approved) => _lastStampApproved = approved;

        public Step Current
        {
            get
            {
                if (!_paperHeld) return Step.HoldPaper;
                if (!_fieldAsked) return Step.AskField;
                if (!_rulesRead) return Step.ReadRules;
                return _lastStampApproved == true ? Step.Drawer : Step.Stamp;
            }
        }

        public string Text => Current switch
        {
            Step.HoldPaper => "Click a paper to hold it up. Click more and they stand side by side; [R] holds up all of them.",
            Step.AskField => "Click a printed field on a paper you hold up to ask me about it.",
            Step.ReadRules => "Now today's rules: [A] looks left, click the notes on the pinboard. " + Buttons,
            Step.Stamp when _lastStampApproved == false => "DENIED? My papers are in order. Stamp again, the last stamp counts.",
            Step.Stamp => "Names and ID numbers must match, nothing may be expired. All in order? Put the papers down, pick up the green stamp and stamp one.",
            _ => "Now drag ALL my papers onto the drawer, then click the drawer.",
        };
    }
}
