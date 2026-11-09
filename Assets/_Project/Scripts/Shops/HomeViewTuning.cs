using UnityEngine;

namespace GameOff.Shops
{
    /// <summary>
    /// P111: how the coin telescope jumps from the home planet to the home street diorama, and how the family's three windows glow.
    /// </summary>
    [CreateAssetMenu(menuName = "GameOff/Shops/Home View Tuning", fileName = "HomeViewTuning")]
    public class HomeViewTuning : ScriptableObject
    {
        [Header("Jump to the street")]
        [Tooltip("Zoomed in on the planet to this field of view (degrees) or closer, the view jumps to the street. The planet zoom stops at 3.")]
        public float enterFov = 4f;
        [Tooltip("The jump only happens while the view is at most this many degrees off the planet's centre (the planet is about 18 degrees wide in radius).")]
        public float aimTolerance = 14f;
        [Tooltip("Seconds of the focus blink (fade to black and back) when switching between planet and street.")]
        public float blinkSeconds = 0.4f;

        [Header("Street view")]
        [Tooltip("Field of view when the street appears; zooming out past it jumps back to the planet.")]
        public float streetWideFov = 22f;
        [Tooltip("Closest zoom on the house.")]
        public float streetNarrowFov = 7f;
        [Tooltip("How far the view may pan away from the house, in degrees.")]
        public float streetPanLimit = 9f;

        [Header("Windows (Mom, Lena, Pip)")]
        [Tooltip("Colour of a lit window at full brightness.")]
        public Color windowColor = new Color(1f, 0.78f, 0.42f);
        [Tooltip("Colour of a window with nobody behind it any more.")]
        public Color darkColor = new Color(0.015f, 0.015f, 0.02f);
        [Tooltip("Brightness while the person is well or weak.")]
        public float brightLevel = 1f;
        [Tooltip("Brightness while the person is sick: a dim lamp.")]
        public float dimLevel = 0.28f;
        [Tooltip("Brightness while critical, before the flicker.")]
        public float criticalLevel = 0.8f;
        [Tooltip("Lowest point of the flicker while critical (share of criticalLevel).")]
        [Range(0f, 1f)] public float flickerFloor = 0.08f;
        [Tooltip("How fast a critical window flickers.")]
        public float flickerSpeed = 4f;
    }
}
