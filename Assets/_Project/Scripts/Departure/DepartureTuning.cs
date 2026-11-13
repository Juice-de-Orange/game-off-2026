using UnityEngine;

namespace GameOff.Departure
{
    /// <summary>
    /// P72: the flight home and the texts of the ticket endings. The ticket price is HouseholdTuning.ticketPrice (balanced by P71).
    /// Ending texts may use {names} for the family members the ending is about.
    /// </summary>
    [CreateAssetMenu(menuName = "GameOff/Departure Tuning", fileName = "DepartureTuning")]
    public class DepartureTuning : ScriptableObject
    {
        [Header("Flight")]
        [Tooltip("Seconds of automatic lift-off before the player steers.")]
        public float liftOffSeconds = 2.5f;
        [Tooltip("Metres the shuttle rises during lift-off.")]
        public float liftOffHeight = 4f;
        [Tooltip("P106: degrees the shuttle turns while it lifts off (negative = to its left, away from the station).")]
        public float undockTurn = -90f;
        [Tooltip("Speed gained per second while W is held (m/s²).")]
        public float acceleration = 10f;
        [Tooltip("Speed lost per second while S is held (m/s²).")]
        public float braking = 14f;
        public float maxSpeed = 30f;
        [Tooltip("Top speed while Shift is held.")]
        public float boostSpeed = 70f;
        [Tooltip("Degrees per pixel of mouse movement.")]
        public float mouseTurn = 0.12f;
        [Tooltip("Degrees per second for A/D (yaw) without the mouse.")]
        public float keyTurn = 60f;
        [Tooltip("Steepest climb or dive (degrees).")]
        public float maxPitch = 70f;
        [Tooltip("Strongest lean into a turn (degrees) and how much the turn speed leans the shuttle.")]
        public float maxBank = 35f;
        public float bankPerTurn = 0.4f;
        [Tooltip("How fast the lean follows (higher = quicker).")]
        public float bankFollow = 4f;
        [Tooltip("Run ends this far from the take-off point (m).")]
        public float exitDistance = 320f;
        [Tooltip("Run ends after this long in any case (s), e.g. when the player just sits there.")]
        public float maxSeconds = 45f;
        [Header("Chase camera")]
        public float cameraDistance = 16f;
        public float cameraHeight = 5f;
        [Tooltip("How fast the camera follows (higher = stiffer).")]
        public float cameraFollow = 4f;
        [Tooltip("The camera looks this far ahead of the shuttle (m).")]
        public float cameraLookAhead = 10f;
        [Tooltip("Engine loop pitch at rest and the extra pitch at boost speed.")]
        public float enginePitchIdle = 0.8f;
        public float enginePitchRange = 0.8f;

        [Header("Endings (P72; P12 adds the others)")]
        public string homeTitle = "HOME";
        [TextArea(3, 8)] public string homeBody =
            "The shuttle drops out of the jump above the colony. Lena is waiting at the dock with Pip on her shoulders, Mom right behind them.\n\n" +
            "Everyone is there. Everyone made it.\nKestrel-9 is just a cold little light in the sky now.";
        public string barelyTitle = "HOME, BARELY";
        [TextArea(3, 8)] public string barelyBody =
            "You made it home. The dock smells of rain and engine oil.\n\n" +
            "{names} can hardly stand, and the clinic is the first stop.\nBut you are together.";
        public string lostTitle = "HOME, WITHOUT THEM";
        [TextArea(3, 8)] public string lostBody =
            "The shuttle sets down in the rain. Lena is at the dock, taller than you remember, and she doesn't run.\n\n" +
            "{names} never got to see you come back.\nThe ticket came too late to matter.";
        [Tooltip("P124: heading above the lines about the characters the player met.")]
        public string figuresHeader = "What became of the others:";

        [Header("Closing sentence about the airlock (P12)")]
        [Tooltip("P35: closing sentence when crates were pocketed but nobody went through the airlock.")]
        [TextArea(1, 2)] public string moraleThief = "You never touched the airlock. The crates were another matter.";
        [Tooltip("Nobody went through the airlock in the whole run (Conduct.Airlocks).")]
        public string moraleCleanLine = "You never opened the airlock on anyone. Whatever else you did up there, you did not do that.";
        [Tooltip("The airlock only for those the orders meant: the correct verdict was airlock or arrest (Conduct.InnocentAirlocks = 0).")]
        public string moraleStainedLine = "You opened the airlock when the orders said to. Some nights you still hear it cycle.";
        [Tooltip("The airlock at least once for someone innocent (Conduct.InnocentAirlocks > 0).")]
        public string moraleBloodyLine = "You opened the airlock on people who had done nothing. It cycles in your sleep, and it always will.";
        public string lateTitle = "TOO LATE?";
        [TextArea(3, 8)] public string lateBody =
            "Nobody is waiting at the dock.\n\n" +
            "A neighbor finds you: {names} went to the clinic three nights ago. You saved every credit for the ticket.\nThe ticket was the easy part.";
    }
}
