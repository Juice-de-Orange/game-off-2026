using UnityEngine;

namespace GameOff.Core
{
    /// <summary>Tuning for the low-resolution retro rendering (docs/STYLE.md → Rendering).</summary>
    [CreateAssetMenu(fileName = "RetroLookSettings", menuName = "GameOff/Retro Look Settings")]
    public class RetroLookSettings : ScriptableObject
    {
        [Tooltip("Retro look active when the game starts.")]
        public bool enabledByDefault = true;
        [Tooltip("Internal render height in pixels. 360 = 640x360 at 16:9.")]
        [Min(90)] public int targetHeight = 360;
    }
}
