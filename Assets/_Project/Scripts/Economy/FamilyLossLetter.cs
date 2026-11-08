using System;
using UnityEngine;

namespace GameOff.Economy
{
    /// <summary>P69: the letters home after a family member died.</summary>
    [Serializable]
    public class FamilyLossLetter
    {
        public string name;
        [TextArea(2, 5)] public string firstMorning;
        [TextArea(2, 5)] public string afterwards;
    }
}
