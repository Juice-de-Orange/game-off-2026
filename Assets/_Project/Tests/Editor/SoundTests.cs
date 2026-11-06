using System.Collections.Generic;
using GameOff.Audio;
using GameOff.Player;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace GameOff.Tests
{
    public class SoundTests
    {
        [Test]
        public void SoundSet_HasEveryClip()
        {
            var set = AssetDatabase.LoadAssetAtPath<SoundSet>("Assets/_Project/Data/Audio/SoundSet.asset");
            Assert.IsTrue(set != null, "no SoundSet: run GameOff/Build Sound Set");
            foreach (var field in typeof(SoundSet).GetFields())
            {
                if (field.GetValue(set) is Sound sound) Assert.IsTrue(sound.clip != null, field.Name);
            }
            Assert.AreEqual(3, set.footsteps.Length);
            foreach (var step in set.footsteps) Assert.IsTrue(step.clip != null, "footstep");
        }

        [Test]
        public void Footsteps_SoundEveryStepLength()
        {
            var steps = new Footsteps();
            Assert.IsFalse(steps.Advance(1f, 1.6f));
            Assert.IsTrue(steps.Advance(0.7f, 1.6f));
            Assert.IsFalse(steps.Advance(0.1f, 1.6f), "the rest counts toward the next step");
            Assert.IsFalse(steps.Advance(1.3f, 1.6f));
            Assert.IsTrue(steps.Advance(0.2f, 1.6f));
        }

        [Test]
        public void Footsteps_NeverTheSameClipTwice()
        {
            var steps = new Footsteps();
            int last = -1;
            var seen = new HashSet<int>();
            foreach (float random in new[] { 0f, 0f, 0.99f, 0.99f, 0.5f, 0.5f, 0.1f, 0.9f, 0.34f, 0.67f, 1f })
            {
                int index = steps.NextIndex(3, random);
                Assert.That(index, Is.InRange(0, 2));
                Assert.AreNotEqual(last, index);
                last = index;
                seen.Add(index);
            }
            Assert.AreEqual(3, seen.Count);
        }

        [Test]
        public void Footsteps_OneClipAlwaysFirst()
        {
            var steps = new Footsteps();
            Assert.AreEqual(0, steps.NextIndex(1, 0.5f));
            Assert.AreEqual(0, steps.NextIndex(1, 0.5f));
        }

        [Test]
        public void AmbienceZones_RoomSoundOnlyInItsRoom()
        {
            var bar = new Bounds(new Vector3(13f, 2f, 4f), new Vector3(10f, 6f, 8f));
            var hangar = new Bounds(new Vector3(0f, 8f, -26f), new Vector3(36f, 18f, 28f));
            Assert.AreEqual((0.35f, 0f, 0f), AmbienceZones.Targets(Vector3.zero, bar, hangar, 0.35f, 0.12f));
            Assert.AreEqual((0.12f, 0.35f, 0f), AmbienceZones.Targets(new Vector3(13f, 1f, 4f), bar, hangar, 0.35f, 0.12f));
            Assert.AreEqual((0.12f, 0f, 0.35f), AmbienceZones.Targets(new Vector3(0f, 1f, -20f), bar, hangar, 0.35f, 0.12f));
        }
    }
}
