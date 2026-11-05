using System.Collections.Generic;
using GameOff.Documents;
using GameOff.Travelers;
using UnityEngine;

namespace GameOff.Counter
{
    /// <summary>
    /// The notes on the counter's pinboard (P41): bulletin and wanted list of the current day, new ones every morning.
    /// Clicking a note to read it is handled by the DocumentDesk.
    /// </summary>
    public class PinBoard : MonoBehaviour
    {
        [SerializeField] private CounterShift shift;
        [SerializeField] private DocumentDesk desk;
        [Tooltip("Where the notes hang: page top (local +Z) up, page face (local +Y) into the room.")]
        [SerializeField] private Transform bulletinSpot;
        [SerializeField] private Transform wantedSpot;

        private DocumentView _bulletin, _wanted;

        private void OnEnable() => shift.DayBegun += Refresh;

        private void OnDisable() => shift.DayBegun -= Refresh;

        // The day flow may already have begun the day in its own Start.
        private void Start()
        {
            if (_bulletin == null) Refresh();
        }

        private void Refresh()
        {
            desk.Unpin(_bulletin);
            desk.Unpin(_wanted);
            _bulletin = _wanted = null;
            var day = shift.Day;
            if (day == null) return;
            _bulletin = desk.Pin(Note("DAILY BULLETIN", day.Date, DayBulletin.Rules(day)), bulletinSpot);
            _wanted = desk.Pin(Note("WANTED", day.Date, DayBulletin.Wanted(day)), wantedSpot);
        }

        private static DocumentData Note(string title, string date, string body) =>
            DocumentData.CreateRuntime(DocumentType.Notice, new List<DocumentField>
            {
                new DocumentField("title", title),
                new DocumentField("date", date),
                new DocumentField("body", body),
            });
    }
}
