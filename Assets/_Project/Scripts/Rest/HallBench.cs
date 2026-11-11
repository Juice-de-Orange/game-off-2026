using System.Collections;
using GameOff.Audio;
using GameOff.Day;
using GameOff.Economy;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GameOff.Rest
{
    /// <summary>
    /// P28: a waiting bench in the hall. Sitting there the loudspeakers can be heard (a chime and a subtitle every few seconds);
    /// after the shift [F] lets the evening pass: the clock runs on to 23:00 within a few real seconds. P113: after the airlock a thought
    /// comes before the first announcement.
    /// </summary>
    public class HallBench : MonoBehaviour
    {
        [SerializeField] private SeatView seat;
        [SerializeField] private DayManager day;

        private Coroutine _announcements;
        private bool _fastForward;
        private float _rate;
        private int _next = -1;

        private RestSpotTuning Tuning => seat.Tuning;
        public SeatView Seat => seat;
        /// <summary>P140: [F] works now (evening, before 23:00); the [H] help lists it then.</summary>
        public bool CanWait => day != null && day.CurrentPhase == DayManager.Phase.Evening && day.EveningSecondsLeft > 0f;

        private void OnEnable()
        {
            seat.SatDown += OnSatDown;
            seat.StandingUp += OnStandingUp;
        }

        private void OnDisable()
        {
            seat.SatDown -= OnSatDown;
            seat.StandingUp -= OnStandingUp;
            OnStandingUp();
        }

        private void OnSatDown()
        {
            _announcements = StartCoroutine(Announcements());
        }

        private void OnStandingUp()
        {
            if (_announcements != null) StopCoroutine(_announcements);
            _announcements = null;
            _fastForward = false;
        }

        private void Update()
        {
            if (!seat.IsSeated) return;
            var keyboard = Keyboard.current;
            if (!_fastForward && CanWait && !GameOff.UI.HelpOverlay.BlocksInput && keyboard != null && keyboard.fKey.wasPressedThisFrame)
            {
                _fastForward = true;
                _rate = day.EveningSecondsLeft / Mathf.Max(0.1f, Tuning.fastForwardSeconds);
            }
            if (!_fastForward) return;
            day.SkipEvening(_rate * Time.deltaTime);
            if (day.EveningSecondsLeft > 0f) return;
            _fastForward = false;
            seat.Hud.ShowLine(Tuning.eveningOver, Tuning.subtitleSeconds);
        }

        private IEnumerator Announcements()
        {
            var lines = Tuning.hallAnnouncements;
            if (lines == null || lines.Length == 0) yield break;
            if (_next < 0) _next = Random.Range(0, lines.Length);
            yield return new WaitForSeconds(Tuning.firstAnnouncementSeconds);
            string haunting = Confiscation.Instance != null ? Confiscation.Instance.MoraleThought() : null;
            if (!string.IsNullOrEmpty(haunting) && !_fastForward)
            {
                seat.Hud.ShowLine(haunting, Tuning.subtitleSeconds);
                yield return new WaitForSeconds(Tuning.announcementSeconds);
            }
            while (true)
            {
                if (!_fastForward)
                {
                    AudioManager.PlayAnnouncement(); // P115: like the counter's announcer
                    seat.Hud.ShowLine(lines[_next % lines.Length], Tuning.subtitleSeconds);
                    _next = (_next + 1) % lines.Length;
                }
                yield return new WaitForSeconds(Tuning.announcementSeconds);
            }
        }
    }
}
