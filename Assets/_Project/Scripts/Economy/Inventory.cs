using System;
using System.Collections.Generic;

namespace GameOff.Economy
{
    /// <summary>The player's items, stacked per item type in the order they were first added (P46). Unique items: at most one.</summary>
    public class Inventory
    {
        private readonly List<ItemData> _order = new List<ItemData>();
        private readonly Dictionary<ItemData, int> _counts = new Dictionary<ItemData, int>();

        public IReadOnlyList<ItemData> Items => _order;

        public event Action Changed;

        public int Count(ItemData item) => item != null && _counts.TryGetValue(item, out int n) ? n : 0;

        /// <summary>False only for a unique item you already have.</summary>
        public bool CanAdd(ItemData item) => item != null && !(item.Unique && Count(item) > 0);

        /// <summary>Adds items; a unique item is capped at one. Returns how many were actually added.</summary>
        public int Add(ItemData item, int count = 1)
        {
            if (item == null || count <= 0) return 0;
            if (item.Unique) count = CanAdd(item) ? 1 : 0;
            if (count == 0) return 0;
            if (!_counts.ContainsKey(item))
            {
                _counts[item] = 0;
                _order.Add(item);
            }
            _counts[item] += count;
            Changed?.Invoke();
            return count;
        }

        /// <summary>Empties the inventory (P81: the shift inventory at shift end).</summary>
        public void Clear()
        {
            if (_order.Count == 0) return;
            _order.Clear();
            _counts.Clear();
            Changed?.Invoke();
        }

        public bool Remove(ItemData item, int count = 1)
        {
            if (Count(item) < count || count <= 0) return false;
            _counts[item] -= count;
            if (_counts[item] == 0)
            {
                _counts.Remove(item);
                _order.Remove(item);
            }
            Changed?.Invoke();
            return true;
        }
    }
}
