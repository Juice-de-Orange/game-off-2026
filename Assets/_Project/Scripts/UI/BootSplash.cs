using System.Collections;
using GameOff.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace GameOff.UI
{
    /// <summary>Boot scene: fades the splash in, holds, fades out, loads the menu. Any key or click skips.</summary>
    public class BootSplash : MonoBehaviour
    {
        [SerializeField] private CanvasGroup splash;
        [SerializeField] private float fadeIn = 0.8f;
        [SerializeField] private float hold = 1.6f;
        [SerializeField] private float fadeOut = 0.6f;

        private bool _skip;

        private IEnumerator Start()
        {
            splash.alpha = 0f;
            yield return Fade(0f, 1f, fadeIn);
            for (float t = 0f; t < hold && !_skip; t += Time.unscaledDeltaTime) yield return null;
            yield return Fade(splash.alpha, 0f, _skip ? 0.15f : fadeOut);
            SceneManager.LoadScene(SceneNames.Main);
        }

        private void Update()
        {
            if (_skip) return;
            bool key = Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame;
            bool click = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
            if (key || click) _skip = true;
        }

        private IEnumerator Fade(float from, float to, float duration)
        {
            for (float t = 0f; t < duration && !(_skip && to > from); t += Time.unscaledDeltaTime)
            {
                splash.alpha = Mathf.Lerp(from, to, t / duration);
                yield return null;
            }
            splash.alpha = to;
        }
    }
}
