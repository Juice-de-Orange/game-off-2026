using UnityEngine;

namespace GameOff.Core
{
    /// <summary>
    /// Player options from the options menu (volume, mouse sensitivity), saved in PlayerPrefs
    /// (in WebGL that is the browser's IndexedDB, so they survive a reload).
    /// </summary>
    public static class GameSettings
    {
        private const string VolumeKey = "settings.volume";
        private const string SensitivityKey = "settings.mouseSensitivity";

        public const float MinSensitivity = 0.25f;
        public const float MaxSensitivity = 3f;

        /// <summary>Master volume 0..1, applied to the AudioListener.</summary>
        public static float Volume
        {
            get => PlayerPrefs.GetFloat(VolumeKey, 0.8f);
            set
            {
                PlayerPrefs.SetFloat(VolumeKey, Mathf.Clamp01(value));
                AudioListener.volume = Volume;
            }
        }

        /// <summary>Multiplier on PlayerTuning.mouseSensitivity (1 = tuned default).</summary>
        public static float MouseSensitivity
        {
            get => PlayerPrefs.GetFloat(SensitivityKey, 1f);
            set => PlayerPrefs.SetFloat(SensitivityKey, Mathf.Clamp(value, MinSensitivity, MaxSensitivity));
        }

        public static void Save() => PlayerPrefs.Save();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void ApplyOnStart() => AudioListener.volume = Volume;
    }
}
