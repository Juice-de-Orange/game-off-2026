using GameOff.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameOff.UI
{
    /// <summary>Volume and mouse sensitivity sliders bound to GameSettings. Used in the main menu and the pause menu.</summary>
    public class OptionsPanel : MonoBehaviour
    {
        [SerializeField] private Slider volume;
        [SerializeField] private TMP_Text volumeValue;
        [SerializeField] private Slider sensitivity;
        [SerializeField] private TMP_Text sensitivityValue;

        private void Awake()
        {
            volume.minValue = 0f;
            volume.maxValue = 1f;
            sensitivity.minValue = GameSettings.MinSensitivity;
            sensitivity.maxValue = GameSettings.MaxSensitivity;
            volume.onValueChanged.AddListener(OnVolume);
            sensitivity.onValueChanged.AddListener(OnSensitivity);
        }

        private void OnEnable()
        {
            volume.SetValueWithoutNotify(GameSettings.Volume);
            sensitivity.SetValueWithoutNotify(GameSettings.MouseSensitivity);
            UpdateLabels();
        }

        private void OnDisable() => GameSettings.Save();

        private void OnVolume(float value)
        {
            GameSettings.Volume = value;
            UpdateLabels();
        }

        private void OnSensitivity(float value)
        {
            GameSettings.MouseSensitivity = value;
            UpdateLabels();
        }

        private void UpdateLabels()
        {
            volumeValue.text = $"{Mathf.RoundToInt(volume.value * 100f)}%";
            sensitivityValue.text = $"{sensitivity.value:0.00}x";
        }
    }
}
