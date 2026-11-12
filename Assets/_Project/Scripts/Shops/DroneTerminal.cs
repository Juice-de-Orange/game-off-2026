using GameOff.Core;
using GameOff.Day;
using GameOff.Economy;
using GameOff.Player;
using GameOff.UI;
using UnityEngine;

namespace GameOff.Shops
{
    /// <summary>
    /// The cargo drone in the hangar (P46): E opens the trade window in "send home" mode. P13: what you send lands in
    /// PlayerEconomy.Delivery – food, power and medicine cover that need in the night's settlement; P131: one item per night and
    /// need, the rest stays in the family's stock for later nights. Other items just go home.
    /// Like every trade, the drone only works after the shift (Martin, 06.11.); with the dev tools a red button forces it.
    /// </summary>
    public class DroneTerminal : MonoBehaviour
    {
        [SerializeField] private Interactable interactable;
        [SerializeField] private TradeWindow window;
        [SerializeField] private DayManager day;
        [SerializeField] private FirstPersonController player;
        [SerializeField] private DayFlowUI dialogs;
        [SerializeField] private GameObject debugView;

        /// <summary>An item was sent home (P13 hooks in here).</summary>
        public event System.Action<ItemData> Sent;

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

        private void ApplyDevTools()
        {
            if (debugView != null) debugView.SetActive(DayManager.DevToolsOn(day));
        }

        private bool IsOpen => day == null || day.TradingOpen;

        private void OnInteract()
        {
            if (IsOpen || dialogs == null || player == null)
            {
                OpenWindow();
                return;
            }
            player.SetControlEnabled(false);
            bool curfew = day.IsCurfew; // P109
            dialogs.ShowDialog(curfew ? "No more drones tonight" : "The drone is loading",
                curfew ? day.CurfewClosedText : "The next drone leaves after your shift.", null, null,
                day.DevTools ? OpenWindow : null, () => player.SetControlEnabled(true), "Dev_force_Send");
        }

        private void OpenWindow() => window.OpenSendHome("CARGO DRONE · HOME",
            "Pick one of your items on the right. It goes home with the next drone.\n\n" +
            "<color=#8B9096>Food, power and medicine go into the family's stock at home. Each night uses one per need, so the bed won't ask " +
            "money for it; the rest lasts for later nights. Fresh food spoils. Other items just go home.</color>",
            item =>
            {
                var economy = PlayerEconomy.Instance;
                if (economy != null) economy.Delivery.Add(item);
                Debug.Log($"[Drone] Sent home: {item.DisplayName}.");
                Sent?.Invoke(item);
            },
            // P110: contraband may be kept by customs tonight.
            item => DroneCustoms.LoadingHint(item, Confiscation.Instance != null ? Confiscation.Instance.Tuning : null));
    }
}
