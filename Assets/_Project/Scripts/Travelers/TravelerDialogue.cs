using GameOff.Documents;

namespace GameOff.Travelers
{
    /// <summary>
    /// What a traveler says when the officer asks about a field on their papers (P40): the authored answer if there is one,
    /// otherwise a generic line that only repeats what is printed, so it never gives a forgery away.
    /// </summary>
    public static class TravelerDialogue
    {
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
