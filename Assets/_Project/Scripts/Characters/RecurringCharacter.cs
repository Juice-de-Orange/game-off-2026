using System;
using System.Collections.Generic;
using GameOff.Economy;
using GameOff.Travelers;
using UnityEngine;

namespace GameOff.Characters
{
    /// <summary>What a decision meant for a recurring character (set per appearance in the script, Martin 07.11.).</summary>
    public enum Outcome
    {
        None = 0,
        Helped = 1,
        Betrayed = 2,
        Neutral = 3,
        /// <summary>Their goods were taken (arrest with confiscation, P08).</summary>
        Confiscated = 4,
        /// <summary>Airlock: the character never comes back.</summary>
        Dead = 5,
    }

    /// <summary>What must have happened before an appearance can take place.</summary>
    public enum AppearanceCondition
    {
        Always = 0,
        /// <summary>Never met before (first appearance).</summary>
        NeverMet = 1,
        /// <summary>Met before and not dead.</summary>
        Alive = 2,
        LastHelped = 3,
        LastBetrayed = 4,
        LastConfiscated = 5,
        /// <summary>Last outcome was betrayed or confiscated (comes back for revenge).</summary>
        LastWronged = 6,
        /// <summary>P87: met, alive, and the appearance's flag is set (e.g. joined the resistance).</summary>
        FlagSet = 7,
        /// <summary>P87: met, alive, and the appearance's flag is not set.</summary>
        FlagNotSet = 8,
    }

    /// <summary>
    /// What an outcome sets in motion: a market event and/or a wanted entry on a later day, effects without another encounter
    /// (gift, credits, announcement, letter line; P85) and a flag for later appearances (P150).
    /// </summary>
    [Serializable]
    public class Consequence
    {
        public Outcome when = Outcome.Betrayed;
        [Tooltip("P85: only when the player chose arrest (e.g. the Shifter's interrogation; deny gives the same outcome).")]
        public bool onlyIfArrested;
        [Tooltip("Market event that happens this many days later (pinboard, announcement, rumour, station mood).")]
        public MarketEvent marketEvent;
        [Min(1)] public int eventInDays = 1;
        [Tooltip("Put the character on the wanted list (with the papers name of their next appearance in that time, RecurringCharacter.WantedName).")]
        public bool wanted;
        public WantedOrder wantedOrder = WantedOrder.Arrest;
        [Min(1)] public int wantedInDays = 1;
        [Tooltip("How many days the wanted entry stays on the list.")]
        [Min(1)] public int wantedForDays = 1;
        public string wantedNote = "";

        [Header("Without another encounter (P85)")]
        [Tooltip("Days after the decision (0 = at once; letters arrive the next morning at the earliest). On a later day, credits and the announcement come at the shift start.")]
        [Min(0)] public int afterDays;
        [Tooltip("Arrives at the family: goes into their stock and covers its need, like a drone delivery (P131).")]
        public ItemData giftItem;
        [Tooltip("The gift (and the letter line that comes with it) only if someone at home is sick.")]
        public bool giftOnlyIfFamilySick;
        [Tooltip("Credits added (or, if negative, taken) in the player's wallet.")]
        public int wageChange;
        public string wageNote = "";
        [TextArea] public string announcement;
        [Tooltip("A line under the next morning letter (with a gift: only when the gift is sent).")]
        [TextArea] public string letterLine;

        [Header("A flag for later appearances (P150)")]
        [Tooltip("Set at once when the decision is made (not after afterDays), e.g. the Shifter is gone: his accomplice comes instead.")]
        public string setFlag;

        /// <summary>This consequence follows from the decision (P85: some only when the player chose arrest).</summary>
        public bool Applies(Outcome outcome, Verdict chosen) => when == outcome && (!onlyIfArrested || chosen == Verdict.Arrest);

        /// <summary>The gift stays away (only for a sick family, and nobody is sick); its letter line stays away with it.</summary>
        public bool GiftHeldBack(bool familySick) => giftItem != null && giftOnlyIfFamilySick && !familySick;
    }

    /// <summary>A line the character adds to the greeting on a day with this market event (they talk about what happens).</summary>
    [Serializable]
    public class EventLine
    {
        public MarketEvent marketEvent;
        public string line;
    }

    /// <summary>One scripted encounter: own papers, when, where in the queue, under which condition, what each decision means.</summary>
    [Serializable]
    public class Appearance
    {
        [Tooltip("Own traveler with own papers for this encounter.")]
        public TravelerData traveler;
        [Tooltip("First day of the window (1-based). Same as Day To = fixed day.")]
        [Min(1)] public int dayFrom = 1;
        [Min(1)] public int dayTo = 1;
        [Tooltip("Place in the queue, 1 = first. 0 = anywhere.")]
        [Min(0)] public int slot;
        public AppearanceCondition condition = AppearanceCondition.Always;
        [Tooltip("P87: the flag FlagSet/FlagNotSet look at, and the flag the question sets when the player accepts.")]
        public string flag;
        [Tooltip("P150: on top of the condition, only if this flag is set (empty = no gate).")]
        public string onlyIfFlag;
        [Tooltip("P150: on top of the condition, not if this flag is set (empty = no gate).")]
        public string unlessFlag;

        [Header("What each decision means")]
        public Outcome onAdmit = Outcome.Helped;
        public Outcome onDeny = Outcome.Betrayed;
        public Outcome onArrest = Outcome.Betrayed;
        [Tooltip("Always Dead in effect (the airlock is final); kept for clarity.")]
        public Outcome onAirlock = Outcome.Dead;

        [Header("Greeting by the last outcome (empty = the traveler's own greeting)")]
        [TextArea] public string greetingAfterHelped;
        [TextArea] public string greetingAfterBetrayed;
        [TextArea] public string greetingAfterConfiscated;
        public List<EventLine> eventLines = new List<EventLine>();

        public List<Consequence> consequences = new List<Consequence>();

        [Header("Left in the player's keeping (P77): given when let through, paid for at the next encounter")]
        public ItemData entrustItem;
        public int entrustDeposit;

        [Header("A thank-you the player keeps (P132): into the inventory when let through")]
        [Tooltip("E.g. a cassette. Not taken back later, unlike the entrusted item.")]
        public ItemData keepsake;
        [Tooltip("Shown as a hint when it is handed over.")]
        public string keepsakeHint;

        [Header("A question at the counter (P87): accepting sets the flag")]
        public string questionTitle;
        [TextArea] public string questionBody;
        public string acceptLabel = "YES";
        public string declineLabel = "NO";
        [TextArea] public string acceptLine;
        [TextArea] public string declineLine;

        [Header("The run ends at this encounter (P87)")]
        public string endingTitle;
        [TextArea] public string endingBody;
        [Tooltip("Shown as a hint a moment before the end screen.")]
        public string endingWarning;

        public bool HasQuestion => !string.IsNullOrWhiteSpace(questionTitle);
        public bool EndsRun => !string.IsNullOrWhiteSpace(endingTitle);

        public bool InWindow(int day) => day >= dayFrom && day <= Mathf.Max(dayFrom, dayTo);

        public Outcome OutcomeOf(Verdict verdict) => verdict switch
        {
            Verdict.Admit => onAdmit,
            Verdict.Deny => onDeny,
            Verdict.Arrest => onArrest,
            _ => Outcome.Dead, // the airlock is always final
        };
    }

    /// <summary>
    /// P75: a recurring character (content in P76, plan in docs/FIGUREN.md). Comes back scripted with leeway, remembers how they
    /// were treated (CharacterMemory) and can change the station (market events, wanted list).
    /// </summary>
    [CreateAssetMenu(menuName = "GameOff/Recurring Character", fileName = "Char_")]
    public class RecurringCharacter : ScriptableObject
    {
        [SerializeField] private string id = "char";
        [SerializeField] private string displayName = "Character";
        [SerializeField] private string species = "Human";
        [SerializeField] private List<Appearance> appearances = new List<Appearance>();

        [Header("A relative in the station prison (P85): {kin} and {kinFate} in greetings and the summary")]
        [Tooltip("Who it is if the player arrested nobody fitting. This name first, then the latest arrest of the same species.")]
        [SerializeField] private string kinName;
        [SerializeField] private string kinSpecies = "Human";
        [Tooltip("{kinFate} when the player made the arrest.")]
        [SerializeField, TextArea] private string kinFateByYou;
        [Tooltip("{kinFate} when somebody else did.")]
        [SerializeField, TextArea] private string kinFateByOthers;
        [Tooltip("{kinFate} when the player sent them out of the airlock (empty = the line for an arrest by the player).")]
        [SerializeField, TextArea] private string kinFateAirlocked;

        [Header("What became of them (P85: day 11 newsletter, P124: ticket ending), by the last outcome; empty = not mentioned")]
        [SerializeField, TextArea] private string summaryHelped;
        [SerializeField, TextArea] private string summaryBetrayed;
        [SerializeField, TextArea] private string summaryNeutral;
        [SerializeField, TextArea] private string summaryDead;
        [Tooltip("The day of their last appearance: until it is played their story is still open (0 = always finished).")]
        [SerializeField, Min(0)] private int storyEndsOnDay;
        [Tooltip("The ending's line while their story is still open; empty = \"You never learned what became of <Name>.\"")]
        [SerializeField, TextArea] private string summaryOpen;

        public string KinName => kinName;
        public string KinSpecies => kinSpecies;
        public bool HasKin => !string.IsNullOrWhiteSpace(kinName);
        public string KinFate(bool byYou) => byYou ? kinFateByYou : kinFateByOthers;

        /// <summary>{kinFate}: who took the relative; airlocked by the player has its own line (else the by-you line).</summary>
        public string KinFate(bool byYou, bool airlocked) =>
            byYou && airlocked && !string.IsNullOrWhiteSpace(kinFateAirlocked) ? kinFateAirlocked : KinFate(byYou);

        public int StoryEndsOnDay => storyEndsOnDay;

        /// <summary>
        /// Their story is told to the end once every day up to their last appearance has been played (<paramref name="playedThrough"/>
        /// = the last day whose encounters are over), or once they are dead.
        /// </summary>
        public bool StoryFinished(Outcome last, int playedThrough) => last == Outcome.Dead || playedThrough >= storyEndsOnDay;

        /// <summary>The line for a story still open: their own, else "You never learned what became of Nell Varga."</summary>
        public string OpenLine => !string.IsNullOrWhiteSpace(summaryOpen) ? summaryOpen : $"You never learned what became of {TitleCase(displayName)}.";

        /// <summary>"NELL VARGA" → "Nell Varga".</summary>
        public static string TitleCase(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "";
            var words = name.Trim().Split(' ');
            for (int i = 0; i < words.Length; i++)
                if (words[i].Length > 0) words[i] = words[i].Substring(0, 1).ToUpperInvariant() + words[i].Substring(1).ToLowerInvariant();
            return string.Join(" ", words);
        }

        /// <summary>
        /// The name on a wanted entry that appearance <paramref name="fromAppearance"/> puts up for fromDay–toDay: the papers name of their
        /// first later appearance in that time (the Shifter changes names), else the character's name.
        /// </summary>
        public string WantedName(int fromAppearance, int fromDay, int toDay)
        {
            for (int i = fromAppearance + 1; i < appearances.Count; i++)
            {
                var a = appearances[i];
                if (a.traveler != null && a.dayFrom <= toDay && Mathf.Max(a.dayFrom, a.dayTo) >= fromDay) return a.traveler.DisplayName;
            }
            return displayName;
        }

        /// <summary>P85: the summary line for the last outcome (confiscated counts as betrayed), or null.</summary>
        public string SummaryFor(Outcome last) => last switch
        {
            Outcome.Helped => summaryHelped,
            Outcome.Betrayed or Outcome.Confiscated => summaryBetrayed,
            Outcome.Neutral => summaryNeutral,
            Outcome.Dead => summaryDead,
            _ => null,
        };

        public string Id => id;
        public string DisplayName => displayName;
        public string Species => species;
        public IReadOnlyList<Appearance> Appearances => appearances;

#if UNITY_EDITOR
        public void EditorSet(string newId, string newName, string newSpecies, List<Appearance> newAppearances)
        {
            id = newId;
            displayName = newName;
            species = newSpecies;
            appearances = newAppearances;
        }

        public void EditorSetKin(string name, string species, string fateByYou, string fateByOthers, string fateAirlocked = null)
        {
            kinName = name;
            kinSpecies = species;
            kinFateByYou = fateByYou;
            kinFateByOthers = fateByOthers;
            kinFateAirlocked = fateAirlocked;
        }

        /// <summary>The day of their last appearance, and their own line for a story still open (null = the default line).</summary>
        public void EditorSetStoryEnd(int lastDay, string openLine = null)
        {
            storyEndsOnDay = lastDay;
            summaryOpen = openLine;
        }

        public void EditorSetSummary(string helped, string betrayed, string neutral, string dead)
        {
            summaryHelped = helped;
            summaryBetrayed = betrayed;
            summaryNeutral = neutral;
            summaryDead = dead;
        }
#endif
    }
}
