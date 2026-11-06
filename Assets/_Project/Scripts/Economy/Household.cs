using System;
using System.Collections.Generic;

namespace GameOff.Economy
{
    /// <summary>
    /// The family's condition over the run (P10). Each person has a step: 0 well, 1 weak, 2 sick, 3 critical.
    /// One night, depending on what was paid:
    ///   no food      → everyone one step worse
    ///   no heating   → people who feel the cold one step worse
    ///   medicine     → every sick person (step 2+) one step better
    ///   food + heat  → weak people (step 1) recover to well
    /// </summary>
    public class Household
    {
        public const int Well = 0, Weak = 1, Sick = 2, Critical = 3;

        private readonly int[] _condition;
        private readonly bool[] _feelsCold;

        public Household(IList<bool> feelsCold)
        {
            _feelsCold = new bool[feelsCold.Count];
            for (int i = 0; i < _feelsCold.Length; i++) _feelsCold[i] = feelsCold[i];
            _condition = new int[_feelsCold.Length];
        }

        public int Count => _condition.Length;
        public int Condition(int person) => _condition[person];

        public bool AnySick
        {
            get
            {
                foreach (int c in _condition) if (c >= Sick) return true;
                return false;
            }
        }

        public event Action Changed;

        /// <summary>What the night would do, without changing anything (for the settlement preview).</summary>
        public int[] Preview(bool food, bool heat, bool medicine)
        {
            var next = (int[])_condition.Clone();
            for (int i = 0; i < next.Length; i++)
            {
                int c = next[i];
                bool wasSick = c >= Sick;
                if (!food) c++;
                if (!heat && _feelsCold[i]) c++;
                if (medicine && wasSick) c--;
                if (food && heat && !wasSick && c == Weak) c = Well;
                next[i] = Math.Max(Well, Math.Min(Critical, c));
            }
            return next;
        }

        public void Settle(bool food, bool heat, bool medicine)
        {
            var next = Preview(food, heat, medicine);
            Array.Copy(next, _condition, next.Length);
            Changed?.Invoke();
        }
    }
}
