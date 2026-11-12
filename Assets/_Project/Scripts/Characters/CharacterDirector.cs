using System.Collections;
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
        [Tooltip("P85: says the announcements of consequences.")]
        [SerializeField] private GameOff.Counter.Announcer announcer;
        [Tooltip("The recurring characters of the story (P76).")]
        [SerializeField] private List<RecurringCharacter> characters = new List<RecurringCharacter>();
        [Tooltip("Only while the dev tools are on (e.g. the P75 test character).")]
        [SerializeField] private List<RecurringCharacter> devCharacters = new List<RecurringCharacter>();
        [Tooltip("At most this many character encounters a day (docs/FIGUREN.md: 3).")]
        [SerializeField] private int maxPerDay = 3;
        [Tooltip("P87: seconds after a character arrives before their question opens (time for the greeting).")]
        [SerializeField] private float questionDelay = 4f;
        [Tooltip("P87: seconds after a character arrives before the run ends at them (time for the reveal).")]
        [SerializeField] private float endingDelay = 7f;
        [Tooltip("P85: on this morning the letter sums up what became of every character met so far (0 = never).")]
        [SerializeField] private int summaryDay = 11;
        [SerializeField, TextArea] private string summaryHeader = "Your mother cut this out of the station newsletter. She says you should know how it all turned out:";

        private float WalkInSeconds => shift.ArrivalDelay != null ? shift.ArrivalDelay() : 0f;

        private readonly Dictionary<TravelerData, CharacterPlanner.Planned> _today = new Dictionary<TravelerData, CharacterPlanner.Planned>();

        public CharacterMemory Memory { get; private set; } = new CharacterMemory();

        /// <summary>P128: the save reads and restores the memory.</summary>
        public static CharacterDirector Instance { get; private set; }

        /// <summary>P128: every character this run can meet (incl. the dev ones while the dev tools are on), to find them by id.</summary>
        public IReadOnlyList<RecurringCharacter> Characters => Active;

        private List<RecurringCharacter> Active
        {
            get
            {
                var list = new List<RecurringCharacter>(characters);
                if (DayManager.DevToolsOn(dayManager)) list.AddRange(devCharacters); // P146: never in the release build
                return list;
            }
        }

        private void OnEnable()
        {
            Instance = this;
            if (shift != null)
            {
                shift.PlanDay = Plan;
                shift.DecisionMade += OnDecision;
                shift.TravelerArrived += OnArrived;
            }
            if (presenter != null) presenter.GreetingFor = Greeting;
        }

        private void OnDisable()
        {
            if (Instance == this) Instance = null;
            if (shift != null)
            {
                if (shift.PlanDay == Plan) shift.PlanDay = null;
                shift.DecisionMade -= OnDecision;
                shift.TravelerArrived -= OnArrived;
            }
            if (presenter != null && presenter.GreetingFor == Greeting) presenter.GreetingFor = null;
        }

        /// <summary>CounterShift.BeginDay: today's rules with the characters in the queue and their wanted entries.</summary>
        private DayRules Plan(DayRules rules)
        {
            int day = Market.Day; // DayManager runs Market.Begin(day) right before the counter's BeginDay
            if (day <= 1) Memory = new CharacterMemory(); // a new run
            _today.Clear();
            _returned.Clear();
            foreach (var effect in Memory.TakeEffects(day)) Apply(effect);
            if (day == summaryDay) Summarize(day);
            if (rules == null) return null;
            var due = CharacterPlanner.Due(day, Active, Memory, maxPerDay, () => Random.value);
            var wanted = Memory.WantedOn(day);
            if (due.Count == 0 && wanted.Count == 0) return rules;
            foreach (var p in due)
            {
                _today[p.traveler] = p;
                Collect(p);
                Debug.Log($"[Characters] day {day}: {p.character.DisplayName} (appearance {p.appearance + 1})");
            }
            return rules.RuntimeCopy(CharacterPlanner.Queue(rules.Travelers, due, () => Random.value), wanted);
        }

        private void OnDecision(TravelerData traveler, Verdict chosen, Judgement judgement)
        {
            if (traveler == null) return;
            int day = Market.Day;
            if (!_today.TryGetValue(traveler, out var p))
            {
                if (chosen == Verdict.Arrest) Memory.RememberArrest(traveler.DisplayName, traveler.Species, day); // P85: maybe someone's son
                return;
            }
            var a = p.character.Appearances[p.appearance];
            var outcome = a.OutcomeOf(chosen);
            if (outcome == Outcome.Helped && _returned.TryGetValue(traveler, out bool kept) && !kept) outcome = Outcome.Betrayed; // P77: sold what they left
            Memory.Decide(p.character, p.appearance, day, outcome, chosen); // P150: also the flags its consequences set
            Debug.Log($"[Characters] {p.character.DisplayName}: {chosen} → {outcome}");
            foreach (var c in a.consequences)
            {
                if (!c.Applies(outcome, chosen)) continue;
                if (c.marketEvent != null)
                {
                    Memory.ScheduleEvent(day + c.eventInDays, c.marketEvent);
                    Market.Schedule(day + c.eventInDays, c.marketEvent);
                }
                if (c.wanted)
                    Memory.ScheduleWanted(p.character, new WantedEntry(p.character.DisplayName, "", c.wantedOrder, c.wantedNote),
                        day + c.wantedInDays, c.wantedForDays);
                if (c.afterDays > 0) Memory.ScheduleEffect(day + c.afterDays, c);
                else Apply(c);
            }
            var economy = PlayerEconomy.Instance;
            if (outcome == Outcome.Helped && a.entrustItem != null && economy != null && economy.Inventory.CanAdd(a.entrustItem))
            {
                economy.Inventory.Add(a.entrustItem);
                Memory.Entrust(p.character, a.entrustItem, a.entrustDeposit);
                if (dayManager != null && dayManager.UI != null)
                    dayManager.UI.ShowHint($"{p.character.DisplayName} left {a.entrustItem.DisplayName} with you. Keep it until they are back.", 5f);
            }
            // P132: a small thank-you to keep (a cassette).
            if (outcome == Outcome.Helped && a.keepsake != null && economy != null && economy.Inventory.CanAdd(a.keepsake)
                && economy.Inventory.Count(a.keepsake) == 0 && !GameOff.Rest.CassetteDeck.AnyHolds(a.keepsake))
            {
                economy.Inventory.Add(a.keepsake);
                if (dayManager != null && dayManager.UI != null && !string.IsNullOrWhiteSpace(a.keepsakeHint))
                    dayManager.UI.ShowHint(a.keepsakeHint, 6f);
            }
        }

        // ---------- P85 and P77 ----------

        private void Apply(Consequence c)
        {
            var economy = PlayerEconomy.Instance;
            if (economy == null) return;
            if (c.giftItem != null && (!c.giftOnlyIfFamilySick || economy.Household.AnySick)) economy.Delivery.AddGift(c.giftItem);
            if (c.wageChange > 0) economy.Wallet.Add(c.wageChange, c.wageNote);
            else if (c.wageChange < 0) economy.Wallet.Charge(-c.wageChange, c.wageNote);
            if (!string.IsNullOrWhiteSpace(c.letterLine)) economy.AddLetterLine(c.letterLine);
            if (!string.IsNullOrWhiteSpace(c.announcement) && announcer != null) announcer.Announce(c.announcement);
        }

        /// <summary>P77: a returning character collects what they left: kept → deposit paid, gone → they count as betrayed.</summary>
        private void Collect(CharacterPlanner.Planned p)
        {
            var economy = PlayerEconomy.Instance;
            if (economy == null || !Memory.TakeEntrusted(p.character, out var item, out int deposit)) return;
            bool kept = economy.Inventory.Remove(item);
            _returned[p.traveler] = kept;
            if (kept) economy.Wallet.Add(deposit, $"Deposit · {p.character.DisplayName}");
            else Memory.Remember(p.character, p.appearance, Market.Day, Outcome.Betrayed);
            Debug.Log($"[Characters] {p.character.DisplayName} collects {item.DisplayName}: {(kept ? "kept, paid " + deposit : "gone")}");
        }

        private readonly Dictionary<TravelerData, bool> _returned = new Dictionary<TravelerData, bool>();

        // ---------- P87: questions and endings at the counter ----------

        private void OnArrived(TravelerData traveler)
        {
            if (traveler == null || !_today.TryGetValue(traveler, out var p)) return;
            var a = p.character.Appearances[p.appearance];
            if (a.EndsRun) StartCoroutine(EndRun(traveler, a));
            else if (a.HasQuestion) StartCoroutine(Ask(traveler, a));
        }

        private IEnumerator Ask(TravelerData traveler, Appearance a)
        {
            yield return new WaitForSeconds(questionDelay + WalkInSeconds); // P91: counted from when they stand at the window
            if (shift.Current != traveler || dayManager == null || dayManager.UI == null) yield break; // already decided
            var station = shift.Station;
            if (station != null) station.SetInputBlocked(true);
            void Answer(bool accepted)
            {
                if (station != null) station.SetInputBlocked(false);
                if (accepted) Memory.SetFlag(a.flag);
                Debug.Log($"[Characters] {traveler.DisplayName}: \"{a.questionTitle}\" → {(accepted ? "accepted" : "declined")}");
                if (presenter != null) presenter.SayLine(accepted ? a.acceptLine : a.declineLine);
            }
            dayManager.UI.ShowDialog(a.questionTitle, a.questionBody, a.acceptLabel, () => Answer(true), null, () => Answer(false),
                cancelLabel: a.declineLabel);
        }

        private IEnumerator EndRun(TravelerData traveler, Appearance a)
        {
            var station = shift.Station;
            yield return new WaitForSeconds(endingDelay + WalkInSeconds);
            if (station != null) station.SetInputBlocked(true);
            if (dayManager != null && dayManager.UI != null && !string.IsNullOrWhiteSpace(a.endingWarning)) dayManager.UI.ShowHint(a.endingWarning, 6f);
            yield return new WaitForSeconds(3f);
            Debug.Log($"[Characters] the run ends at {traveler.DisplayName}: {a.endingTitle}");
            if (dayManager != null) dayManager.FinishRun(a.endingTitle, a.endingBody);
        }

        // ---------- P85: a relative in prison, the summary on day 11 ----------

        /// <summary>Replaces {kin} (first name of the character's relative in prison) and {kinFate} (who put them there).</summary>
        private string Fill(RecurringCharacter c, string text, int day) => Fill(c, text, Memory, day);

        public static string Fill(RecurringCharacter c, string text, CharacterMemory memory, int day)
        {
            if (string.IsNullOrEmpty(text) || !text.Contains("{kin")) return text;
            var (name, byYou) = memory.Kin(c, day);
            return text.Replace("{kinFate}", c.KinFate(byYou) ?? "").Replace("{kin}", FirstName(name));
        }

        public static string FirstName(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName)) return "";
            string first = fullName.Trim().Split(' ')[0];
            return first.Substring(0, 1).ToUpperInvariant() + first.Substring(1).ToLowerInvariant();
        }

        /// <summary>One line per character met so far, by how it ended for them (under the morning letter).</summary>
        private void Summarize(int day)
        {
            var economy = PlayerEconomy.Instance;
            if (economy == null) return;
            var lines = SummaryLines(Active, Memory, day);
            if (lines.Count == 0) return;
            economy.AddLetterLine(summaryHeader + "\n" + string.Join("\n", lines));
        }

        /// <summary>P124: what became of the characters met so far, for the ticket ending.</summary>
        public List<string> EndingLines() => SummaryLines(Active, Memory, Market.Day);

        /// <summary>P85: the summary lines (also for tests): characters met, by their last outcome, placeholders filled.</summary>
        public static List<string> SummaryLines(IEnumerable<RecurringCharacter> chars, CharacterMemory memory, int day)
        {
            var lines = new List<string>();
            foreach (var c in chars)
            {
                if (c == null || !memory.Met(c)) continue;
                string line = c.SummaryFor(memory.Last(c));
                if (string.IsNullOrWhiteSpace(line)) continue;
                lines.Add("· " + Fill(c, line, memory, day));
            }
            return lines;
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
            var sb = new StringBuilder(Fill(p.character, string.IsNullOrWhiteSpace(text) ? traveler.Greeting : text, Market.Day));
            if (_returned.TryGetValue(traveler, out bool kept))
                sb.Insert(0, kept ? "You kept it safe. Here is what I promised. " : "What I left with you is gone, isn't it. I won't forget that. ");
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
