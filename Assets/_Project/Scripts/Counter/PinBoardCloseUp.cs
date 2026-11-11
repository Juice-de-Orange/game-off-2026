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
    /// market, the supervisor's memo); OLDER / NEWER (or the arrow keys) leaf through the earlier mornings, which stay on the board.
    /// [E], Esc or CLOSE puts it away. Content comes from PinBoard.History, so it is the same text as the notes read seated (P41).
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
        [SerializeField] private Button olderButton;
        [SerializeField] private Button newerButton;
        [SerializeField] private Button closeButton;

        private int _age;
        private int _openedFrame = -1;
        private int _closedFrame = -1;

        public bool IsOpen => root.activeSelf;

        private void Awake()
        {
            root.SetActive(false);
            olderButton.onClick.AddListener(() => Leaf(1));
            newerButton.onClick.AddListener(() => Leaf(-1));
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
            if (!IsOpen || GameOff.UI.HelpOverlay.BlocksInput) return; // P140: the Esc that closes the help stays there
            var keys = Keyboard.current;
            if (keys == null) return;
            if (keys.escapeKey.wasPressedThisFrame || (keys.eKey.wasPressedThisFrame && Time.frameCount != _openedFrame)) Close();
            else if (keys.leftArrowKey.wasPressedThisFrame || keys.aKey.wasPressedThisFrame) Leaf(1);
            else if (keys.rightArrowKey.wasPressedThisFrame || keys.dKey.wasPressedThisFrame) Leaf(-1);
        }

        /// <summary>Public for the test driver as well.</summary>
        public void Open()
        {
            if (IsOpen || board == null || Time.frameCount - _closedFrame <= 1) return; // the [E] that closed it doesn't reopen it
            _openedFrame = Time.frameCount;
            _age = 0;
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

        private void Leaf(int step)
        {
            int age = Mathf.Clamp(_age + step, 0, Mathf.Max(0, board.History.DayCount - 1));
            if (age == _age) return;
            _age = age;
            AudioManager.Play(s => s.paperSlide);
            Show();
        }

        private void Show()
        {
            var history = board.History;
            _age = Mathf.Clamp(_age, 0, Mathf.Max(0, history.DayCount - 1));
            var notes = history.NotesOfDay(_age);
            string when = _age == 0 ? "TODAY" : _age == 1 ? "YESTERDAY" : $"{_age} DAYS AGO";
            heading.text = history.DayCount == 0 ? "PINBOARD" : $"PINBOARD  ·  {when}  ·  {history.DayOf(_age)}";
            for (int i = 0; i < cards.Length; i++) cards[i].Show(i < notes.Count ? notes[i] : null);
            if (empty != null) empty.gameObject.SetActive(notes.Count == 0);
            olderButton.interactable = _age < history.DayCount - 1;
            newerButton.interactable = _age > 0;
        }
    }
}
