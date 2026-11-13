using GameOff.Audio;
using GameOff.Core;
using GameOff.Day;
using GameOff.Economy;
using GameOff.Player;
using UnityEngine;

namespace GameOff.Shops
{
    /// <summary>
    /// The drinks machine in the hall (P30): [E] buys one item of its stock straight away (Nova Cola at today's market price),
    /// no window. Limited per day like every vendor (VendorSupply, refilled in the morning) and, like every trade, only after
    /// the shift; with the dev tools the "out of order" dialog has a red button.
    /// </summary>
    public class VendingMachine : MonoBehaviour
    {
        [SerializeField] private VendorStock stock;
        [SerializeField] private Interactable interactable;
        [SerializeField] private FirstPersonController player;
        [SerializeField] private DayManager day;
        [SerializeField] private DayFlowUI dialogs;
        [Tooltip("Translucent box over the trigger, visible only with the dev tools.")]
        [SerializeField] private GameObject debugView;

        private static PlayerEconomy Economy => PlayerEconomy.Instance;
        private ItemData Item => stock != null && stock.Entries.Count > 0 ? stock.Entries[0].item : null;
        private bool IsOpen => day == null || day.TradingOpen;

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

        private void Start()
        {
            ApplyDevTools();
            UpdatePrompt();
        }

        private void ApplyDevTools()
        {
            if (debugView != null) debugView.SetActive(DayManager.DevToolsOn(day));
        }

        private void UpdatePrompt()
        {
            if (Item != null) interactable.SetPrompt($"Buy {Item.DisplayName} ({stock.PriceFor(Item)} CR)");
        }

        private void OnInteract()
        {
            if (Economy == null || Item == null) return;
            UpdatePrompt();
            if (!IsOpen && dialogs != null && player != null)
            {
                player.SetControlEnabled(false);
                bool curfew = day != null && day.IsCurfew;
                dialogs.ShowDialog(curfew ? "CLOSED" : "OUT OF ORDER", curfew ? day.CurfewClosedText : "Out of order until the evening.", null, null,
                    day.DevTools ? Buy : null, () => player.SetControlEnabled(true), "Dev_force_Buy");
                return;
            }
            Buy();
        }

        public void Buy()
        {
            if (player != null && !player.ControlEnabled) player.SetControlEnabled(true);
            var supply = Economy.Supply(stock);
            int price = stock.PriceFor(Item);
            if (supply.Count(Item) == 0) { dialogs?.ShowHint("Sold out. Refilled every morning.", 3f); return; }
            if (!Economy.Inventory.CanAdd(Item)) { dialogs?.ShowHint("You can't carry another one.", 3f); return; }
            if (!Economy.Wallet.TrySpend(price, "Vending machine · " + Item.DisplayName)) { dialogs?.ShowHint("Not enough credits.", 3f); return; }
            supply.Take(Item);
            Economy.Inventory.Add(Item);
            AudioManager.Play(s => s.tradeBuy);
            dialogs?.ShowHint($"A can of {Item.DisplayName} rolls out.", 3f);
            UpdatePrompt();
        }
    }
}
