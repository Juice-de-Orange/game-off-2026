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
    /// P139: loose things (the bribe note and its envelope) lie on the desk and are dragged like papers, but are no traveler
    /// papers: the drawer, [R], the return check and confiscation leave them alone. Everything on the desk lies in one pile:
    /// what was touched last is on top, and each thing lies on what it overlaps (DeskLayout), so nothing hides under another.
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

        [Tooltip("P16/P62: things on the desk that catch the mouse; a paper dropped onto one lands on the first spot.")]
        [SerializeField] private Collider[] keepClear = new Collider[0];

        private readonly List<DocumentView> _onDesk = new List<DocumentView>();
        private readonly List<DocumentView> _inDrawer = new List<DocumentView>();
        private readonly List<DocumentView> _pinned = new List<DocumentView>();
        private Vector3 _drawerRest;
        private bool _busy;
        /// <summary>The paper or loose thing under the pressed mouse button (dragged once it moved).</summary>
        private Transform _pressed;
        private readonly List<DocumentView> _reading = new List<DocumentView>();
        private readonly Dictionary<DocumentView, Pose> _readingReturn = new Dictionary<DocumentView, Pose>();
        private Quaternion _readingCameraRotation;
        private const float DrawerHearingMeters = 12f;

        // P139: loose things on the desk, their measured boxes (in their own space) and the pile of everything on the desk.
        private readonly List<Transform> _loose = new List<Transform>();
        private readonly Dictionary<Transform, Bounds> _shapes = new Dictionary<Transform, Bounds>();
        private readonly List<Transform> _pile = new List<Transform>(); // bottom first
        private readonly List<Transform> _stacked = new List<Transform>();
        private readonly List<DeskLayout.Footprint> _footprints = new List<DeskLayout.Footprint>();
        private readonly List<float> _thickness = new List<float>();
        private float[] _bottoms = new float[16];
        private bool _fieldUnderPointer;

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
        /// <summary>P140: papers or notes are held up for reading.</summary>
        public bool IsReading => _reading.Count > 0;
        /// <summary>P138: while reading, the mouse is on a field that can be asked about.</summary>
        public bool PointerOnField => _reading.Count > 0 && _fieldUnderPointer;
        /// <summary>The traveler's papers and printed slips on the desk (not the loose things).</summary>
        public IReadOnlyList<DocumentView> OnDesk => _onDesk;
        /// <summary>P62: the papers in the drawer.</summary>
        public IReadOnlyList<DocumentView> InDrawer => _inDrawer;
        /// <summary>P139: loose things on the desk (bribe note, envelope): dragged like papers, never handed back or taken.</summary>
        public IReadOnlyList<Transform> Loose => _loose;
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
                LetGo();
                _drawerPress = false;
                UpdateLayers();
                return;
            }
            UpdateLayers();
        }

        /// <summary>
        /// The desk stops getting the pointer (a window opened over it): a pressed or dragged paper is laid down where it is and a
        /// drawer press is dropped, so nothing sticks to the mouse once the desk gets it back.
        /// </summary>
        public void LetGoOfPaper()
        {
            LetGo();
            _drawerPress = false;
        }

        /// <summary>A pressed or dragged thing is laid down where it is (on the desk, never in the drawer).</summary>
        private void LetGo()
        {
            var thing = _pressed;
            _pressed = null;
            _dragging = false;
            if (thing == null) return;
            Drop(thing, onDrawer: false);
            SetColliders(thing, true);
        }

        /// <summary>
        /// A new day (B05): the desk is cleared and every movement stops, whatever was going on. Pinned notes stay; the pinboard
        /// changes them itself. Loose things stay too (their owner takes them away). Citation numbers start again each day, so
        /// old slips must not stay.
        /// </summary>
        public void ClearForNewDay()
        {
            PutDownAll(instant: true); // P139: a loose note held up goes back onto the desk, not left in the air
            if (_pressed != null && _loose.Contains(_pressed)) LetGo();
            foreach (var thing in _loose) FinishGlide(thing);
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

        /// <summary>Takes a pinned note, or a loose one (P139), away for good, with its data if that was made at runtime.</summary>
        public void Unpin(DocumentView view)
        {
            if (view == null) return;
            bool wasRead;
            if (_loose.Contains(view.transform))
            {
                RemoveLoose(view.transform);
                wasRead = false; // RemoveLoose already laid out the rest
            }
            else if (_pinned.Remove(view)) wasRead = Forget(view);
            else return;
            StopGlide(view.transform);
            if (view.Data != null && view.Data.IsRuntime) Destroy(view.Data);
            Destroy(view.gameObject);
            if (wasRead && _reading.Count > 0) LayOutReading();
        }

        /// <summary>
        /// P139: lays a note loose on the desk at a spot (page top = the spot's +Z), on top of what is there. It is dragged and read
        /// like a paper, but it is no traveler paper. Unpin takes it away again.
        /// </summary>
        public DocumentView Lay(DocumentData data, Transform spot)
        {
            var view = Spawn(data);
            if (view == null) return null;
            view.transform.SetPositionAndRotation(spot.position, spot.rotation);
            AddLoose(view.transform);
            return view;
        }

        /// <summary>
        /// P139: a thing (e.g. the bribe envelope) lies loose on the desk from now on, where it is, on top of the pile; it can be
        /// dragged like a paper. One without a collider gets a box around its meshes, so the mouse can catch it.
        /// </summary>
        public void AddLoose(Transform thing)
        {
            if (thing == null || _loose.Contains(thing)) return;
            if (!thing.TryGetComponent<DocumentView>(out _))
            {
                var shape = MeshShape(thing);
                _shapes[thing] = shape;
                if (thing.GetComponentInChildren<Collider>(true) == null)
                {
                    var box = thing.gameObject.AddComponent<BoxCollider>();
                    box.center = shape.center;
                    box.size = shape.size;
                }
            }
            _loose.Add(thing);
            SyncPile();
            Raise(thing);
            UpdateLayers();
            Restack();
        }

        /// <summary>P139: a loose thing is no longer on the desk (it is about to be hidden or destroyed); stops whatever it was doing.</summary>
        public void RemoveLoose(Transform thing)
        {
            if (thing == null || !_loose.Remove(thing)) return;
            if (_pressed == thing)
            {
                _pressed = null;
                _dragging = false;
                SetColliders(thing, true);
            }
            bool wasRead = thing.TryGetComponent<DocumentView>(out var view) && Forget(view);
            if (view != null) view.HighlightField(null);
            StopGlide(thing);
            _shapes.Remove(thing);
            _pile.Remove(thing);
            if (_sharpLayer >= 0) SetLayer(thing, _worldLayer);
            if (wasRead && _reading.Count > 0) LayOutReading();
            Restack(); // what lay on it comes down onto the desk instead of floating where it was
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
            AudioManager.PlayVaried(s => s.paperPickups, s => s.paperPickup);
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
                    _pressed = top.transform;
                    _dragging = true;
                    AudioManager.PlayVaried(s => s.paperPickups, s => s.paperPickup);
                    _dragOffset = Vector3.zero;
                    SetColliders(top, false);
                    BringToFront(top.transform);
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
                    AudioManager.PlayVaried(s => s.paperPickups, s => s.paperPickup);
                    SetColliders(_pressed, false); // the mouse ray must see what is under the paper (desk, drawer)
                }
                if (_dragging) _pressed.position = DragTarget(screenPos) + Vector3.up * Lift(_pressed);
                if (released)
                {
                    if (_dragging) FinishDrag(screenPos);
                    else
                    {
                        if (_pressed.TryGetComponent<DocumentView>(out var view)) Read(view); // a click on the envelope does nothing
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
            var thing = DeskThingAt(hit.collider);
            if (thing != null)
            {
                FinishGlide(thing); // e.g. still gliding back from reading: the hand wins, but from its proper resting pose
                BringToFront(thing);
                if (released)
                {
                    if (thing.TryGetComponent<DocumentView>(out var clicked)) Read(clicked); // pressed and released in the same frame: a click
                    return true;
                }
                _pressed = thing;
                _pressScreen = screenPos;
                _dragOffset = DeskPoint(screenPos, out var p) ? Flat(thing.position - p) : Vector3.zero;
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
            _fieldUnderPointer = field != null;
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
            else if (under != null && (_onDesk.Contains(under) || _loose.Contains(under.transform)))
            {
                if (_busy) return true; // no new paper in hand while the drawer moves
                FinishGlide(under.transform);
                BringToFront(under.transform);
                Read(under);
            }
            else PutDownAll(instant: false);
            return true;
        }

        /// <summary>
        /// P138: the mouse is over something the desk reacts to: a paper or loose thing on the desk, a note, a paper held up,
        /// or the drawer with papers in it.
        /// </summary>
        public bool IsClickable(Collider collider)
        {
            if (collider == null) return false;
            if (DeskThingAt(collider) != null) return true;
            var view = collider.GetComponentInParent<DocumentView>();
            if (view != null && (_pinned.Contains(view) || _reading.Contains(view))) return true;
            return _inDrawer.Count > 0 && collider.transform.IsChildOf(drawer);
        }

        /// <summary>
        /// P139: where a stamp clicked onto this point of the desk stands: on the bare desk top (not on papers, loose things, the
        /// drawer or what is kept clear), pulled inside the paper area. False if the click was not on the bare desk.
        /// </summary>
        public bool StampSpot(RaycastHit hit, float tolerance, out Vector3 spot)
        {
            spot = hit.point;
            if (!DeskLayout.OnDeskTop(hit.point.y, hit.normal.y, DeskTop, tolerance)) return false;
            if (hit.collider.transform.IsChildOf(drawer) || DeskThingAt(hit.collider) != null
                || hit.collider.GetComponentInParent<DocumentView>() != null) return false;
            if (paperArea != null)
            {
                spot = paperArea.TransformPoint(DeskLayout.ClampToArea(paperArea.InverseTransformPoint(hit.point), paperAreaSize));
                spot.y = hit.point.y;
            }
            return !Blocked(spot) && !OnArrivalSpot(spot);
        }

        /// <summary>P139: where the drawer lays new papers; a stamp standing there would be picked up instead of the paper.</summary>
        private bool OnArrivalSpot(Vector3 pos)
        {
            const float clearance = 0.12f;
            foreach (var s in deskSpots)
            {
                if (s == null) continue;
                var d = s.position - pos;
                d.y = 0f;
                if (d.sqrMagnitude < clearance * clearance) return true;
            }
            return false;
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

        /// <summary>
        /// Takes away every traveler paper on the desk and in the drawer (arrest / airlock); printed slips stay, and so do pinned
        /// notes and loose things (P139: the bribe note is no traveler paper).
        /// </summary>
        public void Confiscate()
        {
            foreach (var view in new List<DocumentView>(_reading))
                if (view.Type != DocumentType.Citation && !_pinned.Contains(view) && !_loose.Contains(view.transform)) Forget(view);
            if (_reading.Count > 0) LayOutReading();
            if (_pressed != null && _pressed.TryGetComponent<DocumentView>(out var pressedView) && _onDesk.Contains(pressedView)
                && pressedView.Type != DocumentType.Citation)
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
            AudioManager.PlayVaried(s => s.paperSlides, s => s.paperSlide);
            var thing = _pressed;
            // Only traveler papers and slips go into the drawer, loose things never; a moving drawer takes nothing: its tray has no fixed place yet.
            bool intoDrawer = thing.TryGetComponent<DocumentView>(out var view) && _onDesk.Contains(view) && !_busy && PointerOnDrawer(screenPos);
            _pressed = null;
            _dragging = false;
            Drop(thing, intoDrawer);
            SetColliders(thing, true);
        }

        /// <summary>Lays a dragged thing down: a paper into the drawer if the mouse is on it, else flat on the desk on top of what it covers.</summary>
        private void Drop(Transform thing, bool onDrawer)
        {
            if (onDrawer && thing.TryGetComponent<DocumentView>(out var doc))
            {
                _onDesk.Remove(doc);
                _inDrawer.Add(doc);
                thing.SetParent(drawerTray, true);
                UpdateLayers();
                StartGlide(thing, drawerTray.TransformPoint(Vector3.up * (_inDrawer.Count * tuning.stackStep)),
                    drawerTray.rotation, tuning.slideDuration * 0.5f);
                return;
            }
            var pos = thing.position;
            // Never under the closed drawer, where the mouse couldn't reach it again and the shift could not go on;
            // P16/P62: the same for what lies on the desk and catches the mouse first.
            if (deskSpots.Length > 0 && Blocked(pos)) pos = deskSpots[0].position;
            SyncPile();
            Raise(thing); // laid down last, so on top (papers that came meanwhile were stacked without it)
            pos.y = StackHeight(thing, pos, thing.rotation);
            thing.position = pos;
        }

        /// <summary>
        /// Under the closed drawer or under what is kept clear (gadgets, the CRT): the mouse could not reach a thing there again.
        /// The drawer may be moving right now, so its place at rest counts, not where it is at the moment.
        /// </summary>
        private bool Blocked(Vector3 pos)
        {
            if (drawer.TryGetComponent<Collider>(out var drawerCollider))
            {
                Physics.SyncTransforms();
                var box = drawerCollider.bounds;
                Vector3 rest = drawer.parent != null ? drawer.parent.TransformPoint(_drawerRest) : _drawerRest;
                box.center += rest - drawer.position;
                if (box.Contains(new Vector3(pos.x, box.center.y, pos.z))) return true;
            }
            foreach (var clear in keepClear)
                if (clear != null && clear.gameObject.activeInHierarchy)
                {
                    var box = clear.bounds;
                    if (box.Contains(new Vector3(pos.x, box.center.y, pos.z))) return true;
                }
            return false;
        }

        private bool PointerOnDrawer(Vector2 screenPos) =>
            station.RaycastPointer(screenPos, out var hit) && hit.collider.transform.IsChildOf(drawer);

        /// <summary>
        /// Where a dragged thing goes: the surface under the mouse (desk, drawer, papers and loose things; aiming at the 15 cm
        /// drawer must not land behind it), kept inside the paper area. Keeps the grab offset.
        /// </summary>
        private Vector3 DragTarget(Vector2 screenPos)
        {
            Vector3 point;
            // Things land on what is under the mouse, but pinned notes hang on the wall: not a surface for them
            if (station.RaycastPointer(screenPos, out var hit) && !_pinned.Contains(hit.collider.GetComponentInParent<DocumentView>())
                && (hit.collider.transform.IsChildOf(drawer) || hit.collider.GetComponentInParent<DocumentView>() != null
                    || DeskThingAt(hit.collider) != null))
                point = hit.point;
            else if (!DeskPoint(screenPos, out point))
                return _pressed.position - Vector3.up * Lift(_pressed);
            point += _dragOffset;
            if (paperArea != null) point = paperArea.TransformPoint(DeskLayout.ClampToArea(paperArea.InverseTransformPoint(point), paperAreaSize));
            return point;
        }

        /// <summary>How far above the surface under the mouse a dragged thing's pivot goes (its bottom floats by dragLift).</summary>
        private float Lift(Transform thing) => tuning.dragLift - BottomOffset(thing);

        private static void SetColliders(Component thing, bool enabled)
        {
            foreach (var c in thing.GetComponentsInChildren<Collider>()) c.enabled = enabled;
        }

        /// <summary>The paper on the desk or the loose thing a collider belongs to, else null.</summary>
        private Transform DeskThingAt(Collider collider)
        {
            var t = collider.transform;
            foreach (var thing in _loose)
                if (thing != null && t.IsChildOf(thing)) return thing;
            var view = collider.GetComponentInParent<DocumentView>();
            return view != null && _onDesk.Contains(view) ? view.transform : null;
        }

        /// <summary>
        /// The touched thing goes on top of the pile, and the pile is laid out anew. Running glides end first: one would land a
        /// paper on its old height, level with another.
        /// </summary>
        private void BringToFront(Transform thing)
        {
            SyncPile();
            foreach (var t in _pile)
                if (!IsHeldUp(t)) FinishGlide(t);
            Raise(thing);
            Restack();
        }

        // ---------- P139: the pile on the desk (DeskLayout does the math) ----------

        private bool IsOnDesk(Transform t) =>
            _loose.Contains(t) || (t.TryGetComponent<DocumentView>(out var view) && _onDesk.Contains(view));

        private bool IsHeldUp(Transform t)
        {
            foreach (var view in _reading)
                if (view.transform == t) return true;
            return false;
        }

        /// <summary>Lies still on the desk: not held up, not dragged, not gliding, not hidden.</summary>
        private bool Resting(Transform t) =>
            t != null && t.gameObject.activeInHierarchy && !_gliding.Contains(t) && !(_dragging && t == _pressed) && !IsHeldUp(t);

        /// <summary>Drops what left the desk from the pile and puts what came onto it on top (in the order it came).</summary>
        private void SyncPile()
        {
            for (int i = _pile.Count - 1; i >= 0; i--)
                if (_pile[i] == null || !IsOnDesk(_pile[i])) _pile.RemoveAt(i);
            foreach (var view in _onDesk)
                if (view != null && !_pile.Contains(view.transform)) _pile.Add(view.transform);
            foreach (var thing in _loose)
                if (thing != null && !_pile.Contains(thing)) _pile.Add(thing);
        }

        private void Raise(Transform thing)
        {
            if (!_pile.Remove(thing)) return;
            _pile.Add(thing);
        }

        /// <summary>Lays everything resting on the desk at its height in the pile.</summary>
        private void Restack()
        {
            SyncPile();
            _stacked.Clear();
            _footprints.Clear();
            _thickness.Clear();
            foreach (var t in _pile)
            {
                if (!Resting(t)) continue;
                _stacked.Add(t);
                _footprints.Add(Footprint(t, t.position, t.rotation));
                _thickness.Add(Thickness(t));
            }
            if (_bottoms.Length < _stacked.Count) _bottoms = new float[_stacked.Count * 2];
            DeskLayout.Bottoms(_footprints, _thickness, DeskTop, tuning.stackStep, _bottoms);
            for (int i = 0; i < _stacked.Count; i++)
            {
                var t = _stacked[i];
                var pos = t.position;
                pos.y = _bottoms[i] - BottomOffset(t);
                t.position = pos;
            }
        }

        /// <summary>The height (pivot y) a thing at this pose comes to lie at: on everything resting below it in the pile that it covers.</summary>
        private float StackHeight(Transform thing, Vector3 position, Quaternion rotation)
        {
            SyncPile();
            var mine = Footprint(thing, position, rotation);
            float under = DeskTop;
            foreach (var other in _pile)
            {
                if (other == thing) break;
                if (!Resting(other)) continue;
                if (DeskLayout.Overlap(mine, Footprint(other, other.position, other.rotation)))
                    under = Mathf.Max(under, other.position.y + BottomOffset(other) + Thickness(other));
            }
            return under + tuning.stackStep - BottomOffset(thing);
        }

        /// <summary>A thing's box in its own space: a paper's page (no thickness), or the meshes of a loose thing measured when it came.</summary>
        private Bounds Shape(Transform t)
        {
            if (_shapes.TryGetValue(t, out var shape)) return shape;
            if (t.TryGetComponent<DocumentView>(out var view)) return new Bounds(Vector3.zero, new Vector3(view.PageSize.x, 0f, view.PageSize.y));
            return new Bounds(Vector3.zero, Vector3.zero);
        }

        private DeskLayout.Footprint Footprint(Transform t, Vector3 position, Quaternion rotation)
        {
            var shape = Shape(t);
            var scale = t.lossyScale;
            var center = position + rotation * Vector3.Scale(shape.center, scale);
            var half = new Vector2(shape.extents.x * Mathf.Abs(scale.x), shape.extents.z * Mathf.Abs(scale.z));
            return DeskLayout.Footprint.Of(center, rotation * Vector3.right, rotation * Vector3.forward, half);
        }

        private float Thickness(Transform t) => Shape(t).size.y * Mathf.Abs(t.lossyScale.y);

        /// <summary>From the pivot down to the thing's bottom (negative if the pivot is above it; 0 for papers).</summary>
        private float BottomOffset(Transform t) => Shape(t).min.y * t.lossyScale.y;

        /// <summary>The meshes of a thing as a box in its own space (any mesh hierarchy, e.g. an imported FBX).</summary>
        private static Bounds MeshShape(Transform thing)
        {
            bool any = false;
            var shape = new Bounds(Vector3.zero, Vector3.zero);
            foreach (var filter in thing.GetComponentsInChildren<MeshFilter>(true))
            {
                if (filter.sharedMesh == null) continue;
                var b = filter.sharedMesh.bounds;
                for (int i = 0; i < 8; i++)
                {
                    var corner = b.center + Vector3.Scale(b.extents, new Vector3((i & 1) == 0 ? -1f : 1f, (i & 2) == 0 ? -1f : 1f, (i & 4) == 0 ? -1f : 1f));
                    var local = thing.InverseTransformPoint(filter.transform.TransformPoint(corner));
                    if (any) shape.Encapsulate(local);
                    else
                    {
                        shape = new Bounds(local, Vector3.zero);
                        any = true;
                    }
                }
            }
            return shape;
        }

        // ---------- Reading ----------

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
            if (!quiet) AudioManager.PlayVaried(s => s.paperPickups, s => s.paperPickup);
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
            if (!instant) AudioManager.PlayVaried(s => s.paperSlides, s => s.paperSlide);
            foreach (var view in new List<DocumentView>(_reading)) PutDown(view, instant, quiet: true);
        }

        private void PutDown(DocumentView doc, bool instant, bool quiet = false)
        {
            var pose = _readingReturn[doc];
            Forget(doc);
            // Things were restacked while this one was in the air: it returns to the height its place has now, not one that is taken.
            if (IsOnDesk(doc.transform)) pose.position.y = StackHeight(doc.transform, pose.position, pose.rotation);
            doc.HighlightField(null);
            StopGlide(doc.transform);
            if (instant)
            {
                doc.transform.SetPositionAndRotation(pose.position, pose.rotation);
                if (IsOnDesk(doc.transform)) Restack(); // what lies above it was stacked without it
                return;
            }
            if (!quiet) AudioManager.PlayVaried(s => s.paperSlides, s => s.paperSlide);
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
            if (IsOnDesk(t) && !IsHeldUp(t)) Restack(); // P139: landed on the desk; maybe on something it now has to lie on
        }

        /// <summary>
        /// Papers on the desk use the sharp layer (drawn in full resolution over the retro image) only while the officer sits
        /// there; in the drawer or when walking around they are drawn like the world, so they never show through walls.
        /// Loose things too (P139): drawn on another layer, a paper would always show over the envelope, even under it.
        /// </summary>
        private void UpdateLayers()
        {
            if (_sharpLayer < 0) return;
            foreach (var v in _onDesk) SetLayer(v, station.IsSeated ? _sharpLayer : _worldLayer);
            foreach (var v in _inDrawer) SetLayer(v, _worldLayer);
            foreach (var v in _pinned) SetLayer(v, station.IsSeated ? _sharpLayer : _worldLayer);
            foreach (var t in _loose)
                if (t != null) SetLayer(t, station.IsSeated ? _sharpLayer : _worldLayer);
        }

        private static void SetLayer(Component thing, int layer)
        {
            foreach (var t in thing.GetComponentsInChildren<Transform>(true)) t.gameObject.layer = layer;
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
