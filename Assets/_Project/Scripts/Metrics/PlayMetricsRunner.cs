using System.Collections;
using System.Collections.Generic;
using System.Text;
using GameOff.Core;
using GameOff.Day;
using GameOff.Economy;
using GameOff.Travelers;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace GameOff.Metrics
{
    /// <summary>
    /// P129: collects anonymous play metrics in the WebGL build and posts them every 30 s to deploy/reports (/api/metrics).
    /// Starts itself, so no scene or generated prefab has to change: after every scene load it looks up the day flow and the
    /// counter once and listens to their events. Off in the editor. Nothing is stored in the browser.
    /// Events (fields in docs/DEPLOY.md, read with tools/metrics_report.py): session, day_start, traveler, shift_end, day_end,
    /// run_end, beat (phase and frame rate), error.
    /// </summary>
    public class PlayMetricsRunner : MonoBehaviour
    {
        private const string Endpoint = "https://kestrel9.maxlworks.com/api/metrics";
        private const float BeatSeconds = 30f;
        private const float SlowFrameSeconds = 0.05f;
        private const int MaxErrors = 25;
        private const int TimeoutSeconds = 10;
        private const int ReasonLength = 160;

        private DayManager _days;
        private CounterShift _shift;
        private bool _sending, _off;
        private float _nextBeat;
        private int _frames, _slowFrames;
        private float _frameTime, _worstFrame;
        private float _arrivedAt, _shiftStartedAt, _eveningStartedAt;
        private int _decidedToday, _wrongToday;
        private bool _curfewToday;
        private readonly HashSet<string> _errors = new HashSet<string>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot()
        {
            if (Application.isEditor) return; // play-testing in the editor would only muddy the numbers
            PlayMetrics.Enabled = true;
            var go = new GameObject("PlayMetrics");
            DontDestroyOnLoad(go);
            go.AddComponent<PlayMetricsRunner>();
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            Application.logMessageReceived += OnLog;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Application.logMessageReceived -= OnLog;
            Unhook();
        }

        private void Start()
        {
            PlayMetrics.Track(new MetricsEvent("session", Now)
                .Add("ua", BrowserBridge.UserAgent)
                .Add("w", Screen.width)
                .Add("h", Screen.height)
                .Add("gpu", SystemInfo.graphicsDeviceName)
                .Add("mem", SystemInfo.systemMemorySize)
                .Add("cpu", SystemInfo.processorCount)
                .Add("quality", QualitySettings.GetQualityLevel())
                .Add("lang", Application.systemLanguage.ToString())
                .Add("load", Now));
            Hook();
            _nextBeat = Now + BeatSeconds;
            Flush(); // the first request says the game loaded, even if the player leaves at once
        }

        private static float Now => Time.realtimeSinceStartup;

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode) => Hook();

        private void Hook()
        {
            Unhook();
            _days = FindAnyObjectByType<DayManager>(FindObjectsInactive.Include);
            _shift = FindAnyObjectByType<CounterShift>(FindObjectsInactive.Include);
            if (_days != null)
            {
                _days.DayStarted += OnDayStarted;
                _days.CurfewStarted += OnCurfew;
                _days.DayEnded += OnDayEnded;
                _days.RunFinished += OnRunFinished;
            }
            if (_shift != null)
            {
                _shift.ShiftStarted += OnShiftStarted;
                _shift.TravelerArrived += OnArrived;
                _shift.DecisionMade += OnDecision;
                _shift.ShiftEnded += OnShiftEnded;
            }
        }

        private void Unhook()
        {
            // ReferenceEquals: a destroyed object still holds our handlers until it is collected
            if (!ReferenceEquals(_days, null))
            {
                _days.DayStarted -= OnDayStarted;
                _days.CurfewStarted -= OnCurfew;
                _days.DayEnded -= OnDayEnded;
                _days.RunFinished -= OnRunFinished;
            }
            if (!ReferenceEquals(_shift, null))
            {
                _shift.ShiftStarted -= OnShiftStarted;
                _shift.TravelerArrived -= OnArrived;
                _shift.DecisionMade -= OnDecision;
                _shift.ShiftEnded -= OnShiftEnded;
            }
            _days = null;
            _shift = null;
        }

        private int Day => _days != null ? _days.Day : 0;

        private string Phase
        {
            get
            {
                if (_days == null) return SceneManager.GetActiveScene().name;
                return _days.Day == 0 ? "Menu" : _days.CurrentPhase.ToString();
            }
        }

        private static int Money => PlayerEconomy.Instance != null ? PlayerEconomy.Instance.Wallet.Balance : 0;

        private static int[] Family()
        {
            var household = PlayerEconomy.Instance != null ? PlayerEconomy.Instance.Household : null;
            if (household == null) return new int[0];
            var conditions = new int[household.Count];
            for (int i = 0; i < conditions.Length; i++) conditions[i] = household.Condition(i);
            return conditions;
        }

        private void OnDayStarted(int day)
        {
            _decidedToday = 0;
            _wrongToday = 0;
            _curfewToday = false;
            PlayMetrics.Track(new MetricsEvent("day_start", Now)
                .Add("day", day)
                .Add("story", _days != null && day <= _days.StoryDays)
                .Add("money", Money)
                .Add("family", Family()));
        }

        private void OnShiftStarted() => _shiftStartedAt = Time.time;

        // Time.time stops while the pause menu is open, so durations at the counter leave pauses out
        private void OnArrived(TravelerData traveler) => _arrivedAt = Time.time;

        private void OnDecision(TravelerData traveler, Verdict chosen, Judgement judgement)
        {
            _decidedToday++;
            if (!judgement.Correct) _wrongToday++;
            var e = new MetricsEvent("traveler", Now)
                .Add("day", Day)
                .Add("n", _decidedToday)
                .Add("id", traveler != null ? traveler.name : "")
                .Add("chosen", chosen.ToString())
                .Add("right", judgement.CorrectVerdict.ToString())
                .Add("ok", judgement.Correct)
                .Add("sec", Time.time - _arrivedAt);
            if (judgement.Disobeyed) e.Add("disobeyed", true);
            if (!judgement.Correct && !string.IsNullOrEmpty(judgement.Reason))
                e.Add("why", judgement.Reason.Length > ReasonLength ? judgement.Reason.Substring(0, ReasonLength) : judgement.Reason);
            PlayMetrics.Track(e);
        }

        private void OnShiftEnded()
        {
            _eveningStartedAt = Time.time;
            PlayMetrics.Track(new MetricsEvent("shift_end", Now)
                .Add("day", Day)
                .Add("decided", _decidedToday)
                .Add("wrong", _wrongToday)
                .Add("citations", _shift != null ? _shift.Citations : 0)
                .Add("sec", Time.time - _shiftStartedAt)
                .Add("money", Money));
        }

        private void OnCurfew(int day) => _curfewToday = true;

        private void OnDayEnded(int day)
        {
            int earned = 0, spent = 0;
            var ledger = new Dictionary<string, int>();
            if (PlayerEconomy.Instance != null)
            {
                foreach (var entry in PlayerEconomy.Instance.Wallet.Today)
                {
                    if (entry.Amount >= 0) earned += entry.Amount;
                    else spent -= entry.Amount;
                    string reason = string.IsNullOrEmpty(entry.Reason) ? "?" : entry.Reason;
                    ledger[reason] = (ledger.TryGetValue(reason, out int sum) ? sum : 0) + entry.Amount;
                }
            }
            PlayMetrics.Track(new MetricsEvent("day_end", Now)
                .Add("day", day)
                .Add("money", Money)
                .Add("earned", earned)
                .Add("spent", spent)
                .Add("ledger", Ledger(ledger))
                .Add("family", Family())
                .Add("evening", Time.time - _eveningStartedAt)
                .Add("curfew", _curfewToday));
            Flush();
        }

        /// <summary>"reason=amount; ..." of the day, biggest amounts first (what the money went to and came from).</summary>
        private static string Ledger(Dictionary<string, int> ledger)
        {
            var entries = new List<KeyValuePair<string, int>>(ledger);
            entries.Sort((a, b) => Mathf.Abs(b.Value).CompareTo(Mathf.Abs(a.Value)));
            var text = new StringBuilder();
            foreach (var entry in entries)
            {
                if (text.Length > 0) text.Append("; ");
                text.Append(entry.Key).Append('=').Append(entry.Value);
                if (text.Length > 450) break; // the backend keeps 500 characters
            }
            return text.ToString();
        }

        private void OnRunFinished()
        {
            PlayMetrics.Track(new MetricsEvent("run_end", Now)
                .Add("day", Day)
                .Add("ending", _days != null ? _days.EndingTitle : "")
                .Add("money", Money)
                .Add("family", Family()));
            Flush();
        }

        private void OnLog(string message, string stackTrace, LogType type)
        {
            if (type != LogType.Exception && type != LogType.Error) return;
            if (_errors.Count >= MaxErrors || string.IsNullOrEmpty(message)) return;
            string msg = message.Length > 200 ? message.Substring(0, 200) : message;
            if (!_errors.Add(msg)) return; // the same error every frame counts once
            string at = string.IsNullOrEmpty(stackTrace) ? "" : stackTrace.Split('\n')[0];
            PlayMetrics.Track(new MetricsEvent("error", Now)
                .Add("type", type.ToString())
                .Add("msg", msg)
                .Add("at", at.Length > 200 ? at.Substring(0, 200) : at)
                .Add("day", Day)
                .Add("phase", Phase));
        }

        private void Update()
        {
            float dt = Time.unscaledDeltaTime;
            _frames++;
            _frameTime += dt;
            if (dt > SlowFrameSeconds) _slowFrames++;
            if (dt > _worstFrame) _worstFrame = dt;
            if (Now < _nextBeat) return;
            _nextBeat = Now + BeatSeconds;
            PlayMetrics.Track(new MetricsEvent("beat", Now)
                .Add("day", Day)
                .Add("phase", Phase)
                .Add("fps", _frameTime > 0f ? _frames / _frameTime : 0f)
                .Add("slow", _slowFrames)
                .Add("worst_ms", _worstFrame * 1000f)
                .Add("paused", Time.timeScale == 0f));
            _frames = _slowFrames = 0;
            _frameTime = _worstFrame = 0f;
            Flush();
        }

        // Leaving the tab: send what is queued, the tab may not come back
        private void OnApplicationFocus(bool focus)
        {
            if (!focus) Flush();
        }

        private void Flush()
        {
            if (_sending || _off) return;
            string batch = PlayMetrics.TakeBatch(Application.version);
            if (batch != null) StartCoroutine(Post(batch));
        }

        private IEnumerator Post(string json)
        {
            _sending = true;
            using (var request = new UnityWebRequest(Endpoint, UnityWebRequest.kHttpVerbPOST))
            {
                request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
                request.downloadHandler = new DownloadHandlerBuffer();
                request.timeout = TimeoutSeconds;
                request.SetRequestHeader("Content-Type", "application/json");
                yield return request.SendWebRequest();
                // 404: metrics are switched off on the server (or not deployed yet); 403: a local build, whose origin the server
                // refuses. Stop asking for this session. Other failures lose this batch; the next one comes with the next beat.
                if (request.responseCode == 404 || request.responseCode == 403)
                {
                    _off = true;
                    PlayMetrics.Enabled = false;
                }
            }
            _sending = false;
        }
    }
}
