using GameOff.Counter;
using GameOff.Day;
using NUnit.Framework;
using UnityEngine;

namespace GameOff.Tests
{
    /// <summary>P133: a loudspeaker line shortly after waking up on day 1 only, once.</summary>
    public class FirstMorningAnnouncementTests
    {
        private const float Delay = 2f;

        [Test]
        public void Day1_AfterTheDelay_Plays()
        {
            Assert.IsTrue(DayManager.FirstMorningAnnouncementDue(1, 2f, Delay, false));
            Assert.IsTrue(DayManager.FirstMorningAnnouncementDue(1, 30f, Delay, false));
        }

        [Test]
        public void Day1_BeforeTheDelay_Waits()
        {
            Assert.IsFalse(DayManager.FirstMorningAnnouncementDue(1, 0f, Delay, false));
            Assert.IsFalse(DayManager.FirstMorningAnnouncementDue(1, 1.9f, Delay, false));
        }

        [Test]
        public void LetterStillOpen_OrRunNotBegun_NeverPlays()
        {
            Assert.IsFalse(DayManager.FirstMorningAnnouncementDue(1, -1f, Delay, false));
            Assert.IsFalse(DayManager.FirstMorningAnnouncementDue(1, -1f, 0f, false), "no delay still waits for the letter");
            Assert.IsFalse(DayManager.FirstMorningAnnouncementDue(0, 10f, Delay, false), "day 0 = the menu, before BeginRun");
        }

        [Test]
        public void AlreadyPlayed_DoesNotRepeat()
        {
            Assert.IsFalse(DayManager.FirstMorningAnnouncementDue(1, 10f, Delay, true));
        }

        [TestCase(2)]
        [TestCase(3)]
        [TestCase(13)]
        public void OtherDays_NeverPlay(int day)
        {
            Assert.IsFalse(DayManager.FirstMorningAnnouncementDue(day, 10f, Delay, false));
        }

        [Test]
        public void Announcements_HaveADefaultLineAndDelay()
        {
            var lines = ScriptableObject.CreateInstance<Announcements>();
            try
            {
                Assert.AreEqual(Announcements.DefaultFirstMorning, lines.firstMorning);
                StringAssert.Contains("customs counter", lines.firstMorning);
                Assert.AreEqual(Announcements.DefaultFirstMorningDelay, lines.firstMorningDelay);
                Assert.Greater(lines.firstMorningDelay, 0f);
            }
            finally { Object.DestroyImmediate(lines); }
        }
    }
}
