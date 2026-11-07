using System.Collections.Generic;
using UnityEngine;

namespace GameOff.Economy
{
    /// <summary>
    /// Numbers of the nightly settlement (P10): wage, penalty, rent, the family's needs and the family itself.
    /// All values are provisional – balancing happens here, not in code.
    /// </summary>
    [CreateAssetMenu(menuName = "GameOff/Economy/Household Tuning", fileName = "Household")]
    public class HouseholdTuning : ScriptableObject
    {
        [Header("Income")]
        [Tooltip("Paid per correctly decided traveler (P70); wrong decisions bring nothing.")]
        public int wagePerTraveler = 15;
        [Tooltip("Deducted for every citation beyond the free warnings of the day (CounterShift.FreeWarnings).")]
        public int penaltyPerCitation = 10;

        [Header("Goal (P71/P72)")]
        [Tooltip("The ticket home – the goal of the endless game. Set by balancing (GameOff → Economy Report): about 7 days of good work.")]
        public int ticketPrice = 350;

        [Header("Costs per night")]
        [Tooltip("Room 103 – always paid, may push the balance below zero.")]
        public int rent = 30;

        [Header("Needs (P13): a delivered item with the tag covers the need; otherwise money at the bed")]
        [Tooltip("Sending money at the bed costs the reference item's value × this (vendors are cheaper).")]
        public float moneyFactor = 1.35f;
        public ItemTag foodTag;
        public ItemData foodPriceItem;
        [Tooltip("Heating: the family needs power for the heater.")]
        public ItemTag heatTag;
        public ItemData heatPriceItem;
        [Tooltip("Only needed while someone is sick; heals every sick person by one step.")]
        public ItemTag medicineTag;
        public ItemData medicinePriceItem;

        public ItemTag TagOf(FamilyNeed need) => need == FamilyNeed.Food ? foodTag : need == FamilyNeed.Heat ? heatTag : medicineTag;
        public ItemData PriceItemOf(FamilyNeed need) =>
            need == FamilyNeed.Food ? foodPriceItem : need == FamilyNeed.Heat ? heatPriceItem : medicinePriceItem;

        /// <summary>Money sent home for a need at the bed: reference item value × moneyFactor, rounded.</summary>
        public int MoneyCost(FamilyNeed need) => MoneyCost(PriceItemOf(need), moneyFactor);

        public static int MoneyCost(ItemData priceItem, float factor) =>
            priceItem == null ? 0 : Market.Rounded(Market.PriceOf(priceItem) * factor);

        [Header("Family")]
        public List<FamilyMember> members = new List<FamilyMember>();
        [Tooltip("Condition names, from healthy to critical (index = Household condition).")]
        public string[] conditionNames = { "well", "weak", "sick", "critical" };
        [Tooltip("Lines for the morning letter per condition; {0} = name.")]
        public string[] letterLines =
        {
            "{0} is doing fine.",
            "{0} is weak and tired.",
            "{0} is sick and needs medicine.",
            "{0} is in a very bad way.",
        };
    }
}
