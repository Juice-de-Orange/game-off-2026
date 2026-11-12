using System.Collections.Generic;
using GameOff.Audio;
using GameOff.Day;
using GameOff.Economy;
using GameOff.Player;
using UnityEngine;

namespace GameOff.Rest
{
    /// <summary>
    /// P132: a cassette lying somewhere on the station. [E] puts it into the inventory and it is gone for the rest of the run
    /// (the morning save keeps the picked-up ids, SaveGame). Built by RestSpotBuilder (CassetteBuilder).
    /// </summary>
    public class CassettePickup : MonoBehaviour
    {
        private static readonly List<CassettePickup> Pickups = new List<CassettePickup>();

        [Tooltip("The save remembers picked-up cassettes by this id (P128), e.g. Bar, Hangar.")]
        [SerializeField] private string id = "Pickup";
        [SerializeField] private CassetteData cassette;
        [SerializeField] private Interactable interactable;
        [Tooltip("Shows the hint after picking it up.")]
        [SerializeField] private DayManager day;
        [Tooltip("{0} = the title on the label.")]
        [SerializeField] private string foundHint = "A cassette: \"{0}\". The tape decks are in your room and at the counter.";

        /// <summary>P128: every pickup in the scene, taken or not (the save reads and restores them).</summary>
        public static IReadOnlyList<CassettePickup> All => Pickups;

        public string Id => id;
        public bool Taken { get; private set; }

        private void Awake() => Pickups.Add(this);
        private void OnDestroy() => Pickups.Remove(this);

        private void OnEnable()
        {
            if (interactable != null) interactable.OnInteract.AddListener(PickUp);
        }

        private void OnDisable()
        {
            if (interactable != null) interactable.OnInteract.RemoveListener(PickUp);
        }

        private void PickUp()
        {
            var economy = PlayerEconomy.Instance;
            if (Taken || cassette == null || economy == null) return;
            // Already owned (in the bag or in a deck): it only disappears. Otherwise it must really land in the bag first.
            bool owned = economy.Inventory.Count(cassette) > 0 || CassetteDeck.AnyHolds(cassette);
            if (!owned && economy.Inventory.Add(cassette) <= 0) return;
            AudioManager.Play(s => s.cassetteEject); // the plastic rattle of a cassette in the hand
            if (day != null && day.UI != null) day.UI.ShowHint(string.Format(foundHint, cassette.Title), 5f);
            SetTaken(true);
        }

        /// <summary>Picked up (it disappears) or lying there again. Also used by the save.</summary>
        public void SetTaken(bool taken)
        {
            Taken = taken;
            gameObject.SetActive(!taken);
        }
    }
}
