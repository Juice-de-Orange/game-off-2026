using System.Collections.Generic;

namespace GameOff.Economy
{
    /// <summary>
    /// What one vendor has on the shelf right now (P11, bug B04). Every morning each stock item is topped up to its daily
    /// amount; buying takes one, selling to the vendor puts one there (also items the vendor doesn't normally sell –
    /// those disappear from the list when bought back).
    /// </summary>
    public class VendorSupply
    {
        private readonly VendorStock _stock;
        private readonly Dictionary<ItemData, int> _counts = new Dictionary<ItemData, int>();
        private readonly List<ItemData> _extras = new List<ItemData>();

        public VendorSupply(VendorStock stock)
        {
            _stock = stock;
            Refill();
        }

        /// <summary>Stock items in their order (also when sold out), then items sold to the vendor.</summary>
        public IEnumerable<ItemData> Items
        {
            get
            {
                foreach (var e in _stock.Entries)
                    if (e != null && e.item != null) yield return e.item;
                foreach (var item in _extras) yield return item;
            }
        }

        public int Count(ItemData item) => item != null && _counts.TryGetValue(item, out int n) ? n : 0;

        public void Refill()
        {
            foreach (var e in _stock.Entries)
            {
                if (e == null || e.item == null) continue;
                if (Count(e.item) < e.dailyAmount) _counts[e.item] = e.dailyAmount;
            }
        }

        public bool Take(ItemData item)
        {
            if (Count(item) == 0) return false;
            _counts[item]--;
            if (_counts[item] == 0 && _extras.Remove(item)) _counts.Remove(item);
            return true;
        }

        public void Put(ItemData item)
        {
            if (item == null) return;
            if (!_counts.ContainsKey(item))
            {
                _counts[item] = 0;
                if (!_stock.Sells(item)) _extras.Add(item);
            }
            _counts[item]++;
        }
    }
}
