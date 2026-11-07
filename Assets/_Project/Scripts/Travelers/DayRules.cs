using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace GameOff.Travelers
{
    /// <summary>
    /// One day at the counter (P07): the date (for expiry), the bulletin rules, the wanted list and the travelers in order.
    /// Rules are polymorphic ([SerializeReference]), so a day can combine any rule types.
    /// </summary>
    [CreateAssetMenu(menuName = "GameOff/Day Rules", fileName = "Day_")]
    public class DayRules : ScriptableObject
    {
        public const string DateFormat = "yyyy-MM-dd";

        [Tooltip("Today's date, " + DateFormat + ". Papers expiring before it are expired.")]
        [SerializeField] private string date = "2290-11-05";
        [SerializeReference] private List<DayRule> rules = new List<DayRule>();
        [SerializeField] private List<WantedEntry> wanted = new List<WantedEntry>();
        [SerializeField] private List<TravelerData> travelers = new List<TravelerData>();

        public string Date => date;
        public IReadOnlyList<DayRule> Rules => rules;
        public IReadOnlyList<WantedEntry> Wanted => wanted;
        public IReadOnlyList<TravelerData> Travelers => travelers;

        public static bool TryParseDate(string text, out DateTime value) =>
            DateTime.TryParseExact(text?.Trim(), DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out value);

        /// <summary>
        /// P75: a copy of this day for one run, with another traveler queue and extra wanted entries (recurring characters).
        /// The asset itself stays untouched.
        /// </summary>
        public DayRules RuntimeCopy(List<TravelerData> newTravelers, List<WantedEntry> extraWanted)
        {
            var copy = Instantiate(this);
            copy.name = name;
            copy.travelers = newTravelers ?? new List<TravelerData>(travelers);
            copy.wanted = new List<WantedEntry>(wanted);
            if (extraWanted != null) copy.wanted.AddRange(extraWanted);
            return copy;
        }

#if UNITY_EDITOR
        /// <summary>Editor setup (content builders and tests).</summary>
        public void EditorSet(string newDate, List<DayRule> newRules, List<WantedEntry> newWanted, List<TravelerData> newTravelers)
        {
            date = newDate;
            rules = newRules;
            wanted = newWanted;
            travelers = newTravelers;
        }
#endif
    }
}
