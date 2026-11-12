using System;
using System.Linq;
using GameOff.UI;
using NUnit.Framework;

namespace GameOff.Tests
{
    /// <summary>P140: [H] help: every situation has its keys, the right situation wins, and [H] / Esc open and close it.</summary>
    public class HelpKeysTests
    {
        private static readonly HelpSituation[] Situations =
            ((HelpSituation[])Enum.GetValues(typeof(HelpSituation))).Where(s => s != HelpSituation.None).ToArray();

        private static string All(HelpSituation s) => string.Join("\n", HelpKeys.Lines(s).Select(l => l.keys + " " + l.action));

        [Test]
        public void EverySituationHasTitleAndLines()
        {
            foreach (var s in Situations)
            {
                Assert.IsNotEmpty(HelpKeys.Title(s), s.ToString());
                Assert.IsNotEmpty(HelpKeys.Lines(s), s.ToString());
                foreach (var (keys, action) in HelpKeys.Lines(s))
                {
                    Assert.IsFalse(string.IsNullOrWhiteSpace(keys), s + ": empty keys");
                    Assert.IsFalse(string.IsNullOrWhiteSpace(action), s + ": empty action");
                }
            }
            Assert.IsEmpty(HelpKeys.Lines(HelpSituation.None));
        }

        [TestCase(HelpSituation.Walking, new[] { "[W]", "[A]", "[S]", "[D]", "[E]", "[I]", "[Esc]", "[P]", "Mouse" })]
        [TestCase(HelpSituation.Counter, new[] { "[Q]", "[Esc]", "right click", "[R]", "[A]", "[D]", "[I]", "Click" })]
        [TestCase(HelpSituation.ReadingPapers, new[] { "[Q]", "[Esc]", "[R]", "Click a field" })]
        [TestCase(HelpSituation.Terminal, new[] { "[Q]", "[Esc]", "Click" })]
        [TestCase(HelpSituation.PinBoard, new[] { "[E]", "[Esc]" })]
        [TestCase(HelpSituation.RestSeat, new[] { "[Q]", "[Esc]", "Mouse", "[W]" })]
        [TestCase(HelpSituation.BenchEvening, new[] { "[Q]", "[Esc]", "[F]" })]
        [TestCase(HelpSituation.CloseLook, new[] { "[Q]", "[Esc]" })]
        [TestCase(HelpSituation.Telescope, new[] { "[Q]", "[Esc]", "Mouse wheel", "[W]" })]
        [TestCase(HelpSituation.Flight, new[] { "[W]", "[S]", "[A]", "[D]", "[Shift]", "Mouse" })]
        [TestCase(HelpSituation.Inventory, new[] { "[I]", "[Esc]" })]
        [TestCase(HelpSituation.Trade, new[] { "[Esc]" })]
        public void SituationListsItsRealKeys(HelpSituation situation, string[] keys)
        {
            string text = All(situation);
            foreach (var key in keys) StringAssert.Contains(key, text, situation + " misses " + key);
        }

        [Test]
        public void SeatedAtTheCounterHasNoWalkingKeys()
        {
            foreach (var s in new[] { HelpSituation.Counter, HelpSituation.ReadingPapers, HelpSituation.Terminal })
            {
                string text = All(s);
                StringAssert.DoesNotContain("Walk", text, s.ToString());
                StringAssert.DoesNotContain("[E]", text, s.ToString());
                StringAssert.DoesNotContain("[W]", text, s.ToString());
                StringAssert.DoesNotContain("[P]", text, s.ToString());
            }
        }

        [Test]
        public void OnFootTheTapeDeckLeverSkips()
        {
            // P141: the lever of a tape deck is its own [E] target (next song).
            StringAssert.Contains("Next song", All(HelpSituation.Walking));
        }

        [Test]
        public void PinBoardHasNoPaging()
        {
            // P130 removed the board's history: no A/D leafing any more.
            string text = All(HelpSituation.PinBoard);
            StringAssert.DoesNotContain("[A]", text);
            StringAssert.DoesNotContain("[D]", text);
        }

        [Test]
        public void NoKeyIsBoundToHelpElsewhere()
        {
            foreach (var s in Situations) StringAssert.DoesNotContain("[H]", All(s), s.ToString());
            StringAssert.StartsWith("[H]", HelpKeys.Prompt);
        }

        [Test]
        public void SituationFollowsWhatThePlayerDoes()
        {
            Assert.AreEqual(HelpSituation.Walking, HelpKeys.SituationFor(new HelpContext()));
            Assert.AreEqual(HelpSituation.Counter, HelpKeys.SituationFor(new HelpContext { AtCounter = true }));
            Assert.AreEqual(HelpSituation.ReadingPapers, HelpKeys.SituationFor(new HelpContext { AtCounter = true, ReadingPapers = true }));
            Assert.AreEqual(HelpSituation.Terminal, HelpKeys.SituationFor(new HelpContext { AtCounter = true, Terminal = true }));
            Assert.AreEqual(HelpSituation.Inventory, HelpKeys.SituationFor(new HelpContext { AtCounter = true, Inventory = true }));
            Assert.AreEqual(HelpSituation.PinBoard, HelpKeys.SituationFor(new HelpContext { PinBoard = true }));
            Assert.AreEqual(HelpSituation.Trade, HelpKeys.SituationFor(new HelpContext { Trade = true }));
            Assert.AreEqual(HelpSituation.Telescope, HelpKeys.SituationFor(new HelpContext { Telescope = true }));
            Assert.AreEqual(HelpSituation.Flight, HelpKeys.SituationFor(new HelpContext { Flying = true }));
            Assert.AreEqual(HelpSituation.RestSeat, HelpKeys.SituationFor(new HelpContext { Seat = HelpContext.SeatKind.LookAround }));
            Assert.AreEqual(HelpSituation.BenchEvening,
                HelpKeys.SituationFor(new HelpContext { Seat = HelpContext.SeatKind.LookAroundEvening }));
            Assert.AreEqual(HelpSituation.CloseLook, HelpKeys.SituationFor(new HelpContext { Seat = HelpContext.SeatKind.CloseLook }));
        }

        [Test]
        public void NoHelpInMenuEndingDialogOrPause()
        {
            Assert.AreEqual(HelpSituation.None, HelpKeys.SituationFor(new HelpContext { InMenu = true }));
            Assert.AreEqual(HelpSituation.None, HelpKeys.SituationFor(new HelpContext { Ending = true, Flying = true }));
            Assert.AreEqual(HelpSituation.None, HelpKeys.SituationFor(new HelpContext { PanelOpen = true, AtCounter = true }));
            Assert.AreEqual(HelpSituation.None, HelpKeys.SituationFor(new HelpContext { Paused = true }));
        }

        [Test]
        public void HelpKeyOpensAndHelpOrEscapeClose()
        {
            Assert.IsTrue(HelpKeys.NextOpen(false, true, true, false), "H opens");
            Assert.IsFalse(HelpKeys.NextOpen(false, true, false, true), "Esc doesn't open");
            Assert.IsFalse(HelpKeys.NextOpen(false, true, false, false));
            Assert.IsTrue(HelpKeys.NextOpen(true, true, false, false), "stays open");
            Assert.IsFalse(HelpKeys.NextOpen(true, true, true, false), "H closes");
            Assert.IsFalse(HelpKeys.NextOpen(true, true, false, true), "Esc closes");
            Assert.IsFalse(HelpKeys.NextOpen(false, false, true, false), "no help where none is available");
            Assert.IsFalse(HelpKeys.NextOpen(true, false, false, false), "closes when the situation ends");
        }
    }
}
