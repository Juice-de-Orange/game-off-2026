using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace GameOff.Counter
{
    /// <summary>
    /// The traveler's speech bubble (P40, P143): a terminal-style HUD panel beside the speaker's head, with their name small on top
    /// and a tail pointing at them, following them on screen and kept inside the view (BubbleLayout). It is not above the head
    /// because the desk view ends just over it. It fades in and out; human speech types on behind a block cursor, alien speech
    /// starts as glyphs and is translated by a wave. While the officer looks elsewhere or stands up, it is hidden and its time
    /// stands still, so no line is lost.
    /// </summary>
    public class SpeechBubble : MonoBehaviour
    {
        private const int AllCharacters = 99999; // TMP's "no limit"

        [SerializeField] private CounterStation station;
        [SerializeField] private DialogueTuning tuning;
        [SerializeField] private RectTransform panel;
        [SerializeField] private RectTransform canvasArea;
        [Tooltip("Stretched over the panel; its insets are the bubble's padding.")]
        [SerializeField] private TMP_Text text;
        [Tooltip("P143: the speaker's name, small at the top (optional).")]
        [SerializeField] private TMP_Text nameLabel;
        [Tooltip("P143: fades the whole bubble (optional; without it the bubble just appears).")]
        [SerializeField] private CanvasGroup fade;
        [Tooltip("P143: block cursor while human speech types on, a child of the text with its pivot bottom left (optional).")]
        [SerializeField] private RectTransform cursor;
        [Tooltip("P143: points from the bottom edge at the speaker. The art points down-left; it is mirrored when the speaker stands right of the bubble (optional).")]
        [SerializeField] private RectTransform tail;
        [Tooltip("P143: keeps the tail off the frame's corners (canvas units).")]
        [SerializeField] private float tailInset = 16f;
        [Tooltip("P143: how far the tail reaches up over the frame's bottom edge, so the two join without a seam (canvas units).")]
        [SerializeField] private float tailOverlap = 4f;
        [SerializeField] private float width = 300f;

        private Transform _speaker;
        private Coroutine _speaking;
        private bool _left;

        /// <summary>The current line has been read: time for the speaker to stop talking.</summary>
        public event Action Finished;

        private void Awake() => panel.gameObject.SetActive(false);

        private void OnDisable() => Hide();

        /// <param name="speakerName">P143: shown small at the top of the bubble; null or empty = no name.</param>
        public void Say(string line, bool alien, Transform speaker, string speakerName = null)
        {
            bool replacing = Showing;
            Stop();
            if (!replacing) panel.gameObject.SetActive(false);
            _speaker = speaker;
            _speaking = StartCoroutine(Speak(line ?? "", alien, speakerName, replacing));
        }

        /// <summary>Removes the bubble at once, without Finished.</summary>
        public void Hide()
        {
            Stop();
            _speaker = null;
            if (panel != null) panel.gameObject.SetActive(false);
        }

        /// <summary>P143: the current line fades out, without Finished (the conversation ends here, e.g. at the decision).</summary>
        public void FadeOut()
        {
            if (!Showing)
            {
                Hide();
                return;
            }
            Stop();
            _speaking = StartCoroutine(FadeAway());
        }

        private bool Showing => _speaking != null && panel != null && panel.gameObject.activeInHierarchy;

        private void Stop()
        {
            if (_speaking != null) StopCoroutine(_speaking);
            _speaking = null;
            if (cursor != null) cursor.gameObject.SetActive(false);
        }

        private IEnumerator Speak(string line, bool alien, string speakerName, bool replacing)
        {
            if (replacing) yield return Fade(0f, snapWhenHidden: true); // the line still up makes way first
            SetAlpha(0f);
            text.text = "";
            text.maxVisibleCharacters = AllCharacters;
            if (nameLabel != null)
            {
                nameLabel.text = speakerName ?? "";
                nameLabel.gameObject.SetActive(!string.IsNullOrWhiteSpace(speakerName));
            }
            _left = false;
            panel.gameObject.SetActive(true);
            Follow();
            while (!panel.gameObject.activeInHierarchy) yield return null; // measuring needs active text

            var box = text.rectTransform; // stretched: its insets are the padding
            float inner = width - box.offsetMin.x + box.offsetMax.x;
            string glyphs = alien ? AlienText.Scramble(line) : null;
            float height = text.GetPreferredValues(line, inner, 0f).y;
            if (alien) height = Mathf.Max(height, text.GetPreferredValues(glyphs, inner, 0f).y); // glyphs can wrap differently
            panel.sizeDelta = new Vector2(width, height + box.offsetMin.y - box.offsetMax.y);
            Follow();

            text.text = line;
            if (!alien) text.maxVisibleCharacters = 0;
            text.ForceMeshUpdate(true);
            int lines = Mathf.Max(1, text.textInfo.lineCount);
            if (alien) text.text = AlienText.Reveal(line, glyphs, 0, tuning.translateEdge);
            else PlaceCursor(0);
            yield return Fade(1f, snapWhenHidden: false);

            if (alien) yield return Translate(line, glyphs, lines * tuning.translateSecondsPerLine);
            else yield return TypeOn();

            float hold = tuning.minHoldSeconds + line.Length * tuning.holdSecondsPerCharacter;
            float blink = Mathf.Max(0.05f, tuning.cursorBlinkSeconds);
            for (float t = 0f; t < hold;)
            {
                if (cursor != null && !alien) cursor.gameObject.SetActive(Mathf.Repeat(t, 2f * blink) < blink);
                if (panel.gameObject.activeInHierarchy) t += Time.unscaledDeltaTime;
                yield return null;
            }
            if (cursor != null) cursor.gameObject.SetActive(false);
            yield return Fade(0f, snapWhenHidden: false);
            panel.gameObject.SetActive(false);
            _speaking = null;
            _speaker = null;
            Finished?.Invoke();
        }

        /// <summary>P40: glyphs turn into the plain line from left to right.</summary>
        private IEnumerator Translate(string line, string glyphs, float duration)
        {
            int shown = 0;
            for (float t = 0f; t < duration;)
            {
                int revealed = Mathf.FloorToInt(line.Length * t / duration);
                if (revealed != shown) text.text = AlienText.Reveal(line, glyphs, shown = revealed, tuning.translateEdge);
                if (panel.gameObject.activeInHierarchy) t += Time.unscaledDeltaTime;
                yield return null;
            }
            text.text = line;
        }

        /// <summary>P143: the line appears character by character behind the cursor; the layout is fixed from the start.</summary>
        private IEnumerator TypeOn()
        {
            int total = text.textInfo.characterCount;
            float rate = tuning.typeCharactersPerSecond;
            float typed = 0f;
            while (rate > 0f && text.maxVisibleCharacters < total)
            {
                if (panel.gameObject.activeInHierarchy) typed += Time.unscaledDeltaTime * rate;
                int shown = Mathf.Min(total, Mathf.FloorToInt(typed));
                if (shown != text.maxVisibleCharacters)
                {
                    text.maxVisibleCharacters = shown;
                    PlaceCursor(shown);
                }
                yield return null;
            }
            text.maxVisibleCharacters = AllCharacters;
            PlaceCursor(total);
        }

        /// <summary>P143: the block cursor right after the last character shown, as high as its text line.</summary>
        private void PlaceCursor(int shown)
        {
            if (cursor == null) return;
            var info = text.textInfo;
            if (info == null || info.characterCount == 0)
            {
                cursor.gameObject.SetActive(false);
                return;
            }
            var c = info.characterInfo[Mathf.Clamp(shown - 1, 0, info.characterCount - 1)];
            var row = info.lineInfo[c.lineNumber];
            float height = row.ascender - row.descender;
            cursor.sizeDelta = new Vector2(height * 0.5f, height);
            cursor.localPosition = new Vector3(shown <= 0 ? c.origin : c.xAdvance, row.descender, 0f);
            cursor.gameObject.SetActive(true);
        }

        private IEnumerator FadeAway()
        {
            yield return Fade(0f, snapWhenHidden: true);
            panel.gameObject.SetActive(false);
            _speaking = null;
            _speaker = null;
        }

        /// <param name="snapWhenHidden">True: out of sight it is gone at once. False: its time stands still like the rest of the line.</param>
        private IEnumerator Fade(float target, bool snapWhenHidden)
        {
            if (fade == null) yield break;
            float seconds = tuning.bubbleFadeSeconds;
            while (fade.alpha != target)
            {
                if (seconds <= 0f || (snapWhenHidden && !panel.gameObject.activeInHierarchy)) fade.alpha = target;
                else if (panel.gameObject.activeInHierarchy) fade.alpha = Mathf.MoveTowards(fade.alpha, target, Time.unscaledDeltaTime / seconds);
                if (fade.alpha != target) yield return null;
            }
        }

        private void SetAlpha(float alpha)
        {
            if (fade != null) fade.alpha = alpha;
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
            var size = new Vector2(panel.sizeDelta.x / area.x, panel.sizeDelta.y / area.y);
            float tailHeight = tail != null ? Mathf.Max(0f, tail.rect.height - tailOverlap) / area.y : 0f;
            _left = BubbleLayout.LeftOfSpeaker(view.x, size.x, _left);
            var anchor = BubbleLayout.TopLeft(new Vector2(view.x, view.y), size, tailHeight, _left);
            if (tail != null) PlaceTail((view.x - anchor.x) * area.x);
            if ((panel.anchorMin - anchor).sqrMagnitude < 1e-6f) return; // a standing speaker doesn't rebuild the canvas every frame
            panel.anchorMin = panel.anchorMax = anchor;
            panel.anchoredPosition = Vector2.zero;
        }

        /// <param name="speakerX">The head, from the bubble's left edge (canvas units).</param>
        private void PlaceTail(float speakerX)
        {
            var position = new Vector2(BubbleLayout.TailCentre(speakerX, panel.sizeDelta.x, tail.rect.width, tailInset, _left), tailOverlap);
            if ((tail.anchoredPosition - position).sqrMagnitude > 0.25f) tail.anchoredPosition = position;
            float mirror = _left ? -1f : 1f;
            if (tail.localScale.x != mirror) tail.localScale = new Vector3(mirror, 1f, 1f);
        }
    }
}
