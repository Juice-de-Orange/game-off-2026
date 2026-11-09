using System.Collections.Generic;
using GameOff.Counter;
using GameOff.Economy;
using GameOff.Player;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace GameOff.UI
{
    /// <summary>
    /// Inventory without a vendor (P46, design 1): your items left, details right, today's money movements (TODAY) below,
    /// credits top right – the only place outside vendors where the balance is shown. I opens/closes it while walking freely
    /// (not in other windows); Esc or CLOSE closes. The player stands still meanwhile.
    /// P81: seated at the counter, I opens the same window as the SHIFT inventory (what was confiscated this shift) instead;
    /// your own items can't be reached from the desk. The desk ignores keys and mouse meanwhile.
    /// </summary>
    public class InventoryWindow : MonoBehaviour
    {
        [SerializeField] private FirstPersonController player;
        [Tooltip("P81: seated here, I shows the shift inventory.")]
        [SerializeField] private CounterStation counter;
        [SerializeField] private GameObject root;
        [SerializeField] private TMP_Text title;
        [Tooltip("P81: red dev button in the shift view, fills it with test items.")]
        [SerializeField] private Button devFillButton;
        [SerializeField] private TMP_Text credits;
        [SerializeField] private TMP_Text itemsHeader;
        [SerializeField] private ItemListView itemList;
        [SerializeField] private TMP_Text itemsEmpty;
        [SerializeField] private TMP_Text detailName;
        [SerializeField] private TMP_Text detailTags;
        [SerializeField] private TMP_Text detailDescription;
        [SerializeField] private TMP_Text detailValue;
        [Tooltip("P63: PUT ON / TAKE OFF for wearable items.")]
        [SerializeField] private Button wearButton;
        [SerializeField] private ItemListView todayList;
        [SerializeField] private TMP_Text todayEmpty;
        [SerializeField] private Button closeButton;
        [SerializeField] private Color income = new Color(0.61f, 0.88f, 0.35f);
        [SerializeField] private Color expense = new Color(0.89f, 0.27f, 0.25f);

        private const int TodayRows = 5;

        private InputAction _toggle;
        private ItemData _selected;
        private int _openedFrame = -1;
        private bool _shiftView;

        public bool IsOpen => root.activeSelf;
        public bool IsShiftView => IsOpen && _shiftView;

        /// <summary>The inventory this window shows: your items, or the shift inventory at the counter.</summary>
        private Inventory Shown => _shiftView ? Economy.ShiftItems : Economy.Inventory;

        private static PlayerEconomy Economy => PlayerEconomy.Instance;

        private void Awake()
        {
            _toggle = new InputAction("Inventory", InputActionType.Button, "<Keyboard>/i");
            closeButton.onClick.AddListener(Close);
            wearButton.onClick.AddListener(ToggleWear);
            if (devFillButton != null) devFillButton.onClick.AddListener(() => Economy.DevFillShift());
            root.SetActive(false);
        }

        private void OnEnable() => _toggle.Enable();

        private void OnDisable()
        {
            _toggle.Disable();
            Unsubscribe();
        }

        private void OnDestroy() => _toggle.Dispose();

        private void Update()
        {
            bool pressed = _toggle.WasPressedThisFrame();
            if (IsOpen)
            {
                bool esc = Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
                if ((pressed && Time.frameCount != _openedFrame) || esc) Close();
                return;
            }
            if (!pressed || Economy == null) return;
            // Seated: the shift inventory, unless a stamp or a paper is in hand. Else only while walking freely
            // (not in another window: they all take the player's control).
            if (counter != null && counter.IsSeated)
            {
                if (!counter.IsHoldingStamp && !counter.InputBlocked) Open(true);
            }
            else if (player.ControlEnabled) Open(false);
        }

        public void Open() => Open(false);

        public void Open(bool shiftView)
        {
            _openedFrame = Time.frameCount;
            _selected = null;
            _shiftView = shiftView;
            if (shiftView) counter.SetInputBlocked(true);
            else player.SetControlEnabled(false);
            if (title != null) title.text = shiftView ? "SHIFT · CONFISCATED" : "INVENTORY";
            if (devFillButton != null) devFillButton.gameObject.SetActive(shiftView && Economy.DevTools);
            root.SetActive(true);
            Economy.Wallet.Changed += Refresh;
            Economy.Inventory.Changed += Refresh;
            Economy.ShiftItems.Changed += Refresh;
            Refresh();
        }

        public void Close()
        {
            if (!IsOpen) return;
            root.SetActive(false);
            Unsubscribe();
            // Seated, the player's control stays off: the counter gets its input back instead.
            if (_shiftView) counter.SetInputBlocked(false);
            else player.SetControlEnabled(true);
        }

        private void Unsubscribe()
        {
            if (Economy == null) return;
            Economy.Wallet.Changed -= Refresh;
            Economy.Inventory.Changed -= Refresh;
            Economy.ShiftItems.Changed -= Refresh;
        }

        private void ToggleWear()
        {
            if (_selected == null) return;
            Economy.Wearing = Economy.Wearing == _selected ? null : _selected;
            Refresh();
        }

        private void Refresh()
        {
            if (!IsOpen) return;
            var economy = Economy;
            credits.text = economy.Format(economy.Wallet.Balance);

            var shown = Shown;
            var items = shown.Items;
            itemsHeader.text = (_shiftView ? "CONFISCATED · " : "YOUR ITEMS · ") + items.Count;
            itemsEmpty.text = _shiftView ? "Nothing confiscated this shift." : "Nothing on you.";
            if (_selected != null && shown.Count(_selected) == 0) _selected = null;
            var rows = new List<ItemListView.Row>();
            foreach (var item in items)
            {
                var it = item;
                rows.Add(new ItemListView.Row
                {
                    Icon = item.Icon, Label = item.DisplayName, Value = "×" + shown.Count(item),
                    Selected = _selected == item, OnClick = () => { _selected = it; Refresh(); },
                });
            }
            itemList.Show(rows);
            itemsEmpty.gameObject.SetActive(items.Count == 0);

            bool any = _selected != null;
            detailName.text = any ? _selected.DisplayName : (items.Count > 0 ? "Pick an item" : "");
            detailTags.text = any ? ItemTagText.Chips(_selected) : "";
            detailDescription.text = any ? _selected.Description : "";
            detailValue.text = any ? "Value ~" + economy.Format(_selected.BaseValue) : "";
            bool wearable = any && !_shiftView && _selected.Wearable;
            wearButton.gameObject.SetActive(wearable);
            if (wearable) wearButton.GetComponentInChildren<TMP_Text>().text = economy.Wearing == _selected ? "TAKE OFF" : "PUT ON";

            var today = new List<ItemListView.Row>();
            // P65: notes (the barkeeper's rumour) first; click one to read it in full in the details.
            foreach (var note in economy.Notes)
            {
                var text = note;
                today.Add(new ItemListView.Row
                {
                    Label = "RUMOUR · " + text, Value = "",
                    OnClick = () => { _selected = null; wearButton.gameObject.SetActive(false); detailName.text = "Rumour"; detailTags.text = ""; detailDescription.text = text; detailValue.text = ""; },
                });
            }
            var entries = economy.Wallet.Today;
            for (int i = Mathf.Max(0, entries.Count - (TodayRows - today.Count)); i < entries.Count; i++) // newest rows fit the box
            {
                var entry = entries[i];
                today.Add(new ItemListView.Row
                {
                    Label = entry.Reason, Value = (entry.Amount >= 0 ? "+" : "−") + Mathf.Abs(entry.Amount),
                    ValueColor = entry.Amount >= 0 ? income : expense,
                });
            }
            todayList.Show(today);
            todayEmpty.gameObject.SetActive(today.Count == 0);
        }
    }
}
