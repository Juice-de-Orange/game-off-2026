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
    /// </summary>
    public class PlayerEconomy : MonoBehaviour
    {
        [SerializeField] private EconomyTuning tuning;
        [Tooltip("Optional: clears TODAY each morning.")]
        [SerializeField] private DayManager dayManager;
        [Tooltip("Given at the start while the dev tools are on (to test selling and sending home).")]
        [SerializeField] private List<ItemData> devStartItems = new List<ItemData>();

        public static PlayerEconomy Instance { get; private set; }

        public Wallet Wallet { get; private set; }
        public Inventory Inventory { get; private set; }
        public EconomyTuning Tuning => tuning;

        private void Awake()
        {
            Instance = this;
            Wallet = new Wallet(tuning != null ? tuning.startCredits : 0);
            Inventory = new Inventory();
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

        private void OnDayStarted(int day) => Wallet.BeginDay();

        public string Format(int amount) => tuning != null ? tuning.Format(amount) : amount.ToString();
    }
}
