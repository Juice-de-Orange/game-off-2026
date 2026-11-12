using System.Collections.Generic;
using GameOff.Audio;
using GameOff.Day;
using GameOff.Economy;
using GameOff.Player;
using UnityEngine;
using UnityEngine.Events;

namespace GameOff.Rest
{
    /// <summary>
    /// P132: the tape deck with three slots, in the counter room and in room 103 (where the floppy player of P117 stood). [E] on an
    /// empty slot puts the first cassette of the inventory in, [E] on a full one takes it back into the inventory; no menus. The deck
    /// plays the cassettes in slot order and starts over (CassettePlaylist); the slot lamps show wait and play, the lever play or stop.
    /// P141: [E] on the lever skips to the next cassette (1→2→3→1), a single one starts over, an empty deck ignores it.
    /// Music in 3D on the Music group (the options slider applies), audible in the room only. The morning save keeps the slots by id
    /// (SaveGame). Built by RestSpotBuilder.
    /// </summary>
    public class CassetteDeck : MonoBehaviour
    {
        private static readonly List<CassetteDeck> Decks = new List<CassetteDeck>();

        [Tooltip("The save keeps the slots under this id (P128), e.g. Counter, Room103.")]
        [SerializeField] private string id = "Deck";
        [Tooltip("The [E] trigger of each slot, slot 1 first.")]
        [SerializeField] private Interactable[] slots = new Interactable[0];
        [Tooltip("The cassette model standing in each slot; hidden while the slot is empty.")]
        [SerializeField] private GameObject[] cassetteModels = new GameObject[0];
        [Tooltip("Lamp_1..3 of the model: their texture offset (u) shows off / wait / play.")]
        [SerializeField] private Renderer[] lamps = new Renderer[0];
        [Tooltip("One lamp state in the texture: 0 off, 1 step wait, 2 steps play.")]
        [SerializeField] private float lampStep = 1f / 32f;
        [SerializeField] private Transform lever;
        [Tooltip("Local X rotation of the lever while playing / stopped.")]
        [SerializeField] private float leverPlayAngle = -25f;
        [SerializeField] private float leverStopAngle = 25f;
        [SerializeField] private float leverDegreesPerSecond = 400f;
        [Tooltip("P141: the [E] trigger around the lever: next song.")]
        [SerializeField] private Interactable skipLever;
        [Tooltip("P141: on a skip the lever flicks toward stop for this long, then back to play.")]
        [SerializeField] private float leverFlickSeconds = 0.12f;
        [SerializeField] private AudioSource source;
        [Tooltip("Shows the hint when you have no cassette with you.")]
        [SerializeField] private DayManager day;
        [Tooltip("From inserting into an empty deck to the lever and the music.")]
        [SerializeField] private float startDelay = 0.45f;
        [Tooltip("Range of the insert, eject and play clicks.")]
        [SerializeField] private float soundDistance = 8f;
        [SerializeField] private string insertPrompt = "Insert {0}";
        [SerializeField] private string noCassettePrompt = "Insert a cassette";
        [SerializeField] private string ejectPrompt = "Take out {0}";
        [SerializeField] private string skipPrompt = "Next song";
        [Tooltip("The lever with only one cassette in.")]
        [SerializeField] private string restartPrompt = "Play {0} from the start";
        [SerializeField] private string noCassetteHint = "You have no cassette with you. The kiosk sells some, and some are lying around the station.";

        private CassettePlaylist _playlist;
        private UnityAction[] _handlers = new UnityAction[0];
        private Material[] _lampMaterials = new Material[0];
        private Vector2[] _lampOffsets = new Vector2[0];
        private Quaternion _leverRest = Quaternion.identity;
        private float _leverAngle;
        /// <summary>P141: until then (Time.time) the lever heads for stop after a skip.</summary>
        private float _leverFlickUntil;
        /// <summary>The cassette whose song is on the source (null = stopped).</summary>
        private CassetteData _onSource;
        /// <summary>A cassette went into the empty deck: it starts at _startAt (Time.time), after the lever.</summary>
        private bool _starting;
        private float _startAt;
        /// <summary>The end of a track is only looked for after this, so a source that is just starting isn't taken for one that ended.</summary>
        private float _checkEndAfter;
        /// <summary>Seconds the current track has really been playing (its end is only taken once it got there).</summary>
        private float _played;
        /// <summary>Each cassette model's own material from the FBX, for a cassette without a label material.</summary>
        private Material[][] _modelDefaults = new Material[0][];
        private Inventory _inventory;

        /// <summary>P128: every deck in the scene (the save reads and restores them).</summary>
        public static IReadOnlyList<CassetteDeck> All => Decks;

        /// <summary>Is the cassette in one of the decks right now? Then you own it: the kiosk doesn't sell it to you again.</summary>
        public static bool AnyHolds(ItemData item)
        {
            foreach (var d in Decks)
                if (d != null && d.Playlist.Holds(item)) return true;
            return false;
        }

        public string Id => id;
        /// <summary>The cassette in each slot (null = empty), slot 1 first.</summary>
        public IReadOnlyList<CassetteData> Slots => Playlist.Slots;

        private CassettePlaylist Playlist => _playlist ??= new CassettePlaylist(slots.Length);

        private void Awake()
        {
            Decks.Add(this);
            source.playOnAwake = false;
            source.loop = false;
            source.spatialBlend = 1f;
            _lampMaterials = new Material[lamps.Length];
            _lampOffsets = new Vector2[lamps.Length];
            for (int i = 0; i < lamps.Length; i++)
            {
                if (lamps[i] == null) continue;
                _lampMaterials[i] = lamps[i].material; // an own copy per lamp: they show different states
                if (_lampMaterials[i].HasProperty("_BaseMap")) _lampOffsets[i] = _lampMaterials[i].GetTextureOffset("_BaseMap");
            }
            if (lever != null) _leverRest = lever.localRotation;
            _leverAngle = leverStopAngle;
            ShowSlots();
            ApplyLever();
        }

        private void Start()
        {
            source.outputAudioMixerGroup = AudioManager.MusicGroup;
            var economy = PlayerEconomy.Instance;
            if (economy != null)
            {
                _inventory = economy.Inventory;
                _inventory.Changed += RefreshPrompts;
            }
            RefreshPrompts();
        }

        private void OnEnable()
        {
            _handlers = new UnityAction[slots.Length];
            for (int i = 0; i < slots.Length; i++)
            {
                int slot = i;
                _handlers[i] = () => Use(slot);
                if (slots[i] != null) slots[i].OnInteract.AddListener(_handlers[i]);
            }
            if (skipLever != null) skipLever.OnInteract.AddListener(Skip);
        }

        private void OnDisable()
        {
            for (int i = 0; i < slots.Length && i < _handlers.Length; i++)
                if (slots[i] != null) slots[i].OnInteract.RemoveListener(_handlers[i]);
            if (skipLever != null) skipLever.OnInteract.RemoveListener(Skip);
        }

        private void OnDestroy()
        {
            Decks.Remove(this);
            if (_inventory != null) _inventory.Changed -= RefreshPrompts;
            foreach (var m in _lampMaterials) if (m != null) Destroy(m);
        }

        private void Update()
        {
            if (_starting && Time.time >= _startAt)
            {
                if (Playlist.Current != null) AudioManager.PlayAt(s => s.cassettePlay, transform.position, soundDistance);
                StartTrack();
            }
            else if (_onSource != null && source.clip != null && !AudioListener.pause && Time.time >= _checkEndAfter)
            {
                if (source.isPlaying) _played += Time.unscaledDeltaTime;
                // Over only once it really played to the end: a source that is still decoding, or a suspended
                // browser audio context, also reports "not playing" and must not skip the tape. P146: a song still decoding has
                // no length yet (asking for it logs to the browser console and gives 0): no end before it is known.
                // A clip that failed to decode never gets a length: it counts as over, so the tape moves on instead of stalling.
                else if (_played >= (source.clip.loadState == AudioDataLoadState.Failed ? 0f
                             : AudioManager.LoadedLength(source.clip, float.PositiveInfinity) - 0.5f))
                {
                    Playlist.Advance(); // the track is over: the next cassette, or the first one again
                    StartTrack();
                }
            }
            ApplyLever();
        }

        /// <summary>P128: the slots of the save, without sounds; the first cassette plays at once.</summary>
        public void Restore(IList<CassetteData> saved)
        {
            Playlist.Restore(saved);
            StartTrack();
            _leverAngle = Playlist.Current != null ? leverPlayAngle : leverStopAngle;
            ApplyLever();
            RefreshPrompts();
        }

        private void Use(int slot)
        {
            var economy = PlayerEconomy.Instance;
            if (economy == null || slot < 0 || slot >= slots.Length) return;
            Vector3 at = slots[slot] != null ? slots[slot].transform.position : transform.position;
            if (Playlist.At(slot) != null)
            {
                Playlist.EjectTo(economy.Inventory, slot);
                AudioManager.PlayAt(s => s.cassetteEject, at, soundDistance);
                if (Playlist.Current != _onSource) StartTrack(); // the playing one came out: the next one, or silence
                else source.loop = Playlist.Count == 1;
            }
            else
            {
                bool idle = Playlist.Current == null;
                if (Playlist.InsertFrom(economy.Inventory, slot) == null)
                {
                    if (day != null && day.UI != null) day.UI.ShowHint(noCassetteHint, 4f);
                    return;
                }
                AudioManager.PlayAt(s => s.cassetteInsert, at, soundDistance);
                if (idle) // the lever clicks over, then the music starts
                {
                    _starting = true;
                    _startAt = Time.time + startDelay;
                }
                else source.loop = Playlist.Count == 1;
            }
            ShowSlots();
            RefreshPrompts();
        }

        /// <summary>P141, [E] on the lever: the next cassette in slot order, a single one from the start; an empty deck ignores it.</summary>
        private void Skip()
        {
            if (!Playlist.Skip()) return;
            AudioManager.PlayAt(s => s.cassetteSkip, skipLever != null ? skipLever.transform.position : transform.position, soundDistance);
            _leverFlickUntil = Time.time + leverFlickSeconds;
            if (_starting) ShowSlots(); // the lever is still going down after an insert: that pending start plays the new one
            else StartTrack(); // also counts the played seconds from zero again: only the new song's own end advances the tape
            RefreshPrompts();
        }

        /// <summary>Puts the playing cassette's song on the source (or stops it); a single cassette loops seamlessly.</summary>
        private void StartTrack()
        {
            var current = Playlist.Current;
            _onSource = current;
            _starting = false; // a start still pending (lever, click) is over: this plays now
            source.Stop();
            source.clip = current != null ? current.Song : null;
            source.loop = Playlist.Count == 1;
            if (source.clip != null) source.Play();
            _checkEndAfter = Time.time + 0.5f;
            _played = 0f;
            ShowSlots();
        }

        /// <summary>Cassettes in their slots, the lamps.</summary>
        private void ShowSlots()
        {
            for (int i = 0; i < cassetteModels.Length; i++)
            {
                var model = cassetteModels[i];
                if (model == null) continue;
                var cassette = Playlist.At(i);
                model.SetActive(cassette != null);
                if (cassette == null) continue;
                var renderers = model.GetComponentsInChildren<Renderer>(true);
                if (_modelDefaults.Length != cassetteModels.Length) _modelDefaults = new Material[cassetteModels.Length][];
                if (_modelDefaults[i] == null)
                {
                    _modelDefaults[i] = new Material[renderers.Length];
                    for (int r = 0; r < renderers.Length; r++) _modelDefaults[i][r] = renderers[r].sharedMaterial;
                }
                for (int r = 0; r < renderers.Length; r++)
                    renderers[r].sharedMaterial = cassette.Material != null ? cassette.Material
                        : r < _modelDefaults[i].Length ? _modelDefaults[i][r] : renderers[r].sharedMaterial;
            }
            for (int i = 0; i < _lampMaterials.Length; i++)
            {
                var m = _lampMaterials[i];
                if (m == null || !m.HasProperty("_BaseMap")) continue;
                var lamp = Playlist.LampOf(i);
                if (lamp == CassettePlaylist.Lamp.Play && _starting) lamp = CassettePlaylist.Lamp.Wait; // until the lever is down
                m.SetTextureOffset("_BaseMap", _lampOffsets[i] + new Vector2((int)lamp * lampStep, 0f));
            }
        }

        private void ApplyLever()
        {
            if (lever == null) return;
            float target = Playlist.Current != null && !_starting && Time.time >= _leverFlickUntil ? leverPlayAngle : leverStopAngle;
            _leverAngle = Mathf.MoveTowards(_leverAngle, target, leverDegreesPerSecond * Time.deltaTime);
            lever.localRotation = _leverRest * Quaternion.Euler(_leverAngle, 0f, 0f);
        }

        private void RefreshPrompts()
        {
            var next = CassetteData.FirstIn(_inventory);
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i] == null) continue;
                var cassette = Playlist.At(i);
                slots[i].SetPrompt(cassette != null ? string.Format(ejectPrompt, cassette.Title)
                    : next != null ? string.Format(insertPrompt, next.Title) : noCassettePrompt);
            }
            if (skipLever == null) return;
            var current = Playlist.Current;
            skipLever.SetInteractionEnabled(current != null); // nothing to skip in an empty deck: no prompt either
            skipLever.SetPrompt(current != null && Playlist.Count == 1 ? string.Format(restartPrompt, current.Title) : skipPrompt);
        }
    }
}
