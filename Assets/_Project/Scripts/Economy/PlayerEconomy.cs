using System.Collections.Generic;
using GameOff.Day;
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

        public static PlayerEconomy Instance { get; private set; }

        public Wallet Wallet { get; private set; }
        public Inventory Inventory { get; private set; }
        public EconomyTuning Tuning => tuning;
        public HouseholdTuning HouseholdTuning => household;
        public Household Household { get; private set; }

        private readonly Dictionary<VendorStock, VendorSupply> _supplies = new Dictionary<VendorStock, VendorSupply>();

        /// <summary>The vendor's shelf right now (created full on first use).</summary>
        public VendorSupply Supply(VendorStock stock)
        {
            if (!_supplies.TryGetValue(stock, out var supply)) _supplies[stock] = supply = new VendorSupply(stock);
            return supply;
        }

        /// <summary>One line per family member for the morning letter, e.g. "Ilya is sick and needs medicine."</summary>
        public string FamilyReport()
        {
            if (household == null || Household == null) return "";
            var lines = new List<string>();
            for (int i = 0; i < Household.Count; i++)
            {
                int c = Mathf.Clamp(Household.Condition(i), 0, household.letterLines.Length - 1);
                lines.Add(string.Format(household.letterLines[c], household.members[i].name));
            }
            return string.Join("\n", lines);
        }

        private void Awake()
        {
            Instance = this;
            Wallet = new Wallet(tuning != null ? tuning.startCredits : 0);
            Inventory = new Inventory();
            var cold = new List<bool>();
            if (household != null) foreach (var m in household.members) cold.Add(m.feelsCold);
            Household = new Household(cold);
            if (dayManager == null || dayManager.DevTools)
                foreach (var item in devStartItems) Inventory.Add(item);
        }

        private void OnEnable()
        {
            if (dayManager != null) dayManager.DayStarted += OnDayStarted;
        }

        private void OnDisable()
        {
            if (dayManager != null) dayManager.DayStarted -= OnDayStarted;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void OnDayStarted(int day)
        {
            Wallet.BeginDay();
            if (day > 1) foreach (var supply in _supplies.Values) supply.Refill();
        }

        public string Format(int amount) => tuning != null ? tuning.Format(amount) : amount.ToString();
    }
}
