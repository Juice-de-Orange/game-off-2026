using System.Linq;
using GameOff.Counter;
using GameOff.Documents;
using GameOff.EditorTools;
using GameOff.Travelers;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace GameOff.Tests
{
    /// <summary>The traveler in the lock (P40): alien text, answers about document fields, animated models for all content.</summary>
    public class TravelerPresentationTests
    {
        private const string Plain = "Papers, officer 7.";

        [Test]
        public void Scramble_KeepsLengthAndSpacing_AndHidesTheLetters()
        {
            string glyphs = AlienText.Scramble(Plain);
            Assert.AreEqual(Plain.Length, glyphs.Length);
            Assert.AreEqual(' ', glyphs[7]);
            Assert.AreEqual(',', glyphs[6]);
            Assert.IsFalse(glyphs.Any(c => c >= 'A' && c <= 'z'), glyphs);
            Assert.IsFalse(glyphs.Contains('7'), glyphs);
        }

        [Test]
        public void Scramble_SameLetterSameGlyph() => Assert.AreEqual(AlienText.Scramble("ab Ab"), AlienText.Scramble("AB AB"));

        [Test]
        public void Reveal_GoesFromAllGlyphsToPlainText()
        {
            string start = AlienText.Reveal(Plain, 0, 0);
            StringAssert.Contains(AlienText.Scramble(Plain), start);
            Assert.AreEqual(Plain, AlienText.Reveal(Plain, Plain.Length, 3));
            StringAssert.StartsWith("Pap", AlienText.Reveal(Plain, 3, 2));
            StringAssert.Contains(AlienText.Scramble(Plain.Substring(5)), AlienText.Reveal(Plain, 3, 2));
        }

        [Test]
        public void Reveal_OutOfRangeIsSafe()
        {
            Assert.AreEqual(Plain, AlienText.Reveal(Plain, Plain.Length + 5, 3));
            Assert.DoesNotThrow(() => AlienText.Reveal(Plain, -2, 100));
            Assert.AreEqual("", AlienText.Reveal("", 0, 3));
        }

        [Test]
        public void Answer_AuthoredWinsOverGeneric()
        {
            var traveler = ScriptableObject.CreateInstance<TravelerData>();
            var document = DocumentData.CreateRuntime(DocumentType.IdCard, new System.Collections.Generic.List<DocumentField> { new DocumentField("name", "VESNA") });
            try
            {
                StringAssert.Contains("VESNA", TravelerDialogue.Answer(traveler, document, "name"));
                traveler.EditorSet("V", "Human", new System.Collections.Generic.List<DocumentData>(), Verdict.Admit, "", "", "",
                    new System.Collections.Generic.List<FieldAnswer> { new FieldAnswer("name", "Who's asking?") });
                Assert.AreEqual("Who's asking?", TravelerDialogue.Answer(traveler, document, "name"));
            }
            finally
            {
                Object.DestroyImmediate(traveler);
                Object.DestroyImmediate(document);
            }
        }

        [Test]
        public void Answer_NeverEmpty_EvenForAnEmptyField()
        {
            var traveler = ScriptableObject.CreateInstance<TravelerData>();
            try
            {
                Assert.IsNotEmpty(TravelerDialogue.Answer(traveler, null, "name"));
            }
            finally
            {
                Object.DestroyImmediate(traveler);
            }
        }

        [Test]
        public void AllTravelersInProject_HaveAnAnimatedModelAndAGreeting()
        {
            int count = 0;
            foreach (string guid in AssetDatabase.FindAssets("t:TravelerData"))
            {
                var traveler = AssetDatabase.LoadAssetAtPath<TravelerData>(AssetDatabase.GUIDToAssetPath(guid));
                count++;
                Assert.IsNotNull(traveler.Model, traveler.name);
                Assert.IsNotEmpty(traveler.Greeting, traveler.name);
                var animator = traveler.Model.GetComponent<Animator>();
                Assert.IsNotNull(animator, traveler.name);
                var controller = animator.runtimeAnimatorController as AnimatorController;
                Assert.IsNotNull(controller, traveler.name);
                var states = controller.layers[0].stateMachine.states;
                foreach (string state in TravelerPrefabBuilder.States)
                    Assert.IsTrue(states.Any(s => s.state.name == state && s.state.motion != null), $"{traveler.name}: state {state}");
            }
            Assert.Greater(count, 0, "no travelers found");
        }
    }
}
