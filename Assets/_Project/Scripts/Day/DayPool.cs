using System;
using System.Collections.Generic;
using GameOff.Economy;
using GameOff.Travelers;
using UnityEngine;

namespace GameOff.Day
{
    /// <summary>
    /// Pools for the endless days after the story (P73): what DayGenerator draws travelers, rules, wanted persons, market events,
    /// bar wishes, bribes and letters from, and how fast the days get harder. Filled by GameOff/Build Day Pool; tweak in the inspector.
    /// </summary>
    [CreateAssetMenu(menuName = "GameOff/Day Pool", fileName = "DayPool")]
    public class DayPool : ScriptableObject
    {
        [Tooltip("P108: the passport photos of generated travelers (set by Build Traveler Photos).")]
        public Travelers.TravelerLooks looks;
        [Serializable]
        public class SpeciesEntry
        {
            public string name = "Human";
            [Tooltip("ID number prefix, e.g. HM → HM-2211-40.")]
            public string idPrefix = "HM";
            public GameObject model;
            public Texture2D photo;
            [Tooltip("Home worlds of this species (ID card origin).")]
            public List<string> origins = new List<string>();
        }

        [Serializable]
        public class CargoLine
        {
            public ItemData item;
            [Tooltip("Text on the manifest after the amount, e.g. \"NOVA COLA CRATE\". Must contain the item's cargo word.")]
            public string line;
            [Tooltip("May be forbidden by a day rule (the item's cargo word becomes the keyword).")]
            public bool canBeForbidden;
        }

        [Serializable]
        public class Letter
        {
            public string title = "A letter from home";
            [TextArea(3, 8)] public string body;
        }

        [Serializable]
        public class FlawLine
        {
            public DayGenerator.Flaw flaw;
            [Tooltip("The document field the line answers, e.g. expires.")]
            public string key;
            [TextArea(1, 2)] public string line;
        }

        [Serializable]
        public class WeightedEvent
        {
            public MarketEvent marketEvent;
            [Min(0f)] public float weight = 1f;
        }

        [Header("People")]
        public List<SpeciesEntry> species = new List<SpeciesEntry>();
        public List<string> givenNames = new List<string>();
        public List<string> familyNames = new List<string>();
        [Tooltip("Stations a route may pass (and a quarantine may close). Must not contain each other or an origin.")]
        public List<string> stops = new List<string>();
        public List<string> purposes = new List<string>();
        public List<string> ships = new List<string>();
        public List<CargoLine> cargo = new List<CargoLine>();
        [TextArea(1, 2)] public List<string> greetings = new List<string>();
        public List<string> crimes = new List<string>();

        [Header("P126: what flawed travelers say when asked; authored answers before the generic ones")]
        public List<FlawLine> flawLines = new List<FlawLine>();

        [Header("Always on the wanted list")]
        public List<WantedEntry> standingWanted = new List<WantedEntry>();

        [Header("Evening and morning")]
        public List<Letter> letters = new List<Letter>();
        public List<WeightedEvent> marketEvents = new List<WeightedEvent>();
        [Range(0f, 1f)] public float eventChance = 0.35f;
        public List<BarWish> barWishes = new List<BarWish>();
        [Range(0f, 1f)] public float barWishChance = 0.5f;
        [Range(0f, 1f)] public float bribeChance = 0.25f;
        [Min(0)] public int bribeMin = 30;
        [Min(0)] public int bribeMax = 60;

        [Header("Difficulty (first loop day → after rampDays)")]
        [Min(1)] public int rampDays = 8;
        [Min(1)] public int travelersStart = 6;
        [Min(1)] public int travelersMax = 8;
        [Min(0)] public int rulesStart = 1;
        [Min(0)] public int rulesMax = 3;
        [Tooltip("Share of travelers with something wrong.")]
        [Range(0f, 1f)] public float flawedStart = 0.4f;
        [Range(0f, 1f)] public float flawedMax = 0.6f;
        [Tooltip("Share of flaws that are hard to see (expired yesterday, one letter or digit off).")]
        [Range(0f, 1f)] public float trickyStart = 0.1f;
        [Range(0f, 1f)] public float trickyMax = 0.5f;
        [Tooltip("Extra wanted persons (besides the standing ones).")]
        [Min(0)] public int wantedMax = 2;
        [Tooltip("Chance a wanted person is to be terminated on sight (airlock) instead of arrested.")]
        [Range(0f, 1f)] public float terminateChance = 0.15f;
    }
}
