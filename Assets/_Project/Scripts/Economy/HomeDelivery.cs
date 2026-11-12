using System.Collections.Generic;

namespace GameOff.Economy
{
    /// <summary>
    /// The family's stock at home (P13, P131): food, power and medicine that went home with the cargo drone and were not used up yet.
    /// Every night each need that is due takes one item with that need's tag (Covering → Night); what is left stays for the
    /// next nights, so three battery packs keep the heater on for three nights. Perishable items left after the night go bad,
    /// items that cover no need just arrive and leave the stock. One item covers only one need.
    /// P110: what the player sends (Add) waits as one shipment for customs (TakeUninspected); gifts from characters (AddGift)
    /// never pass customs. Items customs keeps are taken out again (Remove) and no longer cover a need.
    /// </summary>
    public class HomeDelivery
    {
        private readonly List<ItemData> _items = new List<ItemData>();
        private readonly List<ItemData> _uninspected = new List<ItemData>();

        /// <summary>The stock at home, oldest first.</summary>
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

        /// <summary>The first item at home with this tag, or null.</summary>
        public ItemData CoveringItem(ItemTag tag)
        {
            if (tag == null) return null;
            foreach (var item in _items)
                if (item.HasTag(tag)) return item;
            return null;
        }

        public bool Covers(ItemTag tag) => CoveringItem(tag) != null;

        /// <summary>
        /// P131: tonight's items, one per need: <paramref name="needs"/> holds the tag of every need in order, null for a need
        /// that isn't due (medicine while nobody is sick). Returns the item that covers each need, or null. Every item covers only
        /// one need, and as many needs as possible are covered; among the fitting items perishable ones go first (they would
        /// spoil anyway), then the ones that fit fewer of tonight's needs, then the oldest.
        /// </summary>
        public ItemData[] Covering(IReadOnlyList<ItemTag> needs, ItemTag perishable = null)
        {
            var best = new ItemData[needs != null ? needs.Count : 0];
            if (best.Length == 0) return best;
            var candidates = new List<ItemData>[best.Length];
            for (int n = 0; n < best.Length; n++)
            {
                candidates[n] = new List<ItemData>();
                var tag = needs[n];
                if (tag == null) continue;
                foreach (var item in _items)
                    if (item != null && item.HasTag(tag) && !candidates[n].Contains(item)) candidates[n].Add(item);
                // List.Sort isn't stable: the stock position breaks ties, so the oldest goes first.
                var list = candidates[n];
                list.Sort((a, b) =>
                {
                    int byPerishable = Perishable(b, perishable).CompareTo(Perishable(a, perishable));
                    if (byPerishable != 0) return byPerishable;
                    int byFit = Fits(a, needs).CompareTo(Fits(b, needs));
                    return byFit != 0 ? byFit : _items.IndexOf(a).CompareTo(_items.IndexOf(b));
                });
            }
            var used = new Dictionary<ItemData, int>();
            var current = new ItemData[best.Length];
            int bestCount = -1;
            Assign(0, 0, candidates, used, current, best, ref bestCount);
            return best;
        }

        /// <summary>Tries every need in order with every fitting item still in stock; keeps the first assignment that covers the most.</summary>
        private void Assign(int need, int count, List<ItemData>[] candidates, Dictionary<ItemData, int> used, ItemData[] current,
            ItemData[] best, ref int bestCount)
        {
            if (need == candidates.Length)
            {
                if (count > bestCount)
                {
                    bestCount = count;
                    System.Array.Copy(current, best, best.Length);
                }
                return;
            }
            foreach (var item in candidates[need])
            {
                used.TryGetValue(item, out int n);
                if (n >= Count(item)) continue;
                used[item] = n + 1;
                current[need] = item;
                Assign(need + 1, count + 1, candidates, used, current, best, ref bestCount);
                used[item] = n;
            }
            current[need] = null;
            Assign(need + 1, count, candidates, used, current, best, ref bestCount);
        }

        private static bool Perishable(ItemData item, ItemTag perishable) => perishable != null && item.HasTag(perishable);

        private static int Fits(ItemData item, IReadOnlyList<ItemTag> needs)
        {
            int n = 0;
            foreach (var tag in needs) if (tag != null && item.HasTag(tag)) n++;
            return n;
        }

        /// <summary>How many of this item are at home.</summary>
        public int Count(ItemData item)
        {
            int n = 0;
            foreach (var i in _items) if (i == item) n++;
            return n;
        }

        /// <summary>
        /// P131: the night used these items (one each, nulls are skipped); the rest stays at home. The drone has left by then:
        /// nothing is waiting for customs any more.
        /// </summary>
        public void UseUp(IEnumerable<ItemData> used)
        {
            if (used != null)
                foreach (var item in used)
                    if (item != null) _items.Remove(item);
            _uninspected.Clear();
        }

        /// <summary>P131: perishable items left at home go bad overnight (like the inventory, P68). Returns what spoiled.</summary>
        public List<ItemData> Spoil(ItemTag perishable)
        {
            var spoiled = new List<ItemData>();
            if (perishable == null) return spoiled;
            for (int i = _items.Count - 1; i >= 0; i--)
            {
                if (_items[i] == null || !_items[i].HasTag(perishable)) continue;
                spoiled.Insert(0, _items[i]);
                _items.RemoveAt(i);
            }
            return spoiled;
        }

        /// <summary>
        /// P131: the night at home, after the settlement. The covering items (from Covering) are used up, one each; perishable
        /// leftovers go bad; items that carry none of the needs' tags (<paramref name="needTags"/>, all needs, due or not) just
        /// arrived and leave the stock; the rest stays for the next nights. Returns what spoiled.
        /// </summary>
        public List<ItemData> Night(IEnumerable<ItemData> used, IReadOnlyList<ItemTag> needTags, ItemTag perishable)
        {
            UseUp(used);
            var spoiled = Spoil(perishable);
            _items.RemoveAll(item => item == null || needTags == null || Fits(item, needTags) == 0);
            return spoiled;
        }

        /// <summary>P131: a copy of the stock, e.g. to preview the night in the settlement.</summary>
        public HomeDelivery Copy()
        {
            var copy = new HomeDelivery();
            copy._items.AddRange(_items);
            copy._uninspected.AddRange(_uninspected);
            return copy;
        }

        /// <summary>P128: a saved run continues with this stock (nothing waits for customs).</summary>
        public void Restore(IEnumerable<ItemData> items)
        {
            Clear();
            if (items != null)
                foreach (var item in items)
                    if (item != null) _items.Add(item);
        }

        public void Clear()
        {
            _items.Clear();
            _uninspected.Clear();
        }

        /// <summary>P131: "Battery pack ×3, Ration pack ×1" – each item once, in stock order; "" if there is nothing.</summary>
        public static string Summary(IEnumerable<ItemData> items)
        {
            var order = new List<ItemData>();
            var counts = new Dictionary<ItemData, int>();
            if (items != null)
                foreach (var item in items)
                {
                    if (item == null) continue;
                    if (!counts.ContainsKey(item)) { counts[item] = 0; order.Add(item); }
                    counts[item]++;
                }
            var names = new List<string>();
            foreach (var item in order) names.Add($"{item.DisplayName} ×{counts[item]}");
            return string.Join(", ", names);
        }

        /// <summary>The stock as one line, see Summary.</summary>
        public string Summary() => Summary(_items);
    }
}
