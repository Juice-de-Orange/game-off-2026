using System.Reflection;
using GameOff.Economy;
using GameOff.UI;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace GameOff.Tests
{
    /// <summary>P81: the shift inventory is separate from your items, is handed to P08 at shift end and then emptied.</summary>
    public class ShiftInventoryTests
    {
        private GameObject _go;

        [TearDown]
        public void CleanUp()
        {
            if (_go != null) Object.DestroyImmediate(_go);
        }

        private PlayerEconomy MakeEconomy()
        {
            _go = new GameObject("Economy");
            var economy = _go.AddComponent<PlayerEconomy>();
            typeof(PlayerEconomy).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(economy, null);
            return economy;
        }

        [Test]
        public void Clear_EmptiesAndNotifiesOnlyWhenSomethingWasThere()
        {
            var item = ScriptableObject.CreateInstance<ItemData>();
            try
            {
                var inventory = new Inventory();
                int changes = 0;
                inventory.Changed += () => changes++;
                inventory.Clear();
                Assert.AreEqual(0, changes, "clearing an empty inventory is no change");
                inventory.Add(item, 3);
                inventory.Clear();
                Assert.AreEqual(0, inventory.Count(item));
                Assert.AreEqual(0, inventory.Items.Count);
                Assert.AreEqual(2, changes);
            }
            finally { Object.DestroyImmediate(item); }
        }

        [Test]
        public void CloseShift_HandsTheConfiscatedItemsOverFirst_ThenEmptiesOnlyTheShiftInventory()
        {
            var economy = MakeEconomy();
            var mine = ScriptableObject.CreateInstance<ItemData>();
            var seized = ScriptableObject.CreateInstance<ItemData>();
            try
            {
                Assert.AreNotSame(economy.Inventory, economy.ShiftItems);
                economy.Inventory.Add(mine);
                economy.ShiftItems.Add(seized, 2);
                int seen = -1;
                economy.ShiftClosing += shift => seen = shift.Count(seized);

                economy.CloseShift();

                Assert.AreEqual(2, seen, "P08 sees what was confiscated before it is cleared");
                Assert.AreEqual(0, economy.ShiftItems.Items.Count, "shift inventory is empty after the shift");
                Assert.AreEqual(1, economy.Inventory.Count(mine), "your own items stay");
            }
            finally
            {
                Object.DestroyImmediate(mine);
                Object.DestroyImmediate(seized);
            }
        }

        [Test]
        public void Desk_StaysBlocked_UntilTheLastWindowLetsGo()
        {
            _go = new GameObject("Counter");
            var station = _go.AddComponent<GameOff.Counter.CounterStation>();
            object inventory = new object(), question = new object(), stranger = new object();
            Assert.IsFalse(station.InputBlocked);
            station.SetInputBlocked(inventory, true);
            station.SetInputBlocked(question, true);
            station.SetInputBlocked(question, false); // the character's question is answered under the open shift inventory
            Assert.IsTrue(station.InputBlocked, "the shift inventory still holds the desk");
            station.SetInputBlocked(stranger, false);
            Assert.IsTrue(station.InputBlocked, "letting go of what one never held changes nothing");
            station.SetInputBlocked(inventory, false);
            Assert.IsTrue(station.InputBlocked, "the frame the last window closes stays blocked (its Esc must not leave the counter)");
        }

        [Test]
        public void EconomyPrefab_InventoryWindowHasTheShiftViewParts()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Economy/PF_Economy.prefab");
            Assert.NotNull(prefab, "PF_Economy missing - run GameOff/Build Economy");
            var window = prefab.GetComponentInChildren<InventoryWindow>(true);
            Assert.NotNull(window);
            var so = new SerializedObject(window);
            Assert.NotNull(so.FindProperty("title").objectReferenceValue, "title not wired - rebuild the economy");
            Assert.NotNull(so.FindProperty("devFillButton").objectReferenceValue, "dev fill button not wired - rebuild the economy");
        }
    }
}
