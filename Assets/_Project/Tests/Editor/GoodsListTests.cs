using System.Collections.Generic;
using System.Linq;
using GameOff.Economy;
using GameOff.Travelers;
using NUnit.Framework;
using UnityEditor;

namespace GameOff.Tests
{
    /// <summary>
    /// P82 goods list 2: every item looks finished (icon), can be sold somewhere, has a source (a vendor or travelers' cargo),
    /// and everything travelers carry has a CONFISCATED variant that only differs by that label. Cargo words match the rules.
    /// </summary>
    public class GoodsListTests
    {
        private const string ConfiscatedFolder = "Assets/_Project/Data/Items/Confiscated";

        private static List<T> All<T>(string filter, string folder = "Assets/_Project/Data") where T : UnityEngine.Object =>
            AssetDatabase.FindAssets(filter, new[] { folder })
                .Select(g => AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(g))).Where(o => o != null).ToList();

        private static List<ItemData> Goods() =>
            All<ItemData>("t:ItemData", "Assets/_Project/Data/Items").Where(i => !AssetDatabase.GetAssetPath(i).StartsWith(ConfiscatedFolder)).ToList();

        private static ItemTag Confiscated() => All<ItemTag>("t:ItemTag").Single(t => t.name == "Tag_CONFISCATED");

        [Test]
        public void EveryItem_HasAnIcon()
        {
            foreach (var item in All<ItemData>("t:ItemData", "Assets/_Project/Data/Items"))
                Assert.NotNull(item.Icon, $"{item.name}: no icon (order one in ASSET-REQUESTS.md and assign the placeholder)");
        }

        [Test]
        public void EveryItem_CanBeSoldSomewhere_AndComesFromSomewhere()
        {
            var stocks = All<VendorStock>("t:VendorStock");
            foreach (var item in Goods())
            {
                if (item.Unique) continue; // gadgets come with their packages (scanner P16, UV lamp P62)
                Assert.IsTrue(stocks.Any(s => s.Buys(item)), $"{item.name}: nobody buys it");
                bool source = stocks.Any(s => s.Sells(item)) || !string.IsNullOrEmpty(item.CargoWord);
                Assert.IsTrue(source, $"{item.name}: no vendor sells it and no traveler carries it (no cargo word)");
            }
        }

        [Test]
        public void CargoItems_HaveAConfiscatedVariant_ThatOnlyAddsTheLabel()
        {
            var confiscated = Confiscated();
            foreach (var item in Goods().Where(i => !string.IsNullOrEmpty(i.CargoWord)))
            {
                var v = item.ConfiscatedVariant;
                Assert.NotNull(v, $"{item.name}: carried by travelers but no CONFISCATED variant - run GameOff/Update Economy Data");
                Assert.IsTrue(v.HasTag(confiscated), $"{v.name}: missing CONFISCATED");
                Assert.IsTrue(item.Tags.All(v.HasTag), $"{v.name}: lost a label of {item.name} (confiscated medicine must still help the family)");
                Assert.AreEqual(item.Tags.Count + (item.HasTag(confiscated) ? 0 : 1), v.Tags.Count, $"{v.name}: extra labels");
                Assert.AreEqual(item.BaseValue, v.BaseValue, $"{v.name}: price differs from {item.name}");
                Assert.AreSame(item.Icon, v.Icon, $"{v.name}: icon differs");
                Assert.IsTrue(string.IsNullOrEmpty(v.CargoWord), $"{v.name}: variants are never cargo");
                Assert.IsFalse(item.HasTag(confiscated), $"{item.name}: the original must not be CONFISCATED");
            }
        }

        [Test]
        public void CargoWords_AreUnique_AndEveryForbiddenGoodsRuleNamesAnItem()
        {
            var goods = Goods().Where(i => !string.IsNullOrEmpty(i.CargoWord)).ToList();
            var dupes = goods.GroupBy(i => i.CargoWord).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
            Assert.IsEmpty(dupes, "the same cargo word means two items");
            foreach (var item in goods)
                Assert.AreEqual(item.CargoWord.ToUpperInvariant(), item.CargoWord, $"{item.name}: cargo words are upper case");

            var words = new HashSet<string>(goods.Select(i => i.CargoWord));
            foreach (var day in All<DayRules>("t:DayRules"))
                foreach (var rule in day.Rules.OfType<ForbiddenGoodsRule>())
                    Assert.IsTrue(words.Contains(rule.keyword),
                        $"{day.name}: forbidden \"{rule.keyword}\" is no item's cargo word - P08 couldn't confiscate it (see ItemData.cargoWord)");
        }
    }
}
