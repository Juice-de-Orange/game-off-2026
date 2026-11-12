using System.Collections.Generic;
using GameOff.Characters;
using GameOff.Counter;
using GameOff.Travelers;
using NUnit.Framework;
using UnityEngine;

namespace GameOff.Tests
{
    /// <summary>P86: Korr stays first in the queue, and the tutorial always asks for the first step not done yet.</summary>
    public class CounterTutorialTests
    {
        private readonly List<Object> _created = new List<Object>();

        [TearDown]
        public void CleanUp()
        {
            foreach (var o in _created) Object.DestroyImmediate(o);
            _created.Clear();
        }

        private TravelerData Traveler(string name)
        {
            var t = ScriptableObject.CreateInstance<TravelerData>();
            t.name = name;
            _created.Add(t);
            return t;
        }

        private CharacterPlanner.Planned Planned(TravelerData traveler, int slot)
        {
            var c = ScriptableObject.CreateInstance<RecurringCharacter>();
            _created.Add(c);
            c.EditorSet(traveler.name, traveler.name, "Human", new List<Appearance> { new Appearance { traveler = traveler, dayFrom = 1, dayTo = 1, slot = slot } });
            return new CharacterPlanner.Planned { character = c, appearance = 0, traveler = traveler };
        }

        [Test]
        public void FreeCharacters_NeverGoBeforeAFixedPlace()
        {
            var korr = Traveler("korr");
            var regular = new List<TravelerData> { Traveler("r1"), Traveler("r2"), Traveler("r3") };
            var due = new List<CharacterPlanner.Planned> { Planned(korr, 1), Planned(Traveler("quill"), 0), Planned(Traveler("shifter"), 0) };

            foreach (float random in new[] { 0f, 0.5f, 0.999f })
            {
                var queue = CharacterPlanner.Queue(regular, due, () => random);
                Assert.AreSame(korr, queue[0], $"random {random}");
                Assert.AreEqual(6, queue.Count);
            }
        }

        [Test]
        public void FixedPlaces_AreExact_FreeCharactersCanStillComeFirst()
        {
            var third = Traveler("third");
            var free = Traveler("free");
            var regular = new List<TravelerData> { Traveler("r1"), Traveler("r2"), Traveler("r3") };
            var due = new List<CharacterPlanner.Planned> { Planned(third, 3), Planned(free, 0) };

            var queue = CharacterPlanner.Queue(regular, due, () => 0f);
            Assert.AreSame(free, queue[0], "free: anywhere");
            Assert.AreSame(third, queue[2], "slot 3 = third in the queue");
        }

        [Test]
        public void Steps_FollowInOrder()
        {
            var p = new TutorialProgress();
            Assert.AreEqual(TutorialProgress.Step.HoldPaper, p.Current);
            p.PaperHeld(isNote: false);
            Assert.AreEqual(TutorialProgress.Step.AskField, p.Current);
            p.FieldAsked();
            Assert.AreEqual(TutorialProgress.Step.ReadRules, p.Current);
            p.PaperHeld(isNote: true);
            Assert.AreEqual(TutorialProgress.Step.Stamp, p.Current);
            p.Stamped(approved: true);
            Assert.AreEqual(TutorialProgress.Step.Drawer, p.Current);
        }

        [Test]
        public void RunningAhead_SkipsWhatIsAlreadyDone()
        {
            var p = new TutorialProgress();
            p.PaperHeld(isNote: true);
            p.FieldAsked();
            Assert.AreEqual(TutorialProgress.Step.HoldPaper, p.Current, "a note on the pinboard is not one of the papers");
            p.PaperHeld(isNote: false);
            Assert.AreEqual(TutorialProgress.Step.Stamp, p.Current);
        }

        [Test]
        public void TheLastStampCounts()
        {
            var p = new TutorialProgress();
            p.PaperHeld(isNote: false);
            p.FieldAsked();
            p.PaperHeld(isNote: true);
            string asked = p.Text;

            p.Stamped(approved: false);
            Assert.AreEqual(TutorialProgress.Step.Stamp, p.Current);
            Assert.AreNotEqual(asked, p.Text, "a wrong stamp gets its own line");

            p.Stamped(approved: true);
            Assert.AreEqual(TutorialProgress.Step.Drawer, p.Current);
            p.Stamped(approved: false);
            Assert.AreEqual(TutorialProgress.Step.Stamp, p.Current);
        }

        /// <summary>P143: the other two buttons are explained before the decision, with the orders on the pinboard; nothing comes after it.</summary>
        [Test]
        public void TheButtons_ComeWithTheRules()
        {
            var p = new TutorialProgress();
            p.PaperHeld(isNote: false);
            p.FieldAsked();
            Assert.AreEqual(TutorialProgress.Step.ReadRules, p.Current);
            StringAssert.Contains("ARREST", p.Text);
            StringAssert.Contains("AIRLOCK", p.Text);
        }
    }
}
