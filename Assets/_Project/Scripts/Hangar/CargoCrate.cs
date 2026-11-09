using GameOff.Day;
using GameOff.Economy;
using GameOff.Player;
using GameOff.Travelers;
using TMPro;
using UnityEngine;

namespace GameOff.Hangar
{
    /// <summary>
    /// P35: the flagged crate. It stands in the hangar on the days with a case (CargoCaseTuning); the morning letter announces it.
    /// [E] shows what is hidden in it: reporting it (only before the shift) prints a slip when the owner comes to the counter, and
    /// then an arrest counts as right and yields the goods; pocketing it is a theft with a risk.
    /// </summary>
    public class CargoCrate : MonoBehaviour
    {
        [SerializeField] private CargoCaseTuning tuning;
        [SerializeField] private DayManager day;
        [SerializeField] private CounterShift shift;
        [SerializeField] private FirstPersonController player;
        [SerializeField] private Interactable interactable;
        [Tooltip("Model, label and trigger; only on while there is a case today.")]
        [SerializeField] private GameObject visual;
        [SerializeField] private TMP_Text label;

        private CargoCase _case;
        private bool _searched, _reported, _shiftStarted;

        public static CargoCrate Instance { get; private set; }

        private void OnEnable()
        {
            Instance = this;
            day.DayStarted += OnDayStarted;
            interactable.OnInteract.AddListener(OnInteract);
            shift.ArrestAllowed = HasEvidence;
            shift.TravelerArrived += OnTravelerArrived;
            shift.ShiftEnded += AnnounceTomorrow;
            shift.ShiftStarted += OnShiftStarted;
        }

        private void OnDisable()
        {
            if (Instance == this) Instance = null;
            day.DayStarted -= OnDayStarted;
            interactable.OnInteract.RemoveListener(OnInteract);
            shift.ArrestAllowed = null;
            shift.TravelerArrived -= OnTravelerArrived;
            shift.ShiftEnded -= AnnounceTomorrow;
            shift.ShiftStarted -= OnShiftStarted;
        }

        private void OnShiftStarted() => _shiftStarted = true;

        /// <summary>Reporting only helps before the counter opens. Phase is Shift from the moment the letter closes (Morning is the letter itself).</summary>
        public static bool CanReport(DayManager.Phase phase, bool shiftStarted) => phase == DayManager.Phase.Shift && !shiftStarted;

        private void OnDayStarted(int number)
        {
            _case = tuning.ForDay(number);
            _searched = _reported = _shiftStarted = false;
            visual.SetActive(_case != null);
            if (_case != null) label.text = CargoCaseTuning.Fill("HOLD FOR INSPECTION\n{name}", _case);
        }

        private void AnnounceTomorrow()
        {
            var next = tuning.ForDay(day.Day + 1);
            if (next != null && PlayerEconomy.Instance != null)
                PlayerEconomy.Instance.AddLetterLine(CargoCaseTuning.Fill(tuning.letterNotice, next));
        }

        private bool HasEvidence(TravelerData traveler) =>
            _reported && _case != null && traveler != null && traveler.DisplayName == _case.traveler;

        private void OnTravelerArrived(TravelerData traveler)
        {
            if (HasEvidence(traveler)) shift.PrintSlip("E-01", CargoCaseTuning.Fill(tuning.slipReason, _case).ToUpperInvariant(), tuning.slipPenalty);
        }

        /// <summary>On an arrest with the crate reported: the goods go into the shift inventory.</summary>
        /// <returns>True if this arrest rests on the crate: then only its goods are taken, the declared cargo is legal.</returns>
        public bool HandIn(TravelerData traveler, Inventory shiftItems)
        {
            if (!HasEvidence(traveler) || _case.item == null) return false;
            shiftItems.Add(_case.item, _case.count);
            _reported = false; // once
            return true;
        }

        private void OnInteract()
        {
            if (_case == null || player == null) return;
            var ui = day.UI;
            if (_searched) { ui.ShowHint(tuning.alreadySearched, 4f); return; }
            player.SetControlEnabled(false);
            bool canReport = CanReport(day.CurrentPhase, _shiftStarted);
            ui.ShowDialog(CargoCaseTuning.Fill(tuning.dialogTitle, _case), CargoCaseTuning.Fill(tuning.dialogBody, _case),
                canReport ? tuning.reportLabel : null, canReport ? Report : (System.Action)null, Pocket, Resume, tuning.pocketLabel, tuning.leaveLabel);
        }

        private void Resume() => player.SetControlEnabled(true);

        private void Report()
        {
            _searched = _reported = true;
            day.UI.ShowHint(CargoCaseTuning.Fill(tuning.reported, _case), 5f);
            Resume();
        }

        private void Pocket()
        {
            _searched = true;
            var economy = PlayerEconomy.Instance;
            var confiscation = Confiscation.Instance;
            if (confiscation != null) confiscation.Conduct.AddMorale(tuning.moraleCost);
            Resume();
            if (Random.value < tuning.caughtChance)
            {
                if (confiscation != null) confiscation.Conduct.AddDisciplinary(1);
                if (economy != null) economy.AddLetterLine(tuning.caughtLetter);
                day.UI.ShowHint(tuning.caught, 5f);
                if (confiscation != null) confiscation.DischargeIfDue();
                return;
            }
            if (economy != null && _case.item != null) economy.Inventory.Add(_case.item, _case.count);
            day.UI.ShowHint(CargoCaseTuning.Fill(tuning.pocketed, _case), 5f);
        }
    }
}
