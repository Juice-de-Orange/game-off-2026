using System;

namespace GameOff.Economy
{
    /// <summary>A market event changes one vendor's assortment for the day (P68): amount &gt; 0 = on the shelf, 0 = missing today.</summary>
    [Serializable]
    public class StockChange
    {
        public VendorStock stock;
        public ItemData item;
        [UnityEngine.Min(0)] public int amount;
    }
}
