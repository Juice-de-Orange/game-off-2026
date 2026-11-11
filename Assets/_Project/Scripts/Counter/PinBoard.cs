using System;
using System.Collections.Generic;
using GameOff.Documents;
using GameOff.Travelers;
using UnityEngine;

namespace GameOff.Counter
{
    /// <summary>
    /// The notes on the counter's pinboard (P41): bulletin and wanted list of the current day, new ones every morning.
    /// P68: on days with market events a third note "MARKET" hangs at marketSpot (texts from Market.TodayEvents).
    /// P33/P130: only today's notes hang. Every morning's notes go into Today (also a memo with the supervisor's new order, read in
    /// PinBoardCloseUp); the previous day's notes come down.
    /// Clicking a note to read it is handled by the DocumentDesk.
    /// </summary>
    public class PinBoard : MonoBehaviour
    {
        [SerializeField] private CounterShift shift;
        [SerializeField] private DocumentDesk desk;
        [Tooltip("Where the notes hang: page top (local +Z) up, page face (local +Y) into the room.")]
        [SerializeField] private Transform bulletinSpot;
        [SerializeField] private Transform wantedSpot;
        [Tooltip("Third note, only on days with market events (P68).")]
        [SerializeField] private Transform marketSpot;

        private readonly List<DocumentView> _views = new List<DocumentView>();
        private readonly PinBoardNotes _today = new PinBoardNotes();
        private bool _hung;

        /// <summary>P130: today's notes (no history).</summary>
        public PinBoardNotes Today => _today;

        /// <summary>P33: the morning's notes went up.</summary>
        public event Action Changed;

        private void OnEnable() => shift.DayBegun += Refresh;

        private void OnDisable() => shift.DayBegun -= Refresh;

        // The day flow may already have begun the day in its own Start.
        private void Start()
        {
            if (!_hung) Refresh();
        }

        private void Refresh()
        {
            foreach (var view in _views) desk.Unpin(view);
            _views.Clear();
            var day = shift.Day;
            if (day == null) return;
            _hung = true;
            _today.SetDay(day.Date, NotesFor(day, shift.NewToday, DayBulletin.Market()));
            Hang(PinNote.Bulletin, bulletinSpot);
            Hang(PinNote.Wanted, wantedSpot);
            Hang(PinNote.Market, marketSpot);
            Changed?.Invoke();
        }

        /// <summary>Today's note of this kind at the spot, if there is one.</summary>
        private void Hang(string kind, Transform spot)
        {
            if (spot == null) return;
            var note = _today.Of(kind);
            if (note == null) return;
            var view = desk.Pin(Data(note), spot);
            if (view != null) _views.Add(view);
        }

        /// <summary>The notes of a day: bulletin, wanted list (with photos), market (only with events), memo (only with a new order).</summary>
        public static List<PinNote> NotesFor(DayRules day, string newToday, string market)
        {
            var notes = new List<PinNote>
            {
                new PinNote(PinNote.Bulletin, "DAILY BULLETIN", day.Date, DayBulletin.Rules(day)),
                new PinNote(PinNote.Wanted, "WANTED", day.Date, DayBulletin.Wanted(day), WantedPhotos(day)),
            };
            if (!string.IsNullOrWhiteSpace(market)) notes.Add(new PinNote(PinNote.Market, "MARKET", day.Date, market));
            if (!string.IsNullOrWhiteSpace(newToday))
                notes.Add(new PinNote(PinNote.Memo, "MEMO", day.Date, "<b>FROM THE SUPERVISOR</b>\n\n" + newToday));
            return notes;
        }

        /// <summary>
        /// A photo for each wanted name: the photo on a paper of today's travelers that carries exactly that name (null if none).
        /// Someone coming under another name has no photo here, so it tells no more than the name does.
        /// </summary>
        public static List<(string name, Texture2D photo)> WantedPhotos(DayRules day)
        {
            var photos = new List<(string, Texture2D)>();
            foreach (var entry in day.Wanted)
            {
                Texture2D found = null;
                foreach (var traveler in day.Travelers)
                {
                    if (traveler == null) continue;
                    foreach (var doc in traveler.Documents)
                        if (doc != null && doc.Photo != null && TextMatch.Same(doc.Get("name"), entry.name))
                        {
                            found = doc.Photo;
                            break;
                        }
                    if (found != null) break;
                }
                photos.Add((entry.name, found));
            }
            return photos;
        }

        private static DocumentData Data(PinNote note) =>
            DocumentData.CreateRuntime(DocumentType.Notice, new List<DocumentField>
            {
                new DocumentField("title", note.Title),
                new DocumentField("date", note.Date),
                new DocumentField("body", note.Body),
            });
    }
}
