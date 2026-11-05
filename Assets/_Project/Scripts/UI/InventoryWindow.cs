using System.Collections.Generic;
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
    /// (not at the counter, not in other windows); Esc or CLOSE closes. The player stands still meanwhile.
    /// </summary>
    public class InventoryWindow : MonoBehaviour
    {
        [SerializeField] private FirstPersonController player;
        [SerializeField] private GameObject root;
        [SerializeField] private TMP_Text credits;
        [SerializeField] private TMP_Text itemsHeader;
        [SerializeField] private ItemListView itemList;
        [SerializeField] private TMP_Text itemsEmpty;
        [SerializeField] private TMP_Text detailName;
        [SerializeField] private TMP_Text detailTags;
        [SerializeField] private TMP_Text detailDescription;
        [SerializeField] private TMP_Text detailValue;
        [SerializeField] private ItemListView todayList;
        [SerializeField] private TMP_Text todayEmpty;
        [SerializeField] private Button closeButton;
        [SerializeField] private Color income = new Color(0.61f, 0.88f, 0.35f);
        [SerializeField] private Color expense = new Color(0.89f, 0.27f, 0.25f);

        private const int TodayRows = 5;

        private InputAction _toggle;
        private ItemData _selected;
        private int _openedFrame = -1;

        public bool IsOpen => root.activeSelf;

        private static PlayerEconomy Economy => PlayerEconomy.Instance;

        private void Awake()
        {
            _toggle = new InputAction("Inventory", InputActionType.Button, "<Keyboard>/i");
            closeButton.onClick.AddListener(Close);
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
            // Only while walking freely: not seated at the counter, not in another window (they all take the player's control).
            if (pressed && player.ControlEnabled && Economy != null) Open();
        }

        public void Open()
        {
            _openedFrame = Time.frameCount;
            _selected = null;
            player.SetControlEnabled(false);
            root.SetActive(true);
            Economy.Wallet.Changed += Refresh;
            Economy.Inventory.Changed += Refresh;
            Refresh();
        }

        public void Close()
        {
            if (!IsOpen) return;
            root.SetActive(false);
            Unsubscribe();
            player.SetControlEnabled(true);
        }

        private void Unsubscribe()
        {
            if (Economy == null) return;
            Economy.Wallet.Changed -= Refresh;
            Economy.Inventory.Changed -= Refresh;
        }

        private void Refresh()
        {
            if (!IsOpen) return;
            var economy = Economy;
            credits.text = economy.Format(economy.Wallet.Balance);

            var items = economy.Inventory.Items;
            itemsHeader.text = $"YOUR ITEMS · {items.Count}";
            if (_selected != null && economy.Inventory.Count(_selected) == 0) _selected = null;
            var rows = new List<ItemListView.Row>();
            foreach (var item in items)
            {
                var it = item;
                rows.Add(new ItemListView.Row
                {
                    Label = item.DisplayName, Value = "×" + economy.Inventory.Count(item),
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

            var today = new List<ItemListView.Row>();
            var entries = economy.Wallet.Today;
            for (int i = Mathf.Max(0, entries.Count - TodayRows); i < entries.Count; i++) // newest rows fit the box
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
