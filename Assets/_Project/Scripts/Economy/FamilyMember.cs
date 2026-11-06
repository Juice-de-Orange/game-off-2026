using System;

namespace GameOff.Economy
{
    /// <summary>One person of the family at home (P10). Names are placeholders until P15 writes the story.</summary>
    [Serializable]
    public class FamilyMember
    {
        public string name = "Mom";
        public string role = "your wife";
        [UnityEngine.Tooltip("Gets worse on a night without heating.")]
        public bool feelsCold;
    }
}
