using System.Linq;
using GameOff.Audio;
using GameOff.Core;
using GameOff.Day;
using GameOff.Economy;
using GameOff.Player;
using UnityEngine;

namespace GameOff.Shops
{
    /// <summary>
    /// The barkeeper (P65): buys nothing for money. Each day he wants items with one label and says so when spoken to (DayEntry.barWish, P93);
    /// who brings them gets a rumour about tomorrow – the rumour texts of tomorrow's market events, so it is always true.
    /// The rumour then stays readable under TODAY in the inventory until the morning. Like every trade, only after the shift.
    /// </summary>
    public class Barkeeper : MonoBehaviour
    {
        /// <summary>P144: his pitch comes from this (VoicePicker.SpeakerPitch), like a vendor's from vendorName.</summary>
        private const string VoiceName = "Barkeeper";

        [Tooltip("P144: talks with this species' murmurs (Human, Vellani, Quobb, Sorvak, Xirrik).")]
        [SerializeField] private string species = "Vellani";
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
            if (debugView != null) debugView.SetActive(DayManager.DevToolsOn(day));
        }

        // P109: after 23:00 (DayManager.IsCurfew) he still talks and repeats a rumour; only GIVE is closed (see Talk).
        private bool IsOpen => day == null || day.CurrentPhase == DayManager.Phase.Evening;

        private void OnInteract()
        {
            if (Economy == null || day == null || dialogs == null) return;
            AudioManager.PlaySpeech(species, VoiceName);
            player.SetControlEnabled(false);
            if (!IsOpen)
            {
                var wish = Wish;
                dialogs.ShowDialog("THE BAR IS STILL CLOSED", "Come back after your shift." + (wish != null && wish.Active ? $"\nToday I'd like: {wish.Text}." : ""), null, null,
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
            if (day.IsCurfew)
            {
                // P109: after 23:00 he still talks, but takes nothing any more
                dialogs.ShowDialog("THE LAST ORBIT", $"{day.CurfewClosedText}\nToday I wanted: {wish.Text}. Too late now.", null, null,
                    day.DevTools && enough ? HandOver : null, Resume, "Dev_force_Give");
                return;
            }
            string body = $"Today I'd like: {wish.Text}.\nYou have: {have}.\n\n" +
                          (enough ? "Hand them over and I'll tell you what I heard." : "Bring me what I asked for, and I'll tell you something worth knowing.");
            dialogs.ShowDialog("THE LAST ORBIT", body, enough ? "GIVE" : null, enough ? Give : null, null, Resume);
        }

        private void Give()
        {
            // P109: GIVE was offered before 23:00 and pressed after it: he takes nothing any more (Talk says so).
            if (day.IsCurfew) { Talk(); return; }
            HandOver();
        }

        /// <summary>The trade itself, without the curfew check: the red Dev_force_Give button calls it after 23:00 too.</summary>
        private void HandOver()
        {
            if (!BarWishRules.Give(Economy.Inventory, Wish)) { Resume(); return; }
            _servedDay = day.Day;
            Economy.Notes.Add(Rumour());
            ShowRumour();
        }

        private void ShowRumour() =>
            dialogs.ShowDialog("A RUMOR", Rumour(), null, null, null, Resume);

        // P75: tomorrow's fixed events plus what recurring characters set off for tomorrow.
        private string Rumour() => day.Day >= day.DayCount ? BarWishRules.Quiet
            : BarWishRules.Rumour(day.Entry(day.Day + 1).marketEvents.Concat(Market.ScheduledFor(day.Day + 1)).Distinct());

        private void Resume() => player.SetControlEnabled(true);
    }
}
