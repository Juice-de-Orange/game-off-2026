using System;
using System.Collections.Generic;
using GameOff.Counter;
using GameOff.Economy;
using GameOff.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameOff.UI
{
    /// <summary>
    /// P08: at shift end the supervisor asks about today's confiscations. Left the confiscated goods (click one to
    /// pick it for "missed that"), right the three answers; after an answer the outcome text and CONTINUE. Seated, the desk ignores
    /// input meanwhile; the shift also ends a few seconds after the last decision, when the officer may already walk: then the
    /// report takes the player like any window (no walking, free mouse) and gives them back on CONTINUE.
    /// </summary>
    public class ShiftReportWindow : MonoBehaviour
    {
        [SerializeField] private CounterStation counter;
        [Tooltip("Held while the report is open away from the counter.")]
        [SerializeField] private FirstPersonController player;
        [SerializeField] private GameObject root;
        [SerializeField] private ItemListView itemList;
        [SerializeField] private TMP_Text question;
        [SerializeField] private Button nothingButton;
        [SerializeField] private Button missedButton;
        [SerializeField] private TMP_Text missedLabel;
        [SerializeField] private Button orderedButton;
        [SerializeField] private TMP_Text outcome;
        [SerializeField] private Button continueButton;

        private IReadOnlyList<(ItemData item, int count)> _items;
        private ItemData _picked;
        private Func<ShiftChoice, ItemData, ShiftOutcome> _answer;
        private bool _answered;
        private Action _closed;
        private bool _holdsPlayer, _tookControl;

        public bool IsOpen => root.activeSelf;

        private void Awake()
        {
            nothingButton.onClick.AddListener(() => Answer(ShiftChoice.NothingToReport));
            missedButton.onClick.AddListener(() => Answer(ShiftChoice.MissedThat));
            orderedButton.onClick.AddListener(() => Answer(ShiftChoice.AsOrdered));
            continueButton.onClick.AddListener(Close);
            root.SetActive(false);
        }

        /// <summary>
        /// Shows the confiscated goods; answer is called once with the choice and the picked item and returns what happened;
        /// closed runs after CONTINUE (e.g. the discharge, so the text can be read first).
        /// </summary>
        public void Show(IReadOnlyList<(ItemData item, int count)> items, Func<ShiftChoice, ItemData, ShiftOutcome> answer, Action closed = null)
        {
            _closed = closed;
            _items = items;
            _answer = answer;
            _picked = null;
            _answered = false;
            _holdsPlayer = _tookControl = false;
            if (counter != null && counter.IsSeated) counter.SetInputBlocked(this, true);
            else if (player != null)
            {
                // Walking (or the [H] help holds the player): the report needs the mouse. Whoever held the player before keeps them.
                _holdsPlayer = true;
                _tookControl = player.ControlEnabled | HelpOverlay.HandOverPlayer(); // | not ||: the help lets go of them either way
                player.SetControlEnabled(false);
            }
            root.SetActive(true);
            question.text = "End of shift. The supervisor checks today's confiscations.\nWhat do you report?";
            outcome.text = "";
            Refresh();
        }

        private void Update()
        {
            // Someone gave the player back under the report (the counter's stand-up blend, a window closed below it):
            // the report keeps them until CONTINUE, else the mouse is captured and its buttons can't be clicked.
            if (!_holdsPlayer || !IsOpen || !player.ControlEnabled) return;
            _tookControl = true;
            player.SetControlEnabled(false);
        }

        private void Refresh()
        {
            var rows = new List<ItemListView.Row>();
            foreach (var (item, count) in _items)
            {
                var it = item;
                rows.Add(new ItemListView.Row
                {
                    Icon = item.Icon, Label = item.DisplayName, Value = "×" + count, Selected = _picked == item,
                    OnClick = _answered ? (Action)null : () => { _picked = it; Refresh(); },
                });
            }
            itemList.Show(rows);
            nothingButton.gameObject.SetActive(!_answered);
            orderedButton.gameObject.SetActive(!_answered);
            missedButton.gameObject.SetActive(!_answered);
            missedButton.interactable = _picked != null;
            missedLabel.text = _picked != null ? $"\"OH, SEEMS I MISSED THAT\"  ({_picked.DisplayName})" : "\"OH, SEEMS I MISSED THAT\"  (pick an item)";
            continueButton.gameObject.SetActive(_answered);
        }

        private void Answer(ShiftChoice choice)
        {
            if (_answered) return;
            _answered = true;
            var result = _answer?.Invoke(choice, _picked);
            outcome.text = result != null ? result.Text : "";
            Refresh();
        }

        private void Close()
        {
            root.SetActive(false);
            if (counter != null) counter.SetInputBlocked(this, false);
            bool giveBack = _holdsPlayer && _tookControl;
            _holdsPlayer = _tookControl = false;
            var closed = _closed;
            _closed = null;
            closed?.Invoke();
            // After it: a discharge (closed) has ended the run, and the player stays with the end screen (the controller ignores this then).
            if (giveBack) player.SetControlEnabled(true);
        }
    }
}
