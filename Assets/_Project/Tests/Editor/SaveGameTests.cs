using System.Collections.Generic;
using GameOff.Characters;
using GameOff.Core;
using GameOff.Economy;
using GameOff.Travelers;
using GameOff.UI;
using NUnit.Framework;
using UnityEngine;

namespace GameOff.Tests
{
    /// <summary>P128: the morning save – capture → JSON → apply gives the same run; CONTINUE only with a save; endings delete it.</summary>
    public class SaveGameTests
    {
        private readonly List<Object> _made = new List<Object>();

        private T Make<T>(string name) where T : ScriptableObject
        {
            var o = ScriptableObject.CreateInstance<T>();
            o.name = name;
            _made.Add(o);
            return o;
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var o in _made) Object.DestroyImmediate(o);
            _made.Clear();
        }

        /// <summary>A store in memory instead of PlayerPrefs.</summary>
        private class MemoryStore
        {
            public string Json = "";
            public int Writes;
            public SaveSlot Slot() => new SaveSlot(() => Json, j => { Json = j; Writes++; }, () => Json = "");
        }

        private static SaveGame.Run FreshRun(int members = 3) => new SaveGame.Run
        {
            wallet = new Wallet(100),
            inventory = new Inventory(),
            locker = new Inventory(),
            household = new Household(new List<bool>(new bool[members]), 2),
            conduct = new Conduct(),
            memory = new CharacterMemory(),
        };

        [Test]
        public void CaptureToJsonAndApply_GivesTheSameRun()
        {
            var battery = Make<ItemData>("Item_Battery");
            var stim = Make<ItemData>("Item_Stimulant");
            var clown = Make<ItemData>("Item_ClownShoes");
            var vase = Make<ItemData>("Item_Vase");
            var strike = Make<MarketEvent>("Market_Strike");
            var flood = Make<MarketEvent>("Market_Flood");
            var effect = new Consequence { wageChange = 25 };
            var appearances = new List<Appearance>
            {
                new Appearance(),
                new Appearance { consequences = new List<Consequence> { new Consequence(), effect } },
            };
            var vesna = Make<RecurringCharacter>("Char_Vesna");
            vesna.EditorSet("vesna", "Vesna Hale", "Human", appearances);
            vesna.EditorSetKin("Ilya Petrov", "Human", "by you", "by others");
            var korr = Make<RecurringCharacter>("Char_Korr");
            korr.EditorSet("korr", "Korr", "Grell", new List<Appearance> { new Appearance() });

            var run = FreshRun();
            run.day = 4;
            run.seed = 123456;
            run.wallet.Add(57, "Wage");
            run.inventory.Add(battery, 3);
            run.inventory.Add(clown);
            run.inventory.Add(vase);
            run.wearing = clown;
            run.locker.Add(stim, 2);
            run.household.Settle(false, true, false); // everyone weak
            run.household.Settle(false, true, false); // sick
            run.household.Settle(false, true, false); // critical
            run.household.Settle(false, true, false); // one night critical
            run.conduct.AddDisciplinary(2);
            run.conduct.AddMorale(-3);
            run.hasTicket = true;
            run.barkeeperSulksFrom = 3;
            run.stationMood = 0.35f;
            run.letterLines.Add("Vesna sent a thank-you note.");
            run.vendorExtras["Stock_Kiosk"] = new List<(ItemData, int)> { (vase, 2) };
            run.scheduledEvents[5] = new List<MarketEvent> { strike };
            run.scheduledEvents[6] = new List<MarketEvent> { flood, strike };
            run.scheduledEvents[2] = new List<MarketEvent> { flood }; // in the past: not saved
            run.characters = new[] { vesna, korr };
            var m = run.memory;
            m.Remember(vesna, 0, 2, Outcome.Helped);
            m.Remember(korr, 0, 3, Outcome.Confiscated);
            m.SetFlag("resistance");
            m.Entrust(vesna, vase, 40);
            m.ScheduleEffect(6, effect);
            m.ScheduleWanted(korr, new WantedEntry("Korr", "GR-1", WantedOrder.Arrest, "smuggler"), 5, 2);
            m.ScheduleEvent(5, strike);
            m.RememberArrest("Ilya Petrov", "Human", 2);
            m.Kin(vesna, 4); // fixes the relative: the arrest of day 2

            string json = JsonUtility.ToJson(SaveGame.Capture(run));
            var loaded = JsonUtility.FromJson<SaveData>(json);
            var back = FreshRun();
            SaveGame.Apply(loaded, back, new SaveGame.Lookup(new[] { battery, stim, clown, vase }, new[] { strike, flood }, new[] { vesna, korr }));

            Assert.AreEqual(4, back.day);
            Assert.AreEqual(123456, back.seed);
            Assert.AreEqual(157, back.wallet.Balance);
            Assert.IsEmpty(back.wallet.Today, "TODAY starts empty");
            CollectionAssert.AreEqual(new[] { battery, clown, vase }, back.inventory.Items, "same items in the same order");
            Assert.AreEqual(3, back.inventory.Count(battery));
            Assert.AreSame(clown, back.wearing);
            Assert.AreEqual(2, back.locker.Count(stim));
            for (int i = 0; i < 3; i++)
            {
                Assert.AreEqual(run.household.Condition(i), back.household.Condition(i));
                Assert.AreEqual(run.household.CriticalNights(i), back.household.CriticalNights(i));
                Assert.AreEqual(run.household.WillDieNextNight(i), back.household.WillDieNextNight(i));
            }
            Assert.AreEqual(2, back.conduct.Disciplinary);
            Assert.AreEqual(-3, back.conduct.Morale);
            Assert.IsTrue(back.hasTicket);
            Assert.AreEqual(3, back.barkeeperSulksFrom);
            Assert.AreEqual(0.35f, back.stationMood, 1e-5f);
            CollectionAssert.AreEqual(run.letterLines, back.letterLines);
            Assert.AreEqual(1, back.vendorExtras["Stock_Kiosk"].Count);
            Assert.AreEqual((vase, 2), back.vendorExtras["Stock_Kiosk"][0]);
            CollectionAssert.AreEqual(new[] { strike }, back.scheduledEvents[5]);
            CollectionAssert.AreEqual(new[] { flood, strike }, back.scheduledEvents[6]);
            Assert.IsFalse(back.scheduledEvents.ContainsKey(2));

            var b = back.memory;
            Assert.AreEqual(Outcome.Helped, b.Last(vesna));
            Assert.AreEqual(Outcome.Confiscated, b.Last(korr));
            Assert.IsTrue(b.MetOn(korr, 3));
            Assert.IsTrue(b.HasFlag("resistance"));
            Assert.IsTrue(b.TakeEntrusted(vesna, out var item, out int deposit));
            Assert.AreSame(vase, item);
            Assert.AreEqual(40, deposit);
            Assert.IsEmpty(b.TakeEffects(5));
            CollectionAssert.AreEqual(new[] { effect }, b.TakeEffects(6), "the very consequence of the character, by index");
            Assert.AreEqual(1, b.WantedOn(6).Count);
            Assert.AreEqual("GR-1", b.WantedOn(6)[0].id);
            Assert.IsEmpty(b.WantedOn(7));
            CollectionAssert.AreEqual(new[] { strike }, b.EventsOn(5));
            Assert.AreEqual(1, b.Arrests.Count);
            Assert.AreEqual(("Ilya Petrov", true), m.Kin(vesna, 9));
            Assert.AreEqual(("Ilya Petrov", true), b.Kin(vesna, 9), "the relative stays who it was");
        }

        [Test]
        public void DeadAndDiedTonight_SurviveTheSave()
        {
            var run = FreshRun(2);
            for (int i = 0; i < 6; i++) run.household.Settle(false, false, false);
            Assert.AreEqual(Household.Dead, run.household.Condition(0));
            run.household.Restore(new[] { Household.Dead, Household.Weak }, new[] { 0, 0 }, new[] { 0 });

            var back = FreshRun(2);
            SaveGame.Apply(JsonUtility.FromJson<SaveData>(JsonUtility.ToJson(SaveGame.Capture(run))), back, new SaveGame.Lookup(null, null, null));
            Assert.IsTrue(back.household.IsDead(0));
            Assert.AreEqual(Household.Weak, back.household.Condition(1));
            CollectionAssert.AreEqual(new[] { 0 }, back.household.DiedTonight, "the loss letter's first morning");
            Assert.AreEqual(1, back.household.Alive);
        }

        [Test]
        public void UnknownIds_AreSkipped()
        {
            var battery = Make<ItemData>("Item_Battery");
            var run = FreshRun();
            run.inventory.Add(battery, 2);
            run.inventory.Add(Make<ItemData>("Item_Removed"));
            var back = FreshRun();
            SaveGame.Apply(SaveGame.Capture(run), back, new SaveGame.Lookup(new[] { battery }, null, null));
            CollectionAssert.AreEqual(new[] { battery }, back.inventory.Items);
            Assert.AreEqual(2, back.inventory.Count(battery));
        }

        [Test]
        public void NoSave_NoContinue()
        {
            var store = new MemoryStore();
            var slot = store.Slot();
            Assert.IsNull(slot.Load());
            Assert.IsFalse(slot.HasSave);
            Assert.IsFalse(MainMenu.ShowsContinue(slot.Load()));

            store.Json = "not json at all";
            Assert.IsFalse(slot.HasSave, "a broken save counts as none");
            store.Json = JsonUtility.ToJson(new SaveData { day = 3, version = SaveData.CurrentVersion + 1 });
            Assert.IsFalse(slot.HasSave, "another format counts as none");

            store.Json = "";
            slot.SaveMorning(3, () => new SaveData { day = 3 });
            Assert.IsTrue(slot.HasSave);
            Assert.IsTrue(MainMenu.ShowsContinue(slot.Load()));
            Assert.AreEqual(3, slot.Load().day);
            StringAssert.Contains("DAY 3", MainMenu.ContinueLabel(3));
        }

        [Test]
        public void SavesOncePerMorning()
        {
            var store = new MemoryStore();
            var slot = store.Slot();
            int captures = 0;
            SaveData Capture(int day) { captures++; return new SaveData { day = day }; }
            Assert.IsTrue(slot.SaveMorning(1, () => Capture(1)));
            Assert.IsFalse(slot.SaveMorning(1, () => Capture(1)), "the same morning again (a continued run) saves nothing new");
            Assert.IsTrue(slot.SaveMorning(2, () => Capture(2)));
            Assert.AreEqual(2, store.Writes);
            Assert.AreEqual(2, captures);
            Assert.AreEqual(2, slot.Load().day);
        }

        [Test]
        public void Ending_DeletesTheSave_AndNothingIsSavedAfterIt()
        {
            var store = new MemoryStore();
            var slot = store.Slot();
            slot.SaveMorning(5, () => new SaveData { day = 5 });
            slot.EndRun();
            Assert.IsFalse(slot.HasSave);
            Assert.IsFalse(slot.SaveMorning(6, () => new SaveData { day = 6 }), "the night after a discharge must not save a new morning");
            Assert.IsFalse(slot.HasSave);
            Assert.IsFalse(MainMenu.ShowsContinue(slot.Load()));
        }
    }
}
