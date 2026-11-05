using System.Collections;
using System.Collections.Generic;
using GameOff.Audio;
using GameOff.Travelers;
using TMPro;
using UnityEngine;

namespace GameOff.Counter
{
    /// <summary>
    /// Loudspeaker announcements with subtitles at the counter (P41): shift start, the day's new rule, wrong decisions.
    /// One line at a time, and only while the officer sits there to read it: lines wait through a walk away from the counter.
    /// The same line is never queued twice, so a run of mistakes doesn't build up a backlog.
    /// Plays a placeholder tone on the voice channel until a line has its recording.
    /// </summary>
    public class Announcer : MonoBehaviour
    {
        [SerializeField] private CounterShift shift;
        [SerializeField] private CounterStation station;
        [SerializeField] private Announcements lines;
        [Tooltip("Shown while a line is spoken (bar behind the text).")]
        [SerializeField] private GameObject subtitleRoot;
        [SerializeField] private TMP_Text subtitle;

        private readonly Queue<(string text, AudioClip clip)> _queue = new Queue<(string, AudioClip)>();
        private Coroutine _speaking;

        private void OnEnable()
        {
            shift.ShiftStarted += OnShiftStarted;
            shift.DecisionMade += OnDecisionMade;
        }

        private void OnDisable()
        {
            shift.ShiftStarted -= OnShiftStarted;
            shift.DecisionMade -= OnDecisionMade;
            _queue.Clear();
            _speaking = null; // the coroutine stops with the component
            subtitleRoot.SetActive(false);
        }

        private void OnShiftStarted()
        {
            Say(lines.shiftStart, lines.shiftStartClip);
            if (!string.IsNullOrWhiteSpace(shift.NewToday)) Say(shift.NewToday, null);
        }

        private void OnDecisionMade(TravelerData traveler, Verdict chosen, Judgement judgement)
        {
            if (!judgement.Correct) Say(lines.warning, lines.warningClip);
        }

        private void Say(string text, AudioClip clip)
        {
            foreach (var queued in _queue)
                if (queued.text == text) return;
            _queue.Enqueue((text, clip));
            _speaking ??= StartCoroutine(Speak());
        }

        private IEnumerator Speak()
        {
            while (_queue.Count > 0)
            {
                while (!station.IsSeated) yield return null;
                var (text, clip) = _queue.Dequeue();
                subtitle.text = text;
                subtitleRoot.SetActive(true);
                if (clip != null) AudioManager.PlayVoice(clip);
                else AudioManager.PlayPreview(AudioChannel.Voice);
                float left = Mathf.Max(lines.secondsPerLine, clip != null ? clip.length : 0f);
                while (left > 0f)
                {
                    if (station.IsSeated) left -= Time.unscaledDeltaTime;
                    yield return null;
                }
            }
            subtitleRoot.SetActive(false);
            _speaking = null;
        }
    }
}
