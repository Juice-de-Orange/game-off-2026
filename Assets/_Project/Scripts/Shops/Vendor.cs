using GameOff.Audio;
using GameOff.Core;
using GameOff.Day;
using GameOff.Economy;
using GameOff.Player;
using GameOff.UI;
using UnityEngine;

namespace GameOff.Shops
{
    /// <summary>
    /// A vendor in the station (P46 placeholder: grey box until the model is delivered). E plays a placeholder voice tone
    /// (Voice channel, until there are voicelines) and opens the trade window with this vendor's stock. Vendors whose stock is
    /// "open after shift only" are closed in the morning and during the shift (P09 phases); with the dev tools on, the
    /// closed dialog has a red Dev_force_Trade button.
    /// </summary>
    public class Vendor : MonoBehaviour
    {
        [SerializeField] private string vendorName = "Vendor";
        [SerializeField] private VendorStock stock;
        [SerializeField] private Interactable interactable;
        [SerializeField] private TradeWindow window;
        [SerializeField] private FirstPersonController player;
        [Tooltip("Optional: opening hours and dev tools come from the day flow.")]
        [SerializeField] private DayManager day;
        [SerializeField] private DayFlowUI dialogs;
        [Tooltip("Translucent box over the trigger, visible only with the dev tools.")]
        [SerializeField] private GameObject debugView;
        [Tooltip("P90: optional shutter, down whenever this vendor is closed.")]
        [SerializeField] private Counter.SecurityShutter shutter;
        [Tooltip("P109: how fast the shutter falls at the curfew (shutterDropSeconds).")]
        [SerializeField] private Counter.DialogueTuning shutterTuning;

        private bool _shutterOpen = true;

        private void OnEnable()
        {
            interactable.OnInteract.AddListener(OnInteract);
            GameSettings.Changed += ApplyDevTools;
        }

        private void OnDisable()
        {
            interactable.OnInteract.RemoveListener(OnInteract);
            GameSettings.Changed -= ApplyDevTools;
        }

        private void Start() => ApplyDevTools();

        private void Update()
        {
            if (shutter == null || IsOpen == _shutterOpen) return;
            _shutterOpen = IsOpen;
            if (_shutterOpen) shutter.Raise(1.2f);
            else if (day != null && day.IsCurfew)
            {
                // P109: at 23:00 the player may stand right in front of it – the shutter slams down audibly
                shutter.Drop(shutterTuning != null ? shutterTuning.shutterDropSeconds : 0.25f);
                AudioManager.PlayAt(s => s.shutterDrop, shutter.transform.position, 25f);
            }
            else shutter.Drop(0f); // otherwise nobody watches the kiosk close: it happens while the player sleeps
        }

        private void ApplyDevTools()
        {
            if (debugView != null) debugView.SetActive(day == null || day.DevTools);
        }

        private bool IsOpen => !stock.OpenAfterShiftOnly || day == null || day.TradingOpen;

        private void OnInteract()
        {
            if (stock == null)
            {
                Debug.LogError($"[Shops] {vendorName} has no stock: run GameOff/Build Economy into Main.", this);
                return;
            }
            AudioManager.PlayOnVoice(s => s.vendorGreet); // until the vendors have voicelines
            if (IsOpen || dialogs == null)
            {
                window.OpenTrade(stock);
                return;
            }
            player.SetControlEnabled(false);
            dialogs.ShowDialog($"{vendorName} is closed", day.IsCurfew ? day.CurfewClosedText : "Come back after your shift.", null, null,
                day.DevTools ? () => window.OpenTrade(stock) : null, () => player.SetControlEnabled(true), "Dev_force_Trade");
        }
    }
}
