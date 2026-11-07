using System;
using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace GameOff.UI
{
    /// <summary>
    /// P84: the bug report form in the pause menu. The player types what happened and presses SEND; the game adds version, day,
    /// phase, scene and browser (<see cref="Context"/>) and posts it to the backend, which opens a GitHub issue.
    /// Works while paused (Time.timeScale 0): the web request doesn't depend on game time. BACK during sending aborts the request,
    /// so a hanging server never locks the player in the pause menu.
    /// </summary>
    public class BugReportForm : MonoBehaviour
    {
        [SerializeField] private BugReportSettings settings;
        [SerializeField] private TMP_InputField input;
        [SerializeField] private Button sendButton;
        [SerializeField] private Button backButton;
        [SerializeField] private TMP_Text status;

        private const string Hint = "Sent with: version, day, phase, scene, browser.\nReports become issues on our GitHub page - no personal data, please.";

        /// <summary>Builds the JSON for the current text (PauseMenu adds day, phase and the rest).</summary>
        public Func<string, string> Context { get; set; }
        public event Action Closed;

        public bool IsOpen => gameObject.activeSelf;
        public bool IsSending => _request != null;

        private UnityWebRequest _request;
        private bool _sent;

        private void Awake()
        {
            sendButton.onClick.AddListener(Send);
            backButton.onClick.AddListener(Close);
            input.characterLimit = BugReport.MaxLength;
            input.onValueChanged.AddListener(_ => RefreshButtons());
        }

        public void Open()
        {
            gameObject.SetActive(true);
            input.text = "";
            status.text = Hint;
            _sent = false;
            RefreshButtons();
            input.ActivateInputField();
        }

        /// <summary>BACK / Esc: aborts a running request and goes back to the pause menu.</summary>
        public void Close()
        {
            if (!IsOpen) return;
            Hide();
            Closed?.Invoke();
        }

        /// <summary>Hides the form without the Closed event (the pause menu itself closes).</summary>
        public void Hide()
        {
            Abort();
            gameObject.SetActive(false);
        }

        private void OnDisable() => Abort();

        private void Abort()
        {
            if (_request == null) return;
            StopAllCoroutines();
            _request.Abort();
            _request.Dispose();
            _request = null;
        }

        private void RefreshButtons() => sendButton.interactable = !IsSending && !_sent && BugReport.CanSend(input.text);

        private void Send()
        {
            if (IsSending || _sent || Context == null || !BugReport.CanSend(input.text)) return;
            string json = Context(input.text);
            try
            {
                _request = new UnityWebRequest(settings.endpoint, UnityWebRequest.kHttpVerbPOST)
                {
                    uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json)),
                    downloadHandler = new DownloadHandlerBuffer(),
                    timeout = settings.timeoutSeconds,
                };
                _request.SetRequestHeader("Content-Type", "application/json");
            }
            catch (Exception e) // e.g. no settings or a broken endpoint: say so instead of hanging
            {
                Debug.LogError("[BugReport] can't send: " + e.Message);
                Abort();
                status.text = BugReport.Result(0, null);
                return;
            }
            status.text = "Sending...";
            RefreshButtons();
            StartCoroutine(Wait(_request));
        }

        private IEnumerator Wait(UnityWebRequest request)
        {
            yield return request.SendWebRequest();
            long code = request.responseCode;
            string body = request.downloadHandler != null ? request.downloadHandler.text : null;
            _sent = BugReport.Succeeded(code, body);
            status.text = BugReport.Result(code, body);
            if (!_sent) Debug.LogWarning($"[BugReport] not sent: {code} {request.error}");
            request.Dispose();
            _request = null;
            RefreshButtons();
        }
    }
}
