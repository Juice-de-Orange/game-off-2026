using GameOff.UI;
using NUnit.Framework;

namespace GameOff.Tests
{
    /// <summary>When Esc / P / a lost pointer lock may pause; P149: also in the flight home.</summary>
    public class PauseMenuTests
    {
        [Test]
        public void WalkingPauses() => Assert.IsTrue(PauseMenu.MayPause(true, false, false, false));

        [Test]
        public void NotWhileAnotherSystemHasThePlayer()
        {
            Assert.IsFalse(PauseMenu.MayPause(false, false, false, false), "a window, a seat, the telescope");
            Assert.IsFalse(PauseMenu.MayPause(false, true, false, false), "at the counter");
            Assert.IsFalse(PauseMenu.MayPause(true, true, false, false), "seated wins");
        }

        [Test]
        public void FlightHomePauses() => Assert.IsTrue(PauseMenu.MayPause(false, false, true, false), "the flight keeps the player's control off");

        [Test]
        public void NotTheEscThatClosesTheHelp()
        {
            Assert.IsFalse(PauseMenu.MayPause(false, false, true, true), "flying with [H] open");
            Assert.IsFalse(PauseMenu.MayPause(true, false, false, true));
        }

        [Test]
        public void NotAfterTheFlightIsOver() => Assert.IsFalse(PauseMenu.MayPause(false, false, false, false), "end screen: no flight, no control");
    }
}
