using System;
using GameOff.Day;
using GameOff.Economy;
using GameOff.Player;
using UnityEngine;

namespace GameOff.Departure
{
    /// <summary>
    /// P72: the ticket machine next to the shuttle hatch. E shows the price and what is missing; with enough money BUY takes
    /// HouseholdTuning.ticketPrice and the player keeps the ticket (PlayerEconomy.HasTicket) until they board.
    /// Like every trade it only sells after the shift; with the dev tools a red button hands out a free ticket.
    /// </summary>
    public class TicketTerminal : MonoBehaviour
    {
        [SerializeField] private Interactable interactable;
        [SerializeField] private DayManager day;
        [SerializeField] private FirstPersonController player;
        [SerializeField] private DayFlowUI dialogs;

        private static PlayerEconomy Economy => PlayerEconomy.Instance;

        private void OnEnable() => interactable.OnInteract.AddListener(OnInteract);
        private void OnDisable() => interactable.OnInteract.RemoveListener(OnInteract);

        private bool IsOpen => day == null || day.TradingOpen;
        private int Price => Economy != null && Economy.HouseholdTuning != null ? Economy.HouseholdTuning.ticketPrice : 0;

        /// <summary>The dialog for the current state; canBuy shows the BUY button. P109: curfew = closed after 23:00 with curfewText.</summary>
        public static (string title, string body, bool canBuy) Describe(bool open, bool hasTicket, int balance, int price,
            Func<int, string> format, bool curfew = false, string curfewText = null)
        {
            string title = "TICKET HOME · " + format(price);
            if (hasTicket) return ("YOUR TICKET HOME", "You have your ticket. The shuttle behind the hatch is waiting for you.", false);
            if (curfew) return (title, "One way to the colonies.\n" + (string.IsNullOrWhiteSpace(curfewText) ? "Closed for the night." : curfewText), false);
            if (!open) return (title, "One way to the colonies.\nTicket sales open after your shift.", false);
            if (balance < price)
                return (title, $"One way to the colonies.\nYou have {format(balance)}. You need {format(price - balance)} more.", false);
            return (title, $"One way to the colonies.\nYou have {format(balance)}. After the ticket: {format(balance - price)}.\n\n" +
                           "The shuttle leaves when you board it.", true);
        }

        private void OnInteract()
        {
            if (Economy == null || dialogs == null || player == null) return;
            player.SetControlEnabled(false);
            if (Price <= 0)
            {
                Debug.LogError("[Departure] No ticket price (HouseholdTuning missing on PlayerEconomy).");
                dialogs.ShowDialog("OUT OF ORDER", "The ticket machine flickers and shows nothing.", null, null, null, Resume);
                return;
            }
            bool curfew = day != null && day.IsCurfew;
            var (title, body, canBuy) = Describe(IsOpen, Economy.HasTicket, Economy.Wallet.Balance, Price, Economy.Format,
                curfew, curfew ? day.CurfewClosedText : null);
            bool dev = day != null && day.DevTools && !Economy.HasTicket;
            dialogs.ShowDialog(title, body, "BUY", canBuy ? Buy : (Action)null, dev ? GiveFreeTicket : (Action)null, Resume, "Dev_free_Ticket");
        }

        private void Buy()
        {
            // P109: BUY was shown before 23:00 and pressed after it.
            if (!IsOpen) { dialogs.ShowHint(day.CurfewClosedText, 4f); Resume(); return; }
            if (Economy.Wallet.TrySpend(Price, "Ticket home")) GetTicket();
            else dialogs.ShowHint("Not enough credits.", 4f);
            Resume();
        }

        private void GiveFreeTicket()
        {
            Debug.Log("[Departure] Dev_free_Ticket");
            GetTicket();
            Resume();
        }

        private void GetTicket()
        {
            Economy.HasTicket = true;
            dialogs.ShowHint("Ticket bought. The shuttle is behind the hatch, take the seat when you are ready.", 6f);
        }

        private void Resume() => player.SetControlEnabled(true);
    }
}
