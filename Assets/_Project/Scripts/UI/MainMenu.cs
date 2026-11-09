using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace GameOff.UI
{
    /// <summary>
    /// Main menu: Start hands over to FrontEnd (P48: the menu is in Main), Options and Credits swap panels (Esc / Back returns), Quit is hidden in WebGL
    /// (a browser tab can't be closed by the game).
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

        private void Awake()
        {
            startButton.onClick.AddListener(StartGame);
            optionsButton.onClick.AddListener(() => Show(optionsPanel));
            creditsButton.onClick.AddListener(() => Show(creditsPanel));
            quitButton.onClick.AddListener(Application.Quit);
            foreach (var back in backButtons) back.onClick.AddListener(() => Show(mainPanel));
            quitButton.gameObject.SetActive(Application.platform != RuntimePlatform.WebGLPlayer);
        }

        private void Start()
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Show(mainPanel);
        }

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
        }

        private static void StartGame()
        {
            if (FrontEnd.Instance != null) FrontEnd.Instance.Begin();
        }
    }
}
