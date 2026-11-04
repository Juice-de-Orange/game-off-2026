using System;
using System.Collections;
using UnityEngine;

namespace GameOff.Counter
{
    /// <summary>
    /// A stamp on the counter desk (P38). Click → it is picked up and follows the mouse above the desk;
    /// click on a StampTarget → it presses down and leaves an ink mark; otherwise it goes back to its rest spot.
    /// CounterStation drives it; the stamp itself only moves and animates. Needs a Collider (disabled while held).
    /// </summary>
    public class StampTool : MonoBehaviour
    {
        [SerializeField] private CounterAction action;
        [Tooltip("Ink mark material (APPROVED / DENIED).")]
        [SerializeField] private Material ink;
        [SerializeField] private StampTuning tuning;

        private Vector3 _restPosition;
        private Quaternion _restRotation;
        private Collider[] _colliders;
        private Coroutine _press;
        private (GameObject Go, int Layer)[] _restLayers;

        /// <summary>Layer drawn at full resolution over the retro image (documents and what is held over them).</summary>
        public const string SharpLayer = GameOff.Core.Layers.Documents;

        public CounterAction Action => action;
        public bool IsHeld { get; private set; }
        public bool IsPressing => _press != null;

        private void Awake()
        {
            _restPosition = transform.localPosition;
            _restRotation = transform.localRotation;
            _colliders = GetComponentsInChildren<Collider>();
        }

        public void PickUp()
        {
            IsHeld = true;
            SetColliders(false); // the mouse ray must hit what is under the stamp, not the stamp
            // Held over the papers, it is drawn with them on the sharp layer (RetroLook), else it would vanish under them.
            int sharp = LayerMask.NameToLayer(SharpLayer);
            if (sharp >= 0)
            {
                var all = GetComponentsInChildren<Transform>(true);
                _restLayers = new (GameObject, int)[all.Length];
                for (int i = 0; i < all.Length; i++) _restLayers[i] = (all[i].gameObject, all[i].gameObject.layer);
                foreach (var t in all) t.gameObject.layer = sharp;
            }
        }

        /// <summary>Follows the surface point under the mouse, floating off the surface (along its normal).</summary>
        public void Hover(Vector3 surfacePoint, Vector3 normal)
        {
            if (!IsHeld || IsPressing) return;
            transform.position = surfacePoint + normal * tuning.hoverHeight;
        }

        /// <summary>
        /// Presses down onto the target at the given point and leaves the mark at the lowest point; onStamped runs right then.
        /// </summary>
        public void StampOn(StampTarget target, Vector3 point, Vector3 normal, Vector3 textUp, Action<CounterAction> onStamped)
        {
            if (!IsHeld || IsPressing) return;
            _press = StartCoroutine(Press(target, point, normal, textUp, onStamped));
        }

        public void PutBack()
        {
            if (_press != null) StopCoroutine(_press);
            _press = null;
            IsHeld = false;
            transform.SetLocalPositionAndRotation(_restPosition, _restRotation);
            SetColliders(true);
            if (_restLayers != null)
                foreach (var (go, layer) in _restLayers) go.layer = layer;
            _restLayers = null;
        }

        private IEnumerator Press(StampTarget target, Vector3 point, Vector3 normal, Vector3 textUp, Action<CounterAction> onStamped)
        {
            Vector3 up = point + normal * tuning.hoverHeight; // press along the surface normal
            float half = tuning.pressDuration * 0.5f;
            for (float t = 0f; t < half; t += Time.deltaTime)
            {
                transform.position = Vector3.Lerp(up, point, t / half);
                yield return null;
            }
            transform.position = point; // the stamp's pivot is its bottom face
            target.Stamp(action, ink, point, normal, textUp, tuning.markSize, tuning.markTilt);
            onStamped?.Invoke(action);
            for (float t = 0f; t < half; t += Time.deltaTime)
            {
                transform.position = Vector3.Lerp(point, up, t / half);
                yield return null;
            }
            transform.position = up;
            _press = null;
        }

        private void SetColliders(bool enabled)
        {
            foreach (var c in _colliders) c.enabled = enabled;
        }

        private void OnDisable()
        {
            if (IsHeld) PutBack();
        }
    }
}
