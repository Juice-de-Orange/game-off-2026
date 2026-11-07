using System;

namespace GameOff.Economy
{
    /// <summary>What a market event does to every item with one label (P68). 1 = no change.</summary>
    [Serializable]
    public class LabelEffect
    {
        public ItemTag tag;
        [UnityEngine.Tooltip("Market price factor, e.g. 1.3 = 30 % more expensive (vendors and the bed).")]
        public float price = 1f;
        [UnityEngine.Tooltip("Daily amount on the shelves, e.g. 0.5 = half (rounded up), 0 = not available today.")]
        public float amount = 1f;
        [UnityEngine.Tooltip("What vendors pay you, on top of their buy factor.")]
        public float buy = 1f;
    }
}
