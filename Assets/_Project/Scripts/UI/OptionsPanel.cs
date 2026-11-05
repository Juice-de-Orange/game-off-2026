using GameOff.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameOff.UI
{
    /// <summary>
    /// Options in tabs (P43, follows P02): AUDIO, CONTROLS. Every change applies at once and is saved in GameSettings;
    /// each tab has its own "Reset to defaults". The same panel is used in the main menu and the pause menu.
    /// Fullscreen is not here but a corner button in both menus (FullscreenButton).
    /// Built by MenuSceneBuilder; P18 adds the Music/Effects/Voice sliders to the AUDIO tab.
    /// </summary>
    public class OptionsPanel : MonoBehaviour
    {
        [Header("Tabs (same order as pages)")]
        [SerializeField] private Button[] tabButtons;
        [SerializeField] private GameObject[] pages;

        [Header("Audio")]
        [SerializeField] private Slider volume;
        [SerializeField] private TMP_Text volumeValue;
        [SerializeField] private Button resetAudio;

        [Header("Controls")]
        [SerializeField] private Slider sensitivity;
        [SerializeField] private TMP_Text sensitivityValue;
        [SerializeField] private Button invertY;
        [SerializeField] private TMP_Text invertYValue;
        [SerializeField] private Button resetControls;

        [Header("Look")]
        [SerializeField] private Color tabIdle = new Color(0.17f, 0.2f, 0.24f);
        [SerializeField] private Color tabSelected = new Color(0.27f, 0.29f, 0.33f);
        [SerializeField] private Color labelIdle = new Color(0.55f, 0.56f, 0.59f);
        [SerializeField] private Color labelSelected = new Color(1f, 0.83f, 0.42f);

        private int _tab;

        private void Awake()
        {
            for (int i = 0; i < tabButtons.Length; i++)
            {
                int index = i;
                tabButtons[i].onClick.AddListener(() => SelectTab(index));
            }

            volume.minValue = 0f;
            volume.maxValue = 1f;
            sensitivity.minValue = GameSettings.MinSensitivity;
            sensitivity.maxValue = GameSettings.MaxSensitivity;
            volume.onValueChanged.AddListener(v => { GameSettings.Volume = v; Refresh(); });
            sensitivity.onValueChanged.AddListener(v => { GameSettings.MouseSensitivity = v; Refresh(); });
            invertY.onClick.AddListener(() => { GameSettings.InvertY = !GameSettings.InvertY; Refresh(); });

            resetAudio.onClick.AddListener(() => { GameSettings.ResetAudio(); Refresh(); });
            resetControls.onClick.AddListener(() => { GameSettings.ResetControls(); Refresh(); });
        }

        private void OnEnable()
        {
            SelectTab(_tab);
            Refresh();
        }

        private void OnDisable() => GameSettings.Save();

        public void SelectTab(int index)
        {
            _tab = Mathf.Clamp(index, 0, pages.Length - 1);
            for (int i = 0; i < pages.Length; i++)
            {
                bool selected = i == _tab;
                pages[i].SetActive(selected);
                var colors = tabButtons[i].colors;
                colors.normalColor = selected ? tabSelected : tabIdle;
                colors.selectedColor = colors.normalColor;
                tabButtons[i].colors = colors;
                var label = tabButtons[i].GetComponentInChildren<TMP_Text>();
                if (label != null) label.color = selected ? labelSelected : labelIdle;
            }
        }

        private void Refresh()
        {
            volume.SetValueWithoutNotify(GameSettings.Volume);
            sensitivity.SetValueWithoutNotify(GameSettings.MouseSensitivity);
            volumeValue.text = $"{Mathf.RoundToInt(volume.value * 100f)}%";
            sensitivityValue.text = $"{sensitivity.value:0.00}x";
            invertYValue.text = GameSettings.InvertY ? "ON" : "OFF";
        }
    }
}
