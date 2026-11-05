using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace GameOff.Day
{
    /// <summary>
    /// Screens of the day flow (P09): black fade between days, the morning letter, short hints at the bottom, the day label
    /// in the top-left corner, the bed confirmation dialog and the placeholder end screen after the last day (real endings: P12).
    /// Driven by DayManager.
    /// </summary>
    public class DayFlowUI : MonoBehaviour
    {
        [SerializeField] private CanvasGroup fade;
        [SerializeField] private GameObject letterPanel;
        [SerializeField] private TMP_Text letterTitle;
        [SerializeField] private TMP_Text letterBody;
        [SerializeField] private TMP_Text letterNewToday;
        [SerializeField] private Button letterContinue;
        [SerializeField] private GameObject hintBar;
        [SerializeField] private TMP_Text hint;
        [SerializeField] private TMP_Text dayLabel;
        [SerializeField] private GameObject endPanel;
        [SerializeField] private Button endMenuButton;

        [Header("Bed dialog")]
        [SerializeField] private GameObject dialogPanel;
        [SerializeField] private TMP_Text dialogTitle;
        [SerializeField] private TMP_Text dialogBody;
        [SerializeField] private Button dialogConfirm;
        [SerializeField] private Button dialogCancel;
        [Tooltip("Red test button: ends the day even while travelers are waiting (DayManager.showDevTools).")]
        [SerializeField] private Button dialogDev;

        private Action _onLetterClosed;
        private Action _onDialogConfirm;
        private Action _onDialogDev;
        private Action _onDialogCancel;
        private Coroutine _hintRoutine;

        public event Action EndMenuRequested;

        private void Awake()
        {
            letterContinue.onClick.AddListener(CloseLetter);
            endMenuButton.onClick.AddListener(() => EndMenuRequested?.Invoke());
            dialogConfirm.onClick.AddListener(() => CloseDialog(_onDialogConfirm));
            dialogCancel.onClick.AddListener(() => CloseDialog(_onDialogCancel));
            dialogDev.onClick.AddListener(() => CloseDialog(_onDialogDev));
            dialogPanel.SetActive(false);
            letterPanel.SetActive(false);
            endPanel.SetActive(false);
            hintBar.SetActive(false);
        }

        public bool IsDialogOpen => dialogPanel.activeSelf;

        private void Update()
        {
            if (Keyboard.current == null) return;
            // Esc closes the bed dialog like BACK.
            if (dialogPanel.activeSelf && Keyboard.current.escapeKey.wasPressedThisFrame) CloseDialog(_onDialogCancel);
            // Space / Enter also close the letter, so nobody has to hunt for the button.
            if (!letterPanel.activeSelf) return;
            if (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame) CloseLetter();
        }

        /// <summary>
        /// Bed dialog. confirm == null hides the confirm button (e.g. travelers still waiting); dev == null hides the red dev button.
        /// </summary>
        public void ShowDialog(string title, string body, string confirmLabel, Action confirm, Action dev, Action cancel,
            string devLabel = "Dev_end_Day")
        {
            dialogTitle.text = title;
            dialogBody.text = body;
            _onDialogConfirm = confirm;
            _onDialogDev = dev;
            _onDialogCancel = cancel;
            dialogConfirm.gameObject.SetActive(confirm != null);
            if (confirm != null) dialogConfirm.GetComponentInChildren<TMP_Text>().text = confirmLabel;
            dialogDev.gameObject.SetActive(dev != null);
            if (dev != null) dialogDev.GetComponentInChildren<TMP_Text>().text = devLabel;
            // BACK alone sits in the middle, next to SLEEP on the right (positions from DayFlowBuilder).
            var back = (RectTransform)dialogCancel.transform;
            back.anchoredPosition = new Vector2(confirm != null ? 140f : 0f, back.anchoredPosition.y);
            hintBar.SetActive(false);
            dialogPanel.SetActive(true);
        }

        private void CloseDialog(Action callback)
        {
            if (!dialogPanel.activeSelf) return;
            dialogPanel.SetActive(false);
            _onDialogConfirm = _onDialogDev = _onDialogCancel = null;
            callback?.Invoke();
        }

        public void SetBlack(bool black)
        {
            fade.alpha = black ? 1f : 0f;
            fade.blocksRaycasts = black;
        }

        public IEnumerator FadeTo(float target, float seconds)
        {
            fade.blocksRaycasts = true;
            float start = fade.alpha;
            for (float t = 0f; t < seconds; t += Time.unscaledDeltaTime)
            {
                fade.alpha = Mathf.Lerp(start, target, t / seconds);
                yield return null;
            }
            fade.alpha = target;
            fade.blocksRaycasts = target > 0.5f;
        }

        public void SetDayLabel(string text) => dayLabel.text = text;

        public void ShowLetter(string title, string body, string newToday, Action onClosed)
        {
            _onLetterClosed = onClosed;
            letterTitle.text = title;
            letterBody.text = body;
            letterNewToday.text = newToday;
            letterNewToday.gameObject.SetActive(!string.IsNullOrWhiteSpace(newToday));
            letterPanel.SetActive(true);
        }

        public void ShowHint(string text, float seconds = 4f)
        {
            if (_hintRoutine != null) StopCoroutine(_hintRoutine);
            _hintRoutine = StartCoroutine(HintRoutine(text, seconds));
        }

        public void ShowEnd()
        {
            hintBar.SetActive(false);
            endPanel.SetActive(true);
        }

        private void CloseLetter()
        {
            if (!letterPanel.activeSelf) return;
            letterPanel.SetActive(false);
            var callback = _onLetterClosed;
            _onLetterClosed = null;
            callback?.Invoke();
        }

        private IEnumerator HintRoutine(string text, float seconds)
        {
            hint.text = text;
            hintBar.SetActive(true);
            yield return new WaitForSecondsRealtime(seconds);
            hintBar.SetActive(false);
            _hintRoutine = null;
        }
    }
}
