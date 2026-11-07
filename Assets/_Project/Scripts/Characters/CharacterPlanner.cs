using System;
using System.Collections.Generic;
using GameOff.Travelers;

namespace GameOff.Characters
{
    /// <summary>
    /// P75, without Unity (EditMode-tested): which recurring characters come today and where they stand in the queue.
    /// Per character at most one appearance a day: the first one in its list that is due, not done yet, and whose condition holds.
    /// A window of several days picks its day at random (each day 1/(days left) – on the last day it always comes).
    /// At most maxPerDay encounters a day (fixed slots first). Slot n = n-th in the queue; 0 = anywhere.
    /// </summary>
    public static class CharacterPlanner
    {
        public struct Planned
        {
            public RecurringCharacter character;
            public int appearance;
            public TravelerData traveler;
        }

        public static List<Planned> Due(int day, IReadOnlyList<RecurringCharacter> characters, CharacterMemory memory, int maxPerDay,
            Func<float> random)
        {
            var due = new List<Planned>();
            foreach (var c in characters)
            {
                if (c == null || memory.MetOn(c, day)) continue;
                for (int i = 0; i < c.Appearances.Count; i++)
                {
                    var a = c.Appearances[i];
                    if (a.traveler == null || !a.InWindow(day) || memory.Done(c, i) || !memory.Meets(c, a.condition)) continue;
                    int lastDay = Math.Max(a.dayFrom, a.dayTo);
                    int daysLeft = lastDay - day + 1;
                    if (daysLeft > 1 && random() >= 1f / daysLeft) break; // not today; the window goes on
                    due.Add(new Planned { character = c, appearance = i, traveler = a.traveler });
                    break;
                }
            }
            // fixed slots first, then the rest; cap per day
            due.Sort((x, y) => (Slot(x) == 0).CompareTo(Slot(y) == 0));
            if (maxPerDay > 0 && due.Count > maxPerDay) due.RemoveRange(maxPerDay, due.Count - maxPerDay);
            return due;
        }

        /// <summary>The day's travelers with the characters put in: fixed slots at their place (in slot order), free ones at random.</summary>
        public static List<TravelerData> Queue(IReadOnlyList<TravelerData> regular, List<Planned> due, Func<float> random)
        {
            var queue = new List<TravelerData>(regular);
            var fixedOnes = due.FindAll(p => Slot(p) > 0);
            fixedOnes.Sort((x, y) => Slot(x).CompareTo(Slot(y)));
            foreach (var p in fixedOnes) queue.Insert(Math.Min(Slot(p) - 1, queue.Count), p.traveler);
            foreach (var p in due.FindAll(p => Slot(p) == 0))
            {
                int at = Math.Min(queue.Count, (int)(random() * (queue.Count + 1)));
                queue.Insert(at, p.traveler);
            }
            return queue;
        }

        private static int Slot(Planned p) => p.character.Appearances[p.appearance].slot;
    }
}
