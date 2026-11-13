using System.Collections;
using GameOff.Day;
using GameOff.Economy;
using GameOff.Player;
using UnityEngine;

namespace GameOff.Shops
{
    /// <summary>
    /// P83: the evidence locker at the counter. In the evening [E] offers to break in once a night (Confiscation.BreakIn).
    /// P32: [E] swings the doors open (EvidenceLockerDisplay) and shows what was handed in, sealed; [E] again or walking away shuts
    /// them. Taking anything out is still only the break-in: the dialog comes once the doors are open.
    /// </summary>
    public class EvidenceLocker : MonoBehaviour
    {
        [SerializeField] private Interactable interactable;
        [SerializeField] private DayManager day;
        [SerializeField] private DayFlowUI dialogs;
        [SerializeField] private FirstPersonController player;
        [Tooltip("P32: the open locker with the goods; without it [E] goes straight to the break-in as in P83.")]
        [SerializeField] private EvidenceLockerDisplay display;
        [SerializeField] private EvidenceLockerTuning tuning;

        private bool _triedTonight;
        private Coroutine _asking;

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

        private void OnDay(int _)
        {
            _triedTonight = false;
            if (display != null) display.CloseInstantly();
        }

        private void Update()
        {
            if (display == null || !display.IsOpen || player == null) return;
            float distance = tuning != null ? tuning.autoCloseDistance : 3f;
            if ((player.transform.position - interactable.transform.position).sqrMagnitude > distance * distance) display.Close();
        }

        private void OnInteract()
        {
            var confiscation = Confiscation.Instance;
            if (confiscation == null || dialogs == null) return;
            if (display == null)
            {
                Ask(confiscation);
                return;
            }
            if (display.IsOpen)
            {
                if (_asking != null) StopCoroutine(_asking);
                _asking = null;
                display.Close();
                return;
            }
            display.Open();
            _asking = StartCoroutine(AskWhenOpen(confiscation));
        }

        private IEnumerator AskWhenOpen(Confiscation confiscation)
        {
            while (display.IsOpen && !display.IsFullyOpen) yield return null;
            _asking = null;
            if (display.IsOpen) Ask(confiscation);
        }

        private void Ask(Confiscation confiscation)
        {
            int count = 0;
            foreach (var item in confiscation.Locker.Items) count += confiscation.Locker.Count(item);
            if (count == 0) dialogs.ShowHint("Empty. Goods only come in here \"as ordered\".", 3f);
            else if (day.CurrentPhase != DayManager.Phase.Evening)
                dialogs.ShowHint($"{count} sealed {(count == 1 ? "item" : "items")}, logged one by one. Not while the shift is on. Too many eyes.", 4f);
            else if (_triedTonight) dialogs.ShowHint("Not twice in one night.", 3f);
            else
            {
                player.SetControlEnabled(false);
                dialogs.ShowDialog("EVIDENCE LOCKER", "Break the seals and take what was handed in? Someone may notice.", "BREAK IT", Break, null,
                    () => player.SetControlEnabled(true));
            }
        }

        private void Break()
        {
            _triedTonight = true;
            player.SetControlEnabled(true); // first: a discharge at the limit ends the run and frees the mouse for the end screen
            string result = Confiscation.Instance.BreakIn(Random.value);
            dialogs.ShowHint(result, 6f);
        }
    }
}
