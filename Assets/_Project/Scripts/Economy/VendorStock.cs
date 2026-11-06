using System.Collections.Generic;
using UnityEngine;

namespace GameOff.Economy
{
    /// <summary>
    /// What a vendor sells and at which factors (P46). Price for the player = item base value × sellFactor (rounded);
    /// what the vendor pays = base value × buyFactor. Items with a refused tag can't be sold here (e.g. the kiosk refuses CONTRABAND).
    /// </summary>
    [CreateAssetMenu(menuName = "GameOff/Economy/Vendor Stock", fileName = "Stock_")]
    public class VendorStock : ScriptableObject
    {
        [SerializeField] private string title = "VENDOR";
        [SerializeField] private List<ItemData> items = new List<ItemData>();
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
        public IReadOnlyList<ItemData> Items => items;
        public bool OpenAfterShiftOnly => openAfterShiftOnly;
        public string EmptyText => emptyText;

        public int PriceFor(ItemData item) => Mathf.Max(0, Mathf.RoundToInt(item.BaseValue * sellFactor));
        public int OfferFor(ItemData item) => Mathf.Max(0, Mathf.RoundToInt(item.BaseValue * buyFactor));

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
