using System.Collections.Generic;
using GameOff.Economy;

namespace GameOff.Departure
{
    /// <summary>
    /// P72: which ending the flight home gets. The family member in the worst condition decides:
    /// well or weak → HOME, sick → HOME, BARELY, critical → TOO LATE? ({names} = everyone in that worst condition).
    /// </summary>
    public static class TicketEnding
    {
        public static (string title, string body) Pick(IReadOnlyList<int> conditions, IReadOnlyList<string> names, DepartureTuning texts)
        {
            int worst = Household.Well;
            foreach (int c in conditions) if (c > worst) worst = c;
            if (worst < Household.Sick) return (texts.homeTitle, texts.homeBody);

            var who = new List<string>();
            for (int i = 0; i < conditions.Count; i++)
                if (conditions[i] == worst) who.Add(i < names.Count ? names[i] : "?");
            string body = worst >= Household.Critical ? texts.lateBody : texts.barelyBody;
            return (worst >= Household.Critical ? texts.lateTitle : texts.barelyTitle, body.Replace("{names}", JoinNames(who)));
        }

        /// <summary>"Pip", "Mom and Pip", "Mom, Lena and Pip".</summary>
        public static string JoinNames(IReadOnlyList<string> names)
        {
            if (names.Count == 0) return "";
            if (names.Count == 1) return names[0];
            var head = new List<string>();
            for (int i = 0; i < names.Count - 1; i++) head.Add(names[i]);
            return string.Join(", ", head) + " and " + names[names.Count - 1];
        }
    }
}
