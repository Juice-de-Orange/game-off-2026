using System.Collections.Generic;
using GameOff.Audio;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace GameOff.Tests
{
    /// <summary>
    /// P144: vendors and travelers talk with varied voices: a random murmur of their species, never the same one twice in a row, at
    /// the speaker's own pitch; until P94 is delivered, vendorGreet and the PA murmurs stand in.
    /// </summary>
    public class VoiceTests
    {
        private static readonly float[] Randoms = { 0f, 0.05f, 0.1f, 0.2f, 0.33f, 0.34f, 0.5f, 0.66f, 0.67f, 0.8f, 0.9f, 0.99f, 0.999f, 1f };

        private readonly List<Object> _made = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (var o in _made) Object.DestroyImmediate(o);
            _made.Clear();
        }

        private Sound Clip(string name)
        {
            var clip = AudioClip.Create(name, 441, 1, 44100, false);
            _made.Add(clip);
            return new Sound { clip = clip, volume = 0.8f };
        }

        private SoundSet Set()
        {
            var set = ScriptableObject.CreateInstance<SoundSet>();
            _made.Add(set);
            return set;
        }

        [Test]
        public void Next_NeverPicksTheLastOne_AndReachesAllOthers()
        {
            for (int count = 2; count <= 10; count++)
                for (int last = 0; last < count; last++)
                {
                    var seen = new HashSet<int>();
                    for (int r = 0; r <= 100; r++)
                    {
                        int index = VoicePicker.Next(count, last, r / 100f);
                        Assert.That(index, Is.InRange(0, count - 1), $"{count} sounds");
                        Assert.AreNotEqual(last, index, $"{count} sounds, random {r / 100f}");
                        seen.Add(index);
                    }
                    Assert.AreEqual(count - 1, seen.Count, $"{count} sounds: every other one can come");
                }
        }

        [Test]
        public void Next_TalkingFiveTimesInARow_NeverRepeats()
        {
            // The acceptance: five times in a row with the barkeeper, never the same murmur twice in a row (7 fallback sounds, 10 per species).
            foreach (int count in new[] { 7, 10, 2 })
            {
                int last = -1;
                foreach (float random in new[] { 0.5f, 0.5f, 0.5f, 0f, 0f, 0.99f, 0.99f, 1f })
                {
                    int index = VoicePicker.Next(count, last, random);
                    Assert.AreNotEqual(last, index, $"{count} sounds");
                    last = index;
                }
            }
        }

        [Test]
        public void Next_FirstPick_EdgeCases()
        {
            var seen = new HashSet<int>();
            foreach (float random in Randoms) seen.Add(VoicePicker.Next(4, -1, random));
            Assert.AreEqual(4, seen.Count, "nothing played yet: any of them");
            Assert.AreEqual(-1, VoicePicker.Next(0, -1, 0.5f), "no sounds");
            Assert.AreEqual(0, VoicePicker.Next(1, 0, 0.5f), "a single sound is all there is");
            Assert.That(VoicePicker.Next(3, 7, 0.99f), Is.InRange(0, 2), "a last index from a bigger set counts as none");
        }

        [Test]
        public void SpeakerPitch_IsStable_AndInRange()
        {
            foreach (var name in new[] { "Holly", "Barkeeper", "Dealer", "Vaal Korr", "Mira Talsk", "x", "A very long traveler name indeed" })
            {
                float pitch = VoicePicker.SpeakerPitch(name, 0.85f, 1.15f);
                Assert.That(pitch, Is.InRange(0.85f, 1.15f), name);
                Assert.AreEqual(pitch, VoicePicker.SpeakerPitch(name, 0.85f, 1.15f), name + ": the same every time");
            }
            Assert.AreEqual(1f, VoicePicker.SpeakerPitch(null, 0.85f, 1.15f), 1e-5f, "no name: the middle");
            Assert.AreEqual(1f, VoicePicker.SpeakerPitch("", 0.85f, 1.15f), 1e-5f);
        }

        [TestCase("Barkeeper", "Dealer")]
        [TestCase("Barkeeper", "Holly")]
        public void Vendors_SoundDifferent_EvenWithTheJitter(string one, string other)
        {
            // The acceptance: barkeeper and dealer, both at the bar, sound different (same fallback murmurs until P94 is delivered).
            // The names are Vendor.vendorName (EconomyBuilder) and Barkeeper.VoiceName; the range and jitter the SoundSet's defaults.
            var set = Set();
            float a = VoicePicker.SpeakerPitch(one, set.voicePitchMin, set.voicePitchMax);
            float b = VoicePicker.SpeakerPitch(other, set.voicePitchMin, set.voicePitchMax);
            float highestLow = Mathf.Max(a, b) * (1f - set.voicePitchJitter);
            float lowestHigh = Mathf.Min(a, b) * (1f + set.voicePitchJitter);
            Assert.Greater(highestLow, lowestHigh, $"{one} {a:0.000} and {other} {b:0.000} overlap");
        }

        [Test]
        public void Utterance_VariesTheSpeakersPitchByTheJitter()
        {
            Assert.AreEqual(0.95f, VoicePicker.Utterance(0.95f, 0.02f, 0.5f), 1e-5f);
            Assert.AreEqual(0.95f * 0.98f, VoicePicker.Utterance(0.95f, 0.02f, 0f), 1e-5f);
            Assert.AreEqual(0.95f * 1.02f, VoicePicker.Utterance(0.95f, 0.02f, 1f), 1e-5f);
            Assert.AreEqual(1.1f, VoicePicker.Utterance(1.1f, 0f, 0.9f), 1e-5f, "no jitter");
        }

        [Test]
        public void VoicesOf_TheSpeciesSet_OnlyOnceDelivered()
        {
            var set = Set();
            var vellani = new[] { Clip("V1"), Clip("V2") };
            set.voices = new[]
            {
                new SpeciesVoice { species = "Human", sounds = new Sound[0] },
                new SpeciesVoice { species = "Vellani", sounds = vellani },
            };
            Assert.AreSame(vellani, set.VoicesOf("Vellani"));
            Assert.AreSame(vellani, set.VoicesOf("vellani"), "case doesn't matter");
            Assert.IsNull(set.VoicesOf("Human"), "not delivered yet");
            Assert.IsNull(set.VoicesOf("Quobb"), "no set");
            Assert.IsNull(set.VoicesOf(null));
            set.voices = null;
            Assert.IsNull(set.VoicesOf("Vellani"));
        }

        [Test]
        public void FallbackVoices_AreVendorGreetAndThePaMurmurs()
        {
            var set = Set();
            Assert.IsEmpty(set.FallbackVoices(), "nothing with a clip");
            set.vendorGreet = Clip("Greet");
            var murmur = Clip("Murmur");
            set.paMurmurs = new[] { murmur, new Sound(), null };
            CollectionAssert.AreEqual(new[] { set.vendorGreet, murmur }, set.FallbackVoices(), "only those with a clip");
        }

        [Test]
        public void SoundSet_HasAVoiceSetPerSpecies()
        {
            var set = AssetDatabase.LoadAssetAtPath<SoundSet>("Assets/_Project/Data/Audio/SoundSet.asset");
            Assert.IsTrue(set != null, "no SoundSet: run GameOff/Build Sound Set");
            Assert.IsNotNull(set.voices, "run GameOff/Build Sound Set");
            var species = new List<string>();
            foreach (var v in set.voices)
            {
                species.Add(v.species);
                foreach (var s in v.sounds) Assert.IsTrue(s.clip != null, v.species + " voice");
            }
            CollectionAssert.AreEquivalent(new[] { "Human", "Vellani", "Quobb", "Sorvak", "Xirrik" }, species, "run GameOff/Build Sound Set");
            Assert.Less(set.voicePitchMin, set.voicePitchMax);
            Assert.IsNotEmpty(set.FallbackVoices(), "vendorGreet and the PA murmurs stand in until P94 is delivered");
        }
    }
}
