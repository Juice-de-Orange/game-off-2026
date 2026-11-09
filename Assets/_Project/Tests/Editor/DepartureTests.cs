using System.Collections.Generic;
using GameOff.Departure;
using NUnit.Framework;
using UnityEngine;

namespace GameOff.Tests
{
    /// <summary>P72: the ticket machine's offer and the ending the flight home gets from the family's condition.</summary>
    public class DepartureTests
    {
        private static readonly string[] Family = { "Mom", "Lena", "Pip" };
        private DepartureTuning _texts;

        [SetUp]
        public void Create() => _texts = ScriptableObject.CreateInstance<DepartureTuning>();

        [Test]
        public void WithFigures_LeavesTheBodyAloneIfNobodyWasMet()
        {
            Assert.AreEqual("body", TicketEnding.WithFigures("body", new List<string>(), _texts));
            Assert.AreEqual("body", TicketEnding.WithFigures("body", null, _texts));
        }

        [Test]
        public void WithFigures_ListsTheLinesUnderTheHeader()
        {
            string text = TicketEnding.WithFigures("body", new List<string> { "· a", "· b" }, _texts);
            Assert.AreEqual("body\n\n" + _texts.figuresHeader + "\n· a\n· b", text);
        }

        [TearDown]
        public void CleanUp() => Object.DestroyImmediate(_texts);

        private static string Cr(int amount) => amount + " CR";

        [Test]
        public void Everyone_well_or_weak_gets_home()
        {
            Assert.AreEqual(_texts.homeTitle, TicketEnding.Pick(new[] { 0, 0, 0 }, Family, _texts).title);
            Assert.AreEqual(_texts.homeTitle, TicketEnding.Pick(new[] { 1, 0, 1 }, Family, _texts).title);
        }

        [Test]
        public void Sick_family_gets_home_barely_with_the_sick_names()
        {
            var (title, body) = TicketEnding.Pick(new[] { 2, 1, 2 }, Family, _texts);
            Assert.AreEqual(_texts.barelyTitle, title);
            StringAssert.Contains("Mom and Pip", body);
            StringAssert.DoesNotContain("Lena", body);
            StringAssert.DoesNotContain("{names}", body);
        }

        [Test]
        public void Critical_member_decides_over_sick_ones()
        {
            var (title, body) = TicketEnding.Pick(new[] { 2, 3, 0 }, Family, _texts);
            Assert.AreEqual(_texts.lateTitle, title);
            StringAssert.Contains("Lena", body);
            StringAssert.DoesNotContain("Mom", body);
        }

        [Test]
        public void Names_are_joined_like_a_sentence()
        {
            Assert.AreEqual("Pip", TicketEnding.JoinNames(new[] { "Pip" }));
            Assert.AreEqual("Mom and Pip", TicketEnding.JoinNames(new[] { "Mom", "Pip" }));
            Assert.AreEqual("Mom, Lena and Pip", TicketEnding.JoinNames(Family));
        }

        [Test]
        public void Not_enough_money_shows_what_is_missing_and_no_buy()
        {
            var (title, body, canBuy) = TicketTerminal.Describe(true, false, 212, 350, Cr);
            Assert.IsFalse(canBuy);
            StringAssert.Contains("350 CR", title);
            StringAssert.Contains("You need 138 CR more", body);
        }

        [Test]
        public void Enough_money_in_the_evening_can_buy()
        {
            var (_, body, canBuy) = TicketTerminal.Describe(true, false, 400, 350, Cr);
            Assert.IsTrue(canBuy);
            StringAssert.Contains("After the ticket: 50 CR", body);
            Assert.IsTrue(TicketTerminal.Describe(true, false, 350, 350, Cr).canBuy, "exactly the price is enough");
        }

        [Test]
        public void Closed_during_the_day_and_no_second_ticket()
        {
            Assert.IsFalse(TicketTerminal.Describe(false, false, 999, 350, Cr).canBuy);
            StringAssert.Contains("after your shift", TicketTerminal.Describe(false, false, 999, 350, Cr).body);
            Assert.IsFalse(TicketTerminal.Describe(true, true, 999, 350, Cr).canBuy);
        }
    }
}
