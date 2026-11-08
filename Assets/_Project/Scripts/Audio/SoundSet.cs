using UnityEngine;

namespace GameOff.Audio
{
    /// <summary>
    /// All one-shot sounds of the game in one place (P61). The AudioManager holds it, code plays by field:
    /// AudioManager.Play(s => s.drawerOpen). Built by GameOff/Build Sound Set; volumes in the asset survive a rebuild.
    /// </summary>
    [CreateAssetMenu(fileName = "SoundSet", menuName = "GameOff/Sound Set")]
    public class SoundSet : ScriptableObject
    {
        [Header("Counter")]
        public Sound stampApprove = new Sound();
        public Sound stampDeny = new Sound();
        public Sound paperSlide = new Sound();
        public Sound paperPickup = new Sound();
        public Sound drawerOpen = new Sound();
        public Sound drawerClose = new Sound();
        public Sound printerCitation = new Sound();
        public Sound buttonArrest = new Sound();
        public Sound coverFlip = new Sound();
        public Sound airlockFire = new Sound();
        public Sound shutterDrop = new Sound();
        public Sound shutterRise = new Sound();
        public Sound arrestScuffle = new Sound();

        [Header("Station")]
        public Sound paChime = new Sound();
        public Sound vendorGreet = new Sound();
        public Sound tradeBuy = new Sound();
        [Tooltip("P102: variants, played at random with a slight change of pitch; empty = paperPickup / paperSlide.")]
        public Sound[] paperPickups = new Sound[0];
        public Sound[] paperSlides = new Sound[0];
        public Sound[] footsteps = new Sound[0];
        [Tooltip("P63: with the clown shoes on.")]
        public Sound[] clownFootsteps = new Sound[0];

        [Header("Room sound")]
        [Tooltip("Volume of the station hum, and of the room sound in bar and hangar.")]
        [Range(0f, 1f)] public float ambienceVolume = 0.35f;
        [Tooltip("Volume of the station hum inside bar and hangar.")]
        [Range(0f, 1f)] public float humInRoomVolume = 0.12f;
        [Tooltip("Seconds for the change between rooms.")]
        public float ambienceFadeSeconds = 1.5f;

        [Header("Menus")]
        public Sound uiClick = new Sound();
    }
}
