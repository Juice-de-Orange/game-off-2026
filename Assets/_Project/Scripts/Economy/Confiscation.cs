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
    /// penalties add up over the run – at the limit the officer is discharged and the run ends (DISCHARGED).
    /// Wage and citations per decision stay in P07/P10 (SettlementWindow, Payroll).
    /// </summary>
    public class Confiscation : MonoBehaviour
    {
        [SerializeField] private ConfiscationTuning tuning;
        [SerializeField] private CounterShift shift;
        [SerializeField] private ShiftReportWindow window;
        [SerializeField] private DayManager dayManager;
        [Tooltip("P113: the mood lines after the airlock.")]
        [SerializeField] private MoraleTuning morale;

        public Conduct Conduct { get; } = new Conduct();
        /// <summary>P83: what was handed in "as ordered"; the player can try to break in for it in the evening.</summary>
        public Inventory Locker { get; } = new Inventory();
        /// <summary>P12: the ticket ending reads the morale.</summary>
        public static Confiscation Instance { get; private set; }
        public ConfiscationTuning Tuning => tuning;

        private static PlayerEconomy Economy => PlayerEconomy.Instance;
        private int CurrentDay => dayManager != null ? dayManager.Day : 1;

        /// <summary>P113: tonight's thought about the airlock; null if there is none.</summary>
        public string MoraleThought() => morale != null ? morale.Thought(Conduct, CurrentDay) : null;

        /// <summary>P113: what an evening guest says about today's airlock; null if there is nothing to say.</summary>
        public string MoraleCrowdLine(float random01) => morale != null ? morale.CrowdLine(Conduct, random01) : null;

        private void OnEnable()
        {
            Instance = this;
            if (shift != null)
            {
                shift.DecisionMade += OnDecision;
                shift.DayBegun += Conduct.BeginDay;
                shift.ShiftEnded += OnShiftEnded;
            }
            if (Economy != null) Economy.ShiftClosing += OnShiftClosing;
        }

        private void Start()
        {
            // PlayerEconomy sets Instance in Awake; subscribe here if it wasn't there yet in OnEnable.
            if (Economy != null) { Economy.ShiftClosing -= OnShiftClosing; Economy.ShiftClosing += OnShiftClosing; }
        }

        private void OnDisable()
        {
            if (Instance == this) Instance = null;
            if (shift != null)
            {
                shift.DecisionMade -= OnDecision;
                shift.DayBegun -= Conduct.BeginDay;
                shift.ShiftEnded -= OnShiftEnded;
            }
            if (Economy != null) Economy.ShiftClosing -= OnShiftClosing;
        }

        /// <summary>P113: a bad night follows the airlock; the line goes under the next morning's letter.</summary>
        private void OnShiftEnded()
        {
            string line = morale != null ? morale.Nightmare(Conduct, CurrentDay) : null;
            if (!string.IsNullOrEmpty(line) && Economy != null) Economy.AddLetterLine(line);
        }

        private void OnDecision(TravelerData traveler, Verdict chosen, Judgement judgement)
        {
            int change = Conduct.Judge(chosen, judgement, tuning, traveler.DisplayName);
            if (change != 0) Debug.Log($"[Conduct] airlock on {traveler.DisplayName}: morale {change:+0;-0} → {Conduct.Morale}");
            if (chosen == Verdict.Arrest)
            {
                bool onEvidence = Hangar.CargoCrate.Instance != null && Economy != null && Hangar.CargoCrate.Instance.HandIn(traveler, Economy.ShiftItems);
                if (!onEvidence) Confiscate(traveler);
            }
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

        /// <summary>At the disciplinary limit the run ends with the DISCHARGED ending.</summary>
        public void DischargeIfDue()
        {
            if (!Conduct.Discharged(tuning) || dayManager == null) return;
            dayManager.FinishRun(DischargedTitle, DischargedBody);
        }

        public const string DischargedTitle = "DISCHARGED";
        /// <summary>P110: the settlement ends the night with this when drone customs reached the limit.</summary>
        public string DischargedBody =>
            $"{tuning.disciplinaryLimit} disciplinary penalties. Station security walks you to the next shuttle - without pay.";

        /// <summary>P83: the break-in. Caught = a disciplinary penalty, the goods stay; otherwise all of it, as CONFISCATED goods.</summary>
        public string BreakIn(float roll)
        {
            var economy = Economy;
            if (economy == null) return "";
            if (roll < tuning.lockerCaughtChance)
            {
                Conduct.AddDisciplinary(1);
                DischargeIfDue();
                return $"Caught at the locker. DISCIPLINARY PENALTY {Conduct.Disciplinary} OF {tuning.disciplinaryLimit}.";
            }
            var names = new List<string>();
            foreach (var item in new List<ItemData>(Locker.Items))
            {
                int count = Locker.Count(item);
                economy.Inventory.Add(item.ConfiscatedVariant != null ? item.ConfiscatedVariant : item, count);
                names.Add($"{item.DisplayName} ×{count}");
            }
            Locker.Clear();
            return "Nobody saw. Taken: " + string.Join(", ", names) + ".";
        }

        /// <summary>Books the outcome of an answer: fine, kept goods, disciplinary penalty, discharge at the limit.</summary>
        public ShiftOutcome Apply(ShiftChoice choice, IReadOnlyList<(ItemData item, int count)> items, ItemData picked, float roll)
        {
            var o = ShiftReport.Resolve(choice, items, picked, tuning, roll);
            var economy = Economy;
            if (choice == ShiftChoice.AsOrdered)
                foreach (var (item, count) in items) Locker.Add(item, count);
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
