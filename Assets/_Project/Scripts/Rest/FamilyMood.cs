using System.Collections.Generic;
using GameOff.Economy;

namespace GameOff.Rest
{
    /// <summary>
    /// P28/P29: one word for how the family is doing, read off the household. Everything in the room follows it: the thought at
    /// the porthole, the photo on the locker, how shaky the handwriting of the letter on the table is.
    /// Grieving wins over everything (someone has died); otherwise the worst condition of those still alive.
    /// </summary>
    public static class FamilyMood
    {
        public const int Well = 0, Worried = 1, Sick = 2, Critical = 3, Grieving = 4;

        private static readonly float[] Brightness = { 1f, 0.95f, 0.88f, 0.8f, 0.74f };
        // A hand that is fine still wobbles a little: it is handwriting, not print.
        private static readonly float[] ShakeByMood = { 0.08f, 0.3f, 0.55f, 1f, 0.85f };

        public static int Of(Household family) => Of(Conditions(family));

        /// <summary>The household's conditions per person (empty without a household).</summary>
        public static int[] Conditions(Household family)
        {
            if (family == null) return new int[0];
            var conditions = new int[family.Count];
            for (int i = 0; i < conditions.Length; i++) conditions[i] = family.Condition(i);
            return conditions;
        }

        /// <param name="conditions">Household conditions per person (Household.Well … Household.Dead).</param>
        public static int Of(IReadOnlyList<int> conditions)
        {
            int worst = Well;
            foreach (int c in conditions)
            {
                if (c >= Household.Dead) return Grieving;
                if (c > worst) worst = c;
            }
            return worst; // Household.Weak .. Critical map 1:1 onto Worried .. Critical
        }

        /// <summary>The person the thought is about: the first one who died, else the one worst off (-1: nobody, all well).</summary>
        public static int Focus(IReadOnlyList<int> conditions)
        {
            int focus = -1, worst = Well;
            for (int i = 0; i < conditions.Count; i++)
            {
                if (conditions[i] >= Household.Dead) return i;
                if (conditions[i] > worst) { worst = conditions[i]; focus = i; }
            }
            return focus;
        }

        /// <summary>Column of UI_Family_Photo: 0 intact, 1 faded with a crease (someone sick), 2 torn and stained (someone died).</summary>
        public static int PhotoFrame(int mood) => mood == Grieving ? 2 : mood >= Sick ? 1 : 0;

        /// <summary>Grey tint over the photo: it darkens a little with every step.</summary>
        public static float PhotoBrightness(int mood) => Brightness[Clamp(mood)];

        /// <summary>0 to 1: how much the letters wobble. Worst while someone is critical; after a death the hand is tired, a bit calmer.</summary>
        public static float Shake(int mood) => ShakeByMood[Clamp(mood)];

        /// <summary>One of the lines, a different one each day; "" if there are none.</summary>
        public static string Pick(IReadOnlyList<string> lines, int day)
        {
            if (lines == null || lines.Count == 0) return "";
            int i = day % lines.Count;
            return lines[i < 0 ? i + lines.Count : i] ?? "";
        }

        private static int Clamp(int mood) => mood < Well ? Well : mood > Grieving ? Grieving : mood;
    }
}
