using UnityEngine;

namespace GameOff.Economy
{
    /// <summary>Numbers for money and inventory (P46). Balancing happens here, not in code.</summary>
    [CreateAssetMenu(menuName = "GameOff/Economy/Economy Tuning", fileName = "EconomyTuning")]
    public class EconomyTuning : ScriptableObject
    {
        public int startCredits = 40;
        [Tooltip("Shown after amounts, e.g. \"115 CR\".")]
        public string currency = "CR";

        public string Format(int amount) => $"{amount} {currency}";
    }
}
