using System.Collections.Generic;
using GameOff.Day;
using GameOff.Documents;
using GameOff.Travelers;
using GameOff.UI;
using UnityEngine;

namespace GameOff.Economy
{
    /// <summary>
    /// P08 at the counter: an arrest confiscates the goods on the traveler's cargo manifest into the shift inventory (P81); the airlock
    /// costs hidden morale (much more on someone who didn't deserve it). At shift end the supervisor asks what to report
    /// (ShiftReportWindow): fines go to the wallet (TODAY), kept goods into the inventory as CONFISCATED variants, disciplinary
    /// penalties add up over the run – at the limit the officer is discharged (placeholder ending until P12).
    /// Wage and citations per decision stay in P07/P10 (SettlementWindow, Payroll).
    /// </summary>
    public class Confiscation : MonoBehaviour
    {
        [SerializeField] private ConfiscationTuning tuning;
        [SerializeField] private CounterShift shift;
        [SerializeField] private ShiftReportWindow window;
        [SerializeField] private DayManager dayManager;

        public Conduct Conduct { get; } = new Conduct();
        public ConfiscationTuning Tuning => tuning;

        private static PlayerEconomy Economy => PlayerEconomy.Instance;

        private void OnEnable()
        {
            if (shift != null) shift.DecisionMade += OnDecision;
            if (Economy != null) Economy.ShiftClosing += OnShiftClosing;
        }

        private void Start()
        {
            // PlayerEconomy sets Instance in Awake; subscribe here if it wasn't there yet in OnEnable.
            if (Economy != null) { Economy.ShiftClosing -= OnShiftClosing; Economy.ShiftClosing += OnShiftClosing; }
        }

        private void OnDisable()
        {
            if (shift != null) shift.DecisionMade -= OnDecision;
            if (Economy != null) Economy.ShiftClosing -= OnShiftClosing;
        }

        private void OnDecision(TravelerData traveler, Verdict chosen, Judgement judgement)
        {
            int morale = Conduct.Judge(chosen, judgement, tuning);
            if (morale != 0) Debug.Log($"[Conduct] airlock on {traveler.DisplayName}: morale {morale:+0;-0} → {Conduct.Morale}");
            if (chosen == Verdict.Arrest) Confiscate(traveler);
        }

        /// <summary>The goods on the cargo manifest go into the shift inventory (P81).</summary>
        public void Confiscate(TravelerData traveler)
        {
            if (Economy == null || traveler == null) return;
            var manifest = traveler.Document(DocumentType.CargoManifest);
            if (manifest == null) return;
            foreach (var (item, count) in CargoReader.Read(manifest.Get("cargo"), tuning.goods, tuning.maxPerItem))
            {
                Economy.ShiftItems.Add(item, count);
                Debug.Log($"[Confiscation] {traveler.DisplayName}: {item.DisplayName} ×{count} into the shift inventory");
            }
        }

        private void OnShiftClosing(Inventory shiftItems)
        {
            if (shiftItems.Items.Count == 0) return;
            // Snapshot: the shift inventory is emptied right after this event.
            var items = new List<(ItemData, int)>();
            foreach (var item in shiftItems.Items) items.Add((item, shiftItems.Count(item)));
            if (window != null) window.Show(items, (choice, picked) => Apply(choice, items, picked, Random.value), DischargeIfDue);
            else { Apply(ShiftChoice.AsOrdered, items, null, 0f); DischargeIfDue(); }
        }

        /// <summary>At the disciplinary limit the run ends (placeholder text until the endings, P12).</summary>
        public void DischargeIfDue()
        {
            if (!Conduct.Discharged(tuning) || dayManager == null) return;
            dayManager.FinishRun("DISCHARGED",
                $"{tuning.disciplinaryLimit} disciplinary penalties. Station security walks you to the next shuttle - without pay.");
        }

        /// <summary>Books the outcome of an answer: fine, kept goods, disciplinary penalty, discharge at the limit.</summary>
        public ShiftOutcome Apply(ShiftChoice choice, IReadOnlyList<(ItemData item, int count)> items, ItemData picked, float roll)
        {
            var o = ShiftReport.Resolve(choice, items, picked, tuning, roll);
            var economy = Economy;
            if (economy != null)
            {
                foreach (var (item, count) in o.Kept) economy.Inventory.Add(item, count);
                if (o.Fine > 0) economy.Wallet.Charge(o.Fine, "Fine · confiscated goods");
            }
            if (o.Disciplinary > 0)
            {
                Conduct.AddDisciplinary(o.Disciplinary);
                o.Text += $"\n\nDISCIPLINARY PENALTY {Conduct.Disciplinary} OF {tuning.disciplinaryLimit}.";
                if (Conduct.Discharged(tuning)) o.Text += "\nYou are discharged from service.";
            }
            Debug.Log($"[Confiscation] {choice}: caught {o.Caught}, fine {o.Fine}, kept {o.Kept.Count}, disciplinary {Conduct.Disciplinary}");
            return o;
        }
    }
}
