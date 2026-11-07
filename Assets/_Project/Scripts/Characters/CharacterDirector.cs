using System.Collections.Generic;
using System.Text;
using GameOff.Day;
using GameOff.Economy;
using GameOff.Travelers;
using UnityEngine;

namespace GameOff.Characters
{
    /// <summary>
    /// P75 in the game: each morning puts the due recurring characters into the counter queue and their wanted entries on the list
    /// (CounterShift.PlanDay), lets them greet by what happened last time (TravelerPresenter.GreetingFor), remembers every decision
    /// and books its consequences: market events for a later day (Market.Schedule, so pinboard, announcement, rumour and station
    /// mood work as for fixed events) and wanted entries. Content: P76; plan: docs/FIGUREN.md.
    /// </summary>
    public class CharacterDirector : MonoBehaviour
    {
        [SerializeField] private CounterShift shift;
        [SerializeField] private TravelerPresenter presenter;
        [SerializeField] private DayManager dayManager;
        [Tooltip("The recurring characters of the story (P76).")]
        [SerializeField] private List<RecurringCharacter> characters = new List<RecurringCharacter>();
        [Tooltip("Only while the dev tools are on (e.g. the P75 test character).")]
        [SerializeField] private List<RecurringCharacter> devCharacters = new List<RecurringCharacter>();
        [Tooltip("At most this many character encounters a day (docs/FIGUREN.md: 3).")]
        [SerializeField] private int maxPerDay = 3;

        private readonly Dictionary<TravelerData, CharacterPlanner.Planned> _today = new Dictionary<TravelerData, CharacterPlanner.Planned>();

        public CharacterMemory Memory { get; private set; } = new CharacterMemory();

        private List<RecurringCharacter> Active
        {
            get
            {
                var list = new List<RecurringCharacter>(characters);
                if (dayManager == null || dayManager.DevTools) list.AddRange(devCharacters);
                return list;
            }
        }

        private void OnEnable()
        {
            if (shift != null)
            {
                shift.PlanDay = Plan;
                shift.DecisionMade += OnDecision;
            }
            if (presenter != null) presenter.GreetingFor = Greeting;
        }

        private void OnDisable()
        {
            if (shift != null)
            {
                if (shift.PlanDay == Plan) shift.PlanDay = null;
                shift.DecisionMade -= OnDecision;
            }
            if (presenter != null && presenter.GreetingFor == Greeting) presenter.GreetingFor = null;
        }

        /// <summary>CounterShift.BeginDay: today's rules with the characters in the queue and their wanted entries.</summary>
        private DayRules Plan(DayRules rules)
        {
            int day = Market.Day; // DayManager runs Market.Begin(day) right before the counter's BeginDay
            if (day <= 1) Memory = new CharacterMemory(); // a new run
            _today.Clear();
            if (rules == null) return null;
            var due = CharacterPlanner.Due(day, Active, Memory, maxPerDay, () => Random.value);
            var wanted = Memory.WantedOn(day);
            if (due.Count == 0 && wanted.Count == 0) return rules;
            foreach (var p in due)
            {
                _today[p.traveler] = p;
                Debug.Log($"[Characters] day {day}: {p.character.DisplayName} (appearance {p.appearance + 1})");
            }
            return rules.RuntimeCopy(CharacterPlanner.Queue(rules.Travelers, due, () => Random.value), wanted);
        }

        private void OnDecision(TravelerData traveler, Verdict chosen, Judgement judgement)
        {
            if (traveler == null || !_today.TryGetValue(traveler, out var p)) return;
            int day = Market.Day;
            var a = p.character.Appearances[p.appearance];
            var outcome = a.OutcomeOf(chosen);
            Memory.Remember(p.character, p.appearance, day, outcome);
            Debug.Log($"[Characters] {p.character.DisplayName}: {chosen} → {outcome}");
            foreach (var c in a.consequences)
            {
                if (c.when != outcome) continue;
                if (c.marketEvent != null)
                {
                    Memory.ScheduleEvent(day + c.eventInDays, c.marketEvent);
                    Market.Schedule(day + c.eventInDays, c.marketEvent);
                }
                if (c.wanted)
                    Memory.ScheduleWanted(p.character, new WantedEntry(p.character.DisplayName, "", c.wantedOrder, c.wantedNote),
                        day + c.wantedInDays, c.wantedForDays);
            }
        }

        /// <summary>TravelerPresenter: the greeting of a character by the last outcome, plus a line about today's events.</summary>
        public string Greeting(TravelerData traveler)
        {
            if (traveler == null || !_today.TryGetValue(traveler, out var p)) return traveler != null ? traveler.Greeting : "";
            var a = p.character.Appearances[p.appearance];
            string text = Memory.Last(p.character) switch
            {
                Outcome.Helped => a.greetingAfterHelped,
                Outcome.Betrayed => a.greetingAfterBetrayed,
                Outcome.Confiscated => a.greetingAfterConfiscated,
                _ => null,
            };
            var sb = new StringBuilder(string.IsNullOrWhiteSpace(text) ? traveler.Greeting : text);
            foreach (var line in a.eventLines)
            {
                if (line.marketEvent == null || string.IsNullOrWhiteSpace(line.line)) continue;
                foreach (var e in Market.TodayEvents)
                    if (e == line.marketEvent) { if (sb.Length > 0) sb.Append(' '); sb.Append(line.line); }
            }
            return sb.ToString();
        }
    }
}
