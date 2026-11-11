using GameOff.Core;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace GameOff.UI
{
    /// <summary>
    /// Main menu: Start hands over to FrontEnd (P48: the menu is in Main), Options and Credits swap panels (Esc / Back returns), Quit is hidden in WebGL
    /// (a browser tab can't be closed by the game).
    /// P128: CONTINUE (only with a saved run) goes on with it; START over a saved run asks first.
    /// </summary>
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private GameObject optionsPanel;
        [SerializeField] private GameObject creditsPanel;
        [SerializeField] private Button startButton;
        [SerializeField] private Button optionsButton;
        [SerializeField] private Button creditsButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private Button[] backButtons;
        [Header("P128: saved run")]
        [SerializeField] private Button continueButton;
        [Tooltip("Asks before START overwrites the saved run.")]
        [SerializeField] private GameObject confirmPanel;
        [SerializeField] private TMP_Text confirmText;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button confirmBackButton;

        private SaveData _saved;

        private void Awake()
        {
            startButton.onClick.AddListener(StartGame);
            optionsButton.onClick.AddListener(() => Show(optionsPanel));
            creditsButton.onClick.AddListener(() => Show(creditsPanel));
            quitButton.onClick.AddListener(Application.Quit);
            foreach (var back in backButtons) back.onClick.AddListener(() => Show(mainPanel));
            quitButton.gameObject.SetActive(Application.platform != RuntimePlatform.WebGLPlayer);
            if (continueButton != null) continueButton.onClick.AddListener(ContinueGame);
            if (confirmButton != null) confirmButton.onClick.AddListener(StartNewRun);
            if (confirmBackButton != null) confirmBackButton.onClick.AddListener(() => Show(mainPanel));
        }

        private void Start()
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            _saved = SaveSlot.InPlayerPrefs().Load();
            if (continueButton != null)
            {
                continueButton.gameObject.SetActive(ShowsContinue(_saved));
                var label = continueButton.GetComponentInChildren<TMP_Text>(true);
                if (label != null && _saved != null) label.text = ContinueLabel(_saved.day);
            }
            Show(mainPanel);
        }

        /// <summary>P128: CONTINUE only with a saved run.</summary>
        public static bool ShowsContinue(SaveData saved) => saved != null;

        public static string ContinueLabel(int day) => $"CONTINUE · DAY {day}";

        public static string OverwriteText(int day) => $"Your saved run (day {day}) will be lost.\nStart a new run from day 1?";

        private void Update()
        {
            if (!mainPanel.activeSelf && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                Show(mainPanel);
        }

        private void Show(GameObject panel)
        {
            mainPanel.SetActive(panel == mainPanel);
            optionsPanel.SetActive(panel == optionsPanel);
            creditsPanel.SetActive(panel == creditsPanel);
            if (confirmPanel != null) confirmPanel.SetActive(panel == confirmPanel);
        }

        private void StartGame()
        {
            if (_saved != null && confirmPanel != null)
            {
                if (confirmText != null) confirmText.text = OverwriteText(_saved.day);
                Show(confirmPanel);
                return;
            }
            StartNewRun();
        }

        private void StartNewRun()
        {
            if (_saved != null) SaveSlot.InPlayerPrefs().Delete(); // the new run saves its own day 1
            _saved = null;
            if (FrontEnd.Instance != null) FrontEnd.Instance.Begin();
        }

        private void ContinueGame()
        {
            if (_saved == null) return;
            if (FrontEnd.Instance != null) FrontEnd.Instance.Begin(continuing: true);
        }
    }
}
