using GameOff.Day;
using GameOff.Economy;
using GameOff.Player;
using GameOff.UI;
using UnityEngine;

namespace GameOff.Shops
{
    /// <summary>
    /// The cargo drone in the hangar (P46): E opens the trade window in "send home" mode. What sending an item does for the
    /// family comes with P13 – listen to Sent; for now the item just leaves the inventory.
    /// </summary>
    public class DroneTerminal : MonoBehaviour
    {
        [SerializeField] private Interactable interactable;
        [SerializeField] private TradeWindow window;
        [SerializeField] private DayManager day;
        [SerializeField] private GameObject debugView;

        /// <summary>An item was sent home (P13 hooks in here).</summary>
        public event System.Action<ItemData> Sent;

        private void OnEnable() => interactable.OnInteract.AddListener(OnInteract);

        private void OnDisable() => interactable.OnInteract.RemoveListener(OnInteract);

        private void Start()
        {
            if (debugView != null) debugView.SetActive(day == null || day.DevTools);
        }

        private void OnInteract() => window.OpenSendHome("CARGO DRONE · HOME",
            "Pick one of your items on the right. It goes home with the next drone.\n\n<color=#8B9096>(What it does for your family comes with P13.)</color>",
            item =>
            {
                Debug.Log($"[Drone] Sent home: {item.DisplayName} (no effect until P13).");
                Sent?.Invoke(item);
            });
    }
}
