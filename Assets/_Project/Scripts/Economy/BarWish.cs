using System;

namespace GameOff.Economy
{
    /// <summary>What the barkeeper wants today (P65): any items with this label. Amount 0 = no wish today.</summary>
    [Serializable]
    public class BarWish
    {
        public ItemTag tag;
        [UnityEngine.Min(0)] public int amount;
        [UnityEngine.Tooltip("Text on the sign at the bar; empty = made from amount and label, e.g. \"2× LUXURY\".")]
        public string sign;

        public bool Active => tag != null && amount > 0;
        public string Text => !string.IsNullOrWhiteSpace(sign) ? sign : Active ? $"{amount}× {tag.DisplayName}" : "";
    }
}
