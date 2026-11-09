using System.Collections.Generic;

namespace GameOff.Counter
{
    /// <summary>
    /// P33: the pinboard's notes over the run. Every morning the day's notes are added; older ones stay (up to maxDays days).
    /// Setting the same day again replaces its notes (the board may refresh twice on one morning).
    /// </summary>
    public class PinBoardNotes
    {
        private readonly List<(string day, List<PinNote> notes)> _days = new List<(string, List<PinNote>)>();
        private readonly int _maxDays;

        public PinBoardNotes(int maxDays = 30) => _maxDays = maxDays < 1 ? 1 : maxDays;

        public int DayCount => _days.Count;

        public void SetDay(string day, IEnumerable<PinNote> notes)
        {
            var list = new List<PinNote>(notes);
            for (int i = 0; i < _days.Count; i++)
            {
                if (_days[i].day != day) continue;
                _days.RemoveAt(i); // the same morning again: its notes are replaced and it becomes the newest day
                break;
            }
            _days.Add((day, list));
            while (_days.Count > _maxDays) _days.RemoveAt(0);
        }

        /// <summary>Notes of a day by age: 0 = today, 1 = the day before, ... Empty beyond the oldest day kept.</summary>
        public IReadOnlyList<PinNote> NotesOfDay(int age)
        {
            int i = _days.Count - 1 - age;
            return age >= 0 && i >= 0 ? _days[i].notes : (IReadOnlyList<PinNote>)new List<PinNote>();
        }

        /// <summary>The day key (date) by age, "" beyond the oldest day kept.</summary>
        public string DayOf(int age)
        {
            int i = _days.Count - 1 - age;
            return age >= 0 && i >= 0 ? _days[i].day : "";
        }

        /// <summary>Notes of one kind, newest first, at most depth of them; days without that kind are skipped.</summary>
        public List<PinNote> Pile(string kind, int depth)
        {
            var pile = new List<PinNote>();
            for (int i = _days.Count - 1; i >= 0 && pile.Count < depth; i--)
                foreach (var note in _days[i].notes)
                    if (note.Kind == kind)
                    {
                        pile.Add(note);
                        break;
                    }
            return pile;
        }
    }
}
