using GameOff.Audio;
using GameOff.Day;
using GameOff.Player;
using UnityEngine;

namespace GameOff.Rest
{
    /// <summary>
    /// P117: the floppy player on the table in room 103. The player starts with one disk; [E] puts it in or takes it out, and the
    /// music plays in the room only (3D, on the Music group, so the options slider applies). Until the song is delivered the drive
    /// just spins up.
    /// </summary>
    public class FloppyPlayer : MonoBehaviour
    {
        [SerializeField] private Interactable interactable;
        [SerializeField] private DayManager day;
        [SerializeField] private AudioSource source;
        [Tooltip("MUS_Floppy_01; empty until the pipeline delivers it.")]
        [SerializeField] private AudioClip track;
        [Tooltip("The disk lying next to the player while it is not in the drive (optional).")]
        [SerializeField] private GameObject diskOnTable;
        [SerializeField] private string insertPrompt = "Insert the floppy";
        [SerializeField] private string ejectPrompt = "Take the floppy out";
        [SerializeField] private string noMusicHint = "The drive spins up. There is nothing on this disk yet.";

        private bool _inserted;

        private void Awake()
        {
            source.loop = true;
            source.playOnAwake = false;
            source.spatialBlend = 1f;
            Refresh();
        }

        private void Start() => source.outputAudioMixerGroup = AudioManager.MusicGroup;

        private void OnEnable() => interactable.OnInteract.AddListener(Toggle);
        private void OnDisable() => interactable.OnInteract.RemoveListener(Toggle);

        private void Toggle()
        {
            _inserted = !_inserted;
            AudioManager.Play(s => s.uiClick);
            Refresh();
            if (_inserted && track == null && day != null) day.UI.ShowHint(noMusicHint, 4f);
        }

        private void Refresh()
        {
            interactable.SetPrompt(_inserted ? ejectPrompt : insertPrompt);
            if (diskOnTable != null) diskOnTable.SetActive(!_inserted);
            if (_inserted && track != null)
            {
                source.clip = track;
                source.Play();
            }
            else source.Stop();
        }
    }
}
