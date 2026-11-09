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
        [Tooltip("Texts of the end screen; ShowEnd(title, body, button) replaces them (e.g. game over when the rent can't be paid).")]
        [SerializeField] private TMP_Text endTitle;
        [SerializeField] private TMP_Text endBody;
        [SerializeField] private TMP_Text endMenuLabel;

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
        private string _cancelDefault;

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
        private const string DevEndDayLabel = "Dev_end_Day";
        // The third button's own place and look, while it is the red dev button under the dialog.
        private Transform _devParent;
        private Vector2 _devPosition, _devSize, _confirmPosition, _confirmSize, _cancelSize;
        private Sprite _devSprite;
        private Image.Type _devImageType;
        private ColorBlock _devColors;
        private Color _devTint;

        /// <summary>P35: a real third choice sits in the row with the other two and looks like them; the dev button keeps its place below the dialog.</summary>
        private void LayoutThirdButton(bool thirdChoice, bool hasConfirm)
        {
            var devRect = (RectTransform)dialogDev.transform;
            var confirmRect = (RectTransform)dialogConfirm.transform;
            var backRect = (RectTransform)dialogCancel.transform;
            if (_devParent == null)
            {
                _devParent = devRect.parent;
                _devPosition = devRect.anchoredPosition;
                _devSize = devRect.sizeDelta;
                _confirmPosition = confirmRect.anchoredPosition;
                _confirmSize = confirmRect.sizeDelta;
                _cancelSize = backRect.sizeDelta;
                _devSprite = dialogDev.image.sprite;
                _devImageType = dialogDev.image.type;
                _devColors = dialogDev.colors;
                _devTint = dialogDev.image.color;
            }
            confirmRect.anchoredPosition = _confirmPosition;
            confirmRect.sizeDelta = _confirmSize;
            backRect.sizeDelta = _cancelSize;
            if (!thirdChoice)
            {
                devRect.SetParent(_devParent, false);
                devRect.anchoredPosition = _devPosition;
                devRect.sizeDelta = _devSize;
                dialogDev.image.sprite = _devSprite;
                dialogDev.image.type = _devImageType;
                dialogDev.colors = _devColors;
                dialogDev.image.color = _devTint;
                return;
            }
            devRect.SetParent(confirmRect.parent, false);
            float y = _confirmPosition.y;
            dialogDev.image.sprite = dialogCancel.image.sprite;
            dialogDev.image.type = dialogCancel.image.type;
            dialogDev.colors = dialogCancel.colors;
            dialogDev.image.color = dialogCancel.image.color;
            if (!hasConfirm)
            {
                devRect.sizeDelta = _confirmSize;
                devRect.anchoredPosition = new Vector2(-140f, y);
                backRect.anchoredPosition = new Vector2(140f, backRect.anchoredPosition.y);
            }
            else
            {
                var narrow = new Vector2(150f, _confirmSize.y);
                confirmRect.sizeDelta = devRect.sizeDelta = backRect.sizeDelta = narrow;
                confirmRect.anchoredPosition = new Vector2(-165f, y);
                devRect.anchoredPosition = new Vector2(0f, y);
                backRect.anchoredPosition = new Vector2(165f, backRect.anchoredPosition.y);
            }
            // The button keeps the tint of its old colours until its state changes; switching it off and on applies the new ones.
            dialogDev.gameObject.SetActive(false);
            dialogDev.gameObject.SetActive(true);
        }

        public void ShowDialog(string title, string body, string confirmLabel, Action confirm, Action dev, Action cancel,
            string devLabel = DevEndDayLabel, string cancelLabel = null)
        {
            var cancelText = dialogCancel.GetComponentInChildren<TMP_Text>();
            _cancelDefault ??= cancelText.text;
            cancelText.text = string.IsNullOrWhiteSpace(cancelLabel) ? _cancelDefault : cancelLabel;
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
            LayoutThirdButton(dev != null && !devLabel.StartsWith("Dev_"), confirm != null);
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

        /// <summary>End screen with other texts, e.g. the placeholder game over (P10: rent not paid). The only button goes to the menu.</summary>
        public void ShowEnd(string title, string body, string buttonLabel)
        {
            if (endTitle != null) endTitle.text = title;
            if (endBody != null) endBody.text = body;
            if (endMenuLabel != null) endMenuLabel.text = buttonLabel;
            ShowEnd();
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
