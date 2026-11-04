namespace GameOff.Travelers
{
    /// <summary>
    /// Judges the officer's decision against the rules (DESIGN.md, "Regeln am Schalter"):
    /// same decision → right; airlock for a criminal (arrest case) → right but costs morale; arrest when the airlock is
    /// ordered → order disobeyed (citation); anything else → wrong, with a reason for the citation.
    /// </summary>
    public static class DecisionJudge
    {
        public static Judgement Judge(EvaluationResult result, Verdict chosen)
        {
            var correct = result.Correct;
            bool morale = chosen == Verdict.Airlock;
            if (chosen == correct) return new Judgement(true, morale, false, "", correct, chosen);
            if (correct == Verdict.Arrest && chosen == Verdict.Airlock) return new Judgement(true, true, false, "", correct, chosen);
            if (correct == Verdict.Airlock && chosen == Verdict.Arrest)
                return new Judgement(false, false, true, $"Order disobeyed - {result.MainReason}", correct, chosen);
            return new Judgement(false, morale, false, Reason(result, chosen), correct, chosen);
        }

        private static string Reason(EvaluationResult result, Verdict chosen)
        {
            switch (result.Correct)
            {
                case Verdict.Admit:
                    return chosen == Verdict.Deny ? "Valid traveler denied"
                        : chosen == Verdict.Arrest ? "Innocent traveler arrested" : "Innocent traveler ejected";
                case Verdict.Deny:
                    return chosen == Verdict.Admit ? $"Admitted - {result.MainReason}"
                        : chosen == Verdict.Arrest ? $"Wrongful arrest - only grounds to deny ({result.MainReason})"
                        : $"Unlawful ejection - only grounds to deny ({result.MainReason})";
                case Verdict.Arrest:
                    return $"Criminal released - {result.MainReason}";
                default: // ordered airlock, admitted or denied
                    return $"Ordered ejection ignored - {result.MainReason}";
            }
        }
    }
}
