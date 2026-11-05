namespace GameOff.Economy
{
    /// <summary>One money movement with its reason, e.g. +10 "Wage · Vesna Arkhai" (P46). Shown under TODAY.</summary>
    public readonly struct LedgerEntry
    {
        public readonly int Amount;
        public readonly string Reason;

        public LedgerEntry(int amount, string reason)
        {
            Amount = amount;
            Reason = reason;
        }
    }
}
