using System.Collections.Generic;
using System.Linq;
using GameOff.Day;
using GameOff.Departure;
using GameOff.Travelers;
using UnityEngine;

namespace GameOff.Economy
{
    /// <summary>
    /// Money and items of one run (P46): lives in Main, so a new game from the menu starts fresh; survives the day changes.
    /// Other packages use PlayerEconomy.Instance:
    ///   Wallet.Add(10, "Wage · Vesna")  ·  Wallet.TrySpend(60, "Kiosk · Scanner")  ·  Wallet.Charge(30, "Rent")  (P08, P10)
    ///   Inventory.Add(item) / Remove(item)  (P08 confiscation, P11 kiosk, P13 drone)
    /// TODAY (Wallet.Today) is cleared every morning (DayManager.DayStarted).
    /// Also holds the family's condition (Household, P10) and what every vendor has on the shelf (Supply, P11; topped up each morning).
    /// </summary>
    public class PlayerEconomy : MonoBehaviour
    {
        [SerializeField] private EconomyTuning tuning;
        [Tooltip("Wage, rent, needs and the family (P10).")]
        [SerializeField] private HouseholdTuning household;
        [Tooltip("Optional: clears TODAY each morning.")]
        [SerializeField] private DayManager dayManager;
        [Tooltip("Given at the start while the dev tools are on (to test selling and sending home).")]
        [SerializeField] private List<ItemData> devStartItems = new List<ItemData>();
        [Tooltip("P81: empties the shift inventory when the shift ends.")]
        [SerializeField] private CounterShift shift;

        public static PlayerEconomy Instance { get; private set; }

        public Wallet Wallet { get; private set; }
        public Inventory Inventory { get; private set; }
        /// <summary>
        /// P81: what was confiscated during this shift. Only exists at the counter (I while seated) and is emptied at shift end;
        /// ShiftClosing runs first, so P08 can decide what happens to it (report, keep, hand in).
        /// </summary>
        public Inventory ShiftItems { get; private set; }
        public event System.Action<Inventory> ShiftClosing;
        public bool DevTools => dayManager == null || dayManager.DevTools;
        public EconomyTuning Tuning => tuning;
        public HouseholdTuning HouseholdTuning => household;
        public Household Household { get; private set; }
        /// <summary>Items sent home by drone (P13); P131: the family's stock, each night uses one item per need, the rest stays.</summary>
        public HomeDelivery Delivery { get; } = new HomeDelivery();
        /// <summary>Notes of the day, e.g. the barkeeper's rumour (P65); shown under TODAY in the inventory, cleared every morning.</summary>
        public List<string> Notes { get; } = new List<string>();

        /// <summary>P66: the bribe earned today (paid in tonight's settlement) and who paid it; 0 = none.</summary>
        public int BribeTonight { get; set; }
        public string BribeFrom { get; set; } = "";
        /// <summary>P66: the barkeeper sulks from this day until the next (no rumour) after a refused bribe; 0 = never.</summary>
        public int BarkeeperSulksFrom { get; set; }
        /// <summary>P72: the ticket home is bought; boarding the shuttle in the evening ends the run.</summary>
        public bool HasTicket { get; set; }
        /// <summary>P63: the wearable item you have on right now, or null. Leaves your feet when it leaves your inventory.</summary>
        public ItemData Wearing
        {
            get
            {
                if (_wearing != null && Inventory.Count(_wearing) == 0) _wearing = null;
                return _wearing;
            }
            set => _wearing = value;
        }
        private ItemData _wearing;
        public bool BarkeeperSulks(int day) => BarkeeperSulksFrom > 0 && day >= BarkeeperSulksFrom && day <= BarkeeperSulksFrom + 1;

        private readonly Dictionary<VendorStock, VendorSupply> _supplies = new Dictionary<VendorStock, VendorSupply>();

        /// <summary>The vendor's shelf right now (created full on first use).</summary>
        public VendorSupply Supply(VendorStock stock)
        {
            if (!_supplies.TryGetValue(stock, out var supply))
            {
                _supplies[stock] = supply = new VendorSupply(stock);
                // P128: what the player sold to this vendor before the save is back on its shelf.
                if (stock != null && _savedExtras.TryGetValue(stock.name, out var extras))
                {
                    _savedExtras.Remove(stock.name);
                    foreach (var (item, count) in extras)
                        for (int i = 0; i < count; i++) supply.Put(item);
                }
            }
            return supply;
        }

        /// <summary>P128: every vendor shelf used in this run so far.</summary>
        public IReadOnlyDictionary<VendorStock, VendorSupply> Supplies => _supplies;

        private readonly Dictionary<string, List<(ItemData item, int count)>> _savedExtras = new Dictionary<string, List<(ItemData, int)>>();

        /// <summary>P128: items sold to a vendor (by its stock's asset name) before the save; they return when the shelf is first used.</summary>
        public void RestoreExtras(string stockName, List<(ItemData item, int count)> extras)
        {
            if (string.IsNullOrEmpty(stockName) || extras == null || extras.Count == 0) return;
            foreach (var pair in _supplies)
                if (pair.Key != null && pair.Key.name == stockName)
                {
                    foreach (var (item, count) in extras)
                        for (int i = 0; i < count; i++) pair.Value.Put(item);
                    return;
                }
            _savedExtras[stockName] = extras;
        }

        /// <summary>One line per family member for the morning letter, e.g. "Ilya is sick and needs medicine."</summary>
        public string FamilyReport()
        {
            if (household == null || Household == null) return "";
            var lines = new List<string>();
            for (int i = 0; i < Household.Count; i++)
            {
                if (Household.IsDead(i)) continue;
                int c = Mathf.Clamp(Household.Condition(i), 0, household.letterLines.Length - 1);
                lines.Add(string.Format(Household.WillDieNextNight(i) ? household.dyingLine : household.letterLines[c], household.members[i].name));
            }
            return string.Join("\n", lines);
        }

        /// <summary>P69: the letter that replaces the day's letter once someone has died; null while everybody lives.</summary>
        public string LossLetter()
        {
            if (household == null || Household == null || Household.Alive == Household.Count) return null;
            if (Household.Alive == 1) return household.lastSurvivorLetter;
            for (int i = 0; i < Household.Count; i++)
            {
                if (!Household.IsDead(i)) continue;
                string name = household.members[i].name;
                var letter = household.lossLetters.Find(l => l.name == name);
                if (letter != null) return Household.DiedTonight.Contains(i) ? letter.firstMorning : letter.afterwards;
            }
            return household.lastSurvivorLetter;
        }

        /// <summary>P69: everybody is dead, or the one who writes home is.</summary>
        public bool FamilyBroken
        {
            get
            {
                if (household == null || Household == null) return false;
                if (Household.Alive == 0) return true;
                for (int i = 0; i < Household.Count; i++)
                    if (Household.IsDead(i) && household.members[i].name == household.letterWriterName) return true;
                return false;
            }
        }

        /// <summary>P12: the text of the FAMILY LOST ending.</summary>
        public string FamilyLostBody()
        {
            var dead = new List<string>();
            for (int i = 0; i < Household.Count; i++) if (Household.IsDead(i)) dead.Add(household.members[i].name);
            return household.familyLostBody.Replace("{names}", TicketEnding.JoinNames(dead));
        }

        /// <summary>What went bad overnight (P68), e.g. "Spoiled overnight: Fresh fruit ×1." – empty if nothing.</summary>
        public string SpoiledReport { get; private set; } = "";

        /// <summary>P85: one more line under the next morning letter (something a character did).</summary>
        public void AddLetterLine(string line) => _letterLines.Add(line);

        private readonly List<string> _letterLines = new List<string>();

        /// <summary>P128: the lines waiting for the next morning letter (the save keeps them).</summary>
        public IReadOnlyList<string> LetterLines => _letterLines;

        /// <summary>Under the morning letter: what characters did, the family's lines, the stock at home (P131), then what spoiled.</summary>
        public string MorningReport()
        {
            var parts = new List<string>();
            if (_letterLines.Count > 0) parts.Add(string.Join("\n", _letterLines));
            _letterLines.Clear();
            string family = FamilyReport();
            if (!string.IsNullOrEmpty(family)) parts.Add(family);
            if (Delivery.Items.Count > 0) parts.Add("Still at home: " + Delivery.Summary() + ".");
            if (!string.IsNullOrEmpty(SpoiledReport)) parts.Add(SpoiledReport);
            return string.Join("\n\n", parts);
        }

        /// <summary>P68: perishable items in the inventory go bad overnight (what went home by drone before the night still counted).</summary>
        private void Spoil()
        {
            SpoiledReport = "";
            var tag = tuning != null ? tuning.perishableTag : null;
            if (tag == null) return;
            var spoiled = new List<string>();
            foreach (var item in new List<ItemData>(Inventory.Items))
            {
                if (!item.HasTag(tag)) continue;
                int n = Inventory.Count(item);
                Inventory.Remove(item, n);
                spoiled.Add($"{item.DisplayName} ×{n}");
            }
            if (spoiled.Count > 0) SpoiledReport = "Spoiled overnight: " + string.Join(", ", spoiled) + ".";
        }

        private void Awake()
        {
            Instance = this;
            Wallet = new Wallet(tuning != null ? tuning.startCredits : 0);
            Inventory = new Inventory();
            ShiftItems = new Inventory();
            Market.Begin(1, null); // a new run starts without yesterday's events (Market is static)
            var cold = new List<bool>();
            if (household != null) foreach (var m in household.members) cold.Add(m.feelsCold);
            Household = new Household(cold, household != null ? household.criticalNightsToDeath : 0);
            if (dayManager == null || dayManager.DevTools)
                foreach (var item in devStartItems) Inventory.Add(item);
        }

        private void OnEnable()
        {
            if (dayManager != null) dayManager.DayStarted += OnDayStarted;
            if (shift != null) shift.ShiftEnded += CloseShift;
        }

        private void OnDisable()
        {
            if (dayManager != null) dayManager.DayStarted -= OnDayStarted;
            if (shift != null) shift.ShiftEnded -= CloseShift;
        }

        /// <summary>P81: end of shift – listeners (P08) handle the confiscated items, then the shift inventory is emptied.</summary>
        public void CloseShift()
        {
            ShiftClosing?.Invoke(ShiftItems);
            ShiftItems.Clear();
        }

        /// <summary>P81 dev button: one of each dev start item into the shift inventory.</summary>
        public void DevFillShift()
        {
            foreach (var item in devStartItems) ShiftItems.Add(item);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void OnDayStarted(int day)
        {
            Wallet.BeginDay();
            Notes.Clear();
            BribeTonight = 0;
            // Market.Begin(day, events) already ran in DayManager.BeginDay (before the pinboard notes).
            if (day > 1) Spoil();
            foreach (var supply in _supplies.Values) supply.Refill();
        }

        public string Format(int amount) => tuning != null ? tuning.Format(amount) : amount.ToString();
    }
}
