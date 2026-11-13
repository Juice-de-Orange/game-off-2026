using System.Collections.Generic;
using UnityEngine;

namespace GameOff.Counter
{
    /// <summary>Timing of the traveler in the inspection lock and their speech bubble (P40, P143).</summary>
    [CreateAssetMenu(menuName = "GameOff/Counter/Dialogue Tuning", fileName = "DialogueTuning")]
    public class DialogueTuning : ScriptableObject
    {
        [Tooltip("Pause between the traveler appearing and the greeting (s).")]
        public float greetingDelay = 0.6f;
        [Tooltip("Alien speech: how long the translation wave takes per text line (s).")]
        public float translateSecondsPerLine = 0.4f;
        [Tooltip("Translated characters still shown in cyan at the wave's edge.")]
        public int translateEdge = 3;
        [Tooltip("A bubble stays at least this long after the text is readable (s).")]
        public float minHoldSeconds = 2.5f;
        [Tooltip("Extra time per character of text (s).")]
        public float holdSecondsPerCharacter = 0.04f;
        [Tooltip("P143: a bubble fades in and out this fast (s, unscaled).")]
        public float bubbleFadeSeconds = 0.15f;
        [Tooltip("P143: human speech types on like a terminal, this many characters per second (0 = at once).")]
        public float typeCharactersPerSecond = 50f;
        [Tooltip("P143: the block cursor blinks on and off this long while a line is read (s).")]
        public float cursorBlinkSeconds = 0.4f;
        [Tooltip("P143: after the decision, the pause between the last line fading out and the traveler's reaction (s).")]
        public float reactionPauseSeconds = 0.25f;
        [Tooltip("P91: the door opens this long before someone walks through it (s).")]
        public float doorLeadSeconds = 0.4f;
        [Tooltip("P91: how far behind the lock doors a traveler starts and ends, out of sight (m).")]
        public float offstageMeters = 0.9f;
        [Tooltip("P91: time to turn toward the counter after arriving (s).")]
        public float turnSeconds = 0.3f;
        [Tooltip("P91: how long the shutter takes to fall (s).")]
        public float shutterDropSeconds = 0.25f;
        [Tooltip("P91: how long the shutter takes to rise again (s).")]
        public float shutterRiseSeconds = 1.1f;
        [Tooltip("P91: after an arrest, how long the shutter stays down (s). The airlock has no shutter since P105.")]
        public float shutterHoldSeconds = 3f;
        [Tooltip("Crossfade between Idle, Talk and Walk (s).")]
        public float animationBlendSeconds = 0.2f;
        [Tooltip("Walking speed through the lock (m/s).")]
        public float walkSpeed = 1.4f;
        [Tooltip("P104: the guard is in a hurry (m/s).")]
        public float guardWalkSpeed = 2.8f;
        [Tooltip("P104: where the guard stops, this far from the traveler (m).")]
        public float guardStopMeters = 0.7f;
        [Tooltip("P105: how long the outer hatch takes to swing open and shut (s).")]
        public float hatchSeconds = 0.5f;
        [Tooltip("P105: how long the lock empties before the hatch shuts again (s).")]
        public float airlockBlowSeconds = 2.2f;
        [Tooltip("P105: acceleration of every limb toward the hatch (m/s²).")]
        public float airlockSuction = 45f;

        [Header("What travelers say (P100, P101)")]
        public List<SpeciesLines> speciesLines = new List<SpeciesLines>
        {
            new SpeciesLines
            {
                species = "Human",
                denied = new[] { "You can't be serious.", "Who do I even complain to?", "Nine hours in that queue for this?", "Fine. Enjoy your little booth." },
                admitted = new[] { "Thanks. Long day.", "Finally.", "Have a good shift, officer.", "Knew it was fine." },
                evening = new[] { "Finally off that shuttle.", "Room's small. Bar's not bad.", "You're the one from the booth, aren't you?", "Long way, but I'm here." },
            },
            new SpeciesLines
            {
                species = "Vellani",
                denied = new[] { "Oh. I see. I will go back.", "But I was so careful.", "You are making a mistake, gently.", "Please reconsider. No? No." },
                admitted = new[] { "You are kind. Thank you.", "Thank you, thank you.", "May your shift be short.", "Soft landings, officer." },
                evening = new[] { "It is quieter here than I feared.", "Your station hums. I like it.", "You let me through. I remember.", "I will sleep well tonight." },
            },
            new SpeciesLines
            {
                species = "Quobb",
                denied = new[] { "Oh, that's very disappointing!", "No? My whole pod will hear about this.", "This is not fun at all, officer.", "Rude! Politely, but rude." },
                admitted = new[] { "Wonderful! Wonderful!", "Best officer on the station!", "Hooray, hooray.", "Happy waters to you." },
                evening = new[] { "What a lovely little station!", "Have you tried the drinks? I have tried them all.", "Everyone here is so serious!", "Happy waters, officer." },
            },
            new SpeciesLines
            {
                species = "Xirrik",
                denied = new[] { "Kss. Your papers are wrong, not mine.", "Kss! Bureaucrat.", "Kss. I remember faces.", "Kss. Wasted tide." },
                admitted = new[] { "Kss. Good.", "Kss. Efficient.", "Kss. Another day.", "Kss. No trouble then." },
                evening = new[] { "Kss. You again.", "Kss. Warm enough here. Barely.", "Kss. Do not stare.", "Kss. Tomorrow I am gone." },
            },
            new SpeciesLines
            {
                species = "Sorvak",
                denied = new[] { "Hrm. You will regret the paperwork.", "Hrm. Weak stamp.", "Hrm. Another time, officer.", "Bad stamp. Bad day." },
                admitted = new[] { "Hrm. Correct.", "Hrm. Good.", "Stamp holds. Go on.", "Hrm. We move." },
                evening = new[] { "Hrm. Drinks are thin here.", "Hrm. Your stamp held.", "Quiet. Good.", "Hrm. We leave at first light." },
            },
        };
    }
}
