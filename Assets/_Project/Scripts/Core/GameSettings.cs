using UnityEngine;

namespace GameOff.Core
{
    /// <summary>
    /// Player options from the options menu (P02, tabs in P43), saved in PlayerPrefs
    /// (in WebGL that is the browser's IndexedDB, so they survive a reload).
    /// Fullscreen is not stored: browsers only allow it after a click, so FullscreenButton toggles it live via Screen.fullScreen.
    /// </summary>
    public static class GameSettings
    {
        private const string VolumeKey = "settings.volume";
        private const string SensitivityKey = "settings.mouseSensitivity";
        private const string InvertYKey = "settings.invertY";

        public const float DefaultVolume = 0.8f;
        public const float DefaultSensitivity = 1f;
        public const bool DefaultInvertY = false;

        public const float MinSensitivity = 0.25f;
        public const float MaxSensitivity = 3f;

        /// <summary>Master volume 0..1, applied to the AudioListener.</summary>
        public static float Volume
        {
            get => PlayerPrefs.GetFloat(VolumeKey, DefaultVolume);
            set
            {
                PlayerPrefs.SetFloat(VolumeKey, Mathf.Clamp01(value));
                AudioListener.volume = Volume;
            }
        }

        /// <summary>Multiplier on PlayerTuning.mouseSensitivity (1 = tuned default).</summary>
        public static float MouseSensitivity
        {
            get => PlayerPrefs.GetFloat(SensitivityKey, DefaultSensitivity);
            set => PlayerPrefs.SetFloat(SensitivityKey, Mathf.Clamp(value, MinSensitivity, MaxSensitivity));
        }

        /// <summary>Mouse up looks down (flight-sim style).</summary>
        public static bool InvertY
        {
            get => PlayerPrefs.GetInt(InvertYKey, DefaultInvertY ? 1 : 0) == 1;
            set => PlayerPrefs.SetInt(InvertYKey, value ? 1 : 0);
        }

        public static void ResetAudio() => Volume = DefaultVolume;

        public static void ResetControls()
        {
            MouseSensitivity = DefaultSensitivity;
            InvertY = DefaultInvertY;
        }

        public static void Save() => PlayerPrefs.Save();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void ApplyOnStart() => AudioListener.volume = Volume;
    }
}
