using System;
using UnityEngine;

namespace GameOff.Audio
{
    /// <summary>P144: the murmurs one species talks with (P94: SFX_Voice_&lt;Species&gt;_01..10), filled by GameOff/Build Sound Set.</summary>
    [Serializable]
    public class SpeciesVoice
    {
        [Tooltip("Human, Vellani, Quobb, Sorvak or Xirrik, as in TravelerData.species.")]
        public string species = "Human";
        [Tooltip("The delivered variants; empty = the species talks with the fallback voices (SoundSet.FallbackVoices).")]
        public Sound[] sounds = new Sound[0];
    }
}
