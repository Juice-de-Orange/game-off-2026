using System.Collections.Generic;

namespace GameOff.Economy
{
    /// <summary>
    /// What went home with the cargo drone since the last settlement (P13). An item with a need's tag covers that need
    /// tonight, so the bed doesn't ask for money for it. Cleared after every settlement.
    /// </summary>
    public class HomeDelivery
    {
        private readonly List<ItemData> _items = new List<ItemData>();

        public IReadOnlyList<ItemData> Items => _items;

        public void Add(ItemData item)
        {
            if (item != null) _items.Add(item);
        }

        /// <summary>The first delivered item with this tag, or null.</summary>
        public ItemData CoveringItem(ItemTag tag)
        {
            if (tag == null) return null;
            foreach (var item in _items)
                if (item.HasTag(tag)) return item;
            return null;
        }

        public bool Covers(ItemTag tag) => CoveringItem(tag) != null;

        public void Clear() => _items.Clear();
    }
}
