using System.Collections.Generic;

namespace GameOff.Economy
{
    /// <summary>
    /// What one vendor has on the shelf right now (P11, bug B04). Every morning each stock item is set to its daily
    /// amount; buying takes one, selling to the vendor puts one there (also items the vendor doesn't normally sell –
    /// those disappear from the list when bought back).
    /// P68: the morning amount follows the day's market (events can halve it or take it away) and events can add items for the day.
    /// Those event items are kept apart from what the player sold: they leave at the next refill and are never saved (P128).
    /// </summary>
    public class VendorSupply
    {
        private readonly VendorStock _stock;
        private readonly Dictionary<ItemData, int> _counts = new Dictionary<ItemData, int>();
        /// <summary>Sold to the vendor by the player and not normally sold here: stays over night.</summary>
        private readonly List<ItemData> _extras = new List<ItemData>();
        /// <summary>P68: added by today's market events and not normally sold here, with how many of those units are still on the shelf.</summary>
        private readonly List<ItemData> _eventItems = new List<ItemData>();
        private readonly Dictionary<ItemData, int> _eventLeft = new Dictionary<ItemData, int>();

        public VendorSupply(VendorStock stock)
        {
            _stock = stock;
            Refill();
        }

        /// <summary>Stock items in their order (also when sold out), then today's event items, then items sold to the vendor.</summary>
        public IEnumerable<ItemData> Items
        {
            get
            {
                foreach (var e in _stock.Entries)
                    if (e != null && e.item != null && Market.Day >= e.fromDay) yield return e.item;
                foreach (var item in _eventItems) yield return item;
                foreach (var item in _extras)
                    if (!_eventItems.Contains(item)) yield return item;
            }
        }

        /// <summary>P128: what was sold to the vendor that it doesn't normally sell (stays on the shelf over night); see SoldCount.</summary>
        public IReadOnlyList<ItemData> Extras => _extras;

        public int Count(ItemData item) => item != null && _counts.TryGetValue(item, out int n) ? n : 0;

        /// <summary>P128: how many of an extra item the player sold here: the shelf without today's event units of it.</summary>
        public int SoldCount(ItemData item) => _extras.Contains(item) ? System.Math.Max(0, Count(item) - EventLeft(item)) : 0;

        private int EventLeft(ItemData item) => item != null && _eventLeft.TryGetValue(item, out int n) ? n : 0;

        public void Refill()
        {
            // Yesterday's event items leave the shelf; what the player sold of them stays like any other extra.
            foreach (var item in _eventItems)
            {
                int sold = SoldCount(item);
                if (sold > 0) _counts[item] = sold;
                else
                {
                    _counts.Remove(item);
                    _extras.Remove(item);
                }
            }
            _eventItems.Clear();
            _eventLeft.Clear();

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
                if (!_eventItems.Contains(change.item)) _eventItems.Add(change.item);
                int added = change.amount - Count(change.item); // at least this many on the shelf today
                if (added <= 0) continue;
                _counts[change.item] = change.amount;
                _eventLeft[change.item] = EventLeft(change.item) + added;
            }
        }

        public bool Take(ItemData item)
        {
            if (Count(item) == 0) return false;
            _counts[item]--;
            int fromEvent = EventLeft(item);
            if (fromEvent > 0) _eventLeft[item] = fromEvent - 1; // today's event units go first, so what the player sold stays theirs
            if (_counts[item] == 0 && !_stock.Sells(item))
            {
                _counts.Remove(item);
                _extras.Remove(item);
                _eventItems.Remove(item);
                _eventLeft.Remove(item);
            }
            return true;
        }

        public void Put(ItemData item)
        {
            if (item == null) return;
            if (!_stock.Sells(item) && !_extras.Contains(item)) _extras.Add(item);
            _counts[item] = Count(item) + 1;
        }
    }
}
