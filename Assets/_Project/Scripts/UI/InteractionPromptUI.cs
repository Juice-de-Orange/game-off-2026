using TMPro;
using UnityEngine;

namespace GameOff.UI
{
    /// <summary>Shows "[E] Prompt" near the screen center while the player looks at an Interactable.</summary>
    public class InteractionPromptUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text label;
        [SerializeField] private GameObject root;

        private string _shown;

        private void Awake() => Hide();

        public void Show(string prompt)
        {
            if (_shown == prompt && root.activeSelf) return;
            _shown = prompt;
            label.text = $"[E]  {prompt}"; // menu-button look (HudStyleBuilder): key, two spaces, action in capitals
            root.SetActive(true);
        }

        public void Hide()
        {
            if (root == null || !root.activeSelf) return; // root may already be destroyed during scene unload
            root.SetActive(false);
            _shown = null;
        }
    }
}
