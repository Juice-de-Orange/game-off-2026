using System;
using System.Collections.Generic;
using GameOff.Travelers;

namespace GameOff.Core
{
    /// <summary>
    /// P128: the run as it stands on the morning of a day, before anything of that day happened (JsonUtility, PlayerPrefs).
    /// Items, market events and characters by a stable id (asset name, character id), never by reference. Built and applied by SaveGame.
    /// </summary>
    [Serializable]
    public class SaveData
    {
        /// <summary>Raise when the format changes in a way old saves can't be read; older saves are then ignored.</summary>
        public const int CurrentVersion = 1;

        public int version = CurrentVersion;
        public int day;
        /// <summary>The seed of the endless days (P73).</summary>
        public int seed;

        public int credits;
        public List<ItemStack> inventory = new List<ItemStack>();
        public string wearing = "";
        /// <summary>The evidence locker (P83).</summary>
        public List<ItemStack> locker = new List<ItemStack>();
        /// <summary>What was sold to vendors that they don't normally sell (VendorSupply extras), by stock asset name.</summary>
        public List<VendorShelf> vendors = new List<VendorShelf>();
        /// <summary>P131: the family's stock at home (HomeDelivery), what the drone brought and the nights didn't use up yet.</summary>
        public List<ItemStack> home = new List<ItemStack>();

        /// <summary>Household step per family member (0 well … 4 dead), nights critical in a row, who died last night.</summary>
        public List<int> family = new List<int>();
        public List<int> criticalNights = new List<int>();
        public List<int> diedTonight = new List<int>();

        public int disciplinary;
        public int morale;
        public int airlockMorale;
        /// <summary>Airlock uses over the run and those on the innocent (ticket ending's closing sentence; older saves: 0).</summary>
        public int airlocks;
        public int innocentAirlocks;
        public bool hasTicket;
        public int barkeeperSulksFrom;
        /// <summary>Lines waiting for this morning's letter (character effects, nightmares, the drone, the cargo case).</summary>
        public List<string> letterLines = new List<string>();

        /// <summary>Market events scheduled for later days (P75, Market.Schedule).</summary>
        public List<DayEvents> marketEvents = new List<DayEvents>();
        /// <summary>The station's situation of yesterday (P36); today's crises are added on top in the morning.</summary>
        public float stationMood;

        public CharacterState characters = new CharacterState();

        /// <summary>P132: the cassettes in the tape decks, by deck id (older saves: none).</summary>
        public List<DeckSlots> decks = new List<DeckSlots>();
        /// <summary>P132: ids of the cassettes already picked up on the station.</summary>
        public List<string> pickedUp = new List<string>();

        [Serializable]
        public class ItemStack
        {
            public string item;
            public int count;
        }

        [Serializable]
        public class VendorShelf
        {
            public string stock;
            public List<ItemStack> extras = new List<ItemStack>();
        }

        [Serializable]
        public class DayEvents
        {
            public int day;
            public List<string> events = new List<string>();
        }

        /// <summary>CharacterMemory (P75–P87).</summary>
        [Serializable]
        public class CharacterState
        {
            public List<Record> records = new List<Record>();
            public List<string> flags = new List<string>();
            public List<Entrusted> entrusted = new List<Entrusted>();
            public List<Effect> effects = new List<Effect>();
            public List<Wanted> wanted = new List<Wanted>();
            // (older saves also carry "events": a copy of the market schedule, which marketEvents keeps; JsonUtility skips it)
            public List<Arrest> arrests = new List<Arrest>();
            public List<Kin> kin = new List<Kin>();
        }

        [Serializable]
        public class Record
        {
            public string character;
            public int appearance;
            public int day;
            public int outcome;
        }

        [Serializable]
        public class Entrusted
        {
            public string character;
            public string item;
            public int deposit;
        }

        /// <summary>A consequence due on a later day: appearance and consequence index of that character.</summary>
        [Serializable]
        public class Effect
        {
            public int day;
            public string character;
            public int appearance;
            public int consequence;
        }

        [Serializable]
        public class Wanted
        {
            public string character;
            public WantedEntry entry;
            public int fromDay;
            public int toDay;
        }

        /// <summary>An ordinary traveler the player arrested, or sent out of the airlock (older saves: arrests only).</summary>
        [Serializable]
        public class Arrest
        {
            public string name;
            public string species;
            public int day;
            public bool airlocked;
        }

        [Serializable]
        public class Kin
        {
            public string character;
            public string name;
            public bool byYou;
        }

        /// <summary>P132: one tape deck: cassette asset name per slot, slot 1 first, "" = empty.</summary>
        [Serializable]
        public class DeckSlots
        {
            public string deck;
            public List<string> slots = new List<string>();
        }
    }
}
