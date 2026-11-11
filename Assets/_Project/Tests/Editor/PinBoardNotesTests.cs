using System.Collections.Generic;
using GameOff.Counter;
using GameOff.Travelers;
using NUnit.Framework;
using UnityEngine;

namespace GameOff.Tests
{
    /// <summary>P33/P130: the pinboard holds only today's notes; each morning replaces them.</summary>
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
        public void EveryMorning_ReplacesYesterdaysNotes_NoHistory()
        {
            var board = new PinBoardNotes();
            Assert.AreEqual("", board.Day, "nothing before the first morning");
            Assert.AreEqual(0, board.Notes.Count);
            board.SetDay("2187-03-01", Morning("2187-03-01"));
            board.SetDay("2187-03-02", Morning("2187-03-02", memo: true));
            board.SetDay("2187-03-03", Morning("2187-03-03"));
            Assert.AreEqual("2187-03-03", board.Day);
            Assert.AreEqual(2, board.Notes.Count, "only day 3's notes, yesterday's memo is gone");
            foreach (var note in board.Notes) Assert.AreEqual("2187-03-03", note.Date);
            Assert.IsNull(board.Of(PinNote.Memo));
        }

        [Test]
        public void SameMorningTwice_ReplacesIt()
        {
            var board = new PinBoardNotes();
            board.SetDay("2187-03-02", Morning("2187-03-02"));
            board.SetDay("2187-03-02", Morning("2187-03-02", memo: true));
            Assert.AreEqual(3, board.Notes.Count);
        }

        [Test]
        public void Of_FindsTodaysNoteOfAKind_NullWhenMissing()
        {
            var board = new PinBoardNotes();
            board.SetDay("a", Morning("a", memo: true));
            board.SetDay("b", Morning("b"));
            Assert.AreEqual("rules b", board.Of(PinNote.Bulletin).Body);
            Assert.AreEqual("wanted b", board.Of(PinNote.Wanted).Body);
            Assert.IsNull(board.Of(PinNote.Market));
            Assert.IsNull(board.Of(PinNote.Memo), "an older day's memo does not hang on");
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
