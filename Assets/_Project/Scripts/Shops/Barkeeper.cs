using GameOff.Audio;
using GameOff.Core;
using GameOff.Day;
using GameOff.Economy;
using GameOff.Player;
using UnityEngine;

namespace GameOff.Shops
{
    /// <summary>
    /// The barkeeper (P65): buys nothing for money. Each day he wants items with one label (the sign at the bar, DayEntry.barWish);
    /// who brings them gets a rumour about tomorrow – the rumour texts of tomorrow's market events, so it is always true.
    /// The rumour then stays readable under TODAY in the inventory until the morning. Like every trade, only after the shift.
    /// </summary>
    public class Barkeeper : MonoBehaviour
    {
        [SerializeField] private Interactable interactable;
        [SerializeField] private FirstPersonController player;
        [SerializeField] private DayManager day;
        [SerializeField] private DayFlowUI dialogs;
        [Tooltip("Translucent box over the trigger, visible only with the dev tools.")]
        [SerializeField] private GameObject debugView;

        private int _servedDay;

        private static PlayerEconomy Economy => PlayerEconomy.Instance;
        private BarWish Wish => day != null ? day.Entry(day.Day).barWish : null;

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
            if (debugView != null) debugView.SetActive(day == null || day.DevTools);
        }

        private bool IsOpen => day == null || day.CurrentPhase == DayManager.Phase.Evening;

        private void OnInteract()
        {
            if (Economy == null || day == null || dialogs == null) return;
            AudioManager.PlayOnVoice(s => s.vendorGreet);
            player.SetControlEnabled(false);
            if (!IsOpen)
            {
                dialogs.ShowDialog("The bar is still closed", "Come back after your shift.", null, null,
                    day.DevTools ? Talk : null, Resume, "Dev_force_Bar");
                return;
            }
            Talk();
        }

        private void Talk()
        {
            player.SetControlEnabled(false);
            var wish = Wish;
            if (_servedDay == day.Day) { ShowRumour(); return; }
            if (Economy.BarkeeperSulks(day.Day))
            {
                // P66: you turned his man away – no rumour tonight or tomorrow (Martin, 07.11.).
                dialogs.ShowDialog("THE LAST ORBIT", "Not today, officer. You know why.", null, null, null, Resume);
                return;
            }
            if (wish == null || !wish.Active)
            {
                dialogs.ShowDialog("THE LAST ORBIT", "Nothing I need today. Have a drink some other time.", null, null, null, Resume);
                return;
            }
            int have = BarWishRules.Matching(Economy.Inventory, wish.tag);
            bool enough = have >= wish.amount;
            string body = $"Today I'd like: {wish.Text}.\nYou have: {have}.\n\n" +
                          (enough ? "Hand them over and I'll tell you what I heard." : "Bring me what I asked for, and I'll tell you something worth knowing.");
            dialogs.ShowDialog("THE LAST ORBIT", body, enough ? "GIVE" : null, enough ? Give : null, null, Resume);
        }

        private void Give()
        {
            if (!BarWishRules.Give(Economy.Inventory, Wish)) { Resume(); return; }
            _servedDay = day.Day;
            Economy.Notes.Add(Rumour());
            ShowRumour();
        }

        private void ShowRumour() =>
            dialogs.ShowDialog("A RUMOUR", Rumour(), null, null, null, Resume);

        private string Rumour() => day.Day >= day.DayCount ? BarWishRules.Quiet : BarWishRules.Rumour(day.Entry(day.Day + 1).marketEvents);

        private void Resume() => player.SetControlEnabled(true);
    }
}
