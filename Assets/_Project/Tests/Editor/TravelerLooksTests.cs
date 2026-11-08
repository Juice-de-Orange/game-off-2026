using GameOff.Travelers;
using NUnit.Framework;

namespace GameOff.Tests
{
    /// <summary>P108: a name always gets the same look, and only humans get hair.</summary>
    public class TravelerLooksTests
    {
        [Test]
        public void SameNameSameLook()
        {
            var a = TravelerLooks.For("VAAL KORR", "Sorvak", 6, 5);
            var b = TravelerLooks.For("VAAL KORR", "Sorvak", 6, 5);
            Assert.AreEqual(a.Skin, b.Skin);
            Assert.AreEqual(a.Hair, b.Hair);
        }

        [Test]
        public void OnlyHumansHaveHair()
        {
            Assert.AreEqual(0, TravelerLooks.For("KISSA VEL", "Xirrik", 6, 5).Hair);
            bool anyHair = false;
            foreach (string n in new[] { "MAKO DRENN", "IRIN CALDE", "NESTOR KAI", "HEDDA QUINN" })
                anyHair |= TravelerLooks.For(n, "Human", 6, 5).Hair > 0;
            Assert.IsTrue(anyHair);
        }

        [Test]
        public void SkinStaysInRange()
        {
            for (int i = 0; i < 200; i++)
                Assert.Less(TravelerLooks.For("Name" + i, "Quobb", 6, 5).Skin, 6);
        }
    }
}
