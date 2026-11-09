using System.Text;
using GameOff.Day;
using GameOff.Economy;
using GameOff.Travelers;
using TMPro;
using UnityEngine;

namespace GameOff.UI
{
    /// <summary>
    /// The departure board in the hall (P31): each side has two small CRTs – the left one shows day, phase and the next shuttle (a
    /// made-up time that moves on with every decision at the counter), the right one the market notice of the day (P68).
    /// </summary>
    public class HallBoard : MonoBehaviour
    {
        [SerializeField] private DayManager day;
        [SerializeField] private CounterShift shift;
        [SerializeField] private TMP_Text[] statusScreens = new TMP_Text[0];
        [SerializeField] private TMP_Text[] noticeScreens = new TMP_Text[0];
        [Tooltip("First shuttle of the day, minutes after midnight (08:00 = 480).")]
        [SerializeField] private int firstShuttleMinutes = 8 * 60;
        [SerializeField] private int minutesBetweenShuttles = 25;

        private const string Amber = "#FFD36B";
        private int _decisions;
        private float _next;

        private void OnEnable()
        {
            if (day != null) day.DayStarted += OnDayStarted;
            if (shift != null)
            {
                shift.DecisionMade += OnDecision;
                shift.ShiftEnded += Refresh;
                shift.ShiftStarted += Refresh;
            }
        }

        private void OnDisable()
        {
            if (day != null) day.DayStarted -= OnDayStarted;
            if (shift != null)
            {
                shift.DecisionMade -= OnDecision;
                shift.ShiftEnded -= Refresh;
                shift.ShiftStarted -= Refresh;
            }
        }

        private void Start() => Refresh();

        private void OnDayStarted(int n)
        {
            _decisions = 0;
            Refresh();
        }

        private void OnDecision(TravelerData traveler, Verdict chosen, Judgement judgement)
        {
            _decisions++;
            Refresh();
        }

        // The phase changes through the shift and the bed; once a second is enough to follow it.
        private void Update()
        {
            if (Time.unscaledTime < _next) return;
            _next = Time.unscaledTime + 1f;
            Refresh();
        }

        public void Refresh()
        {
            if (day == null) return;
            var status = new StringBuilder();
            status.AppendLine($"<color={Amber}>DAY {day.Day}</color>");
            switch (day.CurrentPhase)
            {
                case DayManager.Phase.Morning: status.AppendLine("COUNTER OPENS SOON"); break;
                case DayManager.Phase.Shift: status.AppendLine("SHIFT IN PROGRESS"); break;
                case DayManager.Phase.Evening: status.AppendLine(day.IsCurfew ? "NIGHT · CURFEW" : "EVENING · SHOPS OPEN"); break; // P109
                default: status.AppendLine("NIGHT"); break;
            }
            if (day.CurrentPhase == DayManager.Phase.Morning || day.CurrentPhase == DayManager.Phase.Shift)
            {
                int minutes = firstShuttleMinutes + _decisions * minutesBetweenShuttles;
                status.AppendLine($"NEXT SHUTTLE {minutes / 60 % 24:00}:{minutes % 60:00}");
                status.Append($"DOCK {_decisions % 3 + 1}");
            }
            else status.Append("NO MORE ARRIVALS");

            var notice = new StringBuilder();
            notice.AppendLine($"<color={Amber}>NOTICE</color>");
            int count = 0;
            foreach (var e in Market.TodayEvents)
            {
                if (string.IsNullOrWhiteSpace(e.bulletinLine)) continue;
                notice.AppendLine(e.bulletinLine.ToUpperInvariant());
                count++;
            }
            if (count == 0) notice.Append("NONE TODAY");

            Write(statusScreens, status.ToString().TrimEnd());
            Write(noticeScreens, notice.ToString().TrimEnd());
        }

        private static void Write(TMP_Text[] screens, string text)
        {
            foreach (var screen in screens)
                if (screen != null && screen.text != text) screen.text = text;
        }
    }
}
