using System.Collections;
using GameOff.Documents;
using GameOff.Travelers;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GameOff.Counter
{
    /// <summary>
    /// P86: while Inspector Korr stands at the counter on day 1, a board in the seated HUD tells the player what to do next.
    /// A board and not the speech bubble: the bubble fades, is replaced by every answer and is hidden while looking at the pinboard.
    /// [N] skips it. Nothing is blocked (TutorialProgress).
    /// </summary>
    public class CounterTutorial : MonoBehaviour
    {
        [SerializeField] private CounterShift shift;
        [SerializeField] private DocumentDesk desk;
        [SerializeField] private CounterStation station;
        [Tooltip("Korr's day-1 appearance: the tutorial runs while this traveler is at the counter.")]
        [SerializeField] private TravelerData tutorialTraveler;
        [SerializeField] private GameObject panel;
        [SerializeField] private TMP_Text text;
        [Tooltip("How long Korr's last words stay up after the decision (s).")]
        [SerializeField] private float endSeconds = 10f;

        private const string Heading = "<b>INSPECTOR KORR:</b> ";
        private const string SkipHint = "  <size=75%>[N] skip</size>";

        private TutorialProgress _progress;
        private Coroutine _closing;

        private void Awake() => panel.SetActive(false);

        private void OnEnable()
        {
            shift.TravelerArrived += OnTravelerArrived;
            shift.DecisionMade += OnDecision;
            shift.DayBegun += Close;
            desk.PaperRead += OnPaperRead;
            desk.FieldClicked += OnFieldClicked;
            station.ActionTriggered += OnAction;
        }

        private void OnDisable()
        {
            shift.TravelerArrived -= OnTravelerArrived;
            shift.DecisionMade -= OnDecision;
            shift.DayBegun -= Close;
            desk.PaperRead -= OnPaperRead;
            desk.FieldClicked -= OnFieldClicked;
            station.ActionTriggered -= OnAction;
            Close();
        }

        private void Update()
        {
            if (!panel.activeSelf || !station.IsSeated || station.InputBlocked) return;
            if (Keyboard.current != null && Keyboard.current.nKey.wasPressedThisFrame) Close();
        }

        private void OnTravelerArrived(TravelerData traveler)
        {
            if (traveler != tutorialTraveler) return;
            _progress = new TutorialProgress();
            Refresh();
            panel.SetActive(true);
        }

        private void OnPaperRead(DocumentView paper)
        {
            if (_progress == null) return;
            _progress.PaperHeld(paper.Type == DocumentType.Notice);
            Refresh();
        }

        private void OnFieldClicked(DocumentData document, string field)
        {
            if (_progress == null) return;
            _progress.FieldAsked();
            Refresh();
        }

        private void OnAction(CounterAction action)
        {
            if (_progress == null || (action != CounterAction.Approve && action != CounterAction.Deny)) return;
            _progress.Stamped(action == CounterAction.Approve);
            Refresh();
        }

        private void OnDecision(TravelerData traveler, Verdict chosen, Judgement judgement)
        {
            if (_progress == null || traveler != tutorialTraveler) return;
            _progress = null;
            if (chosen == Verdict.Arrest || chosen == Verdict.Airlock)
            {
                Close(); // he is in no position to say anything; the printer has the last word
                return;
            }
            text.text = Heading + (judgement.Correct ? TutorialProgress.CorrectEnd(shift.FreeWarnings) : TutorialProgress.WrongEnd);
            _closing = StartCoroutine(CloseAfter(endSeconds, judgement.Correct));
        }

        private IEnumerator CloseAfter(float seconds, bool tip)
        {
            yield return new WaitForSeconds(seconds);
            if (tip)
            {
                text.text = Heading + TutorialProgress.DroneTip;
                yield return new WaitForSeconds(seconds);
            }
            _closing = null;
            Close();
        }

        private void Refresh() => text.text = Heading + _progress.Text + SkipHint;

        private void Close()
        {
            if (_closing != null) StopCoroutine(_closing);
            _closing = null;
            _progress = null;
            panel.SetActive(false);
        }
    }
}
