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
    /// P143: with the decision there is nothing left to explain, so the board fades away at once. Korr's last words are his own
    /// reaction in the speech bubble as he leaves (StoryDayContent "Korr_1"): board and bubble never talk at the same time, and
    /// nothing of his goes on while the next traveler is at the window.
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
        [Tooltip("P143: fades the board out after the decision (optional; without it the board just closes).")]
        [SerializeField] private CanvasGroup panelFade;
        [Tooltip("P143: how long the board takes to fade out after the decision (s).")]
        [SerializeField] private float fadeSeconds = 0.3f;

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
            Close();
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
            _progress = null; // no step text after this, whatever is clicked or stamped
            if (_closing == null) _closing = StartCoroutine(FadeAndClose());
        }

        private IEnumerator FadeAndClose()
        {
            if (panelFade != null)
                for (float t = 0f; t < fadeSeconds; t += Time.unscaledDeltaTime)
                {
                    panelFade.alpha = 1f - t / fadeSeconds;
                    yield return null;
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
            if (panelFade != null) panelFade.alpha = 1f;
        }
    }
}
