using System;
using System.Collections;
using GameOff.Core;
using GameOff.Economy;
using GameOff.Player;
using GameOff.Travelers;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameOff.Day
{
    /// <summary>
    /// Day flow (P09): Morning (wake up in room 103, letter from home) → Shift (the counter, CounterShift) →
    /// Evening (shift over, go to bed) → Night (fade, DayEnded) → next morning; after the last day the placeholder end
    /// screen (real endings: P12). Uses CounterShift.BeginDay / ShiftEnded and SkyRig.SetDay without changing them.
    /// Hooks for later packages: DayStarted (P31 board, P36 flicker), ShiftOver, DayEnded, RunFinished (P12).
    /// P10 plugs in through NightStep (the settlement runs after "SLEEP", before the fade) and LetterAddendum (family lines
    /// under the morning letter from day 2 on).
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
        [Tooltip("Black screen between the days (P10 shows the settlement here later).")]
        [SerializeField] private float nightSeconds = 1f;
        [Tooltip("Master switch for the dev tools (red Dev_end_Day button, visible bed hitbox); players switch them in the options (GameSettings.DevTools). TURN OFF for the jam build.")]
        [SerializeField] private bool showDevTools = true;
        [Tooltip("Translucent box showing the bed trigger (only with showDevTools).")]
        [SerializeField] private GameObject bedDebugView;

        private int _day;
        private Phase _phase;
        private bool _nightCancelled;
        private (string title, string body)? _gameOver;

        /// <summary>Morning of day n (1-based), before the letter.</summary>
        public event Action<int> DayStarted;
        /// <summary>The last traveler of day n is done.</summary>
        public event Action<int> ShiftOver;
        /// <summary>The player went to bed on day n; the screen is black.</summary>
        public event Action<int> DayEnded;
        /// <summary>After the last day.</summary>
        public event Action RunFinished;

        /// <summary>Runs after the player chose to sleep, before the fade (P10 settlement). Gets the day that ends.</summary>
        public Func<int, IEnumerator> NightStep { get; set; }
        /// <summary>Text added under the morning letter from day 2 on (P10: how the family is doing).</summary>
        public Func<string> LetterAddendum { get; set; }

        public int Day => _day;
        /// <summary>Dev tools on (Dev_end_Day, visible hitboxes, vendors open during the shift via a dev button).</summary>
        public bool DevTools => showDevTools && GameSettings.DevTools;
        public int DayCount => schedule.Count;
        /// <summary>The schedule entry of day n (1-based), e.g. tomorrow's market events for the barkeeper's rumour (P65).</summary>
        public DayEntry Entry(int day) => schedule.Get(day);
        public Phase CurrentPhase => _phase;

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
            ui.SetBlack(true);
            StartCoroutine(BeginDay(1));
        }

        private void ApplyDevTools()
        {
            if (bedDebugView != null) bedDebugView.SetActive(DevTools);
        }

        private IEnumerator BeginDay(int day)
        {
            _day = day;
            _phase = Phase.Morning;
            var entry = schedule.Get(day);
            if (sky != null) sky.SetDay(day);
            Market.Begin(day, entry.marketEvents); // before the counter pins today's notes (P68)
            shift.BeginDay(entry.rules, entry.newToday);
            player.Teleport(wakeUpPoint.position, wakeUpPoint.rotation);
            player.SetControlEnabled(false);
            bed.SetPrompt("Sleep");
            ui.SetDayLabel(entry.rules != null ? $"DAY {day}  ·  {entry.rules.Date}" : $"DAY {day}");
            DayStarted?.Invoke(day);

            yield return ui.FadeTo(0f, fadeSeconds);
            string letter = entry.letter;
            string home = day > 1 && LetterAddendum != null ? LetterAddendum() : "";
            if (!string.IsNullOrWhiteSpace(home)) letter += "\n\n" + home;
            ui.ShowLetter(entry.letterTitle, letter, entry.newToday, OnLetterClosed);
        }

        private void OnLetterClosed()
        {
            _phase = Phase.Shift;
            player.SetControlEnabled(true);
            ui.ShowHint("Your shift starts at the customs counter. Go through the hall.", 6f);
        }

        private void OnShiftEnded()
        {
            if (_phase != Phase.Shift) return;
            _phase = Phase.Evening;
            bed.SetPrompt("Sleep");
            ShiftOver?.Invoke(_day);
            ui.ShowHint("Shift over. Go back to room 103 and get some sleep.", 6f);
        }

        // The bed always asks first, so nobody ends a day by accident. While travelers still wait the day can't end,
        // except with the red dev button (testing the day changes without working the whole shift).
        private void OnBed()
        {
            if (ui.IsDialogOpen || (_phase != Phase.Shift && _phase != Phase.Evening)) return;
            player.SetControlEnabled(false); // frees the mouse for the dialog
            Action dev = DevTools ? DevEndDay : null;
            if (_phase == Phase.Evening)
                ui.ShowDialog($"End day {_day}?", "Go to sleep. The next shift starts tomorrow morning.", "SLEEP",
                    () => StartCoroutine(EndDay()), null, ResumeAfterDialog);
            else
                ui.ShowDialog("Travelers are still waiting", "You can't end the day before your shift at the customs counter is done.",
                    null, null, dev, ResumeAfterDialog);
        }

        private void ResumeAfterDialog() => player.SetControlEnabled(true);

        private void DevEndDay()
        {
            Debug.Log($"[Day] Dev_end_Day: day {_day} ended from phase {_phase}.");
            StartCoroutine(EndDay());
        }

        /// <summary>Called by a NightStep (P10 settlement, RETURN): the player doesn't go to sleep after all; the day goes on.</summary>
        public void CancelNight() => _nightCancelled = true;

        /// <summary>
        /// Called by a NightStep when the run is lost (P10: the rent for room 103 can't be paid). Fades out and shows the end
        /// screen with these texts; its only button goes back to the main menu. Placeholder until the real endings (P12).
        /// </summary>
        public void GameOver(string title, string body)
        {
            _gameOver = (title, body);
            _nightCancelled = false;
        }

        /// <summary>
        /// Ends the run with an ending (P72: the flight home; P12 adds more). Fades out and shows the end screen; its only button
        /// goes back to the main menu.
        /// </summary>
        public void FinishRun(string title, string body)
        {
            if (_phase == Phase.Finished) return;
            StartCoroutine(FinishRunRoutine(title, body));
        }

        private IEnumerator FinishRunRoutine(string title, string body)
        {
            _phase = Phase.Finished;
            player.SetControlEnabled(false); // frees the mouse for the end screen
            yield return ui.FadeTo(1f, fadeSeconds);
            ShowRunEnd(title, body);
        }

        /// <summary>The one place a run ends with an ending text (game over, ticket home; P12 adds more).</summary>
        private void ShowRunEnd(string title, string body)
        {
            _phase = Phase.Finished;
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

            if (_day < schedule.Count)
            {
                yield return BeginDay(_day + 1);
                yield break;
            }
            _phase = Phase.Finished;
            RunFinished?.Invoke();
            ui.ShowEnd();
        }

        private static void BackToMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneNames.Menu);
        }
    }
}
