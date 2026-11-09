using System.Collections.Generic;
using UnityEngine;

namespace GameOff.Economy
{
    /// <summary>
    /// P113: what the airlock leaves behind during the run. Pure mood, no effect on money or rules: a thought at the porthole and on the
    /// hall benches, a line under the next morning's letter, and what the evening guests say. A run without the airlock never sees any
    /// of it. {name} = who went through the hatch (today's innocent one if there was one, else today's last).
    /// </summary>
    [CreateAssetMenu(menuName = "GameOff/Morale Tuning", fileName = "MoraleTuning")]
    public class MoraleTuning : ScriptableObject
    {
        [Tooltip("On days without the airlock a thought still comes back while Conduct.Morale is at or below this (ordered -1, innocent -3).")]
        public int lingeringAtOrBelow = -3;

        [Header("Thoughts at the porthole and on the benches")]
        [TextArea(1, 3)] public string[] thoughtsOrdered =
        {
            "The report will say procedure. The report doesn't have to remember {name}'s face.",
            "It took four seconds. I keep counting to four.",
            "They had it coming. I've said it three times now and it sounds the same every time.",
        };
        [TextArea(1, 3)] public string[] thoughtsInnocent =
        {
            "{name} was on no list. I checked afterwards. I shouldn't have checked afterwards.",
            "Nobody ordered that. There was nobody at that button but me.",
            "In my head {name}'s papers are still on the desk. A stamp would have done.",
        };
        [TextArea(1, 3)] public string[] thoughtsLingering =
        {
            "My thumb knows where the airlock button is without looking. It shouldn't know that.",
            "I stopped counting the hatch cycles. That is not the same as forgetting them.",
            "The room smells of cold metal. It doesn't, I asked.",
        };

        [Header("Under the next morning's letter")]
        [TextArea(1, 3)] public string[] nightmaresOrdered =
        {
            "You woke before the alarm. In the dream the hatch was open and {name} was still standing there, waiting for a stamp.",
            "You slept badly. Something kept knocking on the outer hatch. It stopped when you got up.",
        };
        [TextArea(1, 3)] public string[] nightmaresInnocent =
        {
            "You woke up shouting. In the dream {name} passed you their papers through the hatch and asked what was wrong with them.",
            "You didn't sleep. Whenever you closed your eyes, the airlock lamp came on.",
        };

        [Header("Evening guests, the same day")]
        [TextArea(1, 3)] public string[] crowdOrdered =
        {
            "Heard the hatch from the hall. The whole floor hums when it goes. ...Evening, officer.",
            "Long shift? You don't have to answer that. Please don't.",
            "They say it's quick, out there. They say a lot of things.",
            "I'll move, officer. No, I insist.",
        };
        [TextArea(1, 3)] public string[] crowdInnocent =
        {
            "That one in the hatch today. People are saying they didn't have it coming.",
            "Someone at the bar said they knew {name}. Then you came in and they stopped talking.",
            "Don't look at me like that, officer. I got my stamp. I got it.",
            "You're the one from the window. ...Nothing. Good night.",
        };

        /// <summary>Today's airlock first; on other days only while the morale stays low. Null when there is nothing to think about.</summary>
        public string Thought(Conduct conduct, int day)
        {
            if (conduct.AirlocksToday > 0)
                return ByDay(conduct.InnocentToday > 0 ? thoughtsInnocent : thoughtsOrdered, day, conduct);
            return conduct.Morale <= lingeringAtOrBelow && conduct.Morale < 0 ? ByDay(thoughtsLingering, day, conduct) : null;
        }

        /// <summary>The line under tomorrow's letter; null without an airlock today.</summary>
        public string Nightmare(Conduct conduct, int day) =>
            conduct.AirlocksToday == 0 ? null : ByDay(conduct.InnocentToday > 0 ? nightmaresInnocent : nightmaresOrdered, day, conduct);

        /// <summary>What an evening guest says instead of small talk; null without an airlock today.</summary>
        /// <param name="random01">Random value from 0 up to (not including) 1.</param>
        public string CrowdLine(Conduct conduct, float random01)
        {
            if (conduct.AirlocksToday == 0) return null;
            var lines = conduct.InnocentToday > 0 ? crowdInnocent : crowdOrdered;
            if (lines == null || lines.Length == 0) return null;
            return Fill(lines[Mathf.Min((int)(random01 * lines.Length), lines.Length - 1)], conduct);
        }

        private static string ByDay(IReadOnlyList<string> lines, int day, Conduct conduct)
        {
            if (lines == null || lines.Count == 0) return null;
            int i = day % lines.Count;
            return Fill(lines[i < 0 ? i + lines.Count : i], conduct);
        }

        private static string Fill(string line, Conduct conduct) => line?.Replace("{name}", conduct.AirlockedName);
    }
}
