using GameOff.Counter;
using GameOff.Day;
using GameOff.Departure;
using NUnit.Framework;
using UnityEngine;

namespace GameOff.Tests
{
    /// <summary>P109: at 23:00 every trade closes; before that the evening trades as before.</summary>
    public class CurfewTests
    {
        private static string Cr(int amount) => amount + " CR";

        [Test]
        public void Evening_before_23_is_open_and_no_curfew()
        {
            Assert.IsTrue(DayManager.TradingOpenAt(DayManager.Phase.Evening, 120f));
            Assert.IsTrue(DayManager.TradingOpenAt(DayManager.Phase.Evening, 0.01f));
            Assert.IsFalse(DayManager.CurfewAt(DayManager.Phase.Evening, 0.01f));
        }

        [Test]
        public void Evening_at_23_is_curfew_and_closed()
        {
            Assert.IsTrue(DayManager.CurfewAt(DayManager.Phase.Evening, 0f));
            Assert.IsFalse(DayManager.TradingOpenAt(DayManager.Phase.Evening, 0f));
        }

        [TestCase(DayManager.Phase.Morning)]
        [TestCase(DayManager.Phase.Shift)]
        [TestCase(DayManager.Phase.Night)]
        [TestCase(DayManager.Phase.Finished)]
        public void Outside_the_evening_closed_but_no_curfew(DayManager.Phase phase)
        {
            Assert.IsFalse(DayManager.TradingOpenAt(phase, 0f));
            Assert.IsFalse(DayManager.TradingOpenAt(phase, 100f));
            Assert.IsFalse(DayManager.CurfewAt(phase, 0f), "EveningSecondsLeft is 0 outside the evening; that is not a curfew");
        }

        [Test]
        public void Ticket_machine_at_curfew_sells_nothing_and_says_why()
        {
            var (title, body, canBuy) = TicketTerminal.Describe(false, false, 999, 350, Cr, true, "Closed for the night. Curfew.");
            Assert.IsFalse(canBuy);
            StringAssert.Contains("350 CR", title);
            StringAssert.Contains("Curfew", body);
            StringAssert.DoesNotContain("after your shift", body);
        }

        [Test]
        public void Ticket_machine_at_curfew_without_text_still_closed()
        {
            var (_, body, canBuy) = TicketTerminal.Describe(false, false, 999, 350, Cr, true);
            Assert.IsFalse(canBuy);
            StringAssert.Contains("Closed for the night", body);
        }

        [Test]
        public void Ticket_holder_at_curfew_still_sees_the_ticket()
        {
            Assert.AreEqual("YOUR TICKET HOME", TicketTerminal.Describe(false, true, 0, 350, Cr, true, "x").title);
        }

        [Test]
        public void Ticket_machine_without_curfew_unchanged()
        {
            Assert.IsTrue(TicketTerminal.Describe(true, false, 400, 350, Cr).canBuy);
            StringAssert.Contains("after your shift", TicketTerminal.Describe(false, false, 999, 350, Cr).body);
        }

        [Test]
        public void Announcements_default_curfew_texts()
        {
            var lines = ScriptableObject.CreateInstance<Announcements>();
            try
            {
                Assert.AreEqual("Curfew. All shops are closed. Return to your quarters.", lines.curfew);
                Assert.IsFalse(string.IsNullOrWhiteSpace(lines.curfewHint));
                StringAssert.Contains("Curfew", lines.curfewClosed);
            }
            finally { Object.DestroyImmediate(lines); }
        }
    }
}
