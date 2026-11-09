using System;
using System.Collections.Generic;
using GameOff.Economy;
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
