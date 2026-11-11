using System.Collections.Generic;

namespace GameOff.Counter
{
    /// <summary>
    /// P33/P130: the pinboard's notes of today. Every morning the day's notes replace the previous ones; no history is kept.
    /// </summary>
    public class PinBoardNotes
    {
        private readonly List<PinNote> _notes = new List<PinNote>();

        /// <summary>The day key (date) of the notes, "" before the first morning.</summary>
        public string Day { get; private set; } = "";

        public IReadOnlyList<PinNote> Notes => _notes;

        public void SetDay(string day, IEnumerable<PinNote> notes)
        {
            Day = day ?? "";
            _notes.Clear();
            _notes.AddRange(notes);
        }

        /// <summary>Today's note of one kind, null if there is none.</summary>
        public PinNote Of(string kind)
        {
            foreach (var note in _notes)
                if (note.Kind == kind) return note;
            return null;
        }
    }
}
