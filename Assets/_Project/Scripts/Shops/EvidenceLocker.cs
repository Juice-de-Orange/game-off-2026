using GameOff.Day;
using GameOff.Economy;
using GameOff.Player;
using UnityEngine;

namespace GameOff.Shops
{
    /// <summary>P83: the evidence locker at the counter. In the evening [E] offers to break in once a night (Confiscation.BreakIn).</summary>
    public class EvidenceLocker : MonoBehaviour
    {
        [SerializeField] private Interactable interactable;
        [SerializeField] private DayManager day;
        [SerializeField] private DayFlowUI dialogs;
        [SerializeField] private FirstPersonController player;

        private bool _triedTonight;

        private void OnEnable()
        {
            interactable.OnInteract.AddListener(OnInteract);
            if (day != null) day.DayStarted += OnDay;
        }

        private void OnDisable()
        {
            interactable.OnInteract.RemoveListener(OnInteract);
            if (day != null) day.DayStarted -= OnDay;
        }

        private void OnDay(int _) => _triedTonight = false;

        private void OnInteract()
        {
            var confiscation = Confiscation.Instance;
            if (confiscation == null || dialogs == null) return;
            if (day.CurrentPhase != DayManager.Phase.Evening) dialogs.ShowHint("Not while the shift is on. Too many eyes.", 3f);
            else if (_triedTonight) dialogs.ShowHint("Not twice in one night.", 3f);
            else if (confiscation.Locker.Items.Count == 0) dialogs.ShowHint("Nothing in there yet. Hand something in first.", 3f);
            else
            {
                player.SetControlEnabled(false);
                dialogs.ShowDialog("Evidence locker", "Break the lock and take what was handed in? Someone may notice.", "BREAK IT", Break, null,
                    () => player.SetControlEnabled(true));
            }
        }

        private void Break()
        {
            _triedTonight = true;
            string result = Confiscation.Instance.BreakIn(Random.value);
            player.SetControlEnabled(true);
            dialogs.ShowHint(result, 6f);
        }
    }
}
