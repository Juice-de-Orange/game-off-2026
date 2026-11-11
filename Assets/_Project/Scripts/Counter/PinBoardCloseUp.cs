using GameOff.Audio;
using GameOff.Player;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace GameOff.Counter
{
    /// <summary>
    /// P33: the pinboard up close while standing. [E] at the board shows the day's notes as papers (rules, wanted list with photos,
    /// market, the supervisor's memo). P130: only today's notes, no paging through earlier mornings.
    /// [E], Esc or CLOSE puts it away. Content comes from PinBoard.Today, so it is the same text as the notes read seated (P41).
    /// </summary>
    public class PinBoardCloseUp : MonoBehaviour
    {
        [SerializeField] private Interactable interactable;
        [SerializeField] private FirstPersonController player;
        [SerializeField] private PinBoard board;
        [SerializeField] private GameObject root;
        [SerializeField] private TMP_Text heading;
        [SerializeField] private TMP_Text empty;
        [SerializeField] private PinNoteCard[] cards = new PinNoteCard[0];
        [SerializeField] private Button closeButton;

        private int _openedFrame = -1;
        private int _closedFrame = -1;

        public bool IsOpen => root.activeSelf;

        private void Awake()
        {
            root.SetActive(false);
            closeButton.onClick.AddListener(Close);
        }

        private void OnEnable() => interactable.OnInteract.AddListener(Open);

        private void OnDisable()
        {
            interactable.OnInteract.RemoveListener(Open);
            if (board != null) board.Changed -= Show;
        }

        private void Update()
        {
            if (!IsOpen) return;
            var keys = Keyboard.current;
            if (keys == null) return;
            if (keys.escapeKey.wasPressedThisFrame || (keys.eKey.wasPressedThisFrame && Time.frameCount != _openedFrame)) Close();
        }

        /// <summary>Public for the test driver as well.</summary>
        public void Open()
        {
            if (IsOpen || board == null || Time.frameCount - _closedFrame <= 1) return; // the [E] that closed it doesn't reopen it
            _openedFrame = Time.frameCount;
            player.SetControlEnabled(false);
            root.SetActive(true);
            board.Changed += Show; // a new morning while it is open
            AudioManager.Play(s => s.paperPickup);
            Show();
        }

        public void Close()
        {
            if (!IsOpen) return;
            _closedFrame = Time.frameCount;
            board.Changed -= Show;
            root.SetActive(false);
            player.SetControlEnabled(true);
        }

        private void Show()
        {
            var today = board.Today;
            var notes = today.Notes;
            heading.text = string.IsNullOrEmpty(today.Day) ? "PINBOARD" : $"PINBOARD  ·  TODAY  ·  {today.Day}";
            for (int i = 0; i < cards.Length; i++) cards[i].Show(i < notes.Count ? notes[i] : null);
            if (empty != null) empty.gameObject.SetActive(notes.Count == 0);
        }
    }
}
