using System;
using System.Collections;
using UnityEngine;

namespace GameOff.Counter
{
    /// <summary>
    /// A stamp on the counter desk (P38). Click → it is picked up and follows the mouse above the desk;
    /// click on a StampTarget → it presses down and leaves an ink mark. P139: a click on the bare desk sets it down there and it
    /// stays (its new rest spot); otherwise (Esc, leaving, a click elsewhere) it goes back to where it was last set down.
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
        private (GameObject Go, int Layer)[] _worldLayers;
        private bool _sharp;

        /// <summary>Layer drawn at full resolution over the retro image (documents and what is held over them).</summary>
        public const string SharpLayer = GameOff.Core.Layers.Documents;

        public CounterAction Action => action;
        public bool IsHeld { get; private set; }
        public bool IsPressing => _press != null;
        /// <summary>P139: how far from the desk's height a click still counts as on the bare desk (m).</summary>
        public float DeskTolerance => tuning != null ? tuning.deskTolerance : 0.02f;

        private void Awake()
        {
            _restPosition = transform.localPosition;
            _restRotation = transform.localRotation;
            _colliders = GetComponentsInChildren<Collider>();
            var all = GetComponentsInChildren<Transform>(true);
            _worldLayers = new (GameObject, int)[all.Length];
            for (int i = 0; i < all.Length; i++) _worldLayers[i] = (all[i].gameObject, all[i].gameObject.layer);
        }

        public void PickUp()
        {
            IsHeld = true;
            SetColliders(false); // the mouse ray must hit what is under the stamp, not the stamp
            ShowSharp(true); // held over the papers, it is drawn with them (RetroLook), else it would vanish under them
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

        /// <summary>P139: sets the held stamp down upright at a point on the desk (its pivot is its bottom face); it stays there.</summary>
        public void SetDown(Vector3 deskPoint)
        {
            if (!IsHeld || IsPressing) return;
            transform.position = deskPoint;
            transform.localRotation = _restRotation;
            _restPosition = transform.localPosition;
            PutBack();
        }

        /// <summary>Back to where it was last set down (at first: where it stood in the prefab), standing on the desk.</summary>
        public void PutBack()
        {
            if (_press != null) StopCoroutine(_press);
            _press = null;
            IsHeld = false;
            transform.SetLocalPositionAndRotation(_restPosition, _restRotation);
            SetColliders(true);
        }

        /// <summary>
        /// P139: drawn on the sharp layer with the papers while the officer sits at the desk (else a paper slid under the stamp
        /// would be drawn over it); like the world when walking around, so it never shows through walls.
        /// </summary>
        public void ShowSharp(bool sharp)
        {
            if (sharp == _sharp || _worldLayers == null) return;
            int layer = LayerMask.NameToLayer(SharpLayer);
            if (sharp && layer < 0) return;
            _sharp = sharp;
            foreach (var (go, worldLayer) in _worldLayers)
                if (go != null) go.layer = sharp ? layer : worldLayer;
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
            ShowSharp(false); // never left on the sharp layer while nobody sits at the desk
        }
    }
}
