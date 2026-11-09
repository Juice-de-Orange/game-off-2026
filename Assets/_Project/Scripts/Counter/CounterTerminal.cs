using System.Collections;
using System.Collections.Generic;
using GameOff.Audio;
using GameOff.Day;
using GameOff.Travelers;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace GameOff.Counter
{
    /// <summary>
    /// P34: the CRT terminal in the counter desk, a second inspection tool next to the papers. Clicking the CRT (a CounterClickable with the
    /// Terminal action) opens a green terminal window over the desk; it lists every name and ID number on the current traveler's papers,
    /// and clicking one checks it against today's wanted list and the entry record (TerminalLookup). The desk ignores keys and mouse
    /// while it is open; Esc / Q / right click / CLOSE close it. A status line (lane and clock) runs on the CRT itself and in the window.
    /// </summary>
    public class CounterTerminal : MonoBehaviour
    {
        [SerializeField] private CounterStation station;
        [SerializeField] private CounterShift shift;
        [SerializeField] private TerminalTuning tuning;
        [SerializeField] private GameObject window;
        [SerializeField] private TMP_Text output;
        [SerializeField] private TMP_Text status;
        [Tooltip("Holds one button per name / ID number of the current traveler.")]
        [SerializeField] private RectTransform queryList;
        [Tooltip("Inactive template for those buttons (with a TMP_Text child).")]
        [SerializeField] private Button queryTemplate;
        [SerializeField] private Button closeButton;
        [Tooltip("The status line on the CRT in the desk (world-space text); optional.")]
        [SerializeField] private TMP_Text screenStatus;

        private const string Prompt = "SELECT A NAME OR ID NUMBER TO CHECK.";

        private readonly List<Button> _rows = new List<Button>();
        private TerminalLookup.Lane _lane = TerminalLookup.Lane.Closed;
        private int _openedFrame = -1;
        private Coroutine _query;
        private string _shownStatus;
        // What the status line was built from (rebuilt only when one changes).
        private TerminalLookup.Lane _lastLane = (TerminalLookup.Lane)(-1);
        private int _lastServed = -1, _lastTotal = -1, _lastMinutes = -1;
        private bool _lastInspecting;

        public bool IsOpen => window.activeSelf;

        private void Awake()
        {
            window.SetActive(false);
            queryTemplate.gameObject.SetActive(false);
            closeButton.onClick.AddListener(Close);
        }

        private void OnEnable()
        {
            station.ActionTriggered += OnAction;
            station.SeatedChanged += OnSeated;
            shift.DayBegun += OnDayBegun;
            shift.ShiftStarted += OnShiftStarted;
            shift.ShiftEnded += OnShiftEnded;
            shift.TravelerArrived += OnTravelerArrived;
        }

        private void OnDisable()
        {
            station.ActionTriggered -= OnAction;
            station.SeatedChanged -= OnSeated;
            shift.DayBegun -= OnDayBegun;
            shift.ShiftStarted -= OnShiftStarted;
            shift.ShiftEnded -= OnShiftEnded;
            shift.TravelerArrived -= OnTravelerArrived;
            Close();
        }

        private void OnDayBegun() => _lane = TerminalLookup.Lane.Closed;
        private void OnShiftStarted() => _lane = TerminalLookup.Lane.Open;

        private void OnShiftEnded()
        {
            _lane = TerminalLookup.Lane.Over;
            Close(); // the shift report takes the desk input next; closing later would unblock it under the report

        }

        private void OnTravelerArrived(TravelerData traveler)
        {
            if (IsOpen) ShowQueries();
        }

        private void OnSeated(bool seated)
        {
            if (!seated) Close();
        }

        private void OnAction(CounterAction action)
        {
            if (action != CounterAction.Terminal || IsOpen) return;
            Open();
        }

        public void Open()
        {
            _openedFrame = Time.frameCount;
            station.SetInputBlocked(true);
            window.SetActive(true);
            AudioManager.Play(s => s.uiClick);
            ShowQueries();
        }

        public void Close()
        {
            if (!IsOpen) return;
            StopQuery();
            window.SetActive(false);
            station.SetInputBlocked(false);
        }

        private void Update()
        {
            UpdateStatus();
            if (!IsOpen) return;
            // A dialog over the terminal (a character's question) unblocks the desk when answered; the terminal is still open.
            if (!station.InputBlocked) station.SetInputBlocked(true);
            if (Time.frameCount == _openedFrame) return;
            bool back = (Keyboard.current != null && (Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.qKey.wasPressedThisFrame))
                        || (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame);
            if (back) Close();
        }

        private void UpdateStatus()
        {
            var day = shift.Day;
            int total = day != null ? day.Travelers.Count : 0;
            int served = Mathf.RoundToInt(shift.Progress * total);
            bool inspecting = shift.Current != null;
            // The string is built only when something changed (no garbage per frame on WebGL).
            string text = _shownStatus;
            if (_lane != _lastLane || served != _lastServed || total != _lastTotal || inspecting != _lastInspecting || GameClock.Minutes != _lastMinutes)
            {
                _lastLane = _lane; _lastServed = served; _lastTotal = total; _lastInspecting = inspecting; _lastMinutes = GameClock.Minutes;
                text = TerminalLookup.StatusLine(_lane, inspecting, served, total, GameClock.Text);
            }
            if (text == _shownStatus) return;
            _shownStatus = text;
            if (status != null) status.text = text;
            if (screenStatus != null) screenStatus.text = text;
        }

        /// <summary>One button per name / ID number on the current traveler's papers.</summary>
        private void ShowQueries()
        {
            StopQuery();
            foreach (var row in _rows) Destroy(row.gameObject);
            _rows.Clear();
            var traveler = shift.Current;
            if (traveler == null)
            {
                SetOutput(_lane == TerminalLookup.Lane.Over ? "SHIFT OVER. NO MORE TRAVELERS TODAY." : "NO TRAVELER AT THE WINDOW.");
                return;
            }
            foreach (var query in TerminalLookup.Queries(traveler))
            {
                var row = Instantiate(queryTemplate, queryList);
                row.gameObject.SetActive(true);
                row.GetComponentInChildren<TMP_Text>().text = query.Label + "  " + query.Value;
                var q = query;
                row.onClick.AddListener(() => RunQuery(q));
                _rows.Add(row);
            }
            SetOutput(Prompt);
        }

        private void RunQuery(TerminalQuery query)
        {
            StopQuery();
            AudioManager.Play(s => s.uiClick);
            _query = StartCoroutine(QueryRoutine(query));
        }

        private IEnumerator QueryRoutine(TerminalQuery query)
        {
            var traveler = shift.Current;
            SetOutput("> QUERY " + query.Label + " " + query.Value + "\n\nSEARCHING...");
            yield return new WaitForSecondsRealtime(tuning.lookupSeconds);
            var result = TerminalLookup.Check(query, traveler, shift.Day);
            Debug.Log($"[Terminal] {query}: " + (result.IsWanted ? "WANTED" : "no match"));
            SetOutput(TerminalLookup.Render(result));
            output.maxVisibleCharacters = 0;
            output.ForceMeshUpdate();
            int count = output.textInfo.characterCount;
            for (float shown = 0f; shown < count; shown += tuning.charsPerSecond * Time.unscaledDeltaTime)
            {
                output.maxVisibleCharacters = (int)shown;
                yield return null;
            }
            output.maxVisibleCharacters = int.MaxValue;
            _query = null;
        }

        private void StopQuery()
        {
            if (_query != null) StopCoroutine(_query);
            _query = null;
            output.maxVisibleCharacters = int.MaxValue;
        }

        private void SetOutput(string text)
        {
            output.text = text;
            output.maxVisibleCharacters = int.MaxValue;
        }
    }
}
