using System.Collections.Generic;
using GameOff.Documents;
using GameOff.Travelers;
using UnityEngine;

namespace GameOff.Counter
{
    /// <summary>
    /// The notes on the counter's pinboard (P41): bulletin and wanted list of the current day, new ones every morning.
    /// P68: on days with market events a third note "MARKET" hangs at marketSpot (texts from Market.TodayEvents).
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

        private DocumentView _bulletin, _wanted, _market;

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
            desk.Unpin(_market);
            _bulletin = _wanted = _market = null;
            var day = shift.Day;
            if (day == null) return;
            _bulletin = desk.Pin(Note("DAILY BULLETIN", day.Date, DayBulletin.Rules(day)), bulletinSpot);
            _wanted = desk.Pin(Note("WANTED", day.Date, DayBulletin.Wanted(day)), wantedSpot);
            string market = DayBulletin.Market();
            if (marketSpot != null && market.Length > 0) _market = desk.Pin(Note("MARKET", day.Date, market), marketSpot);
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
