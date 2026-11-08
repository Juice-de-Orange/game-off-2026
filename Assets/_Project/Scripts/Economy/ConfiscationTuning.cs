using System.Collections.Generic;
using UnityEngine;

namespace GameOff.Economy
{
    /// <summary>
    /// P08 numbers (call 07.11., all provisional – they change with the balancing): what an arrest confiscates, the three choices
    /// at shift end, disciplinary penalties and morale. Every change to money here needs a new Economy Report.
    /// </summary>
    [CreateAssetMenu(menuName = "GameOff/Economy/Confiscation Tuning", fileName = "ConfiscationTuning")]
    public class ConfiscationTuning : ScriptableObject
    {
        [Header("Confiscation (arrest)")]
        [Tooltip("Goods that cargo lines can name (by ItemData.cargoWord). Filled by the EconomyBuilder.")]
        public List<ItemData> goods = new List<ItemData>();
        [Tooltip("At most this many of one item per arrest (0 = as many as the cargo says). To be matched with P15's cargo amounts.")]
        public int maxPerItem = 3;

        [Header("\"Nothing to report\": keep everything")]
        [Range(0f, 1f)] public float keepAllCaughtChance = 0.8f;
        public int keepAllFine = 100;

        [Header("\"Oh, seems I missed that\": keep one item")]
        [Range(0f, 1f)] public float missedSuccessChance = 0.6f;
        public int missedFine = 30;

        [Header("Breaking into the evidence locker (P83)")]
        [Range(0f, 1f)] public float lockerCaughtChance = 0.35f;

        [Header("Disciplinary penalties (separate from the daily warnings, P07)")]
        [Tooltip("This many over the run and the officer is discharged (game over, ending in P12).")]
        public int disciplinaryLimit = 3;

        [Header("Morale (hidden, read by the endings P12)")]
        [Tooltip("Airlock on someone who had it coming (ordered or deserved).")]
        public int airlockGuilty = -1;
        [Tooltip("Airlock on someone who should have been admitted, denied or arrested.")]
        public int airlockInnocent = -3;
    }
}
