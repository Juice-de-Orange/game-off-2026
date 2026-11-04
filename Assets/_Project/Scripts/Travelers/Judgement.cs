namespace GameOff.Travelers
{
    /// <summary>How the officer's decision is judged (P07); money and morale follow in P08.</summary>
    public readonly struct Judgement
    {
        /// <summary>The decision counts as right.</summary>
        public readonly bool Correct;
        /// <summary>The airlock was used (always costs morale, even when ordered).</summary>
        public readonly bool MoraleCost;
        /// <summary>An ordered airlock was refused (arrested instead).</summary>
        public readonly bool Disobeyed;
        /// <summary>Text for the citation if the decision was wrong, else empty.</summary>
        public readonly string Reason;
        public readonly Verdict CorrectVerdict, Chosen;

        public Judgement(bool correct, bool moraleCost, bool disobeyed, string reason, Verdict correctVerdict, Verdict chosen)
        {
            Correct = correct;
            MoraleCost = moraleCost;
            Disobeyed = disobeyed;
            Reason = reason;
            CorrectVerdict = correctVerdict;
            Chosen = chosen;
        }
    }
}
