using System;
using System.Collections.Generic;
using GameOff.Travelers;

namespace GameOff.Characters
{
    /// <summary>
    /// P75, without Unity (EditMode-tested): which recurring characters come today and where they stand in the queue.
    /// Per character at most one appearance a day: the first one in its list that is due, not done yet, and whose condition holds
    /// (P150: and whose flag gates let it).
    /// A window of several days picks its day at random (each day 1/(days left) – on the last day it always comes); the random numbers
    /// come from the caller (the director feeds a stream seeded by run and day, so a reloaded morning plans the same).
    /// At most maxPerDay encounters a day (fixed slots first); characters in `uncapped` (the dev test character) come on top.
    /// Slot n = n-th in the queue; 0 = anywhere.
    /// </summary>
    public static class CharacterPlanner
    {
        public struct Planned
        {
            public RecurringCharacter character;
            public int appearance;
            public TravelerData traveler;
        }

        /// <param name="uncapped">Characters that don't count towards maxPerDay (the dev test character).</param>
        /// <param name="cut">If given, receives the characters that were due but cut by maxPerDay.</param>
        public static List<Planned> Due(int day, IReadOnlyList<RecurringCharacter> characters, CharacterMemory memory, int maxPerDay,
            Func<float> random, ICollection<RecurringCharacter> uncapped = null, List<Planned> cut = null)
        {
            var due = new List<Planned>();
            foreach (var c in characters)
            {
                if (c == null || memory.MetOn(c, day)) continue;
                for (int i = 0; i < c.Appearances.Count; i++)
                {
                    var a = c.Appearances[i];
                    if (a.traveler == null || !a.InWindow(day) || memory.Done(c, i) || !memory.Meets(c, a.condition, a.flag) || !memory.Allows(a)) continue;
                    int lastDay = Math.Max(a.dayFrom, a.dayTo);
                    int daysLeft = lastDay - day + 1;
                    if (daysLeft > 1 && random() >= 1f / daysLeft) break; // not today; the window goes on
                    due.Add(new Planned { character = c, appearance = i, traveler = a.traveler });
                    break;
                }
            }
            // fixed slots first, then the rest (each in list order); cap per day, the uncapped ones on top
            bool Free(Planned p) => uncapped != null && uncapped.Contains(p.character);
            var capped = due.FindAll(p => !Free(p) && Slot(p) > 0);
            capped.AddRange(due.FindAll(p => !Free(p) && Slot(p) == 0));
            if (maxPerDay > 0 && capped.Count > maxPerDay)
            {
                cut?.AddRange(capped.GetRange(maxPerDay, capped.Count - maxPerDay));
                capped.RemoveRange(maxPerDay, capped.Count - maxPerDay);
            }
            capped.AddRange(due.FindAll(Free));
            return capped;
        }

        /// <summary>The day's travelers with the characters put in: fixed slots at their place (in slot order), free ones at random.</summary>
        public static List<TravelerData> Queue(IReadOnlyList<TravelerData> regular, List<Planned> due, Func<float> random)
        {
            var queue = new List<TravelerData>(regular);
            var fixedOnes = due.FindAll(p => Slot(p) > 0);
            fixedOnes.Sort((x, y) => Slot(x).CompareTo(Slot(y)));
            // Free ones first: a fixed place is only exact if nothing is inserted after it (Korr must stay first on day 1, P86).
            foreach (var p in due.FindAll(p => Slot(p) == 0))
            {
                int at = Math.Min(queue.Count, (int)(random() * (queue.Count + 1)));
                queue.Insert(at, p.traveler);
            }
            foreach (var p in fixedOnes) queue.Insert(Math.Min(Slot(p) - 1, queue.Count), p.traveler);
            return queue;
        }

        private static int Slot(Planned p) => p.character.Appearances[p.appearance].slot;
    }
}
