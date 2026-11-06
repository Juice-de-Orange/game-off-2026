using System;
using System.Collections;
using GameOff.Core;
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

        private IEnumerator EndDay()
        {
            _phase = Phase.Night;
            player.SetControlEnabled(false);
            if (NightStep != null) yield return NightStep(_day);
            yield return ui.FadeTo(1f, fadeSeconds);
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
