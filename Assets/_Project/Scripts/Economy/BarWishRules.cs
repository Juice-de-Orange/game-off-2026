using System.Collections.Generic;
using System.Linq;

namespace GameOff.Economy
{
    /// <summary>
    /// The barkeeper's trade (P65) as plain rules: how many matching items you have, handing over the cheapest ones first, and the
    /// rumour – the rumour texts of tomorrow's market events, so it is always true ("Quiet times ahead." when nothing happens).
    /// </summary>
    public static class BarWishRules
    {
        public const string Quiet = "Quiet times ahead. Nothing worth whispering about.";

        public static int Matching(Inventory inventory, ItemTag tag) =>
            tag == null ? 0 : inventory.Items.Where(i => i.HasTag(tag)).Sum(inventory.Count);

        /// <summary>Takes the wished amount, cheapest market price first. False (and nothing taken) if there isn't enough.</summary>
        public static bool Give(Inventory inventory, BarWish wish)
        {
            if (wish == null || !wish.Active || Matching(inventory, wish.tag) < wish.amount) return false;
            int left = wish.amount;
            foreach (var item in inventory.Items.Where(i => i.HasTag(wish.tag)).OrderBy(Market.PriceOf).ToList())
            {
                int n = System.Math.Min(left, inventory.Count(item));
                inventory.Remove(item, n);
                left -= n;
                if (left == 0) break;
            }
            return true;
        }

        public static string Rumour(IEnumerable<MarketEvent> tomorrow)
        {
            var lines = (tomorrow ?? Enumerable.Empty<MarketEvent>())
                .Where(e => e != null && !string.IsNullOrWhiteSpace(e.rumour)).Select(e => e.rumour).ToList();
            return lines.Count == 0 ? Quiet : string.Join(" ", lines);
        }
    }
}
