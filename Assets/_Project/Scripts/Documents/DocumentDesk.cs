using System;
using System.Collections;
using System.Collections.Generic;
using GameOff.Core;
using GameOff.Counter;
using UnityEngine;

namespace GameOff.Documents
{
    /// <summary>
    /// Documents on the counter desk (P06). Deliver() → the drawer slides out to the traveler, comes back with the papers,
    /// and they glide onto the desk. Press + move = drag over the desk; click without moving = hold the paper up for reading
    /// (click / Esc / right click puts it back). Drag a paper onto the drawer to put it in; click the drawer to hand
    /// everything in it back (DocumentsReturned). CounterStation forwards the pointer while the officer is seated.
    /// </summary>
    public class DocumentDesk : MonoBehaviour
    {
        [SerializeField] private CounterStation station;
        [SerializeField] private DocumentTuning tuning;
        [Tooltip("The drawer (SM_Counter_DocSlot); slides along its local +Z toward the traveler. Needs a Collider.")]
        [SerializeField] private Transform drawer;
        [Tooltip("Where papers lie inside the drawer (child of the drawer).")]
        [SerializeField] private Transform drawerTray;
        [Tooltip("Desk spots where delivered papers land, in order (y = desk top). Their rotation is the paper's rotation.")]
        [SerializeField] private Transform[] deskSpots = Array.Empty<Transform>();
        [Tooltip("Center of the area papers can be moved in (y = desk top); size below, in its local X/Z.")]
        [SerializeField] private Transform paperArea;
        [SerializeField] private Vector2 paperAreaSize = new Vector2(1.1f, 3.8f);
        [SerializeField] private DocumentView idCardPrefab;
        [SerializeField] private DocumentView entryPermitPrefab;
        [SerializeField] private DocumentView cargoManifestPrefab;
        [Header("Testing until travelers exist (P07)")]
        [SerializeField] private List<DocumentData> testDocuments = new List<DocumentData>();
        [SerializeField] private bool deliverTestOnSit = true;

        private readonly List<DocumentView> _onDesk = new List<DocumentView>();
        private readonly List<DocumentView> _inDrawer = new List<DocumentView>();
        private Vector3 _drawerRest;
        private bool _busy, _testDelivered;
        private DocumentView _pressed, _inspected;
        private bool _dragging;
        private Vector2 _pressScreen;
        private Vector3 _dragOffset;
        private Pose _inspectReturn;
        private readonly Dictionary<Transform, Coroutine> _glides = new Dictionary<Transform, Coroutine>();
        private int _sharpLayer = -1, _worldLayer;

        /// <summary>Raised when delivered papers lie on the desk.</summary>
        public event Action<IReadOnlyList<DocumentData>> DocumentsDelivered;
        /// <summary>Raised when the drawer has carried papers back to the traveler.</summary>
        public event Action<IReadOnlyList<DocumentData>> DocumentsReturned;

        public bool IsBusy => _busy;
        public IReadOnlyList<DocumentView> OnDesk => _onDesk;
        private float DeskTop => deskSpots.Length > 0 ? deskSpots[0].position.y : transform.position.y;

        private void Awake()
        {
            _drawerRest = drawer.localPosition;
            _sharpLayer = LayerMask.NameToLayer(Layers.Documents);
            _worldLayer = drawer.gameObject.layer;
        }
        private void OnEnable() => station.SeatedChanged += OnSeatedChanged;
        private void OnDisable() => station.SeatedChanged -= OnSeatedChanged;

        private void OnSeatedChanged(bool seated)
        {
            if (!seated)
            {
                // Leaving the counter: whatever is in the hand goes down on the desk.
                if (_inspected != null) PutDownInspected(instant: true);
                if (_pressed != null)
                {
                    Drop(_pressed, onDrawer: false);
                    SetColliders(_pressed, true);
                }
                _pressed = null;
                _dragging = false;
                UpdateLayers();
                return;
            }
            UpdateLayers();
            if (deliverTestOnSit && !_testDelivered && testDocuments.Count > 0)
            {
                _testDelivered = true;
                Deliver(testDocuments);
            }
        }

        /// <summary>Hands papers in through the drawer. Ignored while the drawer is moving.</summary>
        public void Deliver(IReadOnlyList<DocumentData> documents)
        {
            if (_busy || documents == null || documents.Count == 0) return;
            StartCoroutine(DeliverRoutine(documents));
        }

        /// <summary>Esc / right click: puts a paper held up for reading back. True if it did something.</summary>
        public bool Cancel()
        {
            if (_inspected == null) return false;
            PutDownInspected(instant: false);
            return true;
        }

        /// <summary>
        /// Pointer input from the counter (screen position, pressed / released this frame). True if documents used it,
        /// so the counter doesn't treat it as a button click.
        /// </summary>
        public bool HandlePointer(Vector2 screenPos, bool pressed, bool released)
        {
            if (_inspected != null)
            {
                if (pressed) PutDownInspected(instant: false);
                return pressed;
            }
            if (_pressed != null)
            {
                // A press that started before a delivery is still finished normally.
                if (!_dragging && (screenPos - _pressScreen).sqrMagnitude > tuning.dragThresholdPixels * tuning.dragThresholdPixels)
                {
                    _dragging = true;
                    SetColliders(_pressed, false); // the mouse ray must see what is under the paper (desk, drawer)
                }
                if (_dragging) _pressed.transform.position = DragTarget(screenPos) + Vector3.up * tuning.dragLift;
                if (released)
                {
                    if (_dragging)
                    {
                        Drop(_pressed, PointerOnDrawer(screenPos));
                        SetColliders(_pressed, true);
                    }
                    else Inspect(_pressed);
                    _pressed = null;
                    _dragging = false;
                }
                return true;
            }
            if (_busy) return pressed; // no new presses while the drawer moves
            if (!pressed || !station.RaycastPointer(screenPos, out var hit)) return false;
            var doc = hit.collider.GetComponentInParent<DocumentView>();
            if (doc != null && _onDesk.Contains(doc))
            {
                StopGlide(doc.transform); // e.g. still gliding back from reading: the hand wins
                BringToFront(doc);
                if (released)
                {
                    Inspect(doc); // pressed and released in the same frame: a click
                    return true;
                }
                _pressed = doc;
                _pressScreen = screenPos;
                _dragOffset = DeskPoint(screenPos, out var p) ? Flat(doc.transform.position - p) : Vector3.zero;
                return true;
            }
            if (hit.collider.transform.IsChildOf(drawer) && _inDrawer.Count > 0)
            {
                StartCoroutine(ReturnRoutine());
                return true;
            }
            return false;
        }

        private IEnumerator DeliverRoutine(IReadOnlyList<DocumentData> documents)
        {
            _busy = true;
            yield return MoveDrawer(out_: true);
            var arrived = new List<DocumentView>();
            for (int i = 0; i < documents.Count; i++)
            {
                var view = Spawn(documents[i]);
                if (view == null) continue;
                view.transform.SetParent(drawerTray, false);
                view.transform.localPosition = Vector3.up * (i * tuning.stackStep);
                view.transform.localRotation = Quaternion.identity;
                arrived.Add(view);
            }
            yield return MoveDrawer(out_: false);
            for (int i = 0; i < arrived.Count; i++)
            {
                var view = arrived[i];
                view.transform.SetParent(transform, true);
                _onDesk.Add(view);
                UpdateLayers();
                var spot = deskSpots.Length > 0 ? deskSpots[i % deskSpots.Length] : transform;
                StartGlide(view.transform, spot.position + Vector3.up * (_onDesk.Count * tuning.stackStep), spot.rotation, tuning.slideDuration);
                yield return new WaitForSeconds(tuning.slideStagger);
            }
            yield return new WaitForSeconds(tuning.slideDuration);
            _busy = false;
            var data = new List<DocumentData>();
            foreach (var v in arrived) data.Add(v.Data);
            DocumentsDelivered?.Invoke(data);
        }

        private IEnumerator ReturnRoutine()
        {
            _busy = true;
            yield return MoveDrawer(out_: true);
            var data = new List<DocumentData>();
            foreach (var v in _inDrawer)
            {
                data.Add(v.Data);
                StopGlide(v.transform);
                Destroy(v.gameObject);
            }
            _inDrawer.Clear();
            yield return MoveDrawer(out_: false);
            _busy = false;
            DocumentsReturned?.Invoke(data);
        }

        private DocumentView Spawn(DocumentData data)
        {
            var prefab = data == null ? null : data.Type switch
            {
                DocumentType.IdCard => idCardPrefab,
                DocumentType.EntryPermit => entryPermitPrefab,
                _ => cargoManifestPrefab,
            };
            if (prefab == null) return null;
            var view = Instantiate(prefab, transform);
            view.Bind(data);
            if (_sharpLayer >= 0) SetLayer(view, _worldLayer); // in the drawer: drawn like the world until it lies on the desk
            return view;
        }

        /// <summary>Lays a dragged paper down: into the drawer if the mouse is on it, else flat on the desk on top of the pile.</summary>
        private void Drop(DocumentView doc, bool onDrawer)
        {
            if (onDrawer)
            {
                _onDesk.Remove(doc);
                _inDrawer.Add(doc);
                doc.transform.SetParent(drawerTray, true);
                UpdateLayers();
                StartGlide(doc.transform, drawerTray.TransformPoint(Vector3.up * (_inDrawer.Count * tuning.stackStep)),
                    drawerTray.rotation, tuning.slideDuration * 0.5f);
                return;
            }
            var pos = doc.transform.position;
            pos.y = DeskTop + _onDesk.Count * tuning.stackStep;
            doc.transform.position = pos;
        }

        private bool PointerOnDrawer(Vector2 screenPos) =>
            station.RaycastPointer(screenPos, out var hit) && hit.collider.transform.IsChildOf(drawer);

        /// <summary>
        /// Where a dragged paper goes: the surface under the mouse (desk, drawer, other papers; aiming at the 15 cm drawer
        /// must not land behind it), kept inside the paper area. Keeps the grab offset.
        /// </summary>
        private Vector3 DragTarget(Vector2 screenPos)
        {
            Vector3 point;
            if (station.RaycastPointer(screenPos, out var hit) && (hit.collider.transform.IsChildOf(drawer) || hit.collider.GetComponentInParent<DocumentView>() != null))
                point = hit.point;
            else if (!DeskPoint(screenPos, out point))
                return _pressed.transform.position - Vector3.up * tuning.dragLift;
            point += _dragOffset;
            if (paperArea != null)
            {
                var local = paperArea.InverseTransformPoint(point);
                local.x = Mathf.Clamp(local.x, -paperAreaSize.x * 0.5f, paperAreaSize.x * 0.5f);
                local.z = Mathf.Clamp(local.z, -paperAreaSize.y * 0.5f, paperAreaSize.y * 0.5f);
                point = paperArea.TransformPoint(local);
            }
            return point;
        }

        private static void SetColliders(DocumentView doc, bool enabled)
        {
            foreach (var c in doc.GetComponentsInChildren<Collider>()) c.enabled = enabled;
        }

        /// <summary>The touched paper goes on top of the pile: restack all papers by order, 1 mm apart.</summary>
        private void BringToFront(DocumentView doc)
        {
            _onDesk.Remove(doc);
            _onDesk.Add(doc);
            for (int i = 0; i < _onDesk.Count; i++)
            {
                var t = _onDesk[i].transform;
                if (_onDesk[i] == _inspected) continue;
                var pos = t.position;
                pos.y = DeskTop + (i + 1) * tuning.stackStep;
                t.position = pos;
            }
        }

        /// <summary>Holds the paper up in front of the desk camera, upright and filling most of the screen.</summary>
        private void Inspect(DocumentView doc)
        {
            var cam = station.ViewCamera;
            _inspected = doc;
            _inspectReturn = new Pose(doc.transform.position, doc.transform.rotation);
            float height = Mathf.Max(doc.PageSize.y, doc.PageSize.x * 9f / 16f);
            float distance = height / tuning.inspectScreenFill * 0.5f / Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
            var target = cam.transform.position + cam.transform.forward * distance;
            // Page top (local +Z) points to the camera's up, the page face (local +Y) toward the camera.
            var rotation = Quaternion.LookRotation(cam.transform.up, -cam.transform.forward);
            StartGlide(doc.transform, target, rotation, tuning.inspectDuration);
        }

        private void PutDownInspected(bool instant)
        {
            var doc = _inspected;
            _inspected = null;
            StopGlide(doc.transform);
            if (instant) doc.transform.SetPositionAndRotation(_inspectReturn.position, _inspectReturn.rotation);
            else StartGlide(doc.transform, _inspectReturn.position, _inspectReturn.rotation, tuning.inspectDuration);
        }

        private IEnumerator MoveDrawer(bool out_)
        {
            Vector3 from = drawer.localPosition;
            // Along the drawer's own forward (local +Z), expressed in its parent's space like localPosition
            Vector3 to = out_ ? _drawerRest + drawer.localRotation * Vector3.forward * tuning.drawerTravel : _drawerRest;
            for (float t = 0f; t < tuning.drawerDuration; t += Time.deltaTime)
            {
                drawer.localPosition = Vector3.Lerp(from, to, Mathf.SmoothStep(0f, 1f, t / tuning.drawerDuration));
                yield return null;
            }
            drawer.localPosition = to;
        }

        /// <summary>One movement per paper at a time: a new one (or the hand) replaces the running one.</summary>
        private void StartGlide(Transform t, Vector3 position, Quaternion rotation, float duration)
        {
            StopGlide(t);
            _glides[t] = StartCoroutine(GlideRoutine(t, position, rotation, duration));
        }

        private void StopGlide(Transform t)
        {
            if (_glides.TryGetValue(t, out var running) && running != null) StopCoroutine(running);
            _glides.Remove(t);
        }

        private IEnumerator GlideRoutine(Transform t, Vector3 position, Quaternion rotation, float duration)
        {
            Vector3 p0 = t.position;
            Quaternion r0 = t.rotation;
            for (float time = 0f; time < duration; time += Time.deltaTime)
            {
                float k = Mathf.SmoothStep(0f, 1f, time / duration);
                t.SetPositionAndRotation(Vector3.Lerp(p0, position, k), Quaternion.Slerp(r0, rotation, k));
                yield return null;
            }
            t.SetPositionAndRotation(position, rotation);
            _glides.Remove(t);
        }

        /// <summary>
        /// Papers on the desk use the sharp layer (drawn in full resolution over the retro image) only while the officer sits
        /// there; in the drawer or when walking around they are drawn like the world, so they never show through walls.
        /// </summary>
        private void UpdateLayers()
        {
            if (_sharpLayer < 0) return;
            foreach (var v in _onDesk) SetLayer(v, station.IsSeated ? _sharpLayer : _worldLayer);
            foreach (var v in _inDrawer) SetLayer(v, _worldLayer);
        }

        private static void SetLayer(DocumentView doc, int layer)
        {
            foreach (var t in doc.GetComponentsInChildren<Transform>(true)) t.gameObject.layer = layer;
        }

        /// <summary>Point on the desk plane under the mouse.</summary>
        private bool DeskPoint(Vector2 screenPos, out Vector3 point)
        {
            var ray = station.PointerRay(screenPos);
            var plane = new Plane(Vector3.up, new Vector3(0f, DeskTop, 0f));
            if (plane.Raycast(ray, out float d))
            {
                point = ray.GetPoint(d);
                return true;
            }
            point = default;
            return false;
        }

        private static Vector3 Flat(Vector3 v) => new Vector3(v.x, 0f, v.z);
    }
}
