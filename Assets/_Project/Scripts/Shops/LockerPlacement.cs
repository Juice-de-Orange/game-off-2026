using GameOff.Economy;

namespace GameOff.Shops
{
    /// <summary>P32: one compartment of the evidence locker: which goods lie there and how many copies of the model are shown.</summary>
    public readonly struct LockerPlacement
    {
        public readonly int Slot;
        public readonly ItemData Item;
        public readonly int Copies;

        public LockerPlacement(int slot, ItemData item, int copies)
        {
            Slot = slot;
            Item = item;
            Copies = copies;
        }
    }
}
