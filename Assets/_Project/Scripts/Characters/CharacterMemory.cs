using System.Collections.Generic;
using GameOff.Economy;
using GameOff.Travelers;

namespace GameOff.Characters
{
    /// <summary>
    /// P75: what happened with every recurring character in this run (not saved across restarts, like everything else), plus the
    /// consequences waiting for later days (market events, wanted entries).
    /// </summary>
    public class CharacterMemory
    {
        public struct Record
        {
            public RecurringCharacter character;
            public int appearance;
            public int day;
            public Outcome outcome;
        }

        public struct PendingWanted
        {
            public RecurringCharacter character;
            public WantedEntry entry;
            public int fromDay, toDay;
        }

        private readonly List<Record> _records = new List<Record>();
        private readonly List<PendingWanted> _wanted = new List<PendingWanted>();
        private readonly Dictionary<int, List<MarketEvent>> _events = new Dictionary<int, List<MarketEvent>>();
        private readonly HashSet<string> _flags = new HashSet<string>();

        /// <summary>P87: a choice the player made (e.g. joined the resistance).</summary>
        public void SetFlag(string flag)
        {
            if (!string.IsNullOrWhiteSpace(flag)) _flags.Add(flag);
        }

        /// <summary>P77: something a character left with the player until the next encounter.</summary>
        public void Entrust(RecurringCharacter c, ItemData item, int deposit) => _entrusted[c] = (item, deposit);

        /// <summary>P77: what this character left and is owed back, if anything; ends the arrangement.</summary>
        public bool TakeEntrusted(RecurringCharacter c, out ItemData item, out int deposit)
        {
            item = null;
            deposit = 0;
            if (!_entrusted.TryGetValue(c, out var e)) return false;
            _entrusted.Remove(c);
            (item, deposit) = e;
            return true;
        }

        private readonly Dictionary<RecurringCharacter, (ItemData item, int deposit)> _entrusted = new Dictionary<RecurringCharacter, (ItemData, int)>();

        /// <summary>P85: a consequence that takes effect on a later day.</summary>
        public void ScheduleEffect(int day, Consequence effect) => _effects.Add((day, effect));

        /// <summary>P85: the effects due today; each comes out once.</summary>
        public List<Consequence> TakeEffects(int day)
        {
            var due = new List<Consequence>();
            for (int i = _effects.Count - 1; i >= 0; i--)
                if (_effects[i].day <= day)
                {
                    due.Insert(0, _effects[i].effect);
                    _effects.RemoveAt(i);
                }
            return due;
        }

        private readonly List<(int day, Consequence effect)> _effects = new List<(int, Consequence)>();

        public bool HasFlag(string flag) => !string.IsNullOrWhiteSpace(flag) && _flags.Contains(flag);

        public IReadOnlyList<Record> Records => _records;

        public void Remember(RecurringCharacter c, int appearance, int day, Outcome outcome) =>
            _records.Add(new Record { character = c, appearance = appearance, day = day, outcome = outcome });

        public bool Met(RecurringCharacter c) => _records.Exists(r => r.character == c);
        public bool Alive(RecurringCharacter c) => !_records.Exists(r => r.character == c && r.outcome == Outcome.Dead);
        public bool Done(RecurringCharacter c, int appearance) => _records.Exists(r => r.character == c && r.appearance == appearance);
        public bool MetOn(RecurringCharacter c, int day) => _records.Exists(r => r.character == c && r.day == day);
        public int Count(RecurringCharacter c, Outcome o) => _records.FindAll(r => r.character == c && r.outcome == o).Count;

        public Outcome Last(RecurringCharacter c)
        {
            for (int i = _records.Count - 1; i >= 0; i--)
                if (_records[i].character == c) return _records[i].outcome;
            return Outcome.None;
        }

        public bool Meets(RecurringCharacter c, AppearanceCondition condition, string flag = null)
        {
            if (!Alive(c)) return false; // the dead never come back
            var last = Last(c);
            return condition switch
            {
                AppearanceCondition.NeverMet => !Met(c),
                AppearanceCondition.Alive => Met(c),
                AppearanceCondition.LastHelped => last == Outcome.Helped,
                AppearanceCondition.LastBetrayed => last == Outcome.Betrayed,
                AppearanceCondition.LastConfiscated => last == Outcome.Confiscated,
                AppearanceCondition.LastWronged => last == Outcome.Betrayed || last == Outcome.Confiscated,
                AppearanceCondition.FlagSet => Met(c) && HasFlag(flag),
                AppearanceCondition.FlagNotSet => Met(c) && !HasFlag(flag),
                _ => true,
            };
        }

        // ---------- Consequences ----------

        public void ScheduleEvent(int day, MarketEvent e)
        {
            if (e == null) return;
            if (!_events.TryGetValue(day, out var list)) _events[day] = list = new List<MarketEvent>();
            if (!list.Contains(e)) list.Add(e);
        }

        public IReadOnlyList<MarketEvent> EventsOn(int day) => _events.TryGetValue(day, out var list) ? list : (IReadOnlyList<MarketEvent>)new MarketEvent[0];

        public void ScheduleWanted(RecurringCharacter c, WantedEntry entry, int fromDay, int days) =>
            _wanted.Add(new PendingWanted { character = c, entry = entry, fromDay = fromDay, toDay = fromDay + System.Math.Max(1, days) - 1 });

        public List<WantedEntry> WantedOn(int day)
        {
            var list = new List<WantedEntry>();
            foreach (var w in _wanted)
                if (day >= w.fromDay && day <= w.toDay && Alive(w.character)) list.Add(w.entry);
            return list;
        }
    }
}
