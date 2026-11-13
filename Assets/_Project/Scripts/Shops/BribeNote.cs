using System.Collections.Generic;
using GameOff.Day;
using GameOff.Documents;
using GameOff.Economy;
using GameOff.Travelers;
using UnityEngine;

namespace GameOff.Shops
{
    /// <summary>
    /// P66: on days with a bribe the barkeeper's note lies on the counter desk at shift start, with the envelope beside it (only if
    /// the traveler it names is in today's queue). P139: both
    /// lie loose (DocumentDesk.Lay / AddLoose): they can be dragged like the papers and the note clicked to read, but they are no
    /// traveler papers, so the drawer, the return check and confiscation ignore them. The envelope is laid last, so it is on top.
    /// The traveler is judged as usual. Admitted → the amount is paid in tonight's settlement; turned away or arrested → the
    /// barkeeper sulks (no rumour tonight and tomorrow). Note and envelope go at shift end (and at the start of a day).
    /// </summary>
    public class BribeNote : MonoBehaviour
    {
        [SerializeField] private DayManager day;
        [SerializeField] private CounterShift shift;
        [SerializeField] private DocumentDesk desk;
        [Tooltip("Where the note lies on the desk (page top = local +Z, face = local +Y). Made by the EconomyBuilder.")]
        [SerializeField] private Transform spot;
        [Tooltip("P80: the envelope the note came in, visible while the note lies on the desk (P139: beside it, draggable).")]
        [SerializeField] private GameObject envelope;

        private DocumentView _note;
        private Vector3 _envelopeHome;
        private Quaternion _envelopeHomeRotation;

        private Bribe Today => day != null ? day.Entry(day.Day).bribe : null;

        private void Awake()
        {
            if (envelope == null) return;
            _envelopeHome = envelope.transform.localPosition; // where the builder put it; it comes back there every bribe day
            _envelopeHomeRotation = envelope.transform.localRotation;
        }

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

        /// <summary>The note only lies there if its traveler really comes today (a character may be gone, e.g. Quill out of the airlock).</summary>
        public static bool LaysNote(Bribe bribe, DayRules today)
        {
            if (bribe == null || !bribe.Active || today == null) return false;
            foreach (var t in today.Travelers)
                if (t == bribe.traveler) return true;
            return false;
        }

        private void OnShiftStarted()
        {
            var bribe = Today;
            if (!LaysNote(bribe, shift.Day) || _note != null || desk == null || spot == null) return;
            var data = DocumentData.CreateRuntime(DocumentType.Notice, new List<DocumentField>
            {
                new DocumentField("title", "A NOTE"),
                new DocumentField("date", ""),
                new DocumentField("body", bribe.Text),
            });
            _note = desk.Lay(data, spot);
            if (envelope != null)
            {
                envelope.transform.localPosition = _envelopeHome;
                envelope.transform.localRotation = _envelopeHomeRotation;
                envelope.SetActive(true);
                desk.AddLoose(envelope.transform); // after the note: should they overlap, the money lies on top (#43)
            }
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
            if (desk == null) return;
            if (_note != null) desk.Unpin(_note); // a loose note: also when it was just being read or dragged
            _note = null;
            if (envelope == null || !envelope.activeSelf) return;
            desk.RemoveLoose(envelope.transform);
            envelope.SetActive(false);
        }
    }
}
