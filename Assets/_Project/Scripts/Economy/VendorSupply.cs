using System.Collections.Generic;

namespace GameOff.Economy
{
    /// <summary>
    /// What one vendor has on the shelf right now (P11, bug B04). Every morning each stock item is set to its daily
    /// amount; buying takes one, selling to the vendor puts one there (also items the vendor doesn't normally sell –
    /// those disappear from the list when bought back).
    /// P68: the morning amount follows the day's market (events can halve it or take it away) and events can add items for the day.
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
                    if (e != null && e.item != null && Market.Day >= e.fromDay) yield return e.item;
                foreach (var item in _extras) yield return item;
            }
        }

        /// <summary>P128: what was sold to the vendor that it doesn't normally sell (stays on the shelf over night).</summary>
        public IReadOnlyList<ItemData> Extras => _extras;

        public int Count(ItemData item) => item != null && _counts.TryGetValue(item, out int n) ? n : 0;

        public void Refill()
        {
            foreach (var e in _stock.Entries)
            {
                if (e == null || e.item == null) continue;
                if (Market.Day < e.fromDay) { _counts[e.item] = 0; continue; }
                // Every morning the shelf holds exactly today's amount (P68): a shortage shows even if yesterday's stock was left.
                _counts[e.item] = Market.Amount(e.item, e.dailyAmount);
            }
            foreach (var change in Market.StockChanges(_stock))
            {
                if (change.amount == 0) { _counts[change.item] = 0; continue; }
                // Something the vendor sells anyway comes on top of today's amount; anything else is on the shelf for the day.
                if (_stock.Sells(change.item)) { _counts[change.item] = Count(change.item) + change.amount; continue; }
                if (!_extras.Contains(change.item)) _extras.Add(change.item);
                if (Count(change.item) < change.amount) _counts[change.item] = change.amount;
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
