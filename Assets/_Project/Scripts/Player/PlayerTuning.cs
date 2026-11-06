using UnityEngine;

namespace GameOff.Player
{
    /// <summary>Tuning values for the first-person player. Edit the asset in Data/, not the code.</summary>
    [CreateAssetMenu(fileName = "PlayerTuning", menuName = "GameOff/Player Tuning")]
    public class PlayerTuning : ScriptableObject
    {
        [Header("Movement")]
        [Tooltip("Walking speed in m/s. The floor plan assumes 4 m/s.")]
        public float walkSpeed = 4f;
        [Tooltip("Downward acceleration in m/s².")]
        public float gravity = 20f;
        [Tooltip("Meters between two footstep sounds.")]
        public float stepLength = 1.6f;

        [Header("Look")]
        [Tooltip("Degrees per pixel of mouse movement.")]
        public float mouseSensitivity = 0.12f;
        [Tooltip("Max up/down look angle in degrees.")]
        public float maxPitch = 85f;

        [Header("Interaction")]
        [Tooltip("How far the player can reach to interact, in meters.")]
        public float interactRange = 2.2f;
    }
}
