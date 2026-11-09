using System;
using System.Collections.Generic;
using GameOff.Documents;
using GameOff.Economy;
using GameOff.Travelers;
using UnityEngine;

namespace GameOff.Hangar
{
    /// <summary>P35: one crate in the hangar on one story day: what is hidden in it and whose it is.</summary>
    [Serializable]
    public class CargoCase
    {
        public int day;
        [Tooltip("Exact name of a traveler of that day whose papers are in order.")]
        public string traveler;
        public ItemData item;
        public int count = 1;
    }

    /// <summary>
    /// P35: the flagged crates. The papers stay the truth at the counter: letting the owner through remains right. With the crate
    /// reported, arresting them is right as well. Texts: {name} = the owner, {item} = the goods.
    /// </summary>
    [CreateAssetMenu(menuName = "GameOff/Cargo Cases", fileName = "CargoCases")]
    public class CargoCaseTuning : ScriptableObject
    {
        public List<CargoCase> cases = new List<CargoCase>();
        [Range(0f, 1f), Tooltip("Pocketing the goods: chance of being seen (goods lost, one disciplinary penalty).")]
        public float caughtChance = 0.3f;
        [Tooltip("Morale change for pocketing, caught or not.")]
        public int moraleCost = -1;

        [Header("Days after the story (P126)")]
        [Min(0), Tooltip("A crate is flagged every this many days after the story (0 = never).")]
        public int loopEvery = 3;
        [Tooltip("What can be hidden in those crates.")]
        public List<ItemData> loopGoods = new List<ItemData>();
        [Min(1)] public int loopCount = 2;

        [TextArea(1, 3)] public string letterNotice = "Customs notice: a crate for {name} is flagged in the hangar today. Inspection at the officer's discretion.";
        public string dialogTitle = "CARGO: {name}";
        [TextArea(1, 3)] public string dialogBody = "Under the packing foam: {item}.\nNothing on the manifest says so.";
        public string reportLabel = "REPORT IT", pocketLabel = "POCKET IT", leaveLabel = "LEAVE IT";
        public string alreadySearched = "You have already been through this crate.";
        public string reported = "Reported. The counter will have a slip when {name} comes up.";
        public string pocketed = "Into your pockets: {item}.";
        public string caught = "A loader saw you put it back too late. It's in the report.";
        [TextArea(1, 2)] public string caughtLetter = "Disciplinary notice: tampering with flagged cargo.";
        public string slipReason = "CARGO OF {name}: {item} NOT DECLARED";
        public string slipPenalty = "ARREST AUTHORIZED";

        /// <summary>P126: the crate of a drawn day: its owner is someone whose papers are in order. Always the same for the same day.</summary>
        public CargoCase ForLoopDay(int day, IReadOnlyList<TravelerData> travelers)
        {
            if (loopEvery <= 0 || day % loopEvery != 0 || loopGoods.Count == 0 || travelers == null) return null;
            var clean = new List<TravelerData>();
            foreach (var t in travelers)
                if (t != null && t.IntendedVerdict == Verdict.Admit) clean.Add(t);
            if (clean.Count == 0) return null;
            // The notices speak of the manifest: someone who carries one if there is such a traveler.
            var withManifest = clean.FindAll(t => t.Document(DocumentType.CargoManifest) != null);
            if (withManifest.Count > 0) clean = withManifest;
            return new CargoCase { day = day, traveler = clean[day % clean.Count].DisplayName, item = loopGoods[day % loopGoods.Count], count = loopCount };
        }

        public CargoCase ForDay(int day)
        {
            foreach (var c in cases)
                if (c != null && c.day == day) return c;
            return null;
        }

        public static string Goods(CargoCase c) => c.item != null ? $"{c.item.DisplayName} ×{c.count}" : "";

        public static string Fill(string text, CargoCase c) => text.Replace("{name}", c.traveler).Replace("{item}", Goods(c));
    }
}
