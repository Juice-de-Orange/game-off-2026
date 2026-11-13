using System.Collections.Generic;
using UnityEngine;

namespace GameOff.Rest
{
    /// <summary>
    /// P28/P29 tuning: camera moves for the seats and views, the hall announcements on the benches, the thoughts at the porthole
    /// in room 103 and the handwriting of the letter on the table. Texts: {0} = the family member the thought is about.
    /// </summary>
    [CreateAssetMenu(menuName = "GameOff/Rest Spot Tuning", fileName = "RestSpotTuning")]
    public class RestSpotTuning : ScriptableObject
    {
        [Header("Camera")]
        [Tooltip("Seconds the camera takes to the seat or view and back.")]
        public float moveSeconds = 0.6f;
        [Tooltip("How far the mouse (or A/D, W/S) may turn the view while seated, left/right and up/down in degrees.")]
        public float lookYaw = 70f;
        public float lookPitch = 30f;
        public float mouseDegrees = 0.08f;
        public float keyDegreesPerSecond = 60f;

        [Header("Hall benches")]
        [Tooltip("Seconds between announcements while sitting on a bench (the first comes sooner).")]
        public float announcementSeconds = 14f;
        public float firstAnnouncementSeconds = 2.5f;
        public float subtitleSeconds = 6f;
        [Tooltip("In the evening [F] runs the clock on to 23:00 within these real seconds.")]
        public float fastForwardSeconds = 4f;
        [TextArea(1, 3)] public string[] hallAnnouncements =
        {
            "Attention, please. The observation deck closes at twenty-three hundred. Mind the step.",
            "Passengers for the Aska shuttle: have your papers ready. Officers will not wait for you.",
            "Unattended luggage will be confiscated and inspected. Thank you for your cooperation.",
            "KESTREL-9 thanks you for your patience. Your patience has been noted.",
            "The diner remains closed until further notice. Today: no noodles.",
            "Lost and found: one ration card, one glove, one prosthetic finger. Ask station security.",
        };
        public string eveningOver = "It's late. The hall is empty. Time to sleep.";

        [Header("Porthole, room 103")]
        [Tooltip("Field of view while looking out (the home planet fills more of it).")]
        public float portholeFov = 40f;
        public float thoughtDelay = 1f;
        [TextArea(1, 3)] public string[] thoughtsWell =
        {
            "The lights over home are bright tonight. Somewhere under them Lena is doing her homework.",
            "Pip asked if I can see them from up here. I can see the city. That's almost the same.",
            "Everyone is fine. One more day, and then another.",
        };
        [TextArea(1, 3)] public string[] thoughtsWorried =
        {
            "{0} sounded tired in the letter. A little more food and it will pass.",
            "The city looks dimmer than yesterday. Probably just the glass.",
        };
        [TextArea(1, 3)] public string[] thoughtsSick =
        {
            "{0} needs medicine. I keep doing the sums, and they keep coming out short.",
            "I count the lights down there and lose track. I keep thinking of {0}.",
        };
        [TextArea(1, 3)] public string[] thoughtsCritical =
        {
            "Half the lights are out down there. {0} is somewhere under them.",
            "If {0} doesn't get medicine soon, there won't be another letter about it.",
        };
        [TextArea(1, 3)] public string[] thoughtsGrieving =
        {
            "I used to find our block by its window. I don't look for it anymore.",
            "{0} is not down there anymore. The planet turns anyway.",
        };

        [Header("Letter on the table")]
        [Tooltip("At full shake: how far a letter moves, as a share of its height.")]
        public float letterJitter = 0.12f;
        [Tooltip("At full shake: how far a letter tilts, degrees.")]
        public float letterTilt = 14f;

        /// <summary>
        /// The thought at the porthole tonight: a line for the family's mood, about the person worst off (or the one who died),
        /// a different one each day.
        /// </summary>
        /// <param name="conditions">Household conditions per person.</param>
        /// <param name="names">Family member names, same order.</param>
        public string Thought(IReadOnlyList<int> conditions, IReadOnlyList<string> names, int day)
        {
            int focus = FamilyMood.Focus(conditions);
            string name = names != null && focus >= 0 && focus < names.Count ? names[focus] : "";
            string line = FamilyMood.Pick(Thoughts(FamilyMood.Of(conditions)), day);
            return line.Replace("{0}", name);
        }

        public string[] Thoughts(int mood)
        {
            switch (mood)
            {
                case FamilyMood.Worried: return thoughtsWorried;
                case FamilyMood.Sick: return thoughtsSick;
                case FamilyMood.Critical: return thoughtsCritical;
                case FamilyMood.Grieving: return thoughtsGrieving;
                default: return thoughtsWell;
            }
        }
    }
}
