using System;
using System.Collections.Generic;

namespace GameOff.Economy
{
    /// <summary>
    /// The player's credits (P46). Add = income, TrySpend = purchase (fails without enough money), Charge = forced payment
    /// that may go below zero (rent, P10 decides what happens then). Every movement lands in Today with its reason.
    /// </summary>
    public class Wallet
    {
        private readonly List<LedgerEntry> _today = new List<LedgerEntry>();

        public Wallet(int startCredits) => Balance = startCredits;

        public int Balance { get; private set; }
        public IReadOnlyList<LedgerEntry> Today => _today;

        public event Action Changed;

        public void Add(int amount, string reason) => Apply(Math.Abs(amount), reason);

        public bool CanAfford(int amount) => Balance >= amount;

        public bool TrySpend(int amount, string reason)
        {
            amount = Math.Abs(amount);
            if (!CanAfford(amount)) return false;
            Apply(-amount, reason);
            return true;
        }

        public void Charge(int amount, string reason) => Apply(-Math.Abs(amount), reason);

        /// <summary>P128: a saved run continues with this balance; TODAY starts empty.</summary>
        public void Restore(int balance)
        {
            Balance = balance;
            _today.Clear();
            Changed?.Invoke();
        }

        /// <summary>New morning: TODAY starts empty.</summary>
        public void BeginDay()
        {
            _today.Clear();
            Changed?.Invoke();
        }

        private void Apply(int amount, string reason)
        {
            Balance += amount;
            _today.Add(new LedgerEntry(amount, reason));
            Changed?.Invoke();
        }
    }
}
