using System.Collections.Generic;
using GameOff.Audio;
using GameOff.Core;
using GameOff.Economy;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameOff.UI
{
    /// <summary>
    /// Options in tabs (P43, follows P02): AUDIO, CONTROLS, DEV (P49; hidden in the jam build, P146). Every change applies at once and is saved in GameSettings;
    /// AUDIO and CONTROLS have their own "Reset to defaults". The same panel is used in the main menu and the pause menu.
    /// Fullscreen is not here but a corner button in both menus (FullscreenButton).
    /// AUDIO has one slider per GameMixer channel (P18); releasing a slider plays a short preview tone on that channel.
    /// Built by MenuSceneBuilder.
    /// </summary>
    public class OptionsPanel : MonoBehaviour
    {
        [Header("Tabs (same order as pages)")]
        [SerializeField] private Button[] tabButtons;
        [SerializeField] private GameObject[] pages;

        [Header("Audio (index = AudioChannel: Master, Music, Sfx, Voice)")]
        [SerializeField] private Slider[] volumeSliders;
        [SerializeField] private TMP_Text[] volumeValues;
        [SerializeField] private Button resetAudio;

        [Header("Controls")]
        [SerializeField] private Slider sensitivity;
        [SerializeField] private TMP_Text sensitivityValue;
        [SerializeField] private Button invertY;
        [SerializeField] private TMP_Text invertYValue;
        [SerializeField] private Button resetControls;

        [Header("Dev")]
        [SerializeField] private Button devTools;
        [SerializeField] private TMP_Text devToolsValue;
        [SerializeField] private Button trafficLines;
        [SerializeField] private TMP_Text trafficLinesValue;
        [Tooltip("Dev: ±100 credits for testing money (P10/P11). Only does something in game, where PlayerEconomy exists.")]
        [SerializeField] private Button creditsMinus;
        [SerializeField] private Button creditsPlus;
        [SerializeField] private TMP_Text creditsValue;

        [Header("Look")]
        [SerializeField] private Color tabIdle = new Color(0.17f, 0.2f, 0.24f);
        [SerializeField] private Color tabSelected = new Color(0.27f, 0.29f, 0.33f);
        [SerializeField] private Color labelIdle = new Color(0.55f, 0.56f, 0.59f);
        [SerializeField] private Color labelSelected = new Color(1f, 0.83f, 0.42f);

        private int _tab;

        private void Awake()
        {
            if (!GameSettings.DevToolsAllowed) HideDevTab(); // P146: the jam build has no DEV tab
            for (int i = 0; i < tabButtons.Length; i++)
            {
                int index = i;
                tabButtons[i].onClick.AddListener(() => SelectTab(index));
            }

            for (int i = 0; i < volumeSliders.Length; i++)
            {
                var channel = (AudioChannel)i;
                var slider = volumeSliders[i];
                slider.minValue = 0f;
                slider.maxValue = 1f;
                slider.onValueChanged.AddListener(v => { SetVolume(channel, v); Refresh(); });
                var release = slider.GetComponent<SliderRelease>();
                if (release != null) release.Released += () => AudioManager.PlayPreview(channel);
            }

            sensitivity.minValue = GameSettings.MinSensitivity;
            sensitivity.maxValue = GameSettings.MaxSensitivity;
            sensitivity.onValueChanged.AddListener(v => { GameSettings.MouseSensitivity = v; Refresh(); });
            invertY.onClick.AddListener(() => { GameSettings.InvertY = !GameSettings.InvertY; Refresh(); });
            // The DEV tab may be missing (HideDevTab in the jam build only hides it)
            if (devTools != null) devTools.onClick.AddListener(() => { GameSettings.DevTools = !GameSettings.DevTools; Refresh(); });
            if (trafficLines != null) trafficLines.onClick.AddListener(() => { GameSettings.TrafficLineDev = !GameSettings.TrafficLineDev; Refresh(); });
            if (creditsPlus != null) creditsPlus.onClick.AddListener(() => ChangeCredits(100));
            if (creditsMinus != null) creditsMinus.onClick.AddListener(() => ChangeCredits(-100));

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

        private static float GetVolume(AudioChannel channel) => channel switch
        {
            AudioChannel.Music => GameSettings.MusicVolume,
            AudioChannel.Sfx => GameSettings.SfxVolume,
            AudioChannel.Voice => GameSettings.VoiceVolume,
            _ => GameSettings.Volume,
        };

        private static void SetVolume(AudioChannel channel, float value)
        {
            switch (channel)
            {
                case AudioChannel.Music: GameSettings.MusicVolume = value; break;
                case AudioChannel.Sfx: GameSettings.SfxVolume = value; break;
                case AudioChannel.Voice: GameSettings.VoiceVolume = value; break;
                default: GameSettings.Volume = value; break;
            }
        }

        private void Refresh()
        {
            for (int i = 0; i < volumeSliders.Length; i++)
            {
                float v = GetVolume((AudioChannel)i);
                volumeSliders[i].SetValueWithoutNotify(v);
                volumeValues[i].text = $"{Mathf.RoundToInt(v * 100f)}%";
            }
            sensitivity.SetValueWithoutNotify(GameSettings.MouseSensitivity);
            sensitivityValue.text = $"{sensitivity.value:0.00}x";
            invertYValue.text = GameSettings.InvertY ? "ON" : "OFF";
            if (devToolsValue != null) devToolsValue.text = GameSettings.DevTools ? "ON" : "OFF";
            if (trafficLinesValue != null) trafficLinesValue.text = GameSettings.TrafficLineDev ? "ON" : "OFF";
            if (creditsValue != null)
            {
                var economy = PlayerEconomy.Instance;
                creditsValue.text = economy != null ? economy.Format(economy.Wallet.Balance) : "in game only";
                if (creditsPlus != null) creditsPlus.interactable = economy != null;
                if (creditsMinus != null) creditsMinus.interactable = economy != null;
            }
        }

        /// <summary>
        /// P146: takes the DEV tab out (the page holding the dev switches, and its tab button) and centres the other tabs again
        /// at the same spacing. Called in Awake of the jam build; public for the tests.
        /// </summary>
        public void HideDevTab()
        {
            int dev = -1;
            for (int i = 0; i < pages.Length && i < tabButtons.Length; i++)
                if (devTools != null && pages[i] != null && devTools.transform.IsChildOf(pages[i].transform)) dev = i;
            if (dev < 0) return;
            pages[dev].SetActive(false);
            tabButtons[dev].gameObject.SetActive(false);

            var tabs = new List<Button>(tabButtons);
            var kept = new List<GameObject>(pages);
            tabs.RemoveAt(dev);
            kept.RemoveAt(dev);
            // The builder puts the tabs in a centred row at equal steps: the same row, one tab shorter
            var first = (RectTransform)tabButtons[0].transform;
            var last = (RectTransform)tabButtons[tabButtons.Length - 1].transform;
            float centre = (first.anchoredPosition.x + last.anchoredPosition.x) / 2f;
            float step = tabButtons.Length > 1 ? (last.anchoredPosition.x - first.anchoredPosition.x) / (tabButtons.Length - 1) : 0f;
            for (int i = 0; i < tabs.Count; i++)
            {
                var rect = (RectTransform)tabs[i].transform;
                rect.anchoredPosition = new Vector2(centre + (i - (tabs.Count - 1) / 2f) * step, rect.anchoredPosition.y);
            }
            tabButtons = tabs.ToArray();
            pages = kept.ToArray();
            _tab = Mathf.Min(_tab, pages.Length - 1);
        }

        private void ChangeCredits(int amount)
        {
            var economy = PlayerEconomy.Instance;
            if (economy == null || !GameSettings.DevToolsAllowed) return;
            if (amount > 0) economy.Wallet.Add(amount, "Dev · +" + amount);
            else economy.Wallet.Charge(-amount, "Dev · " + amount);
            Refresh();
        }
    }
}
