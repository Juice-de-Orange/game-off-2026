using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace GameOff.Counter
{
    /// <summary>
    /// A clickable control on the counter desk (button or stamp). Needs a Collider.
    /// With a cover assigned, the first click flips the cover open and only the second click triggers.
    /// </summary>
    public class CounterClickable : MonoBehaviour
    {
        [SerializeField] private CounterAction action;
        [Tooltip("Moves down on press and back up (the button cap or stamp body).")]
        [SerializeField] private Transform pressPart;
        [SerializeField] private float pressDepth = 0.02f;
        [SerializeField] private float pressDuration = 0.12f;
        [Tooltip("Optional protective cover, hinged at its pivot. Must be opened by a first click.")]
        [SerializeField] private Transform cover;
        [SerializeField] private float coverOpenAngle = 110f;
        [SerializeField] private UnityEvent onPressed = new UnityEvent();

        private bool _coverOpen;
        private bool _animating;
        private Quaternion _coverClosedRotation;
        private Vector3 _restPosition;

        public CounterAction Action => action;
        public UnityEvent OnPressed => onPressed;

        private void Awake()
        {
            if (cover != null) _coverClosedRotation = cover.localRotation;
            if (pressPart != null) _restPosition = pressPart.localPosition;
        }

        /// <summary>Called by CounterStation on click. Returns true if the action fired.</summary>
        public bool Click()
        {
            if (_animating) return false;
            if (cover != null && !_coverOpen)
            {
                _coverOpen = true;
                cover.localRotation = _coverClosedRotation * Quaternion.Euler(coverOpenAngle, 0f, 0f);
                return false;
            }
            if (pressPart != null) StartCoroutine(PressAnimation());
            onPressed.Invoke();
            return true;
        }

        /// <summary>Close the cover again (e.g. when the officer leaves the counter).</summary>
        public void ResetCover()
        {
            if (cover == null) return;
            _coverOpen = false;
            cover.localRotation = _coverClosedRotation;
        }

        private IEnumerator PressAnimation()
        {
            _animating = true;
            Vector3 up = _restPosition;
            Vector3 down = up + Vector3.down * pressDepth;
            float half = pressDuration * 0.5f;
            for (float t = 0f; t < half; t += Time.deltaTime)
            {
                pressPart.localPosition = Vector3.Lerp(up, down, t / half);
                yield return null;
            }
            for (float t = 0f; t < half; t += Time.deltaTime)
            {
                pressPart.localPosition = Vector3.Lerp(down, up, t / half);
                yield return null;
            }
            pressPart.localPosition = up;
            _animating = false;
        }

        private void OnDisable()
        {
            // An interrupted press must not leave the part stuck down or the control locked.
            StopAllCoroutines();
            if (pressPart != null) pressPart.localPosition = _restPosition;
            _animating = false;
        }
    }
}
