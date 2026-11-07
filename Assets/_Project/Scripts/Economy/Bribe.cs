using System;
using GameOff.Travelers;

namespace GameOff.Economy
{
    /// <summary>
    /// The barkeeper's bribe note of a day (P66): let this traveler through and get the amount after the shift. Turning him away or
    /// arresting him costs nothing, but the barkeeper sulks: no rumour that evening and the next (Martin, 07.11.).
    /// </summary>
    [Serializable]
    public class Bribe
    {
        public TravelerData traveler;
        [UnityEngine.Min(0)] public int amount;
        [UnityEngine.Tooltip("Text of the note; empty = a standard text with name and amount.")]
        [UnityEngine.TextArea(2, 5)] public string note;

        public bool Active => traveler != null && amount > 0;
        public string Text => !string.IsNullOrWhiteSpace(note) ? note
            : $"Officer -\n\nmy friend {traveler.DisplayName} comes through today. Let him pass and there's {amount} CR for you tonight.\n\nBurn this.\n- L.";
    }
}
