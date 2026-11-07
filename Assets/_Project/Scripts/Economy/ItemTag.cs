using UnityEngine;

namespace GameOff.Economy
{
    /// <summary>Label on items (P46), e.g. CONTRABAND, MEDICINE, GADGET. Shown as a colored chip; vendors can refuse labels.</summary>
    [CreateAssetMenu(menuName = "GameOff/Economy/Item Tag", fileName = "Tag_")]
    public class ItemTag : ScriptableObject
    {
        [SerializeField] private string displayName = "TAG";
        [SerializeField] private Color color = new Color(0.48f, 0.12f, 0.17f);
        [Tooltip("Base price factor of this label (P68), every day: e.g. 1.3 for CONTRABAND. Items with several labels multiply all of them.")]
        [SerializeField] private float priceFactor = 1f;

        public string DisplayName => displayName;
        public Color Color => color;
        public float PriceFactor => priceFactor;
    }
}
