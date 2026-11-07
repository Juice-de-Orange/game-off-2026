using System;
using GameOff.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GameOff.UI
{
    /// <summary>
    /// "REPORT BUG" in the pause menu (P78): opens the pre-filled GitHub issue from <see cref="Url"/> in a new tab.
    /// In WebGL pressing arms it and the browser's release opens the tab (Browser.jslib), like FullscreenButton; a tab opened
    /// from Unity's click would count as a popup without user action and be blocked.
    /// </summary>
    public class BugReportButton : MonoBehaviour, IPointerDownHandler
    {
        [SerializeField] private Button button;

        /// <summary>Built at the moment of the click, so day and phase are current.</summary>
        public Func<string> Url { get; set; }

#if UNITY_WEBGL && !UNITY_EDITOR
        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left && button.interactable && Url != null)
                BrowserBridge.OpenUrlOnRelease(Url());
        }
#else
        public void OnPointerDown(PointerEventData eventData) { }

        private void Awake() => button.onClick.AddListener(() =>
        {
            if (Url != null) BrowserBridge.OpenUrlOnRelease(Url());
        });
#endif
    }
}
