using System;
using UnityEngine;

namespace GameOff.Audio
{
    /// <summary>One clip with its own volume, so sounds can be balanced by ear in Data/Audio/SoundSet without touching code.</summary>
    [Serializable]
    public class Sound
    {
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
    }
}
