using System;
using System.Collections.Generic;
using GameOff.Audio;
using GameOff.Economy;
using GameOff.Player;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace GameOff.UI
{
    /// <summary>
    /// Trade window (P46, design A): vendor stock left, your items right, details + one action below, credits top right.
    /// Two modes: OpenTrade(stock) for vendors (buy/sell with the stock's factors) and OpenSendHome(onSend) for the cargo drone
    /// (P13 decides what sending does; P46 only removes the item). Esc or BACK closes; the player stands still meanwhile.
    /// Vendors have a limited shelf (P11, PlayerEconomy.Supply): the count shows next to the name, sold-out items can't be bought,
    /// and what you sell to a vendor lands on their shelf.
    /// </summary>
    public class TradeWindow : MonoBehaviour
    {
        [SerializeField] private FirstPersonController player;
        [SerializeField] private GameObject root;
        [SerializeField] private TMP_Text title;
        [SerializeField] private TMP_Text credits;
        [SerializeField] private TMP_Text leftHeader;
        [SerializeField] private ItemListView leftList;
        [SerializeField] private TMP_Text leftEmpty;
        [SerializeField] private TMP_Text rightHeader;
        [SerializeField] private ItemListView rightList;
        [SerializeField] private TMP_Text detailName;
        [SerializeField] private TMP_Text detailTags;
        [SerializeField] private TMP_Text detailDescription;
        [SerializeField] private Button actionButton;
        [SerializeField] private TMP_Text actionLabel;
        [SerializeField] private Button backButton;
        [SerializeField] private TMP_Text message;

        private enum Mode { Trade, SendHome }
        private enum Side { None, Vendor, Player }

        private Mode _mode;
        private VendorStock _stock;
        private Action<ItemData> _onSend;
        private Func<ItemData, string> _sendHint;
        private ItemData _selected;
        private Side _side;

        public bool IsOpen => root.activeSelf;

        private static PlayerEconomy Economy => PlayerEconomy.Instance;

        private void Awake()
        {
            actionButton.onClick.AddListener(DoAction);
            backButton.onClick.AddListener(Close);
            root.SetActive(false);
        }

        private void OnDisable() => Unsubscribe();

        private void Update()
        {
            if (IsOpen && !HelpOverlay.BlocksInput && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) Close();
        }

        public void OpenTrade(VendorStock stock)
        {
            _mode = Mode.Trade;
            _stock = stock;
            _onSend = null;
            title.text = stock.Title;
            leftHeader.text = "FOR SALE";
            Open();
        }

        /// <summary>
        /// Drone: pick one of your items and send it home. onSend runs after the item left the inventory.
        /// P110: hint (optional) returns a warning for the selected item, e.g. that customs may inspect it ("" = none).
        /// </summary>
        public void OpenSendHome(string windowTitle, string info, Action<ItemData> onSend, Func<ItemData, string> hint = null)
        {
            _mode = Mode.SendHome;
            _stock = null;
            _onSend = onSend;
            _sendHint = hint;
            title.text = windowTitle;
            leftHeader.text = "SEND HOME";
            leftEmpty.text = info;
            Open();
        }

        public void Close()
        {
            if (!IsOpen) return;
            root.SetActive(false);
            Unsubscribe();
            player.SetControlEnabled(true);
        }

        private void Open()
        {
            if (Economy == null) { Debug.LogError("[Trade] No PlayerEconomy in the scene."); return; }
            _selected = null;
            _side = Side.None;
            message.text = "";
            rightHeader.text = "YOUR ITEMS";
            player.SetControlEnabled(false); // frees the mouse
            root.SetActive(true);
            Economy.Wallet.Changed += Refresh;
            Economy.Inventory.Changed += Refresh;
            Refresh();
        }

        private void Unsubscribe()
        {
            if (Economy == null) return;
            Economy.Wallet.Changed -= Refresh;
            Economy.Inventory.Changed -= Refresh;
        }

        private void Select(ItemData item, Side side)
        {
            _selected = item;
            _side = side;
            message.text = "";
            Refresh();
        }

        private void Refresh()
        {
            if (!IsOpen) return;
            var economy = Economy;
            credits.text = economy.Format(economy.Wallet.Balance);

            var left = new List<ItemListView.Row>();
            if (_mode == Mode.Trade)
                foreach (var item in Economy.Supply(_stock).Items)
                {
                    if (item == null) continue;
                    var it = item;
                    int onShelf = Economy.Supply(_stock).Count(item);
                    left.Add(new ItemListView.Row
                    {
                        Icon = item.Icon, Label = $"{item.DisplayName}  ×{onShelf}", Value = _stock.PriceFor(item).ToString(),
                        Selected = _side == Side.Vendor && _selected == item, OnClick = () => Select(it, Side.Vendor),
                    });
                }
            leftList.Show(left);
            leftEmpty.gameObject.SetActive(left.Count == 0);
            if (_mode == Mode.Trade) leftEmpty.text = _stock.EmptyText;

            var right = new List<ItemListView.Row>();
            foreach (var item in economy.Inventory.Items)
            {
                var it = item;
                right.Add(new ItemListView.Row
                {
                    Icon = item.Icon, Label = item.DisplayName, Value = "×" + economy.Inventory.Count(item),
                    Selected = _side == Side.Player && _selected == item, OnClick = () => Select(it, Side.Player),
                });
            }
            rightList.Show(right);

            if (_side == Side.Player && economy.Inventory.Count(_selected) == 0) { _selected = null; _side = Side.None; }
            RefreshDetails();
        }

        private void RefreshDetails()
        {
            bool any = _selected != null;
            detailName.text = any ? _selected.DisplayName : (_mode == Mode.Trade ? "Pick an item" : "Pick one of your items");
            detailTags.text = any ? ItemTagText.Chips(_selected) : "";
            detailDescription.text = any ? _selected.Description : "";
            actionButton.gameObject.SetActive(any);
            if (!any) return;

            if (_mode == Mode.SendHome)
            {
                actionLabel.text = "SEND HOME";
                string hint = _sendHint != null ? _sendHint(_selected) : "";
                if (!string.IsNullOrEmpty(hint)) detailDescription.text += $"\n<color=#E2463F>{hint}</color>";
            }
            else if (_side == Side.Vendor && !Economy.Inventory.CanAdd(_selected))
            {
                actionButton.gameObject.SetActive(false);
                detailDescription.text += "\n<color=#8B9096>You already have one.</color>";
            }
            else if (_side == Side.Vendor && Economy.Supply(_stock).Count(_selected) == 0)
            {
                actionButton.gameObject.SetActive(false);
                detailDescription.text += "\n<color=#8B9096>Sold out. More tomorrow morning.</color>";
            }
            else if (_side == Side.Vendor) actionLabel.text = "BUY · " + Economy.Format(_stock.PriceFor(_selected));
            else if (_stock.Buys(_selected)) actionLabel.text = "SELL · " + Economy.Format(_stock.OfferFor(_selected));
            else
            {
                actionButton.gameObject.SetActive(false);
                detailDescription.text += "\n<color=#8B9096>Not bought here.</color>";
            }
        }

        private void DoAction()
        {
            if (_selected == null) return;
            var economy = Economy;
            var item = _selected;
            if (_mode == Mode.SendHome)
            {
                if (!economy.Inventory.Remove(item)) return;
                message.text = $"{item.DisplayName} goes home with the next drone.";
                _onSend?.Invoke(item);
            }
            else if (_side == Side.Vendor)
            {
                if (!economy.Inventory.CanAdd(item)) { message.text = "You already have one."; return; }
                if (economy.Supply(_stock).Count(item) == 0) { message.text = "Sold out."; return; }
                int price = _stock.PriceFor(item);
                if (!economy.Wallet.TrySpend(price, $"{_stock.Title} · {item.DisplayName}")) { message.text = "Not enough credits."; return; }
                economy.Supply(_stock).Take(item);
                economy.Inventory.Add(item);
                message.text = $"Bought {item.DisplayName}.";
                AudioManager.Play(s => s.tradeBuy);
            }
            else if (_stock.Buys(item) && economy.Inventory.Remove(item))
            {
                economy.Wallet.Add(_stock.OfferFor(item), $"Sold · {item.DisplayName}");
                economy.Supply(_stock).Put(item);
                message.text = $"Sold {item.DisplayName}.";
                AudioManager.Play(s => s.tradeBuy);
            }
            Refresh();
        }
    }
}
