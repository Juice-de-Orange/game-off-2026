using System.Collections.Generic;
using GameOff.Counter;
using GameOff.Travelers;
using NUnit.Framework;
using UnityEngine;

namespace GameOff.Tests
{
    /// <summary>P33: the pinboard keeps every morning's notes; the close-up leafs through them by age, the board piles them by kind.</summary>
    public class PinBoardNotesTests
    {
        private static List<PinNote> Morning(string date, bool memo = false)
        {
            var notes = new List<PinNote>
            {
                new PinNote(PinNote.Bulletin, "DAILY BULLETIN", date, "rules " + date),
                new PinNote(PinNote.Wanted, "WANTED", date, "wanted " + date),
            };
            if (memo) notes.Add(new PinNote(PinNote.Memo, "MEMO", date, "memo " + date));
            return notes;
        }

        [Test]
        public void EveryMorning_AddsNotes_OldOnesStay()
        {
            var board = new PinBoardNotes();
            board.SetDay("2187-03-01", Morning("2187-03-01"));
            board.SetDay("2187-03-02", Morning("2187-03-02", memo: true));
            board.SetDay("2187-03-03", Morning("2187-03-03"));
            Assert.AreEqual(3, board.DayCount);
            Assert.AreEqual("2187-03-03", board.DayOf(0), "age 0 = today");
            Assert.AreEqual("rules 2187-03-03", board.NotesOfDay(0)[0].Body);
            Assert.AreEqual(3, board.NotesOfDay(1).Count, "yesterday's memo is still there");
            Assert.AreEqual("rules 2187-03-01", board.NotesOfDay(2)[0].Body);
            Assert.AreEqual(0, board.NotesOfDay(3).Count, "nothing before the first morning");
            Assert.AreEqual("", board.DayOf(3));
            Assert.AreEqual(0, board.NotesOfDay(-1).Count);
        }

        [Test]
        public void SameMorningTwice_ReplacesIt_InsteadOfAddingADay()
        {
            var board = new PinBoardNotes();
            board.SetDay("2187-03-01", Morning("2187-03-01"));
            board.SetDay("2187-03-02", Morning("2187-03-02"));
            board.SetDay("2187-03-02", Morning("2187-03-02", memo: true));
            Assert.AreEqual(2, board.DayCount);
            Assert.AreEqual(3, board.NotesOfDay(0).Count);
        }

        [Test]
        public void OldestDays_DropOut_BeyondTheLimit()
        {
            var board = new PinBoardNotes(2);
            board.SetDay("a", Morning("a"));
            board.SetDay("b", Morning("b"));
            board.SetDay("c", Morning("c"));
            Assert.AreEqual(2, board.DayCount);
            Assert.AreEqual("b", board.DayOf(1));
        }

        [Test]
        public void Pile_NewestFirst_SkipsDaysWithoutThatKind()
        {
            var board = new PinBoardNotes();
            board.SetDay("a", Morning("a", memo: true));
            board.SetDay("b", Morning("b"));
            board.SetDay("c", Morning("c", memo: true));
            board.SetDay("d", Morning("d"));
            var memos = board.Pile(PinNote.Memo, 5);
            Assert.AreEqual(new[] { "c", "a" }, new[] { memos[0].Date, memos[1].Date });
            var bulletins = board.Pile(PinNote.Bulletin, 3);
            Assert.AreEqual(new[] { "d", "c", "b" }, new[] { bulletins[0].Date, bulletins[1].Date, bulletins[2].Date });
            Assert.AreEqual(0, board.Pile(PinNote.Market, 3).Count);
        }

        [Test]
        public void NotesFor_ADay_HasRulesAndWanted_MarketAndMemoOnlyWhenThere()
        {
            var day = DayRules.CreateGenerated("Test", "2187-03-04", new List<DayRule>(),
                new List<WantedEntry> { new WantedEntry("Ada Vance", "", WantedOrder.Arrest) }, new List<TravelerData>());
            try
            {
                var plain = PinBoard.NotesFor(day, "", "");
                Assert.AreEqual(new[] { PinNote.Bulletin, PinNote.Wanted }, new[] { plain[0].Kind, plain[1].Kind });
                Assert.AreEqual(2, plain.Count);
                Assert.AreEqual(DayBulletin.Rules(day), plain[0].Body, "same text as the seated note (P41)");
                Assert.AreEqual(DayBulletin.Wanted(day), plain[1].Body);
                Assert.AreEqual(1, plain[1].Photos.Count);
                Assert.AreEqual("Ada Vance", plain[1].Photos[0].name);
                Assert.IsNull(plain[1].Photos[0].photo, "nobody of today carries that name: no photo");

                var full = PinBoard.NotesFor(day, "Search every crate.", "Fuel prices up.");
                Assert.AreEqual(4, full.Count);
                Assert.AreEqual(PinNote.Market, full[2].Kind);
                Assert.AreEqual(PinNote.Memo, full[3].Kind);
                StringAssert.Contains("Search every crate.", full[3].Body);
                Assert.AreEqual("2187-03-04", full[3].Date);
            }
            finally { Object.DestroyImmediate(day); }
        }
    }
}
