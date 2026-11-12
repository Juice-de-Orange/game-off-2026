using GameOff.Core;
using GameOff.UI;
using NUnit.Framework;
using UnityEngine;

namespace GameOff.Tests
{
    /// <summary>P138: which pointer shows, and that catching the mouse never jerks the camera (first movement dropped).</summary>
    public class MousePointerTests
    {
        private static readonly Vector2 Move = new Vector2(3f, -2f);

        [Test]
        public void Pick_HiddenWhileCaptured_HandOverClickables_ArrowElse()
        {
            Assert.AreEqual(CursorLook.Hidden, GameCursor.Pick(false, true, true));
            Assert.AreEqual(CursorLook.Normal, GameCursor.Pick(true, false, false));
            Assert.AreEqual(CursorLook.Hover, GameCursor.Pick(true, true, false), "a button");
            Assert.AreEqual(CursorLook.Hover, GameCursor.Pick(true, false, true), "a paper or stamp at the counter");
        }

        [Test]
        public void LookSettle_DropsTheFirstFramesOfLooking()
        {
            var settle = new LookSettle();
            for (int i = 0; i < LookSettle.SettleFrames; i++)
                Assert.AreEqual(Vector2.zero, settle.Filter(10 + i, true, Move), "frame " + i);
            Assert.AreEqual(Move, settle.Filter(10 + LookSettle.SettleFrames, true, Move));
            Assert.AreEqual(Move, settle.Filter(10 + LookSettle.SettleFrames, true, Move), "a second reader in the same frame");
        }

        [Test]
        public void LookSettle_DropsAgainAfterTheLockChanged()
        {
            var settle = new LookSettle();
            for (int f = 0; f < 5; f++) settle.Filter(f, true, Move);
            Assert.AreEqual(Move, settle.Filter(5, true, Move));
            settle.Filter(6, false, Move); // Esc: the browser let go
            Assert.AreEqual(Vector2.zero, settle.Filter(7, true, Move), "caught again: the jump is dropped");
            Assert.AreEqual(Vector2.zero, settle.Filter(8, true, Move));
            Assert.AreEqual(Move, settle.Filter(9, true, Move));
        }

        [Test]
        public void LookSettle_DropsAfterNobodyLookedForAWhile()
        {
            var settle = new LookSettle();
            for (int f = 0; f < 5; f++) settle.Filter(f, true, Move);
            // a menu or the counter: no looking for some frames, the lock may have come and gone meanwhile
            Assert.AreEqual(Vector2.zero, settle.Filter(40, true, Move));
            Assert.AreEqual(Vector2.zero, settle.Filter(41, true, Move));
            Assert.AreEqual(Move, settle.Filter(42, true, Move));
        }
    }
}
