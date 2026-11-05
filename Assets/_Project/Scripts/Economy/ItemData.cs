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
        [Tooltip("Credits. Vendors multiply this with their sell/buy factor.")]
        [SerializeField] private int baseValue = 10;
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
    }
}
