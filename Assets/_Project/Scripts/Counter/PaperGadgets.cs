using System.Collections.Generic;
using GameOff.Audio;
using GameOff.Documents;
using GameOff.Economy;
using GameOff.Travelers;
using UnityEngine;

namespace GameOff.Counter
{
    /// <summary>
    /// P16 and P62: what the player bought lies on the desk; a click switches it on or off.
    /// UV lamp: every traveler paper shows its seal, forged ones in red (forgery is otherwise invisible).
    /// Scanner: name and ID that differ between papers on the desk turn red; it only saves comparing by eye.
    /// </summary>
    public class PaperGadgets : MonoBehaviour
    {
        private static readonly string[] ComparedFields = { "name", "id" };

        [SerializeField] private CounterStation station;
        [SerializeField] private DocumentDesk desk;
        [SerializeField] private CounterShift shift;
        [SerializeField] private ItemData uvLampItem;
        [SerializeField] private ItemData scannerItem;
        [Tooltip("On the desk while owned; holds a CounterClickable with the UvLamp action.")]
        [SerializeField] private GameObject uvLamp;
        [Tooltip("On the desk while owned; holds a CounterClickable with the Scanner action.")]
        [SerializeField] private GameObject scanner;

        private bool _uvOn, _scanOn;

        private void OnEnable()
        {
            station.ActionTriggered += OnAction;
            shift.DayBegun += SwitchOff;
        }

        private void OnDisable()
        {
            station.ActionTriggered -= OnAction;
            shift.DayBegun -= SwitchOff;
        }

        private void SwitchOff() => _uvOn = _scanOn = false;

        private void OnAction(CounterAction action)
        {
            if (action == CounterAction.UvLamp) _uvOn = !_uvOn;
            else if (action == CounterAction.Scanner) _scanOn = !_scanOn;
            else return;
            AudioManager.Play(s => s.coverFlip);
        }

        private void Update()
        {
            ShowIfOwned(uvLamp, uvLampItem, ref _uvOn);
            ShowIfOwned(scanner, scannerItem, ref _scanOn);
            Apply(desk.OnDesk);
            Apply(desk.InDrawer);
        }

        private static void ShowIfOwned(GameObject model, ItemData item, ref bool on)
        {
            var economy = PlayerEconomy.Instance;
            bool owned = economy != null && economy.Inventory.Count(item) > 0;
            if (model.activeSelf != owned) model.SetActive(owned);
            if (!owned) on = false;
        }

        private void Apply(IReadOnlyList<DocumentView> papers)
        {
            foreach (var paper in papers)
            {
                if (!paper.AcceptsQuestions) continue;
                paper.ShowUv(_uvOn);
                foreach (string key in ComparedFields)
                    paper.MarkContradiction(key, _scanOn && papers == desk.OnDesk && Contradicted(paper, key));
            }
        }

        private bool Contradicted(DocumentView paper, string key)
        {
            string value = paper.Data != null ? paper.Data.Get(key) : null;
            if (string.IsNullOrWhiteSpace(value)) return false;
            foreach (var other in desk.OnDesk)
            {
                if (other == paper || !other.AcceptsQuestions || other.Data == null) continue;
                string theirs = other.Data.Get(key);
                if (!string.IsNullOrWhiteSpace(theirs) && !TextMatch.Same(value, theirs)) return true;
            }
            return false;
        }
    }
}
