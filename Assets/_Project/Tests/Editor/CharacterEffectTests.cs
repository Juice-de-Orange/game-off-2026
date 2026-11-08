using GameOff.Characters;
using GameOff.Economy;
using NUnit.Framework;
using UnityEngine;

namespace GameOff.Tests
{
    /// <summary>P77 and P85: what a character left with the player, and effects that arrive on a later day.</summary>
    public class CharacterEffectTests
    {
        [Test]
        public void Entrusted_IsTakenOnce()
        {
            var memory = new CharacterMemory();
            var c = ScriptableObject.CreateInstance<RecurringCharacter>();
            var item = ScriptableObject.CreateInstance<ItemData>();
            memory.Entrust(c, item, 20);
            Assert.IsTrue(memory.TakeEntrusted(c, out var got, out int deposit));
            Assert.AreSame(item, got);
            Assert.AreEqual(20, deposit);
            Assert.IsFalse(memory.TakeEntrusted(c, out _, out _));
            Object.DestroyImmediate(c);
            Object.DestroyImmediate(item);
        }

        [Test]
        public void Effects_ComeOutOnTheirDayOnly()
        {
            var memory = new CharacterMemory();
            var effect = new Consequence { wageChange = 25 };
            memory.ScheduleEffect(6, effect);
            Assert.IsEmpty(memory.TakeEffects(5));
            CollectionAssert.AreEqual(new[] { effect }, memory.TakeEffects(6));
            Assert.IsEmpty(memory.TakeEffects(6));
        }
    }
}
