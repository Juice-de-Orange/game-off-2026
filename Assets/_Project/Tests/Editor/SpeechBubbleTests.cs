using GameOff.Counter;
using GameOff.EditorTools;
using GameOff.Travelers;
using NUnit.Framework;
using UnityEngine;

namespace GameOff.Tests
{
    /// <summary>P143: where the speech bubble and its tail go, and that Korr only has his last words left after the decision.</summary>
    public class SpeechBubbleTests
    {
        private const float Eps = 1e-4f;
        private static readonly Vector2 Size = new Vector2(0.33f, 0.2f); // 320 x 108 units on the 960 x 540 HUD
        private const float Tail = 0.04f;

        [Test]
        public void Bubble_StandsRightOfTheSpeaker_WhenThereIsRoom()
        {
            var speaker = new Vector2(0.3f, 0.5f);
            bool left = BubbleLayout.LeftOfSpeaker(speaker.x, Size.x);
            Assert.IsFalse(left);
            Assert.AreEqual(speaker.x + BubbleLayout.BesideHead, BubbleLayout.TopLeft(speaker, Size, Tail, left).x, Eps);
        }

        [Test]
        public void Bubble_GoesLeft_NearTheRightEdge_AndDoesNotCoverTheSpeaker()
        {
            var speaker = new Vector2(0.85f, 0.5f);
            bool left = BubbleLayout.LeftOfSpeaker(speaker.x, Size.x);
            Assert.IsTrue(left);
            var corner = BubbleLayout.TopLeft(speaker, Size, Tail, left);
            Assert.LessOrEqual(corner.x + Size.x, speaker.x - BubbleLayout.BesideHead + Eps);
        }

        [Test]
        public void Bubble_KeepsItsSide_WhileItFits()
        {
            Assert.IsTrue(BubbleLayout.LeftOfSpeaker(0.5f, Size.x, wasLeft: true), "room on both sides: stays left");
            Assert.IsFalse(BubbleLayout.LeftOfSpeaker(0.5f, Size.x, wasLeft: false), "room on both sides: stays right");
            Assert.IsFalse(BubbleLayout.LeftOfSpeaker(0.2f, Size.x, wasLeft: true), "no room left any more: goes right");
        }

        [Test]
        public void Bubble_NeverLeavesTheView()
        {
            for (float x = -0.1f; x <= 1.1f; x += 0.05f)
                for (float y = -0.1f; y <= 1.1f; y += 0.05f)
                {
                    var speaker = new Vector2(x, y);
                    bool left = BubbleLayout.LeftOfSpeaker(x, Size.x);
                    var corner = BubbleLayout.TopLeft(speaker, Size, Tail, left);
                    string at = $"speaker {x:0.00}/{y:0.00}";
                    Assert.GreaterOrEqual(corner.x, BubbleLayout.Margin - Eps, at);
                    Assert.LessOrEqual(corner.x + Size.x, 1f - BubbleLayout.Margin + Eps, at);
                    Assert.LessOrEqual(corner.y, BubbleLayout.TopLimit + Eps, at);
                    Assert.GreaterOrEqual(corner.y - Size.y - Tail, BubbleLayout.Margin - Eps, at + ": the tail stays in view too");
                }
        }

        [Test]
        public void Tail_TipAtTheHead_WhenThereIsRoomAbove()
        {
            var speaker = new Vector2(0.3f, 0.4f);
            var corner = BubbleLayout.TopLeft(speaker, Size, Tail, false);
            Assert.AreEqual(speaker.y, corner.y - Size.y - Tail, Eps);
        }

        [Test]
        public void Bubble_UnderTheTopOfTheView_WhenTheHeadIsHigh()
        {
            var corner = BubbleLayout.TopLeft(new Vector2(0.3f, 0.9f), Size, Tail, false);
            Assert.AreEqual(BubbleLayout.TopLimit, corner.y, Eps);
        }

        [Test]
        public void Tail_SitsOnTheSpeakersSide_BetweenTheCorners()
        {
            const float width = 320f, tail = 32f, inset = 16f;
            float min = inset + tail / 2f, max = width - inset - tail / 2f;
            Assert.AreEqual(min, BubbleLayout.TailCentre(-60f, width, tail, inset, false), Eps, "speaker left of the bubble");
            Assert.AreEqual(max, BubbleLayout.TailCentre(width + 60f, width, tail, inset, true), Eps, "speaker right of the bubble, tail mirrored");
            Assert.AreEqual(100f + tail / 2f, BubbleLayout.TailCentre(100f, width, tail, inset, false), Eps, "tip (left end) right at the speaker");
            Assert.AreEqual(100f - tail / 2f, BubbleLayout.TailCentre(100f, width, tail, inset, true), Eps, "mirrored: tip (right end) at the speaker");
        }

        [Test]
        public void Tail_NarrowBubble_TailInTheMiddle() => Assert.AreEqual(20f, BubbleLayout.TailCentre(0f, 40f, 32f, 16f, false), Eps);

        [Test]
        public void TerminalSprites_BubbleIsSliced_TailIsNot()
        {
            Assert.GreaterOrEqual(TerminalStyle.BorderOf("UI_SpeechBubble"), 16);
            Assert.AreEqual(0, TerminalStyle.BorderOf("UI_SpeechBubble_Tail"));
        }

        /// <summary>#46: after the stamp Korr says one closing line as he leaves; the board has closed and gives no instructions.</summary>
        [Test]
        public void Korr_OnlyHisLastWordsAfterTheDecision()
        {
            Assert.IsTrue(StoryDayContent.Reactions.TryGetValue("Korr_1", out var words), "Korr_1 has own last words");
            var korr = ScriptableObject.CreateInstance<TravelerData>();
            var tuning = ScriptableObject.CreateInstance<DialogueTuning>();
            try
            {
                korr.EditorSet("VAAL KORR", "Sorvak", new System.Collections.Generic.List<GameOff.Documents.DocumentData>(), Verdict.Admit, "", "", "", null, null);
                korr.EditorSetReactions(words.admitted, words.denied);
                Assert.AreEqual(words.admitted, TravelerDialogue.Reaction(korr, Verdict.Admit, tuning, 0.5f));
                Assert.AreEqual(words.denied, TravelerDialogue.Reaction(korr, Verdict.Deny, tuning, 0.5f));
                Assert.IsNull(TravelerDialogue.Reaction(korr, Verdict.Arrest, tuning, 0.5f), "arrested: nothing more");
                Assert.IsNull(TravelerDialogue.Reaction(korr, Verdict.Airlock, tuning, 0.5f), "airlock: nothing more");
                foreach (string line in new[] { words.admitted, words.denied })
                    foreach (string instruction in new[] { "ARREST", "AIRLOCK", "drawer", "stamp", "click" })
                        StringAssert.DoesNotContain(instruction, line);
            }
            finally
            {
                Object.DestroyImmediate(korr);
                Object.DestroyImmediate(tuning);
            }
        }
    }
}
