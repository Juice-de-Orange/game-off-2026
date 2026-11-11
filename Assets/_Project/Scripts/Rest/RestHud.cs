using System.Collections;
using TMPro;
using UnityEngine;

namespace GameOff.Rest
{
    /// <summary>
    /// P28: the small HUD while sitting or looking out: one line at the bottom edge (an announcement on the bench, a thought at the
    /// porthole). Shared by every seat; only one can be in use at a time. P140: the keys moved behind [H] (HelpOverlay).
    /// </summary>
    public class RestHud : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private TMP_Text line;
        [SerializeField] private float fadeSeconds = 0.5f;

        private Coroutine _line;

        private void Awake() => root.SetActive(false);

        public void Show()
        {
            root.SetActive(true);
            ClearLine();
        }

        public void Hide()
        {
            ClearLine();
            root.SetActive(false);
        }

        /// <summary>Fades a line in; it fades out again after the given seconds (0 = it stays until the next line or Hide).</summary>
        public void ShowLine(string text, float seconds)
        {
            ClearLine();
            if (string.IsNullOrWhiteSpace(text) || !isActiveAndEnabled) return;
            _line = StartCoroutine(Line(text, seconds));
        }

        private void ClearLine()
        {
            if (_line != null) StopCoroutine(_line);
            _line = null;
            line.text = "";
        }

        private IEnumerator Line(string text, float seconds)
        {
            line.text = text;
            yield return Fade(0f, 1f);
            if (seconds <= 0f) yield break;
            yield return new WaitForSeconds(seconds);
            yield return Fade(1f, 0f);
            line.text = "";
            _line = null;
        }

        private IEnumerator Fade(float from, float to)
        {
            for (float t = 0f; t < 1f; t += Time.deltaTime / Mathf.Max(0.01f, fadeSeconds))
            {
                line.alpha = Mathf.Lerp(from, to, t);
                yield return null;
            }
            line.alpha = to;
        }
    }
}
