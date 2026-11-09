using UnityEngine;

namespace GameOff.Rest
{
    /// <summary>
    /// P29: how far each letter of the letter on the table wobbles. Fixed per letter and day (the same text does not jitter from
    /// frame to frame), scaled by how shaky the hand is (FamilyMood.Shake, 0 to 1).
    /// </summary>
    public static class Handwriting
    {
        /// <summary>Offset of one letter in shares of its height (each axis −shake … +shake).</summary>
        public static Vector2 Offset(int index, int seed, float shake)
        {
            float s = Mathf.Clamp01(shake);
            return new Vector2(Noise(index, seed, 1) * 0.6f, Noise(index, seed, 2)) * s;
        }

        /// <summary>Tilt of one letter, −1 … +1 times shake (times the tuning's maximum degrees).</summary>
        public static float Tilt(int index, int seed, float shake) => Noise(index, seed, 3) * Mathf.Clamp01(shake);

        /// <summary>A fixed pseudo-random value in −1 … 1 for this letter, seed and channel.</summary>
        public static float Noise(int index, int seed, int channel)
        {
            unchecked
            {
                uint h = (uint)(index * 73856093) ^ (uint)(seed * 19349663) ^ (uint)(channel * 83492791);
                h ^= h >> 13;
                h *= 0x5bd1e995;
                h ^= h >> 15;
                return (h & 0xFFFF) / 32767.5f - 1f;
            }
        }
    }
}
