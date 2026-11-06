using System.Collections.Generic;
using UnityEngine;

namespace GameOff.Economy
{
    /// <summary>An item type (P46). The inventory stacks items of the same type; base value × vendor factor = price.</summary>
    [CreateAssetMenu(menuName = "GameOff/Economy/Item", fileName = "Item_")]
    public class ItemData : ScriptableObject
    {
        [SerializeField] private string displayName = "Item";
        [TextArea(2, 5)] [SerializeField] private string description;
        [Tooltip("Base market price in credits. Market price = this × the day's change (P70) × label/event factors (P68); vendors multiply with their sell/buy factor.")]
        [SerializeField] private int baseValue = 10;
        [Tooltip("Fixed price change per day (P70): element 0 = day 1, e.g. 0.25 = +25 %. Missing days = no change.")]
        [SerializeField] private float[] dailyChange = new float[0];
        [SerializeField] private List<ItemTag> tags = new List<ItemTag>();
        [Tooltip("Can be owned only once (gadgets like the scanner, story items). Vendors don't sell it again while you have it.")]
        [SerializeField] private bool unique;
        [Tooltip("Optional, for later (the lists show names only for now).")]
        [SerializeField] private Sprite icon;

        public string DisplayName => displayName;
        public string Description => description;
        public int BaseValue => baseValue;
        public IReadOnlyList<ItemTag> Tags => tags;
        public bool Unique => unique;
        public Sprite Icon => icon;

        public bool HasTag(ItemTag tag) => tag != null && tags.Contains(tag);

        /// <summary>Price change on day n (1-based), e.g. 0.25 for +25 %; 0 where nothing is set.</summary>
        public float DailyChange(int day) => dailyChange != null && day >= 1 && day <= dailyChange.Length ? dailyChange[day - 1] : 0f;
    }
}
