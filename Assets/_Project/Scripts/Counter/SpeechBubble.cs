using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace GameOff.Counter
{
    /// <summary>
    /// The traveler's speech bubble (P40): a HUD panel beside the speaker's head, following it on screen and kept inside the
    /// view. It is not above the head because the desk view ends just over it. While the officer looks elsewhere or stands up,
    /// it is hidden and its time stands still, so no line is lost.
    /// Alien speech starts as glyphs and is translated by a wave.
    /// </summary>
    public class SpeechBubble : MonoBehaviour
    {
        private const float TopLimit = 0.96f;
        private const float Margin = 0.02f;
        private const float BesideHead = 0.07f; // share of the screen width, so the bubble doesn't cover the speaker

        [SerializeField] private CounterStation station;
        [SerializeField] private DialogueTuning tuning;
        [SerializeField] private RectTransform panel;
        [SerializeField] private RectTransform canvasArea;
        [SerializeField] private TMP_Text text;
        [SerializeField] private float width = 300f;
        [SerializeField] private float padding = 12f;

        private Transform _speaker;
        private Coroutine _speaking;

        /// <summary>The current line has been read: time for the speaker to stop talking.</summary>
        public event Action Finished;

        private void Awake() => panel.gameObject.SetActive(false);

        private void OnDisable() => Hide();

        public void Say(string line, bool alien, Transform speaker)
        {
            Hide();
            _speaker = speaker;
            _speaking = StartCoroutine(Speak(line, alien));
        }

        /// <summary>Removes the bubble at once, without Finished.</summary>
        public void Hide()
        {
            if (_speaking != null) StopCoroutine(_speaking);
            _speaking = null;
            _speaker = null;
            if (panel != null) panel.gameObject.SetActive(false);
        }

        private IEnumerator Speak(string line, bool alien)
        {
            text.text = "";
            panel.gameObject.SetActive(true);
            Follow();
            while (!panel.gameObject.activeInHierarchy) yield return null; // measuring needs active text

            float inner = width - 2f * padding;
            float height = text.GetPreferredValues(line, inner, 0f).y;
            if (alien) height = Mathf.Max(height, text.GetPreferredValues(AlienText.Scramble(line), inner, 0f).y); // glyphs can wrap differently
            panel.sizeDelta = new Vector2(width, height + 2f * padding);
            Follow();

            if (alien)
            {
                text.text = line;
                text.ForceMeshUpdate();
                float duration = Mathf.Max(1, text.textInfo.lineCount) * tuning.translateSecondsPerLine;
                float t = 0f;
                while (t < duration)
                {
                    text.text = AlienText.Reveal(line, Mathf.FloorToInt(line.Length * t / duration), tuning.translateEdge);
                    if (panel.gameObject.activeInHierarchy) t += Time.unscaledDeltaTime;
                    yield return null;
                }
            }
            text.text = line;
            float hold = tuning.minHoldSeconds + line.Length * tuning.holdSecondsPerCharacter;
            while (hold > 0f)
            {
                if (panel.gameObject.activeInHierarchy) hold -= Time.unscaledDeltaTime;
                yield return null;
            }
            panel.gameObject.SetActive(false);
            _speaking = null;
            _speaker = null;
            Finished?.Invoke();
        }

        private void LateUpdate()
        {
            if (_speaking != null) Follow();
        }

        private void Follow()
        {
            if (_speaker == null || station.ViewCamera == null) return;
            Vector3 view = station.ViewCamera.WorldToViewportPoint(_speaker.position);
            bool inView = view.z > 0f && view.x > -0.1f && view.x < 1.1f;
            if (panel.gameObject.activeSelf != inView) panel.gameObject.SetActive(inView);
            if (!inView) return;
            var area = canvasArea.rect.size;
            float x = Mathf.Clamp(view.x + BesideHead, Margin, Mathf.Max(Margin, 1f - Margin - panel.sizeDelta.x / area.x));
            float y = Mathf.Clamp(view.y, Margin + panel.sizeDelta.y / area.y, TopLimit);
            panel.anchorMin = panel.anchorMax = new Vector2(x, y);
            panel.anchoredPosition = Vector2.zero;
        }
    }
}
