using GameOff.Economy;
using GameOff.Shops;
using NUnit.Framework;
using UnityEngine;

namespace GameOff.Tests
{
    /// <summary>P111: the family's windows in the home street diorama, and when the coin telescope jumps there and back.</summary>
    public class HomeViewTests
    {
        private HomeViewTuning _tuning;

        [SetUp]
        public void SetUp() => _tuning = ScriptableObject.CreateInstance<HomeViewTuning>();

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(_tuning);

        [Test]
        public void Window_IsBrightWhileWellOrWeak_DimWhileSick()
        {
            Assert.AreEqual((_tuning.brightLevel, false), HomeWindows.Light(Household.Well, _tuning));
            Assert.AreEqual((_tuning.brightLevel, false), HomeWindows.Light(Household.Weak, _tuning));
            var sick = HomeWindows.Light(Household.Sick, _tuning);
            Assert.IsFalse(sick.flicker);
            Assert.Less(sick.level, _tuning.brightLevel);
            Assert.Greater(sick.level, 0f);
        }

        [Test]
        public void Window_FlickersOnlyWhileCritical()
        {
            for (int c = Household.Well; c <= Household.Dead; c++)
                Assert.AreEqual(c == Household.Critical, HomeWindows.Light(c, _tuning).flicker, "condition " + c);
            float low = HomeWindows.Level(Household.Critical, _tuning, 0f), high = HomeWindows.Level(Household.Critical, _tuning, 1f);
            Assert.Less(low, high);
            Assert.AreEqual(_tuning.criticalLevel, high, 1e-5f);
            // Flicker noise changes nothing for the others.
            Assert.AreEqual(HomeWindows.Level(Household.Sick, _tuning, 0f), HomeWindows.Level(Household.Sick, _tuning, 1f));
        }

        [Test]
        public void Window_StaysDarkOnceDead()
        {
            foreach (float noise in new[] { 0f, 0.5f, 1f })
                Assert.AreEqual(0f, HomeWindows.Level(Household.Dead, _tuning, noise));
            Assert.AreEqual(_tuning.darkColor, HomeWindows.Colour(0f, _tuning));
        }

        [Test]
        public void Window_FollowsEachMember_MissingMembersAreDark()
        {
            var family = new Household(new[] { false, false });
            Assert.AreEqual(Household.Well, HomeWindows.ConditionOf(family, 0));
            Assert.AreEqual(Household.Dead, HomeWindows.ConditionOf(family, 2));
            Assert.AreEqual(Household.Well, HomeWindows.ConditionOf(null, 1));
        }

        [Test]
        public void Telescope_JumpsToStreet_OnlyFullyZoomedAndAimedAtThePlanet()
        {
            Assert.IsFalse(TelescopeFocus.EntersStreet(_tuning.enterFov + 1f, 0f, 0f, _tuning));
            Assert.IsTrue(TelescopeFocus.EntersStreet(_tuning.enterFov, 0f, 0f, _tuning));
            Assert.IsTrue(TelescopeFocus.EntersStreet(3f, 5f, -5f, _tuning));
            Assert.IsFalse(TelescopeFocus.EntersStreet(3f, 25f, 0f, _tuning), "zoomed in next to the planet");
        }

        [Test]
        public void Telescope_LeavesStreet_WhenZoomingOutPastItsWidestView_AndDoesNotJumpStraightBack()
        {
            Assert.IsFalse(TelescopeFocus.LeavesStreet(_tuning.streetWideFov, _tuning));
            Assert.IsTrue(TelescopeFocus.LeavesStreet(_tuning.streetWideFov * 1.18f, _tuning));
            float back = TelescopeFocus.PlanetFovAfterStreet(_tuning, 24f);
            Assert.IsFalse(TelescopeFocus.EntersStreet(back, 0f, 0f, _tuning));
            Assert.LessOrEqual(back * 0.85f, _tuning.enterFov + 0.5f, "it comes back close to the jump");
        }

        [Test]
        public void Blink_IsBlackInTheMiddle_ClearAtBothEnds()
        {
            Assert.AreEqual(0f, TelescopeFocus.BlinkAlpha(0f), 1e-5f);
            Assert.AreEqual(1f, TelescopeFocus.BlinkAlpha(0.5f), 1e-5f);
            Assert.AreEqual(0f, TelescopeFocus.BlinkAlpha(1f), 1e-5f);
        }
    }
}
