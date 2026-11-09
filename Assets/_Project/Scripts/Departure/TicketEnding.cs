using System.Collections.Generic;
using GameOff.Economy;

namespace GameOff.Departure
{
    /// <summary>
    /// P72: which ending the flight home gets. The family member in the worst condition decides:
    /// well or weak → HOME, sick → HOME, BARELY, critical → TOO LATE?, somebody died (P69) → the lost ending
    /// ({names} = everyone in that worst condition). P12: a closing sentence about the airlock follows (MoraleLine).
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
            string title = worst == Household.Dead ? texts.lostTitle : worst == Household.Critical ? texts.lateTitle : texts.barelyTitle;
            string body = worst == Household.Dead ? texts.lostBody : worst == Household.Critical ? texts.lateBody : texts.barelyBody;
            return (title, body.Replace("{names}", JoinNames(who)));
        }

        /// <summary>P35: the closing sentence for the whole run: about the airlock if it was used, about the crates if only those were opened.</summary>
        public static string ConductLine(Conduct conduct, DepartureTuning texts) =>
            conduct.AirlockMorale == 0 && conduct.Morale < 0 ? texts.moraleThief : MoraleLine(conduct.AirlockMorale, texts);

        /// <summary>P12: closing sentence for the run's morale (Conduct.Morale: 0 or more clean, then stained, then bloody).</summary>
        public static string MoraleLine(int morale, DepartureTuning texts) =>
            morale >= 0 ? texts.moraleCleanLine : morale >= texts.moraleBloodyBelow ? texts.moraleStainedLine : texts.moraleBloodyLine;

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
