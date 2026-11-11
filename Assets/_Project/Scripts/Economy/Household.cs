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
    /// P69: whoever is still critical after that many further nights dies (0 = never). Dead people need nothing and stay dead.
    /// </summary>
    public class Household
    {
        public const int Well = 0, Weak = 1, Sick = 2, Critical = 3, Dead = 4;

        private readonly int[] _condition;
        private readonly bool[] _feelsCold;
        private readonly int[] _criticalNights;
        private readonly int _criticalNightsToDeath;

        /// <param name="criticalNightsToDeath">P69: nights someone may stay critical before dying; 0 = nobody ever dies.</param>
        public Household(IList<bool> feelsCold, int criticalNightsToDeath = 0)
        {
            _criticalNightsToDeath = criticalNightsToDeath;
            _feelsCold = new bool[feelsCold.Count];
            for (int i = 0; i < _feelsCold.Length; i++) _feelsCold[i] = feelsCold[i];
            _condition = new int[_feelsCold.Length];
            _criticalNights = new int[_feelsCold.Length];
        }

        public int Count => _condition.Length;
        public int Condition(int person) => _condition[person];
        /// <summary>P128: nights this person has been critical in a row (for the save).</summary>
        public int CriticalNights(int person) => _criticalNights[person];
        public bool IsDead(int person) => _condition[person] == Dead;
        /// <summary>P69: true if one more night on critical kills this person (the morning letter warns).</summary>
        public bool WillDieNextNight(int person) =>
            _criticalNightsToDeath > 0 && _condition[person] == Critical && _criticalNights[person] >= _criticalNightsToDeath - 1;
        /// <summary>Who died in the last night.</summary>
        public IReadOnlyList<int> DiedTonight => _diedTonight;
        private readonly List<int> _diedTonight = new List<int>();

        public int Alive
        {
            get
            {
                int n = 0;
                foreach (int c in _condition) if (c != Dead) n++;
                return n;
            }
        }

        public bool AnySick
        {
            get
            {
                foreach (int c in _condition) if (c >= Sick && c != Dead) return true;
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
                if (c == Dead) continue;
                bool wasSick = c >= Sick;
                if (!food) c++;
                if (!heat && _feelsCold[i]) c++;
                if (medicine && wasSick) c--;
                if (food && heat && !wasSick && c == Weak) c = Well;
                next[i] = Math.Max(Well, Math.Min(Critical, c));
                if (next[i] == Critical && WillDieNextNight(i)) next[i] = Dead;
            }
            return next;
        }

        /// <summary>P128: a saved run continues with these steps (missing people stay well).</summary>
        public void Restore(IList<int> condition, IList<int> criticalNights, IEnumerable<int> diedTonight)
        {
            for (int i = 0; i < _condition.Length; i++)
            {
                _condition[i] = condition != null && i < condition.Count ? Math.Max(Well, Math.Min(Dead, condition[i])) : Well;
                _criticalNights[i] = criticalNights != null && i < criticalNights.Count ? Math.Max(0, criticalNights[i]) : 0;
            }
            _diedTonight.Clear();
            if (diedTonight != null)
                foreach (int i in diedTonight)
                    if (i >= 0 && i < _condition.Length && _condition[i] == Dead) _diedTonight.Add(i);
            Changed?.Invoke();
        }

        public void Settle(bool food, bool heat, bool medicine)
        {
            var next = Preview(food, heat, medicine);
            _diedTonight.Clear();
            for (int i = 0; i < next.Length; i++)
            {
                if (next[i] == Dead && _condition[i] != Dead) _diedTonight.Add(i);
                _criticalNights[i] = next[i] == Critical && _condition[i] == Critical ? _criticalNights[i] + 1 : 0;
            }
            Array.Copy(next, _condition, next.Length);
            Changed?.Invoke();
        }
    }
}
