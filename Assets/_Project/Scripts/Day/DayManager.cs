using System;
using System.Collections;
using GameOff.Audio;
using GameOff.Core;
using GameOff.Counter;
using GameOff.Economy;
using GameOff.Player;
using GameOff.Travelers;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameOff.Day
{
    /// <summary>
    /// Day flow (P09): Morning (wake up in room 103, letter from home) → Shift (the counter, CounterShift) →
    /// Evening (shift over, go to bed) → Night (fade, DayEnded) → next morning. After the written story days the days are
    /// drawn from a pool for this run's seed (P73) and the run goes on until an ending (ticket home, game over).
    /// Without a pool the placeholder end screen follows the last day. Uses CounterShift.BeginDay / ShiftEnded and SkyRig.SetDay without changing them.
    /// Hooks: DayStarted (P31 board, P36 flicker), CurfewStarted (P109), DayEnded, RunFinished (P12).
    /// P10 plugs in through NightStep (the settlement runs after "SLEEP", before the fade) and LetterAddendum (family lines
    /// under the morning letter from day 2 on).
    /// P128: every morning starts with the save (SaveSlot/SaveGame); CONTINUE in the menu goes on with it, any ending deletes it.
    /// </summary>
    public class DayManager : MonoBehaviour
    {
        public enum Phase { Morning, Shift, Evening, Night, Finished }

        [SerializeField] private DaySchedule schedule;
        [SerializeField] private CounterShift shift;
        [Tooltip("Optional: turns the sky a bit further every day.")]
        [SerializeField] private SkyRig sky;
        [SerializeField] private FirstPersonController player;
        [Tooltip("Where the player wakes up each morning (PlayerSpawn marker in room 103).")]
        [SerializeField] private Transform wakeUpPoint;
        [SerializeField] private Interactable bed;
        [SerializeField] private DayFlowUI ui;
        [SerializeField] private float fadeSeconds = 1.2f;
        [Tooltip("How long the screen stays black between going to sleep (after the settlement, P10) and the next morning.")]
        [SerializeField] private float nightSeconds = 1f;
        [Tooltip("P97: real seconds in which the station clocks run from 18:00 to 23:00.")]
        [SerializeField] private float eveningClockSeconds = 300f;
        [Tooltip("P109: the curfew lines at 23:00 (shared with the counter's loudspeaker).")]
        [SerializeField] private Announcements announcements;
        [Tooltip("Master switch for the dev tools (red Dev_end_Day button, visible bed hitbox); players switch them in the options (GameSettings.DevTools). The jam build (WebBuild.BuildRelease, P146) has them off anyway.")]
        [SerializeField] private bool showDevTools = true;
        [Tooltip("Translucent box showing the bed trigger (only with showDevTools).")]
        [SerializeField] private GameObject bedDebugView;

        private int _day;
        private int _runSeed;
        private Phase _phase;
        private bool _nightCancelled;
        private float _eveningSince;
        private bool _curfewAnnounced;
        private Coroutine _curfewHints;
        private float _morningSince = -1f; // P133: when the morning letter was put away (-1 = not yet today)
        private bool _firstMorningAnnounced;
        private (string title, string body)? _gameOver;
        /// <summary>P128: the morning save of this run (once per morning, deleted by any ending).</summary>
        private readonly SaveSlot _save = SaveSlot.InPlayerPrefs();

        /// <summary>Morning of day n (1-based), before the letter.</summary>
        public event Action<int> DayStarted;
        /// <summary>The player went to bed on day n; the screen is black.</summary>
        public event Action<int> DayEnded;
        /// <summary>After the last day.</summary>
        public event Action RunFinished;
        /// <summary>P109: the evening clock reached 23:00 on day n; raised once per evening.</summary>
        public event Action<int> CurfewStarted;
        /// <summary>P29: the morning letter as shown (title, body incl. the family lines), for the copy lying on the table in 103.</summary>
        public event Action<string, string> LetterWritten;

        /// <summary>Runs after the player chose to sleep, before the fade (P10 settlement). Gets the day that ends.</summary>
        public Func<int, IEnumerator> NightStep { get; set; }
        /// <summary>Text added under the morning letter from day 2 on (P10: how the family is doing).</summary>
        public Func<string> LetterAddendum { get; set; }
        /// <summary>P15: picks the letter's second version (DayEntry.letterIfTrouble).</summary>
        public Func<bool> FamilyInTrouble { get; set; }
        /// <summary>P69: replaces the day's letter when it returns text (someone in the family has died).</summary>
        public Func<string> LetterOverride { get; set; }

        public int Day => _day;
        /// <summary>Dev tools on (Dev_end_Day, visible hitboxes, vendors open during the shift via a dev button).</summary>
        public bool DevTools => showDevTools && GameSettings.DevTools;
        /// <summary>Dev tools for something that may be without a DayManager (a sandbox scene): on there, never in the jam build (P146).</summary>
        public static bool DevToolsOn(DayManager day) => GameSettings.DevToolsAllowed && (day == null || day.DevTools);
        /// <summary>Days of this run: the written ones, or no end with the endless loop (P73).</summary>
        public int DayCount => schedule.Endless ? int.MaxValue : schedule.Count;

        /// <summary>P126: the written days; after them come the drawn ones.</summary>
        public int StoryDays => schedule.Count;
        /// <summary>The seed the endless days of this run are drawn with (P73); new every run.</summary>
        public int RunSeed => _runSeed;
        /// <summary>The schedule entry of day n (1-based), e.g. tomorrow's market events for the barkeeper's rumour (P65).</summary>
        public DayEntry Entry(int day) => schedule.Get(day, _runSeed);
        public Phase CurrentPhase => _phase;
        /// <summary>P129: title of the end screen once the run is over (empty before), for the play metrics.</summary>
        public string EndingTitle { get; private set; } = "";
        /// <summary>Dialogs and hints (P87: a character's question at the counter).</summary>
        public DayFlowUI UI => ui;
        /// <summary>P28: real seconds until the evening clock reaches 23:00 (0 outside the evening).</summary>
        public float EveningSecondsLeft => _phase == Phase.Evening ? Mathf.Max(0f, eveningClockSeconds - (Time.time - _eveningSince)) : 0f;
        /// <summary>P109: evening and the clock has reached 23:00 – every trade is closed, the bed, the locker and the shuttle stay open.</summary>
        public bool IsCurfew => CurfewAt(_phase, EveningSecondsLeft);
        /// <summary>P109: shops, bar, drone, vending machine and ticket machine trade only now (evening before 23:00).</summary>
        public bool TradingOpen => TradingOpenAt(_phase, EveningSecondsLeft);

        /// <summary>P155: the curfew has struck since this shift ended (stays true through the night's settlement).</summary>
        public bool CurfewStruck => _curfewAnnounced;
        /// <summary>P109: what a closed shop says after 23:00.</summary>
        public string CurfewClosedText => announcements != null && !string.IsNullOrWhiteSpace(announcements.curfewClosed)
            ? announcements.curfewClosed : Announcements.DefaultCurfewClosed;

        /// <summary>P109: is it curfew in this phase with this many real seconds left until 23:00?</summary>
        public static bool CurfewAt(Phase phase, float eveningSecondsLeft) => phase == Phase.Evening && eveningSecondsLeft <= 0f;

        /// <summary>P109: is trading open in this phase with this many real seconds left until 23:00?</summary>
        public static bool TradingOpenAt(Phase phase, float eveningSecondsLeft) => phase == Phase.Evening && eveningSecondsLeft > 0f;

        /// <summary>
        /// P133: should the first-morning announcement play now? Only on day 1, only once, and delay seconds after the morning
        /// letter was put away (secondsSinceMorning is negative while the letter is still open, e.g. before the run has begun).
        /// </summary>
        public static bool FirstMorningAnnouncementDue(int day, float secondsSinceMorning, float delay, bool alreadyPlayed) =>
            day == 1 && !alreadyPlayed && secondsSinceMorning >= 0f && secondsSinceMorning >= delay;

        private void OnEnable()
        {
            shift.ShiftEnded += OnShiftEnded;
            bed.OnInteract.AddListener(OnBed);
            ui.EndMenuRequested += BackToMenu;
            GameSettings.Changed += ApplyDevTools;
        }

        private void OnDisable()
        {
            shift.ShiftEnded -= OnShiftEnded;
            bed.OnInteract.RemoveListener(OnBed);
            ui.EndMenuRequested -= BackToMenu;
            GameSettings.Changed -= ApplyDevTools;
        }

        private void Start()
        {
            ApplyDevTools();
            _runSeed = UnityEngine.Random.Range(1, int.MaxValue);
            Debug.Log($"[Day] Run seed {_runSeed} (endless days after day {schedule.Count}: {schedule.Endless})");
            // P48: the menu is in this scene; the run begins when its flight is over.
            if (GameOff.UI.FrontEnd.Instance != null) GameOff.UI.FrontEnd.Instance.Started += OnFrontEndStarted;
            else BeginRun();
        }

        private void OnFrontEndStarted()
        {
            if (GameOff.UI.FrontEnd.Instance != null && GameOff.UI.FrontEnd.Instance.Continuing && ContinueRun()) return;
            BeginRun();
        }

        private void BeginRun()
        {
            ui.SetBlack(true);
            StartCoroutine(BeginDay(1));
        }

        /// <summary>P128: CONTINUE – the saved run goes on with the morning it was saved on.</summary>
        private bool ContinueRun()
        {
            var data = _save.Load();
            if (data == null) return false;
            _runSeed = data.seed;
            SaveGame.ApplyRunning(data);
            ui.SetBlack(true);
            StartCoroutine(BeginDay(data.day));
            return true;
        }

        private void Update()
        {
            float evening = eveningClockSeconds > 0f ? (Time.time - _eveningSince) / eveningClockSeconds : 1f;
            GameClock.Set(GameClock.Compute(_phase, shift.Progress, evening));
            if (!_curfewAnnounced && IsCurfew && !FlyingHome) StartCurfew();
            float sinceMorning = _phase == Phase.Shift && _morningSince >= 0f ? Time.time - _morningSince : -1f;
            if (FirstMorningAnnouncementDue(_day, sinceMorning, FirstMorningDelay, _firstMorningAnnounced)) StartCoroutine(FirstMorningAnnouncement());
        }

        private float FirstMorningDelay => announcements != null ? announcements.firstMorningDelay : Announcements.DefaultFirstMorningDelay;

        /// <summary>P72: the player is on the flight home: the station's curfew gong and hints are left behind.</summary>
        private static bool FlyingHome => GameOff.Departure.ShuttleFlight.Instance != null && GameOff.Departure.ShuttleFlight.Instance.IsFlying;

        /// <summary>P133: day 1 only – the station tells the new officer where to go (#37), then the usual way hint.</summary>
        private IEnumerator FirstMorningAnnouncement()
        {
            _firstMorningAnnounced = true;
            string line = announcements != null && !string.IsNullOrWhiteSpace(announcements.firstMorning) ? announcements.firstMorning : Announcements.DefaultFirstMorning;
            float seconds = announcements != null ? Mathf.Max(1f, announcements.secondsPerLine) : 4f;
            Debug.Log("[Day] First-morning announcement.");
            AudioManager.PlayAnnouncement(); // P115: a murmur under the subtitle
            ui.ShowHint(line, seconds + 1f);
            yield return new WaitForSeconds(seconds + 1f);
            if (_phase == Phase.Shift) ui.ShowHint(ShiftHint, 6f);
        }

        /// <summary>P109: 23:00 – the shops close (they ask TradingOpen), the station says so once, then a hint to go to bed.</summary>
        private void StartCurfew()
        {
            _curfewAnnounced = true;
            Debug.Log($"[Day] Curfew on day {_day}.");
            CurfewStarted?.Invoke(_day);
            AudioManager.PlayAnnouncement(); // P115: like the counter's announcer
            if (_curfewHints != null) StopCoroutine(_curfewHints);
            _curfewHints = StartCoroutine(CurfewHints());
        }

        private IEnumerator CurfewHints()
        {
            string line = announcements != null && !string.IsNullOrWhiteSpace(announcements.curfew) ? announcements.curfew : Announcements.DefaultCurfew;
            string hint = announcements != null && !string.IsNullOrWhiteSpace(announcements.curfewHint) ? announcements.curfewHint : Announcements.DefaultCurfewHint;
            float seconds = announcements != null ? Mathf.Max(1f, announcements.secondsPerLine) : 4f;
            ui.ShowHint(line, seconds + 1f);
            yield return new WaitForSeconds(seconds + 1f);
            _curfewHints = null;
            if (_phase == Phase.Evening && !FlyingHome) ui.ShowHint(hint, 6f);
        }

        /// <summary>P28: waiting on a bench in the evening runs the clock on by this many seconds (never past 23:00).</summary>
        public void SkipEvening(float seconds)
        {
            if (_phase != Phase.Evening || seconds <= 0f) return;
            _eveningSince = Mathf.Max(_eveningSince - seconds, Time.time - eveningClockSeconds);
        }

        private void ApplyDevTools()
        {
            if (bedDebugView != null) bedDebugView.SetActive(DevTools);
        }

        private IEnumerator BeginDay(int day)
        {
            // P128: the morning save, before anything of the day happens (a continued run replays this morning).
            if (_phase != Phase.Finished) _save.SaveMorning(day, () => SaveGame.CaptureRunning(day, _runSeed));
            _day = day;
            _phase = Phase.Morning;
            _morningSince = -1f;
            var entry = Entry(day);
            if (sky != null) sky.SetDay(day);
            Market.Begin(day, entry.marketEvents); // before the counter pins today's notes (P68)
            shift.BeginDay(entry.rules, entry.newToday);
            player.Teleport(wakeUpPoint.position, wakeUpPoint.rotation);
            player.SetControlEnabled(false);
            bed.SetPrompt("Sleep");
            ui.SetDayLabel(entry.rules != null ? $"DAY {day}  ·  {entry.rules.Date}" : $"DAY {day}");
            DayStarted?.Invoke(day);

            yield return ui.FadeTo(0f, fadeSeconds);
            string letter = LetterOverride?.Invoke();
            if (string.IsNullOrEmpty(letter)) letter = entry.LetterFor(FamilyInTrouble != null && FamilyInTrouble());
            string home = day > 1 && LetterAddendum != null ? LetterAddendum() : "";
            if (!string.IsNullOrWhiteSpace(home)) letter += "\n\n" + home;
            LetterWritten?.Invoke(entry.letterTitle, letter);
            ui.ShowLetter(entry.letterTitle, letter, entry.newToday, OnLetterClosed);
        }

        private void OnLetterClosed()
        {
            _phase = Phase.Shift;
            _morningSince = Time.time;
            player.SetControlEnabled(true);
            if (_day != 1 || _firstMorningAnnounced) ui.ShowHint(ShiftHint, 6f); // P133: on day 1 the announcement comes first
        }

        private const string ShiftHint = "Your shift starts at the customs counter. Go through the hall.";

        private void OnShiftEnded()
        {
            if (_phase != Phase.Shift) return;
            _phase = Phase.Evening;
            _eveningSince = Time.time;
            _curfewAnnounced = false;
            bed.SetPrompt("Sleep");
            ui.ShowHint("Shift over. Shops are open until 23:00 - then sleep in room 103.", 6f);
        }

        // The bed always asks first, so nobody ends a day by accident. While travelers still wait the day can't end,
        // except with the red dev button (testing the day changes without working the whole shift).
        private void OnBed()
        {
            if (ui.IsDialogOpen || (_phase != Phase.Shift && _phase != Phase.Evening)) return;
            player.SetControlEnabled(false); // frees the mouse for the dialog
            Action dev = DevTools ? DevEndDay : null;
            if (_phase == Phase.Evening)
                ui.ShowDialog($"END DAY {_day}?", "Go to sleep. The next shift starts tomorrow morning.", "SLEEP",
                    () => StartCoroutine(EndDay()), null, ResumeAfterDialog);
            else
                ui.ShowDialog("TRAVELERS ARE STILL WAITING", "You can't end the day before your shift at the customs counter is done.",
                    null, null, dev, ResumeAfterDialog);
        }

        private void ResumeAfterDialog() => player.SetControlEnabled(true);

        private void DevEndDay()
        {
            if (!GameSettings.DevToolsAllowed) return; // P146: not even through SendMessage in the release build
            Debug.Log($"[Day] Dev_end_Day: day {_day} ended from phase {_phase}.");
            StartCoroutine(EndDay());
        }

        /// <summary>Called by a NightStep (P10 settlement, RETURN): the player doesn't go to sleep after all; the day goes on.</summary>
        public void CancelNight() => _nightCancelled = true;

        /// <summary>
        /// Called by a NightStep when the run is lost (the settlement: no money left for room 103, the family lost, or discharged
        /// by drone customs). Fades out and shows the end screen with these texts; its only button goes back to the main menu.
        /// </summary>
        public void GameOver(string title, string body)
        {
            _gameOver = (title, body);
            _nightCancelled = false;
            _save.EndRun(); // P128
        }

        /// <summary>
        /// Ends the run with an ending (P72: the flight home; P12 adds more). Fades out and shows the end screen; its only button
        /// goes back to the main menu.
        /// </summary>
        public void FinishRun(string title, string body)
        {
            if (_phase == Phase.Finished) return;
            _save.EndRun(); // P128: at once, in case the tab closes during the fade
            StartCoroutine(FinishRunRoutine(title, body));
        }

        private IEnumerator FinishRunRoutine(string title, string body)
        {
            _phase = Phase.Finished;
            FreezeForEnd(); // frees the mouse for the end screen
            yield return ui.FadeTo(1f, fadeSeconds);
            ShowRunEnd(title, body);
        }

        /// <summary>
        /// The run is over for good: the player's control stays off, and the counter takes no more input (seated, its Q / Esc /
        /// right click would stand up and give the player the mouse back behind the end screen; DISCHARGED, AIRLOCKED).
        /// </summary>
        private void FreezeForEnd()
        {
            player.EndControlForGood();
            if (shift.Station != null) shift.Station.SetInputBlocked(this, true);
        }

        /// <summary>The one place a run ends with an ending text (game over, ticket home; P12 adds more).</summary>
        private void ShowRunEnd(string title, string body)
        {
            _phase = Phase.Finished;
            FreezeForEnd();
            _save.EndRun(); // P128
            EndingTitle = title ?? "";
            Debug.Log($"[Day] Run finished on day {_day}: {title}");
            RunFinished?.Invoke();
            ui.ShowEnd(title, body, "RETURN TO MENU");
        }

        private IEnumerator EndDay()
        {
            var before = _phase;
            _phase = Phase.Night;
            _nightCancelled = false;
            _gameOver = null;
            player.SetControlEnabled(false);
            if (NightStep != null) yield return NightStep(_day);
            if (_nightCancelled)
            {
                _phase = before;
                player.SetControlEnabled(true);
                yield break;
            }
            yield return ui.FadeTo(1f, fadeSeconds);
            if (_gameOver.HasValue)
            {
                ShowRunEnd(_gameOver.Value.title, _gameOver.Value.body);
                yield break;
            }
            DayEnded?.Invoke(_day);
            yield return new WaitForSecondsRealtime(nightSeconds);

            if (_day < schedule.Count || schedule.Endless)
            {
                yield return BeginDay(_day + 1);
                yield break;
            }
            ShowRunEnd(LastDayTitle, LastDayBody);
        }

        // Only without a loop pool (DaySchedule.loop empty, P73): the written days are over and so is the run.
        private const string LastDayTitle = "THE LAST SHIFT";
        private const string LastDayBody = "Every shift on the schedule is worked. Kestrel-9 has no more travelers for you.\nThanks for playing.";

        private static void BackToMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneNames.Main);
        }
    }
}
