using System;
using System.Collections;
using System.Collections.Generic;
using GameOff.Core;
using GameOff.Player;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GameOff.Audio
{
    /// <summary>
    /// Audio backbone (P18). One instance survives scene loads (placed in Boot, Menu and Main, so any scene can be played
    /// directly; extra copies remove themselves). Volumes from GameSettings go to the exposed GameMixer parameters
    /// (the only mixer feature Unity supports on the Web). Static helpers so other systems don't need a reference:
    ///   AudioManager.PlayMusic(clip)        – loops on the Music group, crossfades from the current track
    ///   AudioManager.PlaySfx(clip)          – 2D one-shot on the SFX group (UI, stamps, desk)
    ///   AudioManager.PlaySfxAt(clip, pos)   – 3D one-shot on the SFX group at a world position
    ///   AudioManager.Play(s => s.drawerOpen) – 2D one-shot from Data/Audio/SoundSet with its volume
    ///   AudioManager.PlayVoice(clip)        – 2D on the Voice group, replaces the current line
    ///   AudioManager.PlaySpeech(species, name) – P144: a murmur of the species at the speaker's own pitch (Voice group)
    /// AudioSources placed in scenes without a mixer group (e.g. the doors from the station builder) are routed to SFX
    /// on every scene load. Music and previews keep playing in the pause menu (ignoreListenerPause).
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        private static readonly string[] VolumeParameters = { "MasterVolume", "MusicVolume", "SfxVolume", "VoiceVolume" };

        [SerializeField] private AudioMixer mixer;
        [SerializeField] private AudioMixerGroup musicGroup;
        [SerializeField] private AudioMixerGroup sfxGroup;
        [SerializeField] private AudioMixerGroup voiceGroup;
        [Tooltip("Two looping sources for crossfades.")]
        [SerializeField] private AudioSource musicA;
        [SerializeField] private AudioSource musicB;
        [SerializeField] private AudioSource sfx2D;
        [Tooltip("Sounds of the game itself: stop with the pause menu, unlike the UI on sfx2D.")]
        [SerializeField] private AudioSource sfxGame;
        [SerializeField] private AudioSource voice;
        [SerializeField] private float defaultMusicFade = 1.5f;
        [SerializeField] private SoundSet sounds;

        private AudioSource _activeMusic;
        private Coroutine _fade;
        private AudioClip[] _previews;
        private readonly List<RaycastResult> _uiHits = new List<RaycastResult>();

        public static AudioManager Instance { get; private set; }

        public static AudioMixerGroup SfxGroup => Instance != null ? Instance.sfxGroup : null;
        public static AudioMixerGroup MusicGroup => Instance != null ? Instance.musicGroup : null;
        public static AudioMixerGroup VoiceGroup => Instance != null ? Instance.voiceGroup : null;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            _activeMusic = musicA;
            GameSettings.Changed += ApplyVolumes;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        // Exposed mixer parameters set in Awake can be ignored by Unity, so (re)apply here.
        private void Start()
        {
            ApplyVolumes();
            RouteUnassignedSources();
        }

        private void OnDestroy()
        {
            if (Instance != this) return;
            Instance = null;
            GameSettings.Changed -= ApplyVolumes;
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode) => RouteUnassignedSources();

        // One place for the click of every button, instead of a listener on each of them.
        private void Update()
        {
            if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame || EventSystem.current == null) return;
            var pointer = new PointerEventData(EventSystem.current) { position = Mouse.current.position.ReadValue() };
            _uiHits.Clear();
            EventSystem.current.RaycastAll(pointer, _uiHits);
            if (_uiHits.Count == 0) return;
            var control = _uiHits[0].gameObject.GetComponentInParent<Selectable>();
            if (control == null || !control.IsInteractable() || control is Slider || sounds == null) return; // a slider has its own preview tone
            PlaySfx(sounds.uiClick.clip, sounds.uiClick.volume);
        }

        // ---------- Public helpers ----------

        public static void PlayMusic(AudioClip clip, float fadeSeconds = -1f)
        {
            if (Instance == null) return;
            Instance.CrossfadeTo(clip, fadeSeconds < 0f ? Instance.defaultMusicFade : fadeSeconds);
        }

        public static void StopMusic(float fadeSeconds = -1f)
        {
            if (Instance == null) return;
            Instance.CrossfadeTo(null, fadeSeconds < 0f ? Instance.defaultMusicFade : fadeSeconds);
        }

        public static void PlaySfx(AudioClip clip, float volume = 1f)
        {
            if (Instance == null || clip == null) return;
            Instance.sfx2D.PlayOneShot(clip, volume);
        }

        /// <summary>A sound of the SoundSet on the SFX group.</summary>
        public static void Play(Func<SoundSet, Sound> pick)
        {
            if (Instance == null || Instance.sounds == null) return;
            var sound = pick(Instance.sounds);
            if (sound != null) Instance.PlayGame(sound);
        }

        /// <summary>A sound of the SoundSet at a place in the world, for what can happen while the player is elsewhere.</summary>
        public static void PlayAt(Func<SoundSet, Sound> pick, Vector3 position, float maxDistance)
        {
            if (Instance == null || Instance.sounds == null) return;
            var sound = pick(Instance.sounds);
            if (sound != null) PlaySfxAt(sound.clip, position, sound.volume, maxDistance);
        }

        private void PlayGame(Sound sound)
        {
            if (sound.clip != null) sfxGame.PlayOneShot(sound.clip, sound.volume);
        }

        /// <summary>P102: one of the variants at a slightly different pitch, so many papers in a row don't drill; else the single sound.</summary>
        public static void PlayVaried(Func<SoundSet, Sound[]> variants, Func<SoundSet, Sound> single)
        {
            if (Instance == null || Instance.sounds == null) return;
            var list = variants(Instance.sounds);
            var sound = list != null && list.Length > 0 ? list[UnityEngine.Random.Range(0, list.Length)] : single(Instance.sounds);
            if (sound == null || sound.clip == null) return;
            if (Instance._variedSource == null)
            {
                Instance._variedSource = Instance.gameObject.AddComponent<AudioSource>();
                Instance._variedSource.playOnAwake = false;
                Instance._variedSource.spatialBlend = 0f;
                Instance._variedSource.outputAudioMixerGroup = Instance.sfxGame.outputAudioMixerGroup;
            }
            Instance._variedSource.pitch = UnityEngine.Random.Range(0.94f, 1.06f);
            Instance._variedSource.PlayOneShot(sound.clip, sound.volume);
        }

        private AudioSource _variedSource;

        /// <summary>A sound of the SoundSet on the Voice group, for what stands in for a voice. Doesn't cut off a line that is playing.</summary>
        public static void PlayOnVoice(Func<SoundSet, Sound> pick)
        {
            if (Instance == null || Instance.sounds == null) return;
            var sound = pick(Instance.sounds);
            if (sound != null && sound.clip != null) Instance.voice.PlayOneShot(sound.clip, sound.volume);
        }

        /// <summary>P144: a few sources of their own for speech, so its pitch never touches the announcer's or another speaker's sound.</summary>
        private const int SpeechSources = 3;
        /// <summary>From the one used longest ago to the one used last.</summary>
        private readonly List<AudioSource> _speech = new List<AudioSource>();
        private Sound[] _fallbackVoices;
        /// <summary>P144: the murmur each voice set played last (per set, so nobody hears the same one twice in a row).</summary>
        private readonly Dictionary<Sound[], int> _lastVoice = new Dictionary<Sound[], int>();

        /// <summary>
        /// P144: someone starts to talk (a vendor greets, a traveler's line begins): one murmur of their species (P94; vendorGreet and
        /// the PA murmurs until those are delivered), at random but never the one this set played last, at the speaker's own pitch
        /// (stable per name) with a small change each time. 2D on the Voice group; doesn't cut off announcements or PlayVoice lines.
        /// </summary>
        public static void PlaySpeech(string species, string speaker)
        {
            if (Instance == null || Instance.sounds == null) return;
            var s = Instance.sounds;
            var set = s.VoicesOf(species) ?? (Instance._fallbackVoices ??= s.FallbackVoices());
            int pick = VoicePicker.Next(set.Length, Instance._lastVoice.TryGetValue(set, out int last) ? last : -1, UnityEngine.Random.value);
            if (pick < 0) return;
            Instance._lastVoice[set] = pick;
            var sound = set[pick];
            if (sound == null || sound.clip == null) return;
            var source = Instance.SpeechSource();
            source.clip = sound.clip; // Play, not PlayOneShot: one sound per source, so its pitch bends nothing else
            source.volume = sound.volume;
            source.pitch = VoicePicker.Utterance(VoicePicker.SpeakerPitch(speaker, s.voicePitchMin, s.voicePitchMax), s.voicePitchJitter,
                UnityEngine.Random.value);
            source.Play();
        }

        /// <summary>A silent speech source, else the one used longest ago (its murmur is cut off); made on first use.</summary>
        private AudioSource SpeechSource()
        {
            AudioSource pick = null;
            foreach (var source in _speech)
                if (source != null && !source.isPlaying) { pick = source; break; }
            if (pick == null && _speech.Count < SpeechSources)
            {
                pick = gameObject.AddComponent<AudioSource>();
                pick.playOnAwake = false;
                pick.spatialBlend = 0f;
                pick.outputAudioMixerGroup = voiceGroup;
            }
            if (pick == null) pick = _speech[0];
            _speech.Remove(pick);
            _speech.Add(pick);
            return pick;
        }

        private int _lastAnnouncement = -1;

        /// <summary>P115: a loudspeaker announcement on the Voice group: one of the murmurs, never the same twice in a row; else the chime.</summary>
        public static void PlayAnnouncement()
        {
            if (Instance == null || Instance.sounds == null) return;
            var murmurs = Instance.sounds.paMurmurs;
            if (murmurs == null || murmurs.Length == 0)
            {
                PlayOnVoice(s => s.paChime);
                return;
            }
            int pick = UnityEngine.Random.Range(0, murmurs.Length);
            if (murmurs.Length > 1 && pick == Instance._lastAnnouncement) pick = (pick + 1) % murmurs.Length;
            Instance._lastAnnouncement = pick;
            if (murmurs[pick].clip == null) PlayOnVoice(s => s.paChime);
            else Instance.voice.PlayOneShot(murmurs[pick].clip, murmurs[pick].volume);
        }

        /// <summary>The next footstep; steps remembers the last clip.</summary>
        /// <param name="clown">P63: the clown shoes are on.</param>
        public static void PlayFootstep(Footsteps steps, bool clown = false)
        {
            if (clown && Instance != null && Instance.sounds != null && Instance.sounds.clownFootsteps != null && Instance.sounds.clownFootsteps.Length > 0)
            {
                var squeaks = Instance.sounds.clownFootsteps;
                Instance.PlayGame(squeaks[steps.NextIndex(squeaks.Length, UnityEngine.Random.value)]);
                return;
            }
            if (Instance == null || Instance.sounds == null || Instance.sounds.footsteps == null || Instance.sounds.footsteps.Length == 0) return;
            var list = Instance.sounds.footsteps;
            Instance.PlayGame(list[steps.NextIndex(list.Length, UnityEngine.Random.value)]);
        }

        public static void PlaySfxAt(AudioClip clip, Vector3 position, float volume = 1f, float maxDistance = 20f)
        {
            if (Instance == null || clip == null) return;
            var go = new GameObject($"Sfx_{clip.name}");
            go.transform.position = position;
            var source = go.AddComponent<AudioSource>();
            source.outputAudioMixerGroup = Instance.sfxGroup;
            source.spatialBlend = 1f;
            source.maxDistance = maxDistance;
            source.rolloffMode = AudioRolloffMode.Linear;
            source.PlayOneShot(clip, volume);
            Destroy(go, clip.length + 0.1f);
        }

        public static void PlayVoice(AudioClip clip)
        {
            if (Instance == null || clip == null) return;
            Instance.voice.Stop();
            Instance.voice.clip = clip;
            Instance.voice.Play();
        }

        /// <summary>Short generated tone on one channel, played when a volume slider is released (works without any assets).</summary>
        public static void PlayPreview(AudioChannel channel)
        {
            if (Instance == null) return;
            Instance.PreviewInternal(channel);
        }

        // ---------- Internals ----------

        private void ApplyVolumes()
        {
            if (mixer == null) return;
            SetVolume(AudioChannel.Master, GameSettings.Volume);
            SetVolume(AudioChannel.Music, GameSettings.MusicVolume);
            SetVolume(AudioChannel.Sfx, GameSettings.SfxVolume);
            SetVolume(AudioChannel.Voice, GameSettings.VoiceVolume);
        }

        private void SetVolume(AudioChannel channel, float linear)
        {
            // 0..1 slider → decibels; -80 dB is silent.
            float db = linear <= 0.0001f ? -80f : Mathf.Log10(linear) * 20f;
            mixer.SetFloat(VolumeParameters[(int)channel], db);
        }

        private void RouteUnassignedSources()
        {
            if (sfxGroup == null) return;
            foreach (var source in FindObjectsByType<AudioSource>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (source.outputAudioMixerGroup == null) source.outputAudioMixerGroup = sfxGroup;
        }

        private void CrossfadeTo(AudioClip clip, float seconds)
        {
            if (clip != null && _activeMusic.clip == clip && _activeMusic.isPlaying) return;
            if (_fade != null) StopCoroutine(_fade);
            var from = _activeMusic;
            var to = clip != null ? (from == musicA ? musicB : musicA) : null;
            if (to != null)
            {
                to.clip = clip;
                to.volume = 0f;
                to.Play();
                _activeMusic = to;
            }
            _fade = StartCoroutine(Fade(from, to, seconds));
        }

        private IEnumerator Fade(AudioSource from, AudioSource to, float seconds)
        {
            float startFrom = from != null ? from.volume : 0f;
            for (float t = 0f; t < seconds; t += Time.unscaledDeltaTime)
            {
                float k = t / seconds;
                if (from != null && from != to) from.volume = Mathf.Lerp(startFrom, 0f, k);
                if (to != null) to.volume = Mathf.Lerp(0f, 1f, k);
                yield return null;
            }
            if (from != null && from != to)
            {
                from.Stop();
                from.volume = 1f;
            }
            if (to != null) to.volume = 1f;
            _fade = null;
        }

        private void PreviewInternal(AudioChannel channel)
        {
            _previews ??= new[]
            {
                Tone("Preview_Master", 0.18f, 660f, 990f),
                Tone("Preview_Music", 0.6f, 262f, 330f, 392f),
                Tone("Preview_Sfx", 0.12f, 1200f),
                Tone("Preview_Voice", 0.35f, 180f, 360f),
            };
            var group = channel switch
            {
                AudioChannel.Music => musicGroup,
                AudioChannel.Sfx => sfxGroup,
                AudioChannel.Voice => voiceGroup,
                _ => sfxGroup, // master preview runs through a child group so the master fader affects it too
            };
            var go = new GameObject($"Preview_{channel}");
            DontDestroyOnLoad(go);
            var source = go.AddComponent<AudioSource>();
            source.outputAudioMixerGroup = group;
            source.ignoreListenerPause = true; // audible in the pause menu
            source.PlayOneShot(_previews[(int)channel], 0.5f);
            Destroy(go, _previews[(int)channel].length + 0.1f);
        }

        /// <summary>A soft sine chord with a short fade in/out (no click), generated once.</summary>
        private static AudioClip Tone(string name, float seconds, params float[] frequencies)
        {
            const int rate = 44100;
            int samples = Mathf.CeilToInt(seconds * rate);
            var data = new float[samples];
            int ramp = Mathf.Min(samples / 4, rate / 100);
            for (int i = 0; i < samples; i++)
            {
                float v = 0f;
                foreach (float f in frequencies) v += Mathf.Sin(2f * Mathf.PI * f * i / rate);
                float envelope = Mathf.Min(1f, Mathf.Min(i, samples - 1 - i) / (float)Mathf.Max(1, ramp));
                data[i] = v / frequencies.Length * envelope * 0.6f;
            }
            var clip = AudioClip.Create(name, samples, 1, rate, false); // stream must be false on the Web
            clip.SetData(data, 0);
            return clip;
        }
    }
}
