using System.Collections.Generic;
using GameOff.Economy;

namespace GameOff.Rest
{
    /// <summary>
    /// P132: what a tape deck holds and plays, without Unity (EditMode-tested). Slots in order; the deck plays the filled slots one
    /// after the other and starts over (a single cassette repeats). Inserting into an empty deck starts that cassette; inserting while
    /// one plays only queues it. Taking out the playing cassette goes on with the next one in slot order.
    /// </summary>
    public class CassettePlaylist
    {
        public enum Lamp { Off, Wait, Play }

        private readonly CassetteData[] _slots;

        public CassettePlaylist(int slots) => _slots = new CassetteData[System.Math.Max(0, slots)];

        public int SlotCount => _slots.Length;
        /// <summary>The playing slot, -1 while the deck is empty.</summary>
        public int Playing { get; private set; } = -1;
        public CassetteData Current => Playing >= 0 ? _slots[Playing] : null;
        public IReadOnlyList<CassetteData> Slots => _slots;

        public int Count
        {
            get
            {
                int n = 0;
                foreach (var c in _slots) if (c != null) n++;
                return n;
            }
        }

        public CassetteData At(int slot) => slot >= 0 && slot < _slots.Length ? _slots[slot] : null;

        public bool Holds(ItemData item)
        {
            if (item == null) return false;
            foreach (var c in _slots) if (c == item) return true;
            return false;
        }

        public Lamp LampOf(int slot) => At(slot) == null ? Lamp.Off : slot == Playing ? Lamp.Play : Lamp.Wait;

        /// <summary>Puts the cassette into an empty slot. False if the slot is taken, out of range, or the cassette is in already.</summary>
        public bool Insert(int slot, CassetteData cassette)
        {
            if (cassette == null || slot < 0 || slot >= _slots.Length || _slots[slot] != null || Holds(cassette)) return false;
            _slots[slot] = cassette;
            if (Playing < 0) Playing = slot;
            return true;
        }

        /// <summary>Takes the cassette out of the slot (null if empty). If it was playing, the next one in slot order plays.</summary>
        public CassetteData Eject(int slot)
        {
            var cassette = At(slot);
            if (cassette == null) return null;
            _slots[slot] = null;
            if (slot == Playing) Playing = NextFilledAfter(slot);
            return cassette;
        }

        /// <summary>The track is over: the next filled slot after the playing one, round again. Returns the new playing slot (-1 if empty).</summary>
        public int Advance()
        {
            Playing = NextFilledAfter(Playing);
            return Playing;
        }

        /// <summary>The first cassette of the inventory into this slot (it leaves the inventory). Null if there is none or the slot is taken.</summary>
        public CassetteData InsertFrom(Inventory inventory, int slot)
        {
            var cassette = CassetteData.FirstIn(inventory);
            if (cassette == null || slot < 0 || slot >= _slots.Length || _slots[slot] != null) return null;
            if (!inventory.Remove(cassette)) return null;
            if (Insert(slot, cassette)) return cassette;
            inventory.Add(cassette); // can't happen after the checks above; never lose a cassette
            return null;
        }

        /// <summary>Takes the cassette out of the slot and back into the inventory. Null if the slot was empty.</summary>
        public CassetteData EjectTo(Inventory inventory, int slot)
        {
            if (inventory == null || At(slot) == null) return null;
            var cassette = Eject(slot);
            inventory.Add(cassette);
            return cassette;
        }

        /// <summary>P128: the slots as saved (missing entries stay empty); the first filled slot plays.</summary>
        public void Restore(IList<CassetteData> slots)
        {
            for (int i = 0; i < _slots.Length; i++) _slots[i] = null;
            Playing = -1;
            if (slots == null) return;
            for (int i = 0; i < _slots.Length && i < slots.Count; i++)
                if (slots[i] != null && !Holds(slots[i])) _slots[i] = slots[i];
            Playing = NextFilledAfter(-1);
        }

        /// <summary>The first filled slot after this one in slot order, wrapping round (the same slot if it is the only one); -1 if empty.</summary>
        private int NextFilledAfter(int slot)
        {
            int n = _slots.Length;
            for (int step = 1; step <= n; step++)
            {
                int i = ((slot + step) % n + n) % n;
                if (_slots[i] != null) return i;
            }
            return -1;
        }
    }
}
