using System;
using UnityEngine;

namespace GameOff.Core
{
    /// <summary>
    /// Player options from the options menu (P02, tabs in P43, audio channels in P18), saved in PlayerPrefs
    /// (in WebGL that is the browser's IndexedDB, so they survive a reload). Volumes are 0..1; AudioManager turns them
    /// into decibels on GameMixer whenever <see cref="Changed"/> fires.
    /// Fullscreen is not stored: browsers only allow it after a click, so FullscreenButton toggles it live via Screen.fullScreen.
    /// </summary>
    public static class GameSettings
    {
        private const string VolumeKey = "settings.volume";
        private const string MusicVolumeKey = "settings.musicVolume";
        private const string SfxVolumeKey = "settings.sfxVolume";
        private const string VoiceVolumeKey = "settings.voiceVolume";
        private const string SensitivityKey = "settings.mouseSensitivity";
        private const string InvertYKey = "settings.invertY";

        public const float DefaultVolume = 0.8f;
        public const float DefaultMusicVolume = 0.7f;
        public const float DefaultSfxVolume = 1f;
        public const float DefaultVoiceVolume = 1f;
        public const float DefaultSensitivity = 1f;
        public const bool DefaultInvertY = false;

        public const float MinSensitivity = 0.25f;
        public const float MaxSensitivity = 3f;

        /// <summary>Raised after any value changed (AudioManager re-applies the mixer volumes).</summary>
        public static event Action Changed;

        /// <summary>Master volume 0..1.</summary>
        public static float Volume
        {
            get => PlayerPrefs.GetFloat(VolumeKey, DefaultVolume);
            set => SetFloat(VolumeKey, Mathf.Clamp01(value));
        }

        public static float MusicVolume
        {
            get => PlayerPrefs.GetFloat(MusicVolumeKey, DefaultMusicVolume);
            set => SetFloat(MusicVolumeKey, Mathf.Clamp01(value));
        }

        public static float SfxVolume
        {
            get => PlayerPrefs.GetFloat(SfxVolumeKey, DefaultSfxVolume);
            set => SetFloat(SfxVolumeKey, Mathf.Clamp01(value));
        }

        public static float VoiceVolume
        {
            get => PlayerPrefs.GetFloat(VoiceVolumeKey, DefaultVoiceVolume);
            set => SetFloat(VoiceVolumeKey, Mathf.Clamp01(value));
        }

        /// <summary>Multiplier on PlayerTuning.mouseSensitivity (1 = tuned default).</summary>
        public static float MouseSensitivity
        {
            get => PlayerPrefs.GetFloat(SensitivityKey, DefaultSensitivity);
            set => SetFloat(SensitivityKey, Mathf.Clamp(value, MinSensitivity, MaxSensitivity));
        }

        /// <summary>Mouse up looks down (flight-sim style).</summary>
        public static bool InvertY
        {
            get => PlayerPrefs.GetInt(InvertYKey, DefaultInvertY ? 1 : 0) == 1;
            set
            {
                PlayerPrefs.SetInt(InvertYKey, value ? 1 : 0);
                Changed?.Invoke();
            }
        }

        public static void ResetAudio()
        {
            Volume = DefaultVolume;
            MusicVolume = DefaultMusicVolume;
            SfxVolume = DefaultSfxVolume;
            VoiceVolume = DefaultVoiceVolume;
        }

        public static void ResetControls()
        {
            MouseSensitivity = DefaultSensitivity;
            InvertY = DefaultInvertY;
        }

        public static void Save() => PlayerPrefs.Save();

        private static void SetFloat(string key, float value)
        {
            PlayerPrefs.SetFloat(key, value);
            Changed?.Invoke();
        }
    }
}
