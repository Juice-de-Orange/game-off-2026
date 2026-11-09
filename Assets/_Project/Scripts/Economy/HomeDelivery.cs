using System.Collections.Generic;

namespace GameOff.Economy
{
    /// <summary>
    /// What went home with the cargo drone since the last settlement (P13). An item with a need's tag covers that need
    /// tonight, so the bed doesn't ask for money for it. Cleared after every settlement.
    /// P110: what the player sends (Add) waits as one shipment for customs (TakeUninspected); gifts from characters (AddGift)
    /// never pass customs. Items customs keeps are taken out again (Remove) and no longer cover a need.
    /// </summary>
    public class HomeDelivery
    {
        private readonly List<ItemData> _items = new List<ItemData>();
        private readonly List<ItemData> _uninspected = new List<ItemData>();

        public IReadOnlyList<ItemData> Items => _items;

        /// <summary>Sent by the player at the drone terminal; customs may inspect it.</summary>
        public void Add(ItemData item)
        {
            if (item == null) return;
            _items.Add(item);
            _uninspected.Add(item);
        }

        /// <summary>P85: a character's gift arrives at home directly; customs never sees it.</summary>
        public void AddGift(ItemData item)
        {
            if (item != null) _items.Add(item);
        }

        /// <summary>P110: the player's items sent since the last inspection – one shipment. Each item is handed out only once.</summary>
        public List<ItemData> TakeUninspected()
        {
            var shipment = new List<ItemData>(_uninspected);
            _uninspected.Clear();
            return shipment;
        }

        /// <summary>P110: customs kept one of these; it doesn't arrive.</summary>
        public bool Remove(ItemData item) => _items.Remove(item);

        /// <summary>The first delivered item with this tag, or null.</summary>
        public ItemData CoveringItem(ItemTag tag)
        {
            if (tag == null) return null;
            foreach (var item in _items)
                if (item.HasTag(tag)) return item;
            return null;
        }

        public bool Covers(ItemTag tag) => CoveringItem(tag) != null;

        public void Clear()
        {
            _items.Clear();
            _uninspected.Clear();
        }
    }
}
