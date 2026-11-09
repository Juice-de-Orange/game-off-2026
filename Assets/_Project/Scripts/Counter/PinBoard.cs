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
    /// P33: old notes stay. Every morning's notes go into History (also a memo with the supervisor's new order); at each spot the
    /// newest note hangs on top of the older ones of its kind, which peek out below it. PinBoardCloseUp shows all of them standing.
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
        [Tooltip("P33: older notes of the same kind hanging under the newest one at each spot.")]
        [SerializeField] private int olderNotesShown = 2;
        [Tooltip("P33: days of notes kept for the close-up.")]
        [SerializeField] private int daysKept = 30;

        private readonly List<DocumentView> _views = new List<DocumentView>();
        private PinBoardNotes _history;
        private bool _hung;

        /// <summary>P33: all notes so far, newest day last.</summary>
        public PinBoardNotes History => _history ??= new PinBoardNotes(daysKept);

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
            History.SetDay(day.Date, NotesFor(day, shift.NewToday, DayBulletin.Market()));
            Hang(PinNote.Bulletin, bulletinSpot);
            Hang(PinNote.Wanted, wantedSpot);
            Hang(PinNote.Market, marketSpot);
            Changed?.Invoke();
        }

        /// <summary>The newest note of this kind at the spot, older ones under it, slightly turned and pushed back.</summary>
        private void Hang(string kind, Transform spot)
        {
            if (spot == null) return;
            var pile = History.Pile(kind, 1 + Mathf.Max(0, olderNotesShown));
            for (int i = pile.Count - 1; i >= 0; i--)
            {
                var view = desk.Pin(Data(pile[i]), i == 0 ? spot : OlderSpot(spot, i));
                if (view != null) _views.Add(view);
            }
        }

        private static Transform OlderSpot(Transform spot, int depth)
        {
            string name = "Older_" + depth;
            var t = spot.Find(name);
            if (t != null) return t;
            t = new GameObject(name).transform;
            t.SetParent(spot, false);
            float side = depth % 2 == 1 ? 1f : -1f;
            t.localPosition = new Vector3(side * 0.012f * depth, -0.0015f * depth, -0.006f * depth); // behind (-Y) and a bit lower
            t.localRotation = Quaternion.Euler(0f, side * (2.5f + 1.5f * depth), 0f); // turned in the board's plane
            return t;
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
