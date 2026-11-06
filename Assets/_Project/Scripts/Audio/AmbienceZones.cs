using UnityEngine;

namespace GameOff.Audio
{
    /// <summary>
    /// Room sound in Main (P61): the station hum everywhere, bar and hangar fade in while the player is inside them and the hum
    /// steps back. Built by GameOff/Build Ambience into Main. All sources run on the SFX group, so the slider and the pause menu apply.
    /// </summary>
    public class AmbienceZones : MonoBehaviour
    {
        [SerializeField] private AudioSource station;
        [SerializeField] private AudioSource bar;
        [SerializeField] private AudioSource hangar;
        [SerializeField] private Transform listener;
        [SerializeField] private Bounds barArea;
        [SerializeField] private Bounds hangarArea;
        [SerializeField] private SoundSet tuning;

        private void Start() => Apply(1f); // already right on the first frame, no fade after loading

        private void Update() => Apply(Time.unscaledDeltaTime * tuning.ambienceVolume / Mathf.Max(0.01f, tuning.ambienceFadeSeconds));

        private void Apply(float step)
        {
            if (listener == null) return;
            var (hum, barVolume, hangarVolume) = Targets(listener.position, barArea, hangarArea, tuning.ambienceVolume, tuning.humInRoomVolume);
            station.volume = Mathf.MoveTowards(station.volume, hum, step);
            bar.volume = Mathf.MoveTowards(bar.volume, barVolume, step);
            hangar.volume = Mathf.MoveTowards(hangar.volume, hangarVolume, step);
        }

        public static (float hum, float bar, float hangar) Targets(Vector3 position, Bounds barArea, Bounds hangarArea, float room, float humInRoom)
        {
            if (barArea.Contains(position)) return (humInRoom, room, 0f);
            if (hangarArea.Contains(position)) return (humInRoom, 0f, room);
            return (room, 0f, 0f);
        }
    }
}
