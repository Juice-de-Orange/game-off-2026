using GameOff.Core;
using GameOff.Counter;
using GameOff.Day;
using GameOff.Player;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GameOff.UI
{
    /// <summary>
    /// Pause menu in Main. Esc or P while walking freely pauses (time stops, audio pauses, mouse is free).
    /// At the counter Esc keeps meaning "one step back / stand up"; once standing, Esc pauses.
    /// In WebGL the browser swallows Esc to release the pointer lock, so losing the lock while walking pauses too.
    /// </summary>
    public class PauseMenu : MonoBehaviour
    {
        [SerializeField] private FirstPersonController player;
        [Tooltip("Optional: while seated at the counter, Esc belongs to the counter.")]
        [SerializeField] private CounterStation counter;
        [SerializeField] private GameObject root;
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private GameObject optionsPanel;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button optionsButton;
        [SerializeField] private Button optionsBackButton;
        [SerializeField] private Button menuButton;
        [Tooltip("P84: opens the in-game bug report form.")]
        [SerializeField] private Button bugReportButton;
        [SerializeField] private BugReportForm bugReportForm;
        [Tooltip("Optional: day and phase for the bug report.")]
        [SerializeField] private DayManager dayManager;

        private InputAction _pauseAction;
        private bool _paused;
        private bool _wasCaptured;
        private int _lastBlockedFrame = -1;
        private float _capturedSince;

        private const float LockGrace = 0.5f;
        private static bool IsWebGL => Application.platform == RuntimePlatform.WebGLPlayer;

        // In the browser, Cursor.lockState keeps saying "Locked" after the user's Esc released the lock (the first Esc never
        // reaches the game), so ask the browser directly (PointerLockBridge → Plugins/WebGL/PointerLock.jslib).
        private static bool IsPointerLocked() => PointerLockBridge.IsLocked;

        public bool IsPaused => _paused;

        private void Awake()
        {
            _pauseAction = new InputAction("Pause", InputActionType.Button);
            _pauseAction.AddBinding("<Keyboard>/escape");
            _pauseAction.AddBinding("<Keyboard>/p");
            resumeButton.onClick.AddListener(Resume);
            optionsButton.onClick.AddListener(() => ShowOptions(true));
            optionsBackButton.onClick.AddListener(() => ShowOptions(false));
            menuButton.onClick.AddListener(BackToMenu);
            if (bugReportButton != null && bugReportForm == null) bugReportButton.gameObject.SetActive(false); // nothing to open
            if (bugReportButton != null && bugReportForm != null)
            {
                bugReportButton.onClick.AddListener(OpenReport);
                bugReportForm.Context = BugReportJson;
                bugReportForm.Closed += () => mainPanel.SetActive(true);
                bugReportForm.gameObject.SetActive(false);
            }
            root.SetActive(false);
        }

        private void OnEnable() => _pauseAction.Enable();

        private void OnDisable()
        {
            _pauseAction.Disable();
            if (_paused) SetTime(false);
        }

        private void OnDestroy() => _pauseAction.Dispose();

        private void Update()
        {
            bool pressed = _pauseAction.WasPressedThisFrame();
            if (_paused && ReportOpen)
            {
                // Typing goes into the form: P must not resume. Esc goes back to the pause menu (not while sending).
                if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) bugReportForm.Close();
                return;
            }
            if (_paused)
            {
                if (pressed)
                {
                    if (optionsPanel.activeSelf) ShowOptions(false);
                    else Resume();
                }
                return;
            }

            // Browsers may refuse a lock right after Esc and drop it again at once; only a lock that held counts.
            bool captured = IsPointerLocked();
            if (captured && !_wasCaptured) _capturedSince = Time.unscaledTime;
            bool lostLock = _wasCaptured && !captured && IsWebGL && Time.unscaledTime - _capturedSince > LockGrace;
            _wasCaptured = captured;

            // Not while another system has the player (counter view, leaving blend); the frame after it gave the player
            // back doesn't count either, so the Esc that made us stand up doesn't also pause.
            bool blocked = !player.ControlEnabled || (counter != null && counter.IsSeated);
            if (blocked) _lastBlockedFrame = Time.frameCount;
            if (blocked || Time.frameCount - _lastBlockedFrame <= 1) return;

            if (pressed || lostLock) Pause();
        }

        private bool ReportOpen => bugReportForm != null && bugReportForm.IsOpen;

        public void Pause()
        {
            _paused = true;
            root.SetActive(true);
            if (bugReportForm != null) bugReportForm.Hide();
            ShowOptions(false);
            player.SetControlEnabled(false); // frees the cursor, stops look/move and interaction
            SetTime(true);
        }

        public void Resume()
        {
            if (bugReportForm != null) bugReportForm.Hide(); // aborts a report that is still sending
            _paused = false;
            root.SetActive(false);
            SetTime(false);
            player.SetControlEnabled(true); // captures the mouse again (in the browser right after the Resume click)
            _wasCaptured = IsPointerLocked();
            _capturedSince = Time.unscaledTime;
            _lastBlockedFrame = Time.frameCount;
        }

        private void BackToMenu()
        {
            SetTime(false);
            SceneManager.LoadScene(SceneNames.Menu);
        }

        private void OpenReport()
        {
            mainPanel.SetActive(false);
            bugReportForm.Open();
        }

        /// <summary>The report for the backend: the player's text plus where the game stands (P84).</summary>
        private string BugReportJson(string text)
        {
            int day = dayManager != null ? dayManager.Day : 0;
            string phase = day > 0 ? dayManager.CurrentPhase.ToString() : "-";
            return BugReport.Json(text, Application.version, day, phase, SceneManager.GetActiveScene().name, BrowserBridge.UserAgent);
        }

        private void ShowOptions(bool show)
        {
            mainPanel.SetActive(!show);
            optionsPanel.SetActive(show);
        }

        private static void SetTime(bool paused)
        {
            Time.timeScale = paused ? 0f : 1f;
            AudioListener.pause = paused;
        }
    }
}
