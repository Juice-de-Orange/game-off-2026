using System.Collections.Generic;
using System.Text.RegularExpressions;
using GameOff.Travelers;

namespace GameOff.Economy
{
    /// <summary>
    /// P08: turns cargo manifest lines ("6x CHEM STIMULANT VIALS") into items by their cargo word (P82, whole word).
    /// A line without an amount counts as 1; a line naming no known item is ignored (crates, water filters...).
    /// </summary>
    public static class CargoReader
    {
        private static readonly Regex Amount = new Regex(@"^\s*(\d+)\s*[xX×]\s*(.*)$");

        public static List<(ItemData item, int count)> Read(string cargo, IReadOnlyList<ItemData> goods, int maxPerItem)
        {
            var result = new List<(ItemData, int)>();
            if (string.IsNullOrWhiteSpace(cargo) || goods == null) return result;
            var totals = new Dictionary<ItemData, int>();
            var order = new List<ItemData>();
            foreach (var raw in cargo.Split('\n'))
            {
                string line = raw.Trim();
                if (line.Length == 0) continue;
                int count = 1;
                var m = Amount.Match(line);
                if (m.Success)
                {
                    count = int.Parse(m.Groups[1].Value);
                    line = m.Groups[2].Value;
                }
                foreach (var item in goods)
                {
                    if (item == null || string.IsNullOrEmpty(item.CargoWord) || !TextMatch.ContainsWords(line, item.CargoWord)) continue;
                    if (!totals.ContainsKey(item)) { totals[item] = 0; order.Add(item); }
                    totals[item] += count;
                    break; // one item per line
                }
            }
            foreach (var item in order)
            {
                int n = maxPerItem > 0 ? System.Math.Min(totals[item], maxPerItem) : totals[item];
                if (n > 0) result.Add((item, n));
            }
            return result;
        }
    }
}
