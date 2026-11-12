namespace GameOff.Audio
{
    /// <summary>
    /// P144: how a speaker sounds, without Unity (EditMode-tested). Which murmur of a set comes next (at random, never the one just
    /// played), the speaker's own pitch (stable, from the name, so the barkeeper always sounds like himself) and a small change of it
    /// per utterance.
    /// </summary>
    public static class VoicePicker
    {
        /// <summary>A random index below count that isn't last (unless there is only one); -1 for an empty set.</summary>
        /// <param name="last">The index played before, -1 for none.</param>
        /// <param name="random01">Random value from 0 up to (not including) 1.</param>
        public static int Next(int count, int last, float random01)
        {
            if (count <= 0) return -1;
            if (count == 1) return 0;
            bool skipLast = last >= 0 && last < count;
            int candidates = skipLast ? count - 1 : count;
            int index = (int)(random01 * candidates);
            if (index >= candidates) index = candidates - 1;
            if (index < 0) index = 0;
            if (skipLast && index >= last) index++; // jumps over the last one
            return index;
        }

        /// <summary>The speaker's own pitch in [min, max], the same for the same name on every run and platform (FNV-1a, not GetHashCode).</summary>
        public static float SpeakerPitch(string speaker, float min, float max)
        {
            if (string.IsNullOrEmpty(speaker)) return (min + max) * 0.5f;
            uint hash = 2166136261;
            foreach (char c in speaker)
            {
                hash ^= c;
                hash = unchecked(hash * 16777619);
            }
            return min + (max - min) * (hash % 1001) / 1000f;
        }

        /// <summary>The pitch of one utterance: the speaker's own, changed by up to ±jitter (0.02 = 2 %).</summary>
        /// <param name="random01">Random value from 0 up to 1.</param>
        public static float Utterance(float speakerPitch, float jitter, float random01) =>
            speakerPitch * (1f + (random01 * 2f - 1f) * jitter);
    }
}
