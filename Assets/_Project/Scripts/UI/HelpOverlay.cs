using System.Text;
using GameOff.Core;
using GameOff.Counter;
using GameOff.Day;
using GameOff.Departure;
using GameOff.Documents;
using GameOff.Player;
using GameOff.Rest;
using GameOff.Shops;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GameOff.UI
{
    /// <summary>
    /// P140: "[H] Help" at the bottom edge instead of the key bars; [H] shows a calm list of the keys for what the player is doing
    /// right now (HelpKeys), [H] again or Esc closes it. While it is open the other key handlers stand still (BlocksInput), so the
    /// Esc that closes it doesn't also stand up, close a window or pause. Walking, it takes the player's control like a window does.
    /// Not in the main menu, at night, on the end screen, under a dialog or in the pause menu. Built by GameOff/Build Help Overlay into Main.
    /// </summary>
    public class HelpOverlay : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private TMP_Text title;
        [SerializeField] private TMP_Text body;
        [Tooltip("The \"[H] Help\" line at the bottom edge.")]
        [SerializeField] private GameObject prompt;
        [SerializeField] private Color keyColor = new Color(1f, 0.83f, 0.42f);
        [Tooltip("Where the action column starts, in percent of the body width.")]
        [SerializeField] private float actionColumn = 50f;

        [Header("What the overlay looks at (wired by the builder; missing ones are simply never true)")]
        [SerializeField] private FirstPersonController player;
        [SerializeField] private DayManager day;
        [SerializeField] private DayFlowUI dayUi;
        [SerializeField] private PauseMenu pause;
        [SerializeField] private ShiftReportWindow shiftReport;
        [SerializeField] private CounterStation counter;
        [SerializeField] private DocumentDesk desk;
        [SerializeField] private CounterTerminal terminal;
        [SerializeField] private PinBoardCloseUp pinBoard;
        [SerializeField] private InventoryWindow inventory;
        [SerializeField] private TradeWindow[] trades = new TradeWindow[0];
        [SerializeField] private CoinTelescope telescope;
        [SerializeField] private ShuttleFlight flight;
        [SerializeField] private SeatView[] seats = new SeatView[0];
        [SerializeField] private HallBench[] benches = new HallBench[0];

        private static bool _open;
        private static int _closedFrame = -1;

        private InputAction _toggle;
        private bool _begun, _tookControl, _lockHeld;
        private HelpSituation _shown = HelpSituation.None;
        private readonly StringBuilder _text = new StringBuilder();

        /// <summary>
        /// True while the help is open and in the frame it closed: other key handlers ignore their keys meanwhile
        /// (same idea as CounterStation.InputBlocked).
        /// </summary>
        public static bool BlocksInput => _open || Time.frameCount == _closedFrame;

        private void Awake()
        {
            _open = false;
            _closedFrame = -1;
            _toggle = new InputAction("Help", InputActionType.Button, "<Keyboard>/h");
            panel.SetActive(false);
            if (prompt != null) prompt.SetActive(false);
        }

        private void Start()
        {
            // P48: the menu lives in Main; no help until its flight has handed over to the day.
            var frontEnd = FrontEnd.Instance;
            _begun = frontEnd == null;
            if (frontEnd != null) frontEnd.Started += OnStarted;
        }

        private void OnStarted() => _begun = true;

        private void OnEnable() => _toggle.Enable();

        private void OnDisable()
        {
            _toggle.Disable();
            if (_open) Close(false);
        }

        private void OnDestroy()
        {
            if (FrontEnd.Instance != null) FrontEnd.Instance.Started -= OnStarted;
            _toggle.Dispose();
        }

        private void Update()
        {
            var situation = HelpKeys.SituationFor(Context());
            bool available = situation != HelpSituation.None;
            bool helpPressed = _toggle.WasPressedThisFrame();
            bool escape = Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
            if (_open && !_tookControl)
            {
                // In the browser Esc never reaches the game while the mouse is locked (seat, telescope, flight): it only releases the lock.
                _lockHeld |= PointerLockBridge.IsLocked;
                escape |= _lockHeld && !PointerLockBridge.IsLocked;
            }

            bool next = HelpKeys.NextOpen(_open, available, helpPressed, escape);
            if (next && !_open) Open();
            else if (!next && _open) Close(available); // closed by the player: give back what we took; else whoever ended it owns the player
            if (_open && situation != _shown) Show(situation);
            if (prompt != null && prompt.activeSelf != (available && !_open)) prompt.SetActive(available && !_open);
        }

        private HelpContext Context()
        {
            var c = new HelpContext
            {
                InMenu = !_begun,
                Ending = day != null && (day.CurrentPhase == DayManager.Phase.Night || day.CurrentPhase == DayManager.Phase.Finished),
                PanelOpen = (dayUi != null && dayUi.IsPanelOpen) || (shiftReport != null && shiftReport.IsOpen),
                Paused = pause != null && pause.IsPaused,
                Flying = flight != null && flight.IsFlying,
                Telescope = telescope != null && telescope.IsLooking,
                PinBoard = pinBoard != null && pinBoard.IsOpen,
                Inventory = inventory != null && inventory.IsOpen,
                Terminal = terminal != null && terminal.IsOpen,
                AtCounter = counter != null && counter.IsSeated,
                ReadingPapers = desk != null && desk.IsReading,
            };
            foreach (var t in trades)
                if (t != null && t.IsOpen) c.Trade = true;
            foreach (var bench in benches)
                if (bench != null && bench.Seat != null && bench.Seat.IsSeated && bench.CanWait) c.Seat = HelpContext.SeatKind.LookAroundEvening;
            if (c.Seat == HelpContext.SeatKind.None)
                foreach (var seat in seats)
                    if (seat != null && seat.IsSeated)
                        c.Seat = seat.MouseLook ? HelpContext.SeatKind.LookAround : HelpContext.SeatKind.CloseLook;
            return c;
        }

        private void Open()
        {
            _open = true;
            _lockHeld = false;
            // Walking: the list stands still like a window (frees the mouse; PauseMenu then leaves this Esc alone too).
            _tookControl = player != null && player.ControlEnabled;
            if (_tookControl) player.SetControlEnabled(false);
            _shown = HelpSituation.None;
            panel.SetActive(true);
        }

        private void Close(bool giveBackControl)
        {
            _open = false;
            _closedFrame = Time.frameCount;
            panel.SetActive(false);
            if (_tookControl && giveBackControl && player != null) player.SetControlEnabled(true);
            _tookControl = false;
            _shown = HelpSituation.None;
        }

        private void Show(HelpSituation situation)
        {
            _shown = situation;
            if (title != null) title.text = HelpKeys.Title(situation);
            if (body == null) return;
            string color = ColorUtility.ToHtmlStringRGB(keyColor);
            _text.Clear();
            foreach (var (keys, action) in HelpKeys.Lines(situation))
            {
                if (_text.Length > 0) _text.Append('\n');
                _text.Append("<color=#").Append(color).Append('>').Append(keys).Append("</color><pos=")
                    .Append(actionColumn.ToString(System.Globalization.CultureInfo.InvariantCulture)).Append("%>").Append(action);
            }
            body.text = _text.ToString();
        }
    }
}
