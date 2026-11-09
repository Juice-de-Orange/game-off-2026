using System.Collections.Generic;
using GameOff.Economy;
using UnityEngine;

namespace GameOff.Shops
{
    /// <summary>P32: pure rules for what the evidence locker shows: goods → model, goods → compartments, model size → scale.</summary>
    public static class EvidenceLockerLayout
    {
        /// <summary>The item's own model, else the model of its first label found in tagModels (list order), else null (= evidence bag).</summary>
        public static GameObject ModelFor(ItemData item, IReadOnlyList<LockerTagModel> tagModels)
        {
            if (item == null) return null;
            if (item.Model != null) return item.Model;
            if (tagModels == null) return null;
            foreach (var entry in tagModels)
                if (entry.model != null && item.HasTag(entry.tag)) return entry.model;
            return null;
        }

        /// <summary>
        /// One compartment per item type, in the order they were handed in; each shows min(count, maxCopies) models (at least one).
        /// More item types than compartments: the rest isn't shown (it is still in the locker).
        /// </summary>
        public static List<LockerPlacement> Place(IReadOnlyList<(ItemData item, int count)> goods, int slots, int maxCopies)
        {
            var result = new List<LockerPlacement>();
            if (goods == null) return result;
            foreach (var (item, count) in goods)
            {
                if (result.Count >= slots) break;
                if (item == null || count <= 0) continue;
                result.Add(new LockerPlacement(result.Count, item, Mathf.Clamp(count, 1, Mathf.Max(1, maxCopies))));
            }
            return result;
        }

        /// <summary>Uniform scale that fits a model of this size into the box; never larger than maxUpscale. 1 for an empty size.</summary>
        public static float FitScale(Vector3 size, Vector3 box, float maxUpscale)
        {
            float scale = float.MaxValue;
            if (size.x > 1e-4f) scale = Mathf.Min(scale, box.x / size.x);
            if (size.y > 1e-4f) scale = Mathf.Min(scale, box.y / size.y);
            if (size.z > 1e-4f) scale = Mathf.Min(scale, box.z / size.z);
            if (scale == float.MaxValue) return 1f;
            return Mathf.Min(scale, Mathf.Max(maxUpscale, 0.01f));
        }

        /// <summary>The goods of an inventory in its order, with their counts.</summary>
        public static List<(ItemData item, int count)> Contents(Inventory inventory)
        {
            var list = new List<(ItemData, int)>();
            if (inventory == null) return list;
            foreach (var item in inventory.Items) list.Add((item, inventory.Count(item)));
            return list;
        }
    }
}
