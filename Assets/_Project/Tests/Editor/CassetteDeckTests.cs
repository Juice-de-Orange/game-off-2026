using System.Collections.Generic;
using GameOff.Economy;
using GameOff.Rest;
using NUnit.Framework;
using UnityEngine;

namespace GameOff.Tests
{
    /// <summary>
    /// P132: the tape deck's playlist (CassettePlaylist): slots, slot order, starting over, taking out and putting back into the
    /// inventory; cassettes are no goods (no vendor buys them). P141: the lever skips to the next cassette. P156: the start cassette.
    /// </summary>
    public class CassetteDeckTests
    {
        private readonly List<Object> _made = new List<Object>();

        private T Make<T>(string name) where T : ScriptableObject
        {
            var o = ScriptableObject.CreateInstance<T>();
            o.name = name;
            _made.Add(o);
            return o;
        }

        private CassetteData Tape(string name) => Make<CassetteData>(name);

        [TearDown]
        public void TearDown()
        {
            foreach (var o in _made) Object.DestroyImmediate(o);
            _made.Clear();
        }

        [Test]
        public void EmptyDeck_PlaysNothing_AndEveryLampIsOff()
        {
            var deck = new CassettePlaylist(3);
            Assert.AreEqual(-1, deck.Playing);
            Assert.IsNull(deck.Current);
            Assert.AreEqual(0, deck.Count);
            Assert.AreEqual(-1, deck.Advance(), "nothing to go on with");
            for (int i = 0; i < 3; i++) Assert.AreEqual(CassettePlaylist.Lamp.Off, deck.LampOf(i));
        }

        [Test]
        public void FirstCassette_Plays_TheOthersWait()
        {
            var a = Tape("A");
            var b = Tape("B");
            var deck = new CassettePlaylist(3);
            Assert.IsTrue(deck.Insert(1, a));
            Assert.AreEqual(1, deck.Playing, "into an empty deck: it starts");
            Assert.IsTrue(deck.Insert(0, b));
            Assert.AreSame(a, deck.Current, "inserting while one plays only queues it");
            Assert.AreEqual(CassettePlaylist.Lamp.Wait, deck.LampOf(0));
            Assert.AreEqual(CassettePlaylist.Lamp.Play, deck.LampOf(1));
            Assert.AreEqual(CassettePlaylist.Lamp.Off, deck.LampOf(2));
        }

        [Test]
        public void Tracks_FollowTheSlotOrder_AndStartOver()
        {
            var a = Tape("A");
            var b = Tape("B");
            var c = Tape("C");
            var deck = new CassettePlaylist(3);
            deck.Insert(0, a);
            deck.Insert(2, c);
            deck.Insert(1, b);
            Assert.AreSame(a, deck.Current);
            deck.Advance();
            Assert.AreSame(b, deck.Current, "slot order, not the order they went in");
            deck.Advance();
            Assert.AreSame(c, deck.Current);
            deck.Advance();
            Assert.AreSame(a, deck.Current, "after the last one the first again");
        }

        [Test]
        public void EmptySlots_AreSkipped_AndASingleCassetteRepeats()
        {
            var a = Tape("A");
            var c = Tape("C");
            var deck = new CassettePlaylist(3);
            deck.Insert(2, c);
            deck.Insert(0, a);
            Assert.AreEqual(2, deck.Playing);
            Assert.AreEqual(0, deck.Advance(), "round again past the end, the empty middle slot is skipped");
            Assert.AreEqual(2, deck.Advance());

            var single = new CassettePlaylist(3);
            single.Insert(1, a);
            Assert.AreEqual(1, single.Advance());
            Assert.AreEqual(1, single.Advance());
        }

        [Test]
        public void TakingOutThePlayingCassette_GoesOnWithTheNextOne()
        {
            var a = Tape("A");
            var b = Tape("B");
            var c = Tape("C");
            var deck = new CassettePlaylist(3);
            deck.Insert(0, a);
            deck.Insert(1, b);
            deck.Insert(2, c);
            deck.Advance(); // B plays
            Assert.AreSame(b, deck.Eject(1));
            Assert.AreSame(c, deck.Current, "the next one in slot order");
            Assert.AreSame(a, deck.Eject(0));
            Assert.AreSame(c, deck.Current, "taking out a waiting one doesn't interrupt");
            Assert.AreSame(c, deck.Eject(2));
            Assert.IsNull(deck.Current, "the last one out: silence");
            Assert.AreEqual(-1, deck.Playing);
            Assert.IsNull(deck.Eject(2), "an empty slot gives nothing");
        }

        [Test]
        public void Skip_GoesToTheNextCassette_AndRoundAgain()
        {
            // P141: the lever, three cassettes in: 1 -> 2 -> 3 -> 1.
            var a = Tape("A");
            var b = Tape("B");
            var c = Tape("C");
            var deck = new CassettePlaylist(3);
            deck.Insert(0, a);
            deck.Insert(1, b);
            deck.Insert(2, c);
            Assert.IsTrue(deck.Skip());
            Assert.AreSame(b, deck.Current);
            Assert.IsTrue(deck.Skip());
            Assert.AreSame(c, deck.Current);
            Assert.IsTrue(deck.Skip());
            Assert.AreSame(a, deck.Current, "after the last one the first again");
            Assert.AreEqual(3, deck.Count, "skipping takes nothing out");
            Assert.AreEqual(CassettePlaylist.Lamp.Play, deck.LampOf(0));
            Assert.AreEqual(CassettePlaylist.Lamp.Wait, deck.LampOf(1));
        }

        [Test]
        public void Skip_PassesEmptySlots_AndASingleCassetteStartsOver()
        {
            var a = Tape("A");
            var c = Tape("C");
            var deck = new CassettePlaylist(3);
            deck.Insert(2, c);
            deck.Insert(0, a);
            Assert.IsTrue(deck.Skip());
            Assert.AreSame(a, deck.Current, "past the end and the empty middle slot");
            Assert.IsTrue(deck.Skip());
            Assert.AreSame(c, deck.Current);

            var single = new CassettePlaylist(3);
            single.Insert(1, a);
            Assert.IsTrue(single.Skip(), "one cassette: it starts over");
            Assert.AreEqual(1, single.Playing);
            Assert.AreSame(a, single.Current);
        }

        [Test]
        public void Skip_OnAnEmptyDeck_DoesNothing()
        {
            var deck = new CassettePlaylist(3);
            Assert.IsFalse(deck.Skip());
            Assert.AreEqual(-1, deck.Playing);
            Assert.IsNull(deck.Current);

            var a = Tape("A");
            deck.Insert(1, a);
            deck.Eject(1);
            Assert.IsFalse(deck.Skip(), "emptied again");
            Assert.IsNull(deck.Current);
        }

        [Test]
        public void Skip_ThenATrackEnd_GoesOnFromTheSkippedTo()
        {
            var a = Tape("A");
            var b = Tape("B");
            var c = Tape("C");
            var deck = new CassettePlaylist(3);
            deck.Insert(0, a);
            deck.Insert(1, b);
            deck.Insert(2, c);
            deck.Skip(); // B
            deck.Skip(); // C
            deck.Advance(); // C played to its end
            Assert.AreSame(a, deck.Current);
            Assert.AreSame(b, deck.Eject(1));
            deck.Skip();
            Assert.AreSame(c, deck.Current, "the slot taken out is passed");
        }

        [Test]
        public void FullSlot_TheSameCassetteTwice_AndOutOfRange_AreRefused()
        {
            var a = Tape("A");
            var b = Tape("B");
            var deck = new CassettePlaylist(3);
            Assert.IsTrue(deck.Insert(0, a));
            Assert.IsFalse(deck.Insert(0, b), "slot taken");
            Assert.IsFalse(deck.Insert(1, a), "the same cassette can't be in two slots");
            Assert.IsFalse(deck.Insert(3, b));
            Assert.IsFalse(deck.Insert(-1, b));
            Assert.IsFalse(deck.Insert(1, null));
            Assert.AreEqual(1, deck.Count);
        }

        [Test]
        public void InsertFrom_TakesTheFirstCassetteOfTheInventory()
        {
            var battery = Make<ItemData>("Item_Battery");
            var a = Tape("A");
            var b = Tape("B");
            var inventory = new Inventory();
            inventory.Add(battery, 2);
            inventory.Add(a);
            inventory.Add(b);
            var deck = new CassettePlaylist(3);

            Assert.AreSame(a, deck.InsertFrom(inventory, 2), "the first cassette, other items are skipped");
            Assert.AreSame(a, deck.At(2));
            Assert.AreEqual(0, inventory.Count(a), "it left the inventory");
            Assert.AreEqual(2, inventory.Count(battery));
            Assert.IsNull(deck.InsertFrom(inventory, 2), "slot taken");
            Assert.AreEqual(1, inventory.Count(b), "nothing lost on a refused insert");
            Assert.AreSame(b, deck.InsertFrom(inventory, 0));
            Assert.IsNull(deck.InsertFrom(inventory, 1), "no cassette left");
            Assert.AreEqual(2, inventory.Count(battery));
        }

        [Test]
        public void EjectTo_PutsTheCassetteBackIntoTheInventory()
        {
            var a = Tape("A");
            var b = Tape("B");
            var inventory = new Inventory();
            inventory.Add(a);
            inventory.Add(b);
            var deck = new CassettePlaylist(3);
            deck.InsertFrom(inventory, 0);
            deck.InsertFrom(inventory, 1);
            Assert.AreEqual(0, inventory.Items.Count);

            Assert.AreSame(a, deck.EjectTo(inventory, 0));
            Assert.AreEqual(1, inventory.Count(a));
            Assert.AreSame(b, deck.Current, "the playing one came out: the next one plays");
            Assert.IsNull(deck.EjectTo(inventory, 0), "empty slot");
            Assert.AreSame(b, deck.EjectTo(inventory, 1));
            CollectionAssert.AreEquivalent(new[] { a, b }, inventory.Items);
            Assert.IsNull(deck.Current);
        }

        [Test]
        public void Restore_FillsTheSlots_Stopped_AndTheLeverPlaysTheFirst()
        {
            var a = Tape("A");
            var b = Tape("B");
            var deck = new CassettePlaylist(3);
            deck.Insert(0, Tape("Old"));
            deck.Restore(new List<CassetteData> { null, b, a });
            CollectionAssert.AreEqual(new[] { null, b, a }, deck.Slots);
            Assert.IsNull(deck.Current, "P156: a continued run doesn't start the music by itself");
            Assert.IsTrue(deck.Skip());
            Assert.AreSame(b, deck.Current, "the lever plays the first filled slot");
            Assert.IsTrue(deck.Holds(a));

            deck.Restore(new List<CassetteData> { a, a }); // a broken save: never the same cassette twice
            Assert.AreEqual(1, deck.Count);
            deck.Restore(null);
            Assert.AreEqual(0, deck.Count);
            Assert.IsNull(deck.Current);
        }

        [Test]
        public void Load_PutsACassetteInStopped_AndTheLeverStartsIt()
        {
            var start = Tape("Start");
            var other = Tape("Other");
            var deck = new CassettePlaylist(3);
            Assert.IsTrue(deck.Load(0, start), "P156: the start cassette goes in");
            Assert.IsNull(deck.Current, "a loaded cassette doesn't start by itself");
            Assert.AreEqual(CassettePlaylist.Lamp.Wait, deck.LampOf(0));
            Assert.IsTrue(deck.Holds(start), "it counts as owned: the kiosk doesn't sell it again");
            Assert.IsFalse(deck.Load(0, other), "a full slot stays refused");

            Assert.IsTrue(deck.Skip(), "the lever of a stopped deck plays the first cassette");
            Assert.AreSame(start, deck.Current);

            var stopped = new CassettePlaylist(3);
            stopped.Load(0, start);
            Assert.IsTrue(stopped.Insert(2, other));
            Assert.AreSame(other, stopped.Current, "inserting into a stopped deck plays the new cassette");
        }

        [Test]
        public void StartCassette_OfTheRoomDeck_IsSet_AndTheCounterDeckStartsEmpty()
        {
            var rest = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Rest/PF_RestSpots.prefab");
            if (rest == null) Assert.Ignore("PF_RestSpots is not built yet");
            int checkedDecks = 0;
            foreach (var deck in rest.GetComponentsInChildren<CassetteDeck>(true))
            {
                var start = new UnityEditor.SerializedObject(deck).FindProperty("startCassette").objectReferenceValue;
                if (deck.Id == "Room103") Assert.IsNotNull(start, "P156: a new run finds a cassette in the room deck");
                else Assert.IsNull(start, deck.Id + " deck starts empty");
                checkedDecks++;
            }
            Assert.That(checkedDecks, Is.GreaterThanOrEqualTo(2));
        }

        [Test]
        public void Cassettes_AreNoGoods()
        {
            var stock = Make<VendorStock>("Stock_Any"); // buys everything not refused
            var watch = Make<ItemData>("Item_Watch");
            var tape = Tape("Item_Cassette_01");
            Assert.IsTrue(stock.Buys(watch));
            Assert.IsFalse(stock.Buys(tape), "no vendor buys a cassette back");

            var inventory = new Inventory();
            Assert.IsNull(CassetteData.FirstIn(inventory));
            inventory.Add(watch);
            Assert.IsNull(CassetteData.FirstIn(inventory));
            inventory.Add(tape);
            Assert.AreSame(tape, CassetteData.FirstIn(inventory));
            Assert.IsNull(CassetteData.FirstIn(null));
        }
    }
}
