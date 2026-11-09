using System.Collections.Generic;
using UnityEngine;

namespace GameOff.Shops
{
    /// <summary>P32: how the evidence locker opens and how the goods inside are shown. No money in here.</summary>
    [CreateAssetMenu(menuName = "GameOff/Shops/Evidence Locker Tuning", fileName = "EvidenceLockerTuning")]
    public class EvidenceLockerTuning : ScriptableObject
    {
        [Header("Doors")]
        [Tooltip("How far each door swings open, in degrees.")]
        public float doorOpenAngle = 105f;
        [Tooltip("Seconds to swing fully open or shut.")]
        public float doorSeconds = 0.6f;
        [Tooltip("The doors close by themselves when the player is further away than this (metres).")]
        public float autoCloseDistance = 3f;

        [Header("Goods")]
        [Tooltip("At most this many copies of one item's model in its compartment (the count can be higher).")]
        public int maxCopiesPerSlot = 3;
        [Tooltip("Space for the goods of one compartment (metres): width, height, depth. Models are scaled to fit.")]
        public Vector3 slotSize = new Vector3(0.4f, 0.42f, 0.36f);
        [Tooltip("Small models may grow at most by this factor to be seen (1 = never larger than real).")]
        public float maxUpscale = 1.6f;
        [Tooltip("Goods without a model of their own: the first label in this list that the item has decides the model. " +
                 "None fits = a sealed evidence bag.")]
        public List<LockerTagModel> tagModels = new List<LockerTagModel>();
    }
}
