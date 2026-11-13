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
    /// mood work as for fixed events) and wanted entries. Credits and announcements due in the morning wait for the shift start:
    /// the loudspeaker forgets its lines with the new day and TODAY in the wallet starts empty after the plan.
    /// Content: P76; plan: docs/FIGUREN.md.
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
        [Tooltip("At most this many character encounters a day (docs/FIGUREN.md: 3). The dev characters come on top.")]
        [SerializeField] private int maxPerDay = 3;
        [Tooltip("P87: seconds after a character arrives before their question opens (time for the greeting).")]
        [SerializeField] private float questionDelay = 4f;
        [Tooltip("P87: seconds after a character arrives before the run ends at them (time for the reveal).")]
        [SerializeField] private float endingDelay = 7f;
        [Tooltip("P85: on this morning the letter sums up what became of the characters whose story is over (0 = never).")]
        [SerializeField] private int summaryDay = 11;
        [SerializeField, TextArea] private string summaryHeader = "I cut this out of the station newsletter. You should know how it all turned out:";

        /// <summary>P75: the characters' own random stream of the run's seed (DayGenerator.Rng), apart from the drawn days' salts.</summary>
        private const int PlanSalt = 75;
        private const string KeptLine = "You kept it safe. Here is what I promised.";
        private const string GoneLine = "What I left with you is gone. I won't forget that.";
        private const string HoldNotice = "Hear them out first - they want to ask you something.";

        private float WalkInSeconds => shift.ArrivalDelay != null ? shift.ArrivalDelay() : 0f;

        private readonly Dictionary<TravelerData, CharacterPlanner.Planned> _today = new Dictionary<TravelerData, CharacterPlanner.Planned>();
        /// <summary>Credits (and their announcement) due this morning, booked at the shift start.</summary>
        private readonly List<(int credits, string note, string announcement)> _atShiftStart = new List<(int, string, string)>();
        /// <summary>P87: this character's question is still to come; their papers can't be decided until it is answered.</summary>
        private TravelerData _asking;

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
                shift.HoldDecision = Hold;
                shift.DecisionMade += OnDecision;
                shift.TravelerArrived += OnArrived;
                shift.ShiftStarted += OnShiftStarted;
            }
            if (presenter != null) presenter.GreetingFor = Greeting;
        }

        private void OnDisable()
        {
            if (Instance == this) Instance = null;
            _asking = null; // the question coroutine stops with the component
            if (shift != null)
            {
                if (shift.PlanDay == Plan) shift.PlanDay = null;
                if (shift.HoldDecision == Hold) shift.HoldDecision = null;
                shift.DecisionMade -= OnDecision;
                shift.TravelerArrived -= OnArrived;
                shift.ShiftStarted -= OnShiftStarted;
            }
            if (presenter != null && presenter.GreetingFor == Greeting) presenter.GreetingFor = null;
        }

        /// <summary>CounterShift.BeginDay: today's rules with the characters in the queue and their wanted entries.</summary>
        private DayRules Plan(DayRules rules)
        {
            int day = Market.Day; // DayManager runs Market.Begin(day) right before the counter's BeginDay
            StopAllCoroutines(); // yesterday's questions and endings
            _asking = null;
            if (day <= 1)
            {
                Memory = new CharacterMemory(); // a new run
                _atShiftStart.Clear();
            }
            else BookAtShiftStart(false); // left over from a day whose shift never started (dev end day): at least the money
            _today.Clear();
            _returned.Clear();
            foreach (var effect in Memory.TakeEffects(day)) Apply(effect, true);
            if (day == summaryDay) Summarize(day);
            if (rules == null) return null;
            // P128: the same run seed and day plan the same morning again after a reload
            var rng = DayGenerator.Rng(dayManager != null ? dayManager.RunSeed : 0, day, PlanSalt);
            float Next() => (float)rng.NextDouble();
            var cut = new List<CharacterPlanner.Planned>();
            var due = CharacterPlanner.Due(day, Active, Memory, maxPerDay, Next, devCharacters, cut);
            foreach (var p in cut)
                Debug.LogWarning($"[Characters] day {day}: {p.character.DisplayName} (appearance {p.appearance + 1}) cut by the limit of {maxPerDay} a day");
            var wanted = Memory.WantedOn(day);
            if (due.Count == 0 && wanted.Count == 0) return rules;
            foreach (var p in due)
            {
                _today[p.traveler] = p;
                Collect(p);
                Debug.Log($"[Characters] day {day}: {p.character.DisplayName} (appearance {p.appearance + 1})");
            }
            return rules.RuntimeCopy(CharacterPlanner.Queue(rules.Travelers, due, Next), wanted);
        }

        private void OnDecision(TravelerData traveler, Verdict chosen, Judgement judgement)
        {
            if (traveler == null) return;
            int day = Market.Day;
            if (!_today.TryGetValue(traveler, out var p))
            {
                // P85: maybe someone's son (Nell asks for him in prison - or learns he never left the station)
                if (chosen == Verdict.Arrest) Memory.RememberArrest(traveler.DisplayName, traveler.Species, day);
                else if (chosen == Verdict.Airlock) Memory.RememberAirlock(traveler.DisplayName, traveler.Species, day);
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
                if (c.marketEvent != null) Market.Schedule(day + c.eventInDays, c.marketEvent); // the save keeps the market's schedule
                if (c.wanted)
                {
                    int from = day + c.wantedInDays, to = from + Mathf.Max(1, c.wantedForDays) - 1;
                    string name = p.character.WantedName(p.appearance, from, to); // the name on their papers then
                    Memory.ScheduleWanted(p.character, new WantedEntry(name, "", c.wantedOrder, c.wantedNote), from, c.wantedForDays);
                }
                if (c.afterDays > 0) Memory.ScheduleEffect(day + c.afterDays, c);
                else Apply(c, false);
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

        /// <summary>
        /// Gift and letter line at once (the morning letter shows them; a gift held back takes its line along). Credits and
        /// announcement at once during the shift; from the morning plan they wait for the shift start.
        /// </summary>
        private void Apply(Consequence c, bool inTheMorning)
        {
            var economy = PlayerEconomy.Instance;
            if (economy == null) return;
            bool heldBack = c.GiftHeldBack(economy.Household.AnySick);
            if (c.giftItem != null && !heldBack) economy.Delivery.AddGift(c.giftItem);
            if (!heldBack && !string.IsNullOrWhiteSpace(c.letterLine)) economy.AddLetterLine(c.letterLine);
            if (c.wageChange == 0 && string.IsNullOrWhiteSpace(c.announcement)) return;
            if (inTheMorning) _atShiftStart.Add((c.wageChange, c.wageNote, c.announcement));
            else Book(c.wageChange, c.wageNote, c.announcement);
        }

        private void Book(int credits, string note, string announcement, bool announce = true)
        {
            var economy = PlayerEconomy.Instance;
            if (economy != null && credits > 0) economy.Wallet.Add(credits, note);
            else if (economy != null && credits < 0) economy.Wallet.Charge(-credits, note);
            if (announce && !string.IsNullOrWhiteSpace(announcement) && announcer != null) announcer.Announce(announcement);
        }

        /// <summary>The morning's credits and announcements, a frame after the shift start (after the counter's opening lines).</summary>
        private void OnShiftStarted()
        {
            if (_atShiftStart.Count > 0) StartCoroutine(BookNextFrame());
        }

        private IEnumerator BookNextFrame()
        {
            yield return null;
            BookAtShiftStart(true);
        }

        private void BookAtShiftStart(bool announce)
        {
            var due = new List<(int credits, string note, string announcement)>(_atShiftStart);
            _atShiftStart.Clear();
            foreach (var (credits, note, announcement) in due) Book(credits, note, announcement, announce);
        }

        /// <summary>P77: a returning character collects what they left: kept → deposit paid (at the shift start), gone → they count as betrayed.</summary>
        private void Collect(CharacterPlanner.Planned p)
        {
            var economy = PlayerEconomy.Instance;
            if (economy == null || !Memory.TakeEntrusted(p.character, out var item, out int deposit)) return;
            bool kept = economy.Inventory.Remove(item);
            _returned[p.traveler] = kept;
            if (kept) _atShiftStart.Add((deposit, $"Deposit · {p.character.DisplayName}", null));
            else Memory.Remember(p.character, p.appearance, Market.Day, Outcome.Betrayed);
            Debug.Log($"[Characters] {p.character.DisplayName} collects {item.DisplayName}: {(kept ? "kept, pays " + deposit : "gone")}");
        }

        private readonly Dictionary<TravelerData, bool> _returned = new Dictionary<TravelerData, bool>();

        // ---------- P87: questions and endings at the counter ----------

        private void OnArrived(TravelerData traveler)
        {
            if (traveler == null || !_today.TryGetValue(traveler, out var p)) return;
            var a = p.character.Appearances[p.appearance];
            if (a.EndsRun) StartCoroutine(EndRun(traveler, a));
            else if (a.HasQuestion)
            {
                _asking = traveler; // no stamp or button decides them before the question is answered (else the flag could never be set)
                StartCoroutine(Ask(traveler, a));
            }
        }

        /// <summary>CounterShift.HoldDecision: a character whose question is still open can't be decided yet.</summary>
        private string Hold(TravelerData traveler) => traveler != null && traveler == _asking ? HoldNotice : null;

        /// <summary>The officer can answer now: seated, the counter not busy with something else, no other dialog, not paused.</summary>
        private bool CanAsk(GameOff.Counter.CounterStation station) =>
            Time.timeScale > 0f && !dayManager.UI.IsDialogOpen && (station == null || (station.IsSeated && !station.InputBlocked));

        private IEnumerator Ask(TravelerData traveler, Appearance a)
        {
            yield return new WaitForSeconds(questionDelay + WalkInSeconds); // P91: counted from when they stand at the window
            var station = shift.Station;
            bool canShow = dayManager != null && dayManager.UI != null;
            while (canShow && shift.Current == traveler && !CanAsk(station)) yield return null;
            if (!canShow || shift.Current != traveler) // nobody to ask with, or the shift is over: don't hold the counter
            {
                if (_asking == traveler) _asking = null;
                yield break;
            }
            if (station != null) station.SetInputBlocked(a, true);
            void Answer(bool accepted)
            {
                if (station != null) station.SetInputBlocked(a, false);
                if (_asking == traveler) _asking = null; // now the papers can be decided
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
            if (station != null) station.SetInputBlocked(a, true);
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
            return text.Replace("{kinFate}", c.KinFate(byYou, memory.Airlocked(name)) ?? "").Replace("{kin}", FirstName(name));
        }

        public static string FirstName(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName)) return "";
            string first = fullName.Trim().Split(' ')[0];
            return first.Substring(0, 1).ToUpperInvariant() + first.Substring(1).ToLowerInvariant();
        }

        /// <summary>The newsletter under the morning letter: one line per character whose story is over (NewsletterLines).</summary>
        private void Summarize(int day)
        {
            var economy = PlayerEconomy.Instance;
            if (economy == null) return;
            var lines = NewsletterLines(Active, Memory, day);
            if (lines.Count == 0) return;
            economy.AddLetterLine(summaryHeader + "\n" + string.Join("\n", lines));
        }

        /// <summary>
        /// P124: what became of the characters met, for the ticket ending (in the evening: today's encounters are over). Stories
        /// still running get their open line ("You never learned what became of …").
        /// </summary>
        public List<string> EndingLines() => SummaryLines(Active, Memory, Market.Day, true);

        /// <summary>
        /// P85: the newsletter on the morning of <paramref name="day"/> only reports stories over by yesterday. Characters still to
        /// come (Nell on day 11, Oddo on day 12) are left out: the player meets them again, and the family can't know yet.
        /// </summary>
        public static List<string> NewsletterLines(IEnumerable<RecurringCharacter> chars, CharacterMemory memory, int day) =>
            SummaryLines(chars, memory, day - 1, false);

        /// <summary>
        /// P85/P124: one line per character met, placeholders filled. A story finished by <paramref name="playedThrough"/> (the last day
        /// whose encounters are over; RecurringCharacter.StoryFinished) by the last outcome; a story still running by its open line
        /// if <paramref name="withOpen"/>, else not at all.
        /// </summary>
        public static List<string> SummaryLines(IEnumerable<RecurringCharacter> chars, CharacterMemory memory, int playedThrough, bool withOpen = true)
        {
            var lines = new List<string>();
            foreach (var c in chars)
            {
                if (c == null || !memory.Met(c)) continue;
                var last = memory.Last(c);
                string line = c.StoryFinished(last, playedThrough) ? c.SummaryFor(last) : withOpen ? c.OpenLine : null;
                if (string.IsNullOrWhiteSpace(line)) continue;
                lines.Add("· " + Fill(c, line, memory, playedThrough + 1));
            }
            return lines;
        }

        /// <summary>
        /// TravelerPresenter: the greeting of a character by the last outcome, plus a line about today's events. P77: collecting what
        /// they left, kept → thanks before it; gone → only that (their lines after a betrayal speak of a refusal at the counter).
        /// </summary>
        public string Greeting(TravelerData traveler)
        {
            if (traveler == null || !_today.TryGetValue(traveler, out var p)) return traveler != null ? traveler.Greeting : "";
            var a = p.character.Appearances[p.appearance];
            bool collects = _returned.TryGetValue(traveler, out bool kept);
            string text;
            if (collects && !kept) text = GoneLine;
            else
            {
                string byOutcome = Memory.Last(p.character) switch
                {
                    Outcome.Helped => a.greetingAfterHelped,
                    Outcome.Betrayed => a.greetingAfterBetrayed,
                    Outcome.Confiscated => a.greetingAfterConfiscated,
                    _ => null,
                };
                text = Fill(p.character, string.IsNullOrWhiteSpace(byOutcome) ? traveler.Greeting : byOutcome, Market.Day);
                if (collects) text = KeptLine + " " + text;
            }
            var sb = new StringBuilder(text);
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
