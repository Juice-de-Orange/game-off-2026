using System;

namespace GameOff.Economy
{
    /// <summary>
    /// Wage of one shift (P10): a fixed amount per correctly decided traveler (P70), minus a penalty for every citation beyond
    /// the free warnings of the day. Bribes and confiscation fines are booked elsewhere (SettlementWindow, Confiscation).
    /// </summary>
    public static class Payroll
    {
        public static int Wage(int travelers, HouseholdTuning tuning) => Math.Max(0, travelers) * tuning.wagePerTraveler;

        public static int PenalizedCitations(int citations, int freeWarnings) => Math.Max(0, citations - Math.Max(0, freeWarnings));

        public static int Penalty(int citations, int freeWarnings, HouseholdTuning tuning) =>
            PenalizedCitations(citations, freeWarnings) * tuning.penaltyPerCitation;
    }
}
