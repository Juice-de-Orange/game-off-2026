using GameOff.Day;
using GameOff.Economy;
using GameOff.Player;
using UnityEngine;

namespace GameOff.Departure
{
    /// <summary>
    /// P72: the shuttle ramp. With a ticket in the evening E asks "Leave Kestrel-9 for good?" and LEAVE starts the flight home.
    /// Without a ticket it points to the terminal; during the day the shuttle is busy with the travelers.
    /// </summary>
    public class ShuttleBoarding : MonoBehaviour
    {
        [SerializeField] private Interactable interactable;
        [SerializeField] private DayManager day;
        [SerializeField] private FirstPersonController player;
        [SerializeField] private DayFlowUI dialogs;
        [SerializeField] private ShuttleFlight flight;

        private void OnEnable() => interactable.OnInteract.AddListener(OnInteract);
        private void OnDisable() => interactable.OnInteract.RemoveListener(OnInteract);

        private void OnInteract()
        {
            var economy = PlayerEconomy.Instance;
            if (economy == null || dialogs == null || player == null || flight == null || flight.IsFlying) return;
            bool evening = day == null || day.CurrentPhase == DayManager.Phase.Evening;
            player.SetControlEnabled(false);
            if (!economy.HasTicket)
                dialogs.ShowDialog("NO TICKET", "You need a ticket home to board.\nThe ticket machine is next to the ramp.", null, null, null, Resume);
            else if (!evening)
                dialogs.ShowDialog("NOT NOW", "The shuttle is busy with today's travelers.\nIt leaves after your shift.", null, null, null, Resume);
            else
                dialogs.ShowDialog("Leave Kestrel-9 for good?",
                    "Your family is waiting. There is no way back to this station.", "LEAVE", flight.Launch, null, Resume);
        }

        private void Resume() => player.SetControlEnabled(true);
    }
}
