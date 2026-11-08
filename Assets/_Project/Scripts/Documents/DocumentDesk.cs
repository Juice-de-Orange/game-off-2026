using System;
using System.Collections;
using System.Collections.Generic;
using GameOff.Audio;
using GameOff.Core;
using GameOff.Counter;
using UnityEngine;

namespace GameOff.Documents
{
    /// <summary>
    /// Documents on the counter desk (P06). Deliver() → the drawer slides out to the traveler, comes back with the papers,
    /// and they glide onto the desk. Press + move = drag over the desk; click without moving = hold the paper up for reading.
    /// Clicking more papers or notes holds them up beside it (P89); [R] holds up all papers. Clicking a held paper puts it back,
    /// a click elsewhere / Esc / right click puts all back. Drag a paper onto the drawer to put it in; click the drawer to hand
    /// everything in it back (DocumentsReturned). CounterStation forwards the pointer while the officer is seated.
    /// Pinned notes (P41) hang elsewhere and can only be read: they are never moved and don't count as papers on the desk.
    /// </summary>
    public class DocumentDesk : MonoBehaviour
    {
        [SerializeField] private CounterStation station;
        [SerializeField] private DocumentTuning tuning;
        [Tooltip("The drawer (SM_Counter_DocSlot); slides along its local +X toward the traveler (its front, local -X, faces the officer). Needs a Collider.")]
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
        [SerializeField] private DocumentView citationPrefab;
        [SerializeField] private DocumentView noticePrefab;
        [Tooltip("Where printed slips come out (the printer's paper slot) and where they land on the desk.")]
        [SerializeField] private Transform printerOut;
        [SerializeField] private Transform printerSpot;

        private readonly List<DocumentView> _onDesk = new List<DocumentView>();
        private readonly List<DocumentView> _inDrawer = new List<DocumentView>();
        private readonly List<DocumentView> _pinned = new List<DocumentView>();
        private Vector3 _drawerRest;
        private bool _busy;
        private DocumentView _pressed;
        private readonly List<DocumentView> _reading = new List<DocumentView>();
        private readonly Dictionary<DocumentView, Pose> _readingReturn = new Dictionary<DocumentView, Pose>();
        private Quaternion _readingCameraRotation;
        private const float DrawerHearingMeters = 12f;

        private bool _dragging, _drawerPress;
        private Vector2 _pressScreen;
        private Vector3 _dragOffset;
        private readonly Dictionary<Transform, (Coroutine routine, Vector3 position, Quaternion rotation)> _glides =
            new Dictionary<Transform, (Coroutine, Vector3, Quaternion)>();
        private readonly HashSet<Transform> _gliding = new HashSet<Transform>();
        private int _sharpLayer = -1, _worldLayer;

        /// <summary>Raised when delivered papers lie on the desk.</summary>
        public event Action<IReadOnlyList<DocumentData>> DocumentsDelivered;
        /// <summary>Raised when the drawer has carried papers back to the traveler.</summary>
        public event Action<IReadOnlyList<DocumentData>> DocumentsReturned;
        /// <summary>Raised when the drawer refuses to go (ReturnCheck said no), with the reason.</summary>
        public event Action<string> ReturnRefused;
        /// <summary>The officer clicked a printed field on a traveler's paper held up for reading (P40): document, field key.</summary>
        public event Action<DocumentData, string> FieldClicked;
        /// <summary>A paper or pinned note was held up for reading (P86).</summary>
        public event Action<DocumentView> PaperRead;

        /// <summary>
        /// Asked before the drawer hands papers back: (papers in the drawer, papers still on the desk) → null to allow, or a
        /// short message why not (e.g. "Stamp the papers first"). Set by the shift logic (P07).
        /// </summary>
        public Func<IReadOnlyList<DocumentView>, IReadOnlyList<DocumentView>, string> ReturnCheck;

        public bool IsBusy => _busy;
        /// <summary>A paper is being moved or the drawer is pressed. (Reading doesn't count: the officer may look around meanwhile.)</summary>
        public bool IsMovingPaper => _pressed != null || _drawerPress;
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

        private void Update()
        {
            // Held papers follow the officer's look (SeatedLook), so the pinboard can be read beside them.
            if (_reading.Count > 0 && Quaternion.Angle(station.ViewCamera.transform.rotation, _readingCameraRotation) > 0.01f) LayOutReading(instant: true);
        }

        private void OnSeatedChanged(bool seated)
        {
            if (!seated)
            {
                // Leaving the counter: whatever is in the hand goes down on the desk.
                PutDownAll(instant: true);
                if (_pressed != null)
                {
                    Drop(_pressed, onDrawer: false);
                    SetColliders(_pressed, true);
                }
                _pressed = null;
                _dragging = false;
                _drawerPress = false;
                UpdateLayers();
                return;
            }
            UpdateLayers();
        }

        /// <summary>
        /// A new day (B05): the desk is cleared and every movement stops, whatever was going on. Pinned notes stay; the pinboard
        /// changes them itself. Citation numbers start again each day, so old slips must not stay.
        /// </summary>
        public void ClearForNewDay()
        {
            StopAllCoroutines();
            _glides.Clear();
            _gliding.Clear();
            _reading.Clear();
            _readingReturn.Clear();
            _pressed = null;
            _dragging = false;
            _drawerPress = false;
            _busy = false;
            drawer.localPosition = _drawerRest;
            foreach (var view in _onDesk) DestroyPaper(view);
            foreach (var view in _inDrawer) DestroyPaper(view);
            foreach (var view in drawerTray.GetComponentsInChildren<DocumentView>(true)) // caught in a delivery
                if (!_onDesk.Contains(view) && !_inDrawer.Contains(view)) DestroyPaper(view);
            _onDesk.Clear();
            _inDrawer.Clear();
            foreach (var note in _pinned)
            {
                note.transform.localPosition = Vector3.zero;
                note.transform.localRotation = Quaternion.identity;
                note.HighlightField(null);
            }
        }

        private static void DestroyPaper(DocumentView view)
        {
            if (view.Data != null && view.Data.IsRuntime) Destroy(view.Data);
            Destroy(view.gameObject);
        }

        /// <summary>Hangs a note at a spot (page top = its local +Z, page face = its local +Y). It can be clicked to read it.</summary>
        public DocumentView Pin(DocumentData data, Transform spot)
        {
            var view = Spawn(data);
            if (view == null) return null;
            view.transform.SetParent(spot, false);
            view.transform.localPosition = Vector3.zero;
            view.transform.localRotation = Quaternion.identity;
            _pinned.Add(view);
            UpdateLayers();
            return view;
        }

        /// <summary>Takes a pinned note down for good, with its data if that was made at runtime.</summary>
        public void Unpin(DocumentView view)
        {
            if (view == null || !_pinned.Remove(view)) return;
            bool wasRead = Forget(view);
            StopGlide(view.transform);
            if (view.Data != null && view.Data.IsRuntime) Destroy(view.Data);
            Destroy(view.gameObject);
            if (wasRead && _reading.Count > 0) LayOutReading();
        }

        /// <summary>Hands papers in through the drawer; if the drawer is busy, they come as soon as it is free.</summary>
        public void Deliver(IReadOnlyList<DocumentData> documents)
        {
            if (documents == null || documents.Count == 0) return;
            StartCoroutine(DeliverRoutine(documents));
        }

        /// <summary>Esc / right click: puts the papers held up for reading back. True if it did something.</summary>
        public bool Cancel()
        {
            if (_reading.Count == 0) return false;
            PutDownAll(instant: false);
            return true;
        }

        /// <summary>[R] (P89): holds up all traveler papers on the desk side by side, or puts everything held back.</summary>
        public void ToggleReadAll()
        {
            if (IsMovingPaper) return;
            if (_reading.Count > 0)
            {
                PutDownAll(instant: false);
                return;
            }
            if (_busy) return; // no new paper in hand while the drawer moves
            var papers = new List<DocumentView>();
            foreach (var view in _onDesk)
                if (view.Type != DocumentType.Citation) papers.Add(view);
            if (papers.Count == 0) return;
            papers.Sort((a, b) => a.Type.CompareTo(b.Type)); // always ID, permit, manifest from left to right
            AudioManager.Play(s => s.paperPickup);
            foreach (var view in papers)
            {
                if (_reading.Count >= tuning.maxPapersRead) break;
                FinishGlide(view.transform);
                Read(view, quiet: true);
            }
        }

        /// <summary>
        /// Pointer input from the counter (screen position, pressed / released this frame). True if documents used it,
        /// so the counter doesn't treat it as a button click.
        /// </summary>
        public bool HandlePointer(Vector2 screenPos, bool pressed, bool released)
        {
            if (_reading.Count > 0) return HandlePointerWhileReading(screenPos, pressed);
            if (_drawerPress)
            {
                bool moved = (screenPos - _pressScreen).sqrMagnitude > tuning.dragThresholdPixels * tuning.dragThresholdPixels;
                if (moved && !_busy && _inDrawer.Count > 0)
                {
                    // Pull the top paper out of the drawer and keep dragging it
                    _drawerPress = false;
                    var top = _inDrawer[_inDrawer.Count - 1];
                    _inDrawer.RemoveAt(_inDrawer.Count - 1);
                    _onDesk.Add(top);
                    top.transform.SetParent(transform, true);
                    StopGlide(top.transform);
                    UpdateLayers();
                    _pressed = top;
                    _dragging = true;
                    AudioManager.Play(s => s.paperPickup);
                    _dragOffset = Vector3.zero;
                    SetColliders(top, false);
                    BringToFront(top);
                    if (released) FinishDrag(screenPos); // moved and let go within one frame
                }
                else if (released) ReleaseDrawerPress();
                return true;
            }
            if (_pressed != null)
            {
                // A press that started before a delivery is still finished normally.
                if (!_dragging && (screenPos - _pressScreen).sqrMagnitude > tuning.dragThresholdPixels * tuning.dragThresholdPixels)
                {
                    _dragging = true;
                    AudioManager.Play(s => s.paperPickup);
                    SetColliders(_pressed, false); // the mouse ray must see what is under the paper (desk, drawer)
                }
                if (_dragging) _pressed.transform.position = DragTarget(screenPos) + Vector3.up * tuning.dragLift;
                if (released)
                {
                    if (_dragging) FinishDrag(screenPos);
                    else
                    {
                        Read(_pressed);
                        _pressed = null;
                    }
                }
                return true;
            }
            if (!pressed)
                return false;
            bool hitSomething = station.RaycastPointer(screenPos, out var hit);
            var doc = hitSomething ? hit.collider.GetComponentInParent<DocumentView>() : null;
            if (doc != null && _pinned.Contains(doc)) // also while the drawer moves: the rules are needed most when papers come in
            {
                ReadNote(doc);
                return true;
            }
            if (_busy) return true; // no other new presses while the drawer moves
            if (!hitSomething) return false;
            if (doc != null && _onDesk.Contains(doc))
            {
                FinishGlide(doc.transform); // e.g. still gliding back from reading: the hand wins, but from its proper resting pose
                BringToFront(doc);
                if (released)
                {
                    Read(doc); // pressed and released in the same frame: a click
                    return true;
                }
                _pressed = doc;
                _pressScreen = screenPos;
                _dragOffset = DeskPoint(screenPos, out var p) ? Flat(doc.transform.position - p) : Vector3.zero;
                return true;
            }
            if (hit.collider.transform.IsChildOf(drawer) && _inDrawer.Count > 0)
            {
                // Click = send the drawer; drag = take the top paper back out (decided in the _drawerPress branch)
                _drawerPress = true;
                _pressScreen = screenPos;
                if (released) ReleaseDrawerPress();
                return true;
            }
            return false;
        }

        private bool HandlePointerWhileReading(Vector2 screenPos, bool pressed)
        {
            var ray = station.PointerRay(screenPos);
            DocumentView asked = null;
            string field = null;
            foreach (var view in _reading)
            {
                field = view.AcceptsQuestions ? view.FieldAt(ray) : null;
                if (field == null) continue;
                asked = view;
                break;
            }
            foreach (var view in _reading) view.HighlightField(view == asked ? field : null);
            if (!pressed) return false;
            if (field != null)
            {
                FieldClicked?.Invoke(asked.Data, field);
                return true;
            }
            var under = station.RaycastPointer(screenPos, out var hit) ? hit.collider.GetComponentInParent<DocumentView>() : null;
            if (under != null && _reading.Contains(under))
            {
                PutDown(under, instant: false);
                if (_reading.Count > 0) LayOutReading();
            }
            else if (under != null && _pinned.Contains(under)) ReadNote(under);
            else if (under != null && _onDesk.Contains(under))
            {
                if (_busy) return true; // no new paper in hand while the drawer moves
                FinishGlide(under.transform);
                BringToFront(under);
                Read(under);
            }
            else PutDownAll(instant: false);
            return true;
        }

        private IEnumerator DeliverRoutine(IReadOnlyList<DocumentData> documents)
        {
            while (_busy) yield return null; // e.g. a slip is just being handed back
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
            // Slips made at runtime (citations) are gone now; free their data too
            foreach (var d in data)
                if (d != null && d.IsRuntime) Destroy(d);
        }

        /// <summary>Takes away every traveler paper on the desk and in the drawer (arrest / airlock); printed slips stay.</summary>
        public void Confiscate()
        {
            foreach (var view in new List<DocumentView>(_reading))
                if (view.Type != DocumentType.Citation && !_pinned.Contains(view)) Forget(view);
            if (_reading.Count > 0) LayOutReading();
            if (_pressed != null && _pressed.Type != DocumentType.Citation)
            {
                _pressed = null;
                _dragging = false;
            }
            foreach (var list in new[] { _onDesk, _inDrawer })
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    if (list[i].Type == DocumentType.Citation) continue;
                    StopGlide(list[i].transform);
                    Destroy(list[i].gameObject);
                    list.RemoveAt(i);
                }
        }

        /// <summary>A slip comes out of the counter printer and lands next to it (citations, P07).</summary>
        public void Print(DocumentData data)
        {
            if (printerOut == null)
            {
                Debug.LogError("[Documents] No printer wired on the counter: run GameOff/Build Documents.", this);
                return;
            }
            AudioManager.Play(s => s.printerCitation);
            var view = Spawn(data);
            if (view == null) return;
            view.transform.SetPositionAndRotation(printerOut.position, printerOut.rotation);
            _onDesk.Add(view);
            UpdateLayers();
            var target = printerSpot != null ? printerSpot : printerOut;
            StartGlide(view.transform, target.position + Vector3.up * (_onDesk.Count * tuning.stackStep), target.rotation, tuning.slideDuration * 1.5f);
        }

        /// <summary>A click on the drawer (no drag): hand the papers in it back, unless ReturnCheck refuses.</summary>
        private void ReleaseDrawerPress()
        {
            _drawerPress = false;
            if (_busy || _inDrawer.Count == 0) return;
            string refusal = ReturnCheck?.Invoke(_inDrawer, _onDesk);
            if (refusal != null)
            {
                ReturnRefused?.Invoke(refusal);
                StartCoroutine(RattleDrawer());
            }
            else StartCoroutine(ReturnRoutine());
        }

        private IEnumerator RattleDrawer()
        {
            // A short shake: the drawer is locked
            _busy = true;
            var rest = drawer.localPosition;
            var side = drawer.localRotation * Vector3.right * 0.006f; // along its travel
            for (int i = 0; i < 6; i++)
            {
                drawer.localPosition = rest + (i % 2 == 0 ? side : -side);
                yield return new WaitForSeconds(0.03f);
            }
            drawer.localPosition = rest;
            _busy = false;
        }

        private DocumentView Spawn(DocumentData data)
        {
            var prefab = data == null ? null : data.Type switch
            {
                DocumentType.IdCard => idCardPrefab,
                DocumentType.EntryPermit => entryPermitPrefab,
                DocumentType.CargoManifest => cargoManifestPrefab,
                DocumentType.Notice => noticePrefab,
                _ => citationPrefab,
            };
            if (prefab == null) return null;
            var view = Instantiate(prefab, transform);
            view.Bind(data);
            if (_sharpLayer >= 0) SetLayer(view, _worldLayer); // in the drawer: drawn like the world until it lies on the desk
            return view;
        }

        private void FinishDrag(Vector2 screenPos)
        {
            AudioManager.Play(s => s.paperSlide);
            Drop(_pressed, !_busy && PointerOnDrawer(screenPos)); // a moving drawer takes nothing: its tray has no fixed place yet
            SetColliders(_pressed, true);
            _pressed = null;
            _dragging = false;
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
            // Never under the closed drawer, where the mouse couldn't reach it again and the shift could not go on.
            // The drawer may be moving right now, so its place at rest counts, not where it is at the moment.
            if (drawer.TryGetComponent<Collider>(out var drawerCollider) && deskSpots.Length > 0)
            {
                Physics.SyncTransforms();
                var box = drawerCollider.bounds;
                Vector3 rest = drawer.parent != null ? drawer.parent.TransformPoint(_drawerRest) : _drawerRest;
                box.center += rest - drawer.position;
                if (box.Contains(new Vector3(pos.x, box.center.y, pos.z))) pos = deskSpots[0].position;
            }
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
            // Papers land on what is under the mouse, but pinned notes hang on the wall: not a surface for them
            if (station.RaycastPointer(screenPos, out var hit) && !_pinned.Contains(hit.collider.GetComponentInParent<DocumentView>())
                && (hit.collider.transform.IsChildOf(drawer) || hit.collider.GetComponentInParent<DocumentView>() != null))
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
                if (_reading.Contains(_onDesk[i])) continue;
                FinishGlide(t); // a running glide would land it on its old height, level with another paper
                var pos = t.position;
                pos.y = DeskTop + (i + 1) * tuning.stackStep;
                t.position = pos;
            }
        }

        /// <summary>A pinned note joins the papers held up; it goes back to its pin, even if it was still gliding there.</summary>
        private void ReadNote(DocumentView note)
        {
            if (_reading.Contains(note)) return;
            StopGlide(note.transform);
            Read(note, new Pose(note.transform.parent.position, note.transform.parent.rotation));
        }

        /// <summary>Holds the paper up in front of the desk camera, beside the ones already held (the oldest goes back when all places are taken).</summary>
        private void Read(DocumentView doc, Pose? returnPose = null, bool quiet = false)
        {
            if (_reading.Contains(doc)) return;
            if (_reading.Count >= tuning.maxPapersRead) PutDown(_reading[0], instant: false);
            if (!quiet) AudioManager.Play(s => s.paperPickup);
            _readingReturn[doc] = returnPose ?? new Pose(doc.transform.position, doc.transform.rotation);
            _reading.Add(doc);
            LayOutReading();
            PaperRead?.Invoke(doc);
        }

        /// <summary>
        /// Upright in a row across the screen from the left, each paper as large as its place and the height allow. There are always
        /// at least minReadingSlots places, so a single paper leaves the desk visible and the next one can be clicked.
        /// Every paper has its own distance from the camera, so papers of different sizes still fill their places.
        /// </summary>
        private void LayOutReading(bool instant = false)
        {
            var cam = station.ViewCamera;
            _readingCameraRotation = cam.transform.rotation;
            float halfTan = Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
            // Page top (local +Z) points to the camera's up, the page face (local +Y) toward the camera.
            var rotation = Quaternion.LookRotation(cam.transform.up, -cam.transform.forward);
            int count = _reading.Count;
            int places = Mathf.Max(count, tuning.minReadingSlots);
            for (int i = 0; i < count; i++)
            {
                var size = _reading[i].PageSize;
                float visibleHeight = Mathf.Max(size.y / tuning.inspectScreenFill, size.x / (tuning.readingWidthFill / places * cam.aspect));
                float distance = visibleHeight * 0.5f / halfTan;
                float across = ((i + 0.5f) / places - 0.5f) * tuning.readingWidthFill * visibleHeight * cam.aspect;
                var target = cam.transform.position + cam.transform.forward * distance + cam.transform.right * across;
                var paper = _reading[i].transform;
                if (instant)
                {
                    // Following the look: every frame anew, so a glide would never get past its first step (and no garbage per frame).
                    StopGlide(paper);
                    paper.SetPositionAndRotation(target, rotation);
                }
                else StartGlide(paper, target, rotation, tuning.inspectDuration);
            }
        }

        private void PutDownAll(bool instant)
        {
            if (_reading.Count == 0) return;
            if (!instant) AudioManager.Play(s => s.paperSlide);
            foreach (var view in new List<DocumentView>(_reading)) PutDown(view, instant, quiet: true);
        }

        private void PutDown(DocumentView doc, bool instant, bool quiet = false)
        {
            var pose = _readingReturn[doc];
            // Papers were restacked while this one was in the air: it returns to its own height, not one that is now taken.
            int stackIndex = _onDesk.IndexOf(doc);
            if (stackIndex >= 0) pose.position.y = DeskTop + (stackIndex + 1) * tuning.stackStep;
            Forget(doc);
            doc.HighlightField(null);
            StopGlide(doc.transform);
            if (instant)
            {
                doc.transform.SetPositionAndRotation(pose.position, pose.rotation);
                return;
            }
            if (!quiet) AudioManager.Play(s => s.paperSlide);
            StartGlide(doc.transform, pose.position, pose.rotation, tuning.inspectDuration);
        }

        /// <summary>Takes a paper out of the reading row without moving it. True if it was being read.</summary>
        private bool Forget(DocumentView doc)
        {
            _readingReturn.Remove(doc);
            return _reading.Remove(doc);
        }

        private IEnumerator MoveDrawer(bool out_)
        {
            // At the counter, not in the player's ear: the next delivery can come while they are in the bar.
            if (out_) AudioManager.PlayAt(s => s.drawerOpen, drawer.position, DrawerHearingMeters);
            else AudioManager.PlayAt(s => s.drawerClose, drawer.position, DrawerHearingMeters);
            Vector3 from = drawer.localPosition;
            // Along the drawer's own local +X (its front, local -X, faces the officer), expressed in its parent's space like localPosition
            Vector3 to = out_ ? _drawerRest + drawer.localRotation * Vector3.right * tuning.drawerTravel : _drawerRest;
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
            var routine = StartCoroutine(GlideRoutine(t, position, rotation, duration));
            if (t != null && _gliding.Contains(t)) _glides[t] = (routine, position, rotation);
        }

        private void StopGlide(Transform t)
        {
            if (_glides.TryGetValue(t, out var running) && running.routine != null) StopCoroutine(running.routine);
            _glides.Remove(t);
            _gliding.Remove(t);
        }

        /// <summary>Ends a running movement at its target, so the next one doesn't start from a tilted pose in mid-air.</summary>
        private void FinishGlide(Transform t)
        {
            if (!_glides.TryGetValue(t, out var running)) return;
            StopGlide(t);
            t.SetPositionAndRotation(running.position, running.rotation);
        }

        private IEnumerator GlideRoutine(Transform t, Vector3 position, Quaternion rotation, float duration)
        {
            _gliding.Add(t);
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
            _gliding.Remove(t);
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
            foreach (var v in _pinned) SetLayer(v, station.IsSeated ? _sharpLayer : _worldLayer);
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
