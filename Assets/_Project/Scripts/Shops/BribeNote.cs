using System.Collections.Generic;
using GameOff.Day;
using GameOff.Documents;
using GameOff.Economy;
using GameOff.Travelers;
using UnityEngine;

namespace GameOff.Shops
{
    /// <summary>
    /// P66: on days with a bribe the barkeeper's note lies on the counter desk at shift start (pinned like the board notes, so the
    /// drawer and confiscation ignore it; it can be clicked to read). The traveler is judged as usual. Admitted → the amount is paid
    /// in tonight's settlement; turned away or arrested → the barkeeper sulks (no rumour tonight and tomorrow). The note goes at shift end.
    /// </summary>
    public class BribeNote : MonoBehaviour
    {
        [SerializeField] private DayManager day;
        [SerializeField] private CounterShift shift;
        [SerializeField] private DocumentDesk desk;
        [Tooltip("Where the note lies on the desk (page top = local +Z, face = local +Y). Made by the EconomyBuilder.")]
        [SerializeField] private Transform spot;

        private DocumentView _note;

        private Bribe Today => day != null ? day.Entry(day.Day).bribe : null;

        private void OnEnable()
        {
            if (shift == null) return;
            shift.ShiftStarted += OnShiftStarted;
            shift.DecisionMade += OnDecision;
            shift.ShiftEnded += TakeAway;
            shift.DayBegun += TakeAway;
        }

        private void OnDisable()
        {
            if (shift == null) return;
            shift.ShiftStarted -= OnShiftStarted;
            shift.DecisionMade -= OnDecision;
            shift.ShiftEnded -= TakeAway;
            shift.DayBegun -= TakeAway;
        }

        private void OnShiftStarted()
        {
            var bribe = Today;
            if (bribe == null || !bribe.Active || _note != null || desk == null || spot == null) return;
            var data = DocumentData.CreateRuntime(DocumentType.Notice, new List<DocumentField>
            {
                new DocumentField("title", "A NOTE"),
                new DocumentField("date", ""),
                new DocumentField("body", bribe.Text),
            });
            _note = desk.Pin(data, spot);
        }

        private void OnDecision(TravelerData traveler, Verdict chosen, Judgement judgement)
        {
            var bribe = Today;
            var economy = PlayerEconomy.Instance;
            if (bribe == null || !bribe.Active || traveler != bribe.traveler || economy == null) return;
            if (chosen == Verdict.Admit)
            {
                economy.BribeTonight = bribe.amount;
                economy.BribeFrom = traveler.DisplayName;
            }
            else economy.BarkeeperSulksFrom = day.Day;
        }

        private void TakeAway()
        {
            if (_note == null) return;
            desk.Unpin(_note);
            _note = null;
        }
    }
}
