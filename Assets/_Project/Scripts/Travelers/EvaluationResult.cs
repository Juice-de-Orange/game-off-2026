using System.Collections.Generic;

namespace GameOff.Travelers
{
    /// <summary>What the rules say about a traveler: the correct decision and every problem found (most severe first).</summary>
    public class EvaluationResult
    {
        public Verdict Correct { get; }
        public IReadOnlyList<Violation> Violations { get; }
        /// <summary>The reason behind the correct decision (the most severe violation), or "No violations".</summary>
        public string MainReason => Violations.Count > 0 ? Violations[0].Reason : "No violations";

        public EvaluationResult(Verdict correct, IReadOnlyList<Violation> violations)
        {
            Correct = correct;
            Violations = violations;
        }
    }
}
