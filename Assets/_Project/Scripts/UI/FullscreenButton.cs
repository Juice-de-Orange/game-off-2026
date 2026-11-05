using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GameOff.UI
{
    /// <summary>
    /// Fullscreen toggle in the top-right corner of the main menu and the pause menu (P43). Shows four corners pointing
    /// outward (enter fullscreen) or inward (leave fullscreen) and follows the real state, so leaving fullscreen with the
    /// browser's Esc flips the icon back.
    /// In WebGL the toggle runs in the browser (Plugins/WebGL/Fullscreen.jslib): pressing arms it, releasing toggles it.
    /// Setting Screen.fullScreen from Unity's click would only take effect on the user's next click.
    /// </summary>
    public class FullscreenButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
    {
        [SerializeField] private Button button;
        [SerializeField] private GameObject expandIcon;
        [SerializeField] private GameObject collapseIcon;
        [Tooltip("All corner pieces of both icons, tinted on hover.")]
        [SerializeField] private Graphic[] iconParts;
        [Tooltip("Small text below the button, shown on hover.")]
        [SerializeField] private TMP_Text hoverLabel;
        [SerializeField] private Color idleColor = new Color(0.89f, 0.87f, 0.81f);
        [SerializeField] private Color hoverColor = new Color(1f, 0.83f, 0.42f);

        private bool _shownFullscreen;

#if UNITY_WEBGL && !UNITY_EDITOR
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern int GameOff_IsFullscreen();

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void GameOff_ToggleFullscreenOnRelease();

        private static bool IsFullscreen() => GameOff_IsFullscreen() == 1;

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left) GameOff_ToggleFullscreenOnRelease();
        }
#else
        private static bool IsFullscreen() => Screen.fullScreen;

        public void OnPointerDown(PointerEventData eventData) { }

        private void Awake() => button.onClick.AddListener(() => Screen.fullScreen = !Screen.fullScreen);
#endif

        private void OnEnable()
        {
            SetHover(false);
            Refresh();
        }

        private void Update()
        {
            if (IsFullscreen() != _shownFullscreen) Refresh();
        }

        public void OnPointerEnter(PointerEventData eventData) => SetHover(true);

        public void OnPointerExit(PointerEventData eventData) => SetHover(false);

        private void Refresh()
        {
            _shownFullscreen = IsFullscreen();
            expandIcon.SetActive(!_shownFullscreen);
            collapseIcon.SetActive(_shownFullscreen);
            hoverLabel.text = _shownFullscreen ? "EXIT FULLSCREEN" : "FULLSCREEN";
        }

        private void SetHover(bool hover)
        {
            foreach (var part in iconParts) part.color = hover ? hoverColor : idleColor;
            hoverLabel.gameObject.SetActive(hover);
        }
    }
}
