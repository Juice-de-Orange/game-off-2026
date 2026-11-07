using System.Collections.Generic;
using UnityEngine;

namespace GameOff.Economy
{
    /// <summary>
    /// What a vendor sells and at which factors (P46). Price for the player = item base value × sellFactor (rounded);
    /// what the vendor pays = base value × buyFactor. Items with a refused tag can't be sold here (e.g. the kiosk refuses CONTRABAND).
    /// Each entry has a daily amount (P11): the shelf is topped up every morning, see VendorSupply.
    /// </summary>
    [CreateAssetMenu(menuName = "GameOff/Economy/Vendor Stock", fileName = "Stock_")]
    public class VendorStock : ScriptableObject
    {
        [SerializeField] private string title = "VENDOR";
        [Tooltip("What is for sale and how many of it every morning (P11).")]
        [SerializeField] private List<StockEntry> entries = new List<StockEntry>();
        [Tooltip("Before P11: items without amounts. EconomyBuilder moves them into Entries.")]
        [SerializeField, HideInInspector] private List<ItemData> items = new List<ItemData>();
        [SerializeField] private float sellFactor = 1f;
        [SerializeField] private float buyFactor = 0.5f;
        [Tooltip("The vendor doesn't buy items with these tags.")]
        [SerializeField] private List<ItemTag> refusedTags = new List<ItemTag>();
        [Tooltip("Empty = buys everything not refused. Otherwise only items with at least one of these tags (P64: black market).")]
        [SerializeField] private List<ItemTag> onlyTags = new List<ItemTag>();
        [Tooltip("Open only after the shift (P09 evening). Off = always open.")]
        [SerializeField] private bool openAfterShiftOnly = true;
        [Tooltip("Shown when the stock is empty, e.g. \"Black market comes later.\"")]
        [SerializeField] private string emptyText = "Nothing for sale.";

        public string Title => title;
        public IReadOnlyList<StockEntry> Entries => entries;
        /// <summary>Stock items without amounts (for lists and checks).</summary>
        public IEnumerable<ItemData> Items
        {
            get { foreach (var e in entries) if (e != null) yield return e.item; }
        }
        public bool Sells(ItemData item) => item != null && entries.Exists(e => e != null && e.item == item);
        public bool OpenAfterShiftOnly => openAfterShiftOnly;
        public string EmptyText => emptyText;

        // From today's market price (P70: base × fixed daily change; P68 adds labels and events).
        public int PriceFor(ItemData item) => Market.Rounded(Market.PriceOf(item) * sellFactor);
        public int OfferFor(ItemData item) => Market.Rounded(Market.PriceOf(item) * buyFactor * Market.BuyFactor(item));

        public bool Buys(ItemData item)
        {
            if (buyFactor <= 0f) return false;
            foreach (var tag in refusedTags)
                if (item.HasTag(tag)) return false;
            if (onlyTags.Count == 0) return true;
            foreach (var tag in onlyTags)
                if (item.HasTag(tag)) return true;
            return false;
        }
    }
}
