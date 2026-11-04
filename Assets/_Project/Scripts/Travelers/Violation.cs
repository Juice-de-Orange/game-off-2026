namespace GameOff.Travelers
{
    /// <summary>One problem found with a traveler: the decision it calls for and a short reason (shown on citations).</summary>
    public readonly struct Violation
    {
        public readonly Verdict Requires;
        public readonly string Reason;

        public Violation(Verdict requires, string reason)
        {
            Requires = requires;
            Reason = reason;
        }

        public override string ToString() => $"{Requires}: {Reason}";
    }
}
