using System.Collections.Generic;
using GameOff.Economy;
using GameOff.Rest;
using NUnit.Framework;
using UnityEngine;

namespace GameOff.Tests
{
    /// <summary>P28/P29: what the family's state does to the porthole thought, the photo on the locker and the letter's handwriting.</summary>
    public class RestSpotTests
    {
        private static readonly string[] Names = { "Mom", "Lena", "Pip" };
        private RestSpotTuning _tuning;

        [SetUp]
        public void SetUp() => _tuning = ScriptableObject.CreateInstance<RestSpotTuning>();

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(_tuning);

        [Test]
        public void Mood_IsTheWorstCondition_AndADeathOutweighsEverything()
        {
            Assert.AreEqual(FamilyMood.Well, FamilyMood.Of(new[] { 0, 0, 0 }));
            Assert.AreEqual(FamilyMood.Worried, FamilyMood.Of(new[] { 0, 1, 0 }));
            Assert.AreEqual(FamilyMood.Sick, FamilyMood.Of(new[] { 1, 0, 2 }));
            Assert.AreEqual(FamilyMood.Critical, FamilyMood.Of(new[] { 3, 2, 0 }));
            Assert.AreEqual(FamilyMood.Grieving, FamilyMood.Of(new[] { 0, Household.Dead, 3 }));
            Assert.AreEqual(FamilyMood.Well, FamilyMood.Of((Household)null));
        }

        [Test]
        public void Mood_FollowsARealHousehold()
        {
            var family = new Household(new List<bool> { false, true }, 2);
            Assert.AreEqual(FamilyMood.Well, FamilyMood.Of(family));
            family.Settle(true, false, false); // the one who feels the cold gets weak
            Assert.AreEqual(FamilyMood.Worried, FamilyMood.Of(family));
            family.Settle(false, false, false);
            family.Settle(false, false, false);
            Assert.AreEqual(FamilyMood.Critical, FamilyMood.Of(family));
            family.Settle(false, false, false);
            family.Settle(false, false, false);
            Assert.AreEqual(FamilyMood.Grieving, FamilyMood.Of(family));
        }

        [Test]
        public void Focus_IsTheDeadFirst_ElseTheOneWorstOff_ElseNobody()
        {
            Assert.AreEqual(-1, FamilyMood.Focus(new[] { 0, 0, 0 }));
            Assert.AreEqual(2, FamilyMood.Focus(new[] { 1, 0, 2 }));
            Assert.AreEqual(1, FamilyMood.Focus(new[] { 3, Household.Dead, 0 }));
        }

        [Test]
        public void Photo_IntactWhileWell_FadedWhenSick_TornAfterADeath()
        {
            Assert.AreEqual(0, FamilyMood.PhotoFrame(FamilyMood.Well));
            Assert.AreEqual(0, FamilyMood.PhotoFrame(FamilyMood.Worried));
            Assert.AreEqual(1, FamilyMood.PhotoFrame(FamilyMood.Sick));
            Assert.AreEqual(1, FamilyMood.PhotoFrame(FamilyMood.Critical));
            Assert.AreEqual(2, FamilyMood.PhotoFrame(FamilyMood.Grieving));
            for (int mood = FamilyMood.Well; mood < FamilyMood.Grieving; mood++)
                Assert.Greater(FamilyMood.PhotoBrightness(mood), FamilyMood.PhotoBrightness(mood + 1), $"mood {mood}");
        }

        [Test]
        public void Handwriting_GetsShakierTheWorseItGets()
        {
            for (int mood = FamilyMood.Well; mood < FamilyMood.Critical; mood++)
                Assert.Less(FamilyMood.Shake(mood), FamilyMood.Shake(mood + 1), $"mood {mood}");
            Assert.Greater(FamilyMood.Shake(FamilyMood.Grieving), FamilyMood.Shake(FamilyMood.Sick));
            Assert.Greater(FamilyMood.Shake(FamilyMood.Well), 0f, "handwriting, not print");
        }

        [Test]
        public void Handwriting_IsStillWithoutShake_FixedPerLetter_AndBounded()
        {
            Assert.AreEqual(Vector2.zero, Handwriting.Offset(5, 3, 0f));
            Assert.AreEqual(0f, Handwriting.Tilt(5, 3, 0f));
            Assert.AreEqual(Handwriting.Offset(7, 2, 1f), Handwriting.Offset(7, 2, 1f));
            bool varies = false;
            for (int i = 0; i < 200; i++)
            {
                var o = Handwriting.Offset(i, 4, 1f);
                float t = Handwriting.Tilt(i, 4, 1f);
                Assert.LessOrEqual(Mathf.Abs(o.x), 1f);
                Assert.LessOrEqual(Mathf.Abs(o.y), 1f);
                Assert.LessOrEqual(Mathf.Abs(t), 1f);
                varies |= o != Handwriting.Offset(0, 4, 1f);
            }
            Assert.IsTrue(varies, "every letter wobbles differently");
        }

        [Test]
        public void Thought_MatchesTheMood_AndNamesTheRightPerson()
        {
            for (int day = 1; day <= 4; day++)
            {
                CollectionAssert.Contains(_tuning.thoughtsWell, _tuning.Thought(new[] { 0, 0, 0 }, Names, day));
                string sick = _tuning.Thought(new[] { 0, 0, 2 }, Names, day);
                StringAssert.DoesNotContain("{0}", sick);
                if (FamilyMood.Pick(_tuning.thoughtsSick, day).Contains("{0}")) StringAssert.Contains("Pip", sick);
                string grief = _tuning.Thought(new[] { Household.Dead, 3, 0 }, Names, day);
                StringAssert.DoesNotContain("Lena", grief, "the thought is about the one who died");
            }
        }

        [Test]
        public void Thoughts_ExistForEveryMood_AndLeaveNoPlaceholderBehind()
        {
            for (int mood = FamilyMood.Well; mood <= FamilyMood.Grieving; mood++)
            {
                Assert.IsNotEmpty(_tuning.Thoughts(mood), $"mood {mood}");
                foreach (var line in _tuning.Thoughts(mood)) Assert.IsFalse(string.IsNullOrWhiteSpace(line));
            }
            var conditions = new[] { new[] { 0, 1, 0 }, new[] { 2, 0, 0 }, new[] { 0, 3, 1 }, new[] { 0, 0, Household.Dead } };
            foreach (var c in conditions)
                for (int day = 0; day < 6; day++)
                    StringAssert.DoesNotContain("{", _tuning.Thought(c, Names, day));
        }

        [Test]
        public void Pick_ChangesWithTheDay_AndCopesWithEmptyLists()
        {
            var lines = new[] { "a", "b", "c" };
            Assert.AreEqual("b", FamilyMood.Pick(lines, 1));
            Assert.AreEqual("b", FamilyMood.Pick(lines, 4));
            Assert.AreEqual("c", FamilyMood.Pick(lines, -1));
            Assert.AreEqual("", FamilyMood.Pick(new string[0], 3));
            Assert.AreEqual("", FamilyMood.Pick(null, 3));
        }

        [Test]
        public void HallBenches_HaveAnnouncementsToHear()
        {
            Assert.IsNotEmpty(_tuning.hallAnnouncements);
            Assert.Greater(_tuning.fastForwardSeconds, 0f);
            Assert.IsFalse(string.IsNullOrWhiteSpace(_tuning.eveningOver));
        }
    }
}
