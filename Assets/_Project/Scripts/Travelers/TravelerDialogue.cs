using GameOff.Documents;

namespace GameOff.Travelers
{
    /// <summary>
    /// What a traveler says when the officer asks about a field on their papers (P40): the authored answer if there is one,
    /// otherwise a generic line that only repeats what is printed, so it never gives a forgery away.
    /// </summary>
    public static class TravelerDialogue
    {
        /// <summary>P100: the line after the stamp: the traveler's own, or one from the species' pool; null if there is none.</summary>
        /// <param name="random01">Random value from 0 up to (not including) 1.</param>
        public static string Reaction(TravelerData traveler, Verdict chosen, Counter.DialogueTuning tuning, float random01)
        {
            if (chosen != Verdict.Admit && chosen != Verdict.Deny) return null;
            string own = chosen == Verdict.Admit ? traveler.ReactionAdmitted : traveler.ReactionDenied;
            if (!string.IsNullOrWhiteSpace(own)) return own;
            var lines = LinesOf(traveler.Species, tuning);
            return lines == null ? null : Pick(chosen == Verdict.Admit ? lines.admitted : lines.denied, random01);
        }

        /// <summary>P101: what someone who was let through says in the station that evening; null if there is nothing.</summary>
        public static string EveningLine(string species, Counter.DialogueTuning tuning, float random01)
        {
            var lines = LinesOf(species, tuning);
            return lines == null ? null : Pick(lines.evening, random01);
        }

        private static Counter.SpeciesLines LinesOf(string species, Counter.DialogueTuning tuning)
        {
            if (tuning == null || tuning.speciesLines == null) return null;
            foreach (var lines in tuning.speciesLines)
                if (lines != null && TextMatch.Same(lines.species, species)) return lines;
            return null;
        }

        private static string Pick(string[] lines, float random01)
        {
            if (lines == null || lines.Length == 0) return null;
            int i = (int)(random01 * lines.Length);
            return lines[System.Math.Min(i, lines.Length - 1)];
        }

        public static string Answer(TravelerData traveler, DocumentData document, string fieldKey)
        {
            string authored = traveler.AnswerFor(fieldKey);
            if (!string.IsNullOrWhiteSpace(authored)) return authored;
            string value = document != null ? document.Get(fieldKey) : null;
            if (string.IsNullOrWhiteSpace(value)) return "I don't know what to tell you about that, officer.";
            return fieldKey switch
            {
                "name" => $"That's me. {value}.",
                "species" => $"{value}. As you can see.",
                "origin" => $"I come from {value}.",
                "id" => $"My number is {value}.",
                "expires" => $"It says {value}. That's what they gave me.",
                "purpose" => $"{value}. That's why I'm here.",
                "duration" => $"{value}, officer. No longer.",
                "issued" => $"They issued it at {value}.",
                "route" => $"I came the way it says: {value}.",
                "ship" => $"My ship is the {value}.",
                "destination" => $"I'm headed for {value}.",
                "cargo" => "What's on the manifest is what I'm carrying.",
                _ => "It's all there on the paper, officer.",
            };
        }
    }
}
