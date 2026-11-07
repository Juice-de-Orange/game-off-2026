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
    }

    /// <summary>What an outcome sets in motion: a market event and/or a wanted entry on a later day.</summary>
    [Serializable]
    public class Consequence
    {
        public Outcome when = Outcome.Betrayed;
        [Tooltip("Market event that happens this many days later (pinboard, announcement, rumour, station mood).")]
        public MarketEvent marketEvent;
        [Min(1)] public int eventInDays = 1;
        [Tooltip("Put the character on the wanted list (with the next appearance's papers name).")]
        public bool wanted;
        public WantedOrder wantedOrder = WantedOrder.Arrest;
        [Min(1)] public int wantedInDays = 1;
        [Tooltip("How many days the wanted entry stays on the list.")]
        [Min(1)] public int wantedForDays = 1;
        public string wantedNote = "";
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
#endif
    }
}
