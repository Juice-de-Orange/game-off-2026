namespace GameOff.Counter
{
    /// <summary>
    /// P86, without Unity (EditMode-tested): what Inspector Korr asks for next. Always the first step not done yet, so a player who
    /// runs ahead simply skips steps and nothing is ever blocked. A step counts as done once the player has done it.
    /// </summary>
    public class TutorialProgress
    {
        public enum Step { HoldPaper, AskField, ReadRules, Stamp, Drawer }

        private const string Buttons = "Smugglers and wanted names: ARREST. Quarantine and terminate orders: AIRLOCK.";

        /// <summary>Korr's last words after an approval.</summary>
        public static string CorrectEnd(int freeWarnings) => $"Good. {Buttons} Your first {freeWarnings} mistakes each day are only warnings. Carry on.";

        /// <summary>P103: after the good ending, a hint without the sums.</summary>
        public const string DroneTip = "One more thing. Money sent home gets skimmed on the way. Goods on the cargo drone don't. You didn't hear that from me.";

        /// <summary>Korr's last words when his valid papers were refused (he can't say anything after an arrest or the airlock).</summary>
        public static string WrongEnd => $"My papers were in order, officer. It's in my report. {Buttons} Carry on.";

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
            Step.ReadRules => "Now today's rules: [A] looks left, click the notes on the pinboard.",
            Step.Stamp when _lastStampApproved == false => "DENIED? My papers are in order. Stamp again, the last stamp counts.",
            Step.Stamp => "Names and ID numbers must match, nothing may be expired. All in order? Put the papers down, pick up the green stamp and stamp one.",
            _ => "Now drag ALL my papers onto the drawer, then click the drawer.",
        };
    }
}
