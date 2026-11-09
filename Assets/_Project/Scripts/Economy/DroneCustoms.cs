using System.Collections.Generic;

namespace GameOff.Economy
{
    /// <summary>
    /// P110 rules for smuggling by drone, without Unity (EditMode-tested). A shipment (what the player sent since the last
    /// inspection) with at least one CONTRABAND or CONFISCATED item is inspected with droneInspectChance: then only those
    /// items are lost, the rest arrives, and the officer gets one disciplinary penalty. No fine. Gifts never pass customs.
    /// roll is a random number in [0, 1); "caught" means roll &lt; chance.
    /// </summary>
    public static class DroneCustoms
    {
        public static bool IsContraband(ItemData item, ConfiscationTuning tuning) =>
            item != null && tuning != null && (item.HasTag(tuning.contrabandTag) || item.HasTag(tuning.confiscatedTag));

        public static DroneCustomsResult Resolve(IReadOnlyList<ItemData> shipment, ConfiscationTuning tuning, float roll)
        {
            var result = new DroneCustomsResult();
            if (shipment == null || tuning == null) return result;
            foreach (var item in shipment)
                if (IsContraband(item, tuning)) result.Lost.Add(item);
            result.Caught = result.Lost.Count > 0 && roll < tuning.droneInspectChance;
            if (!result.Caught) result.Lost.Clear();
            return result;
        }

        /// <summary>
        /// Inspects what was sent since the last inspection (once per item, so reopening the settlement doesn't roll again),
        /// takes the kept items out of the delivery and adds the disciplinary penalty.
        /// </summary>
        public static DroneCustomsResult Inspect(HomeDelivery delivery, Conduct conduct, ConfiscationTuning tuning, float roll)
        {
            if (delivery == null) return new DroneCustomsResult();
            var result = Resolve(delivery.TakeUninspected(), tuning, roll);
            if (!result.Caught) return result;
            foreach (var item in result.Lost) delivery.Remove(item);
            conduct?.AddDisciplinary(1);
            return result;
        }

        /// <summary>The loading hint for this item, or "" if customs doesn't care about it.</summary>
        public static string LoadingHint(ItemData item, ConfiscationTuning tuning) =>
            IsContraband(item, tuning) ? tuning.droneLoadingHint : "";

        /// <summary>"spice", "spice and data chip", "spice ×2, data chip and pistol" – lower case, for the middle of a sentence.</summary>
        public static string ItemList(IReadOnlyList<ItemData> items)
        {
            var order = new List<ItemData>();
            var counts = new Dictionary<ItemData, int>();
            foreach (var item in items)
            {
                if (item == null) continue;
                if (!counts.ContainsKey(item)) { counts[item] = 0; order.Add(item); }
                counts[item]++;
            }
            var names = new List<string>();
            foreach (var item in order)
                names.Add(item.DisplayName.ToLowerInvariant() + (counts[item] > 1 ? $" ×{counts[item]}" : ""));
            if (names.Count <= 1) return names.Count == 1 ? names[0] : "";
            return string.Join(", ", names.GetRange(0, names.Count - 1)) + " and " + names[names.Count - 1];
        }
    }
}
