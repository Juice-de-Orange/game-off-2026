using System;
using System.Collections;
using GameOff.Audio;
using GameOff.Documents;
using GameOff.Player;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GameOff.Counter
{
    /// <summary>
    /// The officer's workplace. E on the seat → camera blends to the desk view and the mouse is free;
    /// clicks on CounterClickables (buttons) raise ActionTriggered; stamps are picked up, moved with the mouse and pressed
    /// onto a StampTarget, which raises ActionTriggered too. P139: a click on the bare desk sets a held stamp down there.
    /// Q / Esc / right click puts a held stamp back (where it was last set down), or else leaves.
    /// P138: tells the GameCursor when the mouse is over something a click would use.
    /// Document logic (P06/P07) listens to ActionTriggered and IsSeated.
    /// </summary>
    public class CounterStation : MonoBehaviour
    {
        [SerializeField] private Interactable seat;
        [SerializeField] private FirstPersonController player;
        [SerializeField] private CinemachineCamera deskCamera;
        [SerializeField] private Camera viewCamera;
        [Tooltip("Papers on the desk (P06); optional. Gets the pointer before the buttons do.")]
        [SerializeField] private DocumentDesk documents;
        [Tooltip("Where the officer stands after leaving (behind the chair, facing the desk).")]
        [SerializeField] private Transform standPoint;
        [Tooltip("Shown only while seated (e.g. the \"[Q] Leave counter\" hint).")]
        [SerializeField] private GameObject seatedHud;
        [Tooltip("Short messages while seated (\"Stamp the papers first\"); optional.")]
        [SerializeField] private TMP_Text notice;
        [Tooltip("Hidden while seated (e.g. the player's crosshair).")]
        [SerializeField] private GameObject[] hideWhileSeated = Array.Empty<GameObject>();
        [SerializeField] private InputActionReference leaveAction;
        [SerializeField] private InputActionReference clickAction;
        [SerializeField] private InputActionReference pointAction;
        [SerializeField] private float clickRange = 4f;
        [SerializeField] private int seatedPriority = 20;
        [SerializeField] private int idlePriority = 0;
        [Tooltip("Time for the camera to blend back before the player can move again (≈ the Cinemachine blend).")]
        [SerializeField] private float leaveBlendTime = 0.6f;

        private bool _seated;
        private int _seatedFrame = -1;
        private StampTool _held;
        private StampTool[] _stamps;
        private Coroutine _returnControl;
        private readonly RaycastHit[] _hits = new RaycastHit[16];
        private Coroutine _noticeRoutine;

        /// <summary>
        /// Raised when a counter control (button) is used or a document is stamped: THE decision event for game logic.
        /// </summary>
        public event Action<CounterAction> ActionTriggered;
        /// <summary>Raised with true when the officer sits down, false when they leave.</summary>
        public event Action<bool> SeatedChanged;

        public bool IsSeated => _seated;
        public bool IsHoldingStamp => _held != null;
        /// <summary>P81: a window over the desk (shift inventory) takes the keys and the mouse; the desk ignores them meanwhile.</summary>
        public bool InputBlocked => _inputBlocked || Time.frameCount == _unblockedFrame || GameOff.UI.HelpOverlay.BlocksInput; // P140: [H] help open

        private bool _inputBlocked;
        private int _unblockedFrame = -1;

        /// <summary>Blocks desk input while a window is open; the frame it closes stays blocked, so its Esc doesn't also leave the counter.</summary>
        public void SetInputBlocked(bool blocked)
        {
            if (_inputBlocked && !blocked) _unblockedFrame = Time.frameCount;
            _inputBlocked = blocked;
        }
        /// <summary>The camera the mouse rays come from (desk view while seated).</summary>
        public Camera ViewCamera => viewCamera;

        private void Awake()
        {
            deskCamera.Priority = idlePriority;
            if (seatedHud != null) seatedHud.SetActive(false);
            _stamps = GetComponentsInChildren<StampTool>(true);
        }

        private void OnEnable() => seat.OnInteract.AddListener(Sit);

        private void OnDisable()
        {
            seat.OnInteract.RemoveListener(Sit);
            if (_seated) Leave();
            // Coroutines stop with the component: finish a pending leave now, or the player would stay frozen.
            if (_returnControl != null)
            {
                _returnControl = null;
                FinishLeave();
            }
        }

        /// <summary>P105: the desk shakes with the airlock (only the view).</summary>
        public void Shake(float seconds, float amount)
        {
            if (_shaking != null) return;
            _shaking = StartCoroutine(ShakeRoutine(seconds, amount));
        }

        private Coroutine _shaking;

        private System.Collections.IEnumerator ShakeRoutine(float seconds, float amount)
        {
            var view = deskCamera.transform;
            var home = view.localPosition;
            for (float t = 0f; t < seconds; t += Time.deltaTime)
            {
                view.localPosition = home + UnityEngine.Random.insideUnitSphere * (amount * (1f - t / seconds));
                yield return null;
            }
            view.localPosition = home;
            _shaking = null;
        }

        public void Sit()
        {
            if (_seated || player == null || viewCamera == null) return;
            if (_returnControl != null) StopCoroutine(_returnControl);
            _returnControl = null;
            _seated = true;
            _seatedFrame = Time.frameCount;
            player.SetControlEnabled(false);
            deskCamera.Priority = seatedPriority;
            seat.SetInteractionEnabled(false);
            SetHud(true);
            SetStampsSharp(true);
            leaveAction.action.Enable();
            clickAction.action.Enable();
            pointAction.action.Enable();
            SeatedChanged?.Invoke(true);
        }

        public void Leave()
        {
            if (!_seated) return;
            _seated = false;
            PutBackHeld();
            SetStampsSharp(false);
            leaveAction.action.Disable();
            clickAction.action.Disable();
            pointAction.action.Disable();
            deskCamera.Priority = idlePriority;
            foreach (var c in GetComponentsInChildren<CounterClickable>()) c.ResetCover();
            SetHud(false);
            if (player != null && standPoint != null) player.Teleport(standPoint.position, standPoint.rotation);
            SeatedChanged?.Invoke(false);
            // Movement and the seat come back after the camera blend, so the seat prompt can't flash and E can't re-seat at once.
            if (isActiveAndEnabled) _returnControl = StartCoroutine(ReturnControl());
            else FinishLeave();
        }

        private IEnumerator ReturnControl()
        {
            yield return new WaitForSecondsRealtime(leaveBlendTime); // also when a pause menu sets timeScale 0
            FinishLeave();
            _returnControl = null;
        }

        private void FinishLeave()
        {
            seat.SetInteractionEnabled(true);
            if (player != null) player.SetControlEnabled(true);
        }

        private void Update()
        {
            if (!_seated || Time.frameCount == _seatedFrame || InputBlocked) return;
            var screenPos = pointAction.action.ReadValue<Vector2>();
            MarkHover(screenPos);
            if (_held != null && _held.IsPressing) return; // a started press always finishes and counts

            if (leaveAction.action.WasPressedThisFrame())
            {
                // One step back at a time: stamp down, then the paper held up for reading, then stand up.
                if (_held != null) PutBackHeld();
                else if (documents == null || !documents.Cancel()) Leave();
                return;
            }

            if (_held == null && documents != null && !documents.IsMovingPaper && Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
            {
                documents.ToggleReadAll();
                return;
            }

            bool clicked = clickAction.action.WasPressedThisFrame();
            if (_held != null)
            {
                UpdateHeld(screenPos, clicked);
                return;
            }
            // Papers first (drag, read, drawer); only a click they don't use goes to stamps and buttons.
            bool usedByDocuments = documents != null && documents.HandlePointer(screenPos, clicked, clickAction.action.WasReleasedThisFrame());
            if (!usedByDocuments && clicked) TryClick(screenPos);
        }

        private void TryClick(Vector2 screenPos)
        {
            if (!RaycastPointer(screenPos, out RaycastHit hit)) return;
            var stamp = hit.collider.GetComponentInParent<StampTool>();
            if (stamp != null)
            {
                _held = stamp;
                stamp.PickUp();
                return;
            }
            var clickable = hit.collider.GetComponentInParent<CounterClickable>();
            if (clickable != null && clickable.Click())
            {
                Debug.Log($"[Counter] {clickable.Action}");
                ActionTriggered?.Invoke(clickable.Action);
            }
        }

        private void UpdateHeld(Vector2 screenPos, bool clicked)
        {
            if (_held.IsPressing) return;
            bool hasHit = RaycastPointer(screenPos, out RaycastHit hit);
            if (hasHit) _held.Hover(hit.point, hit.normal);
            if (!clicked) return;
            var target = hasHit ? hit.collider.GetComponentInParent<StampTarget>() : null;
            if (target != null)
                _held.StampOn(target, hit.point, hit.normal, transform.right, OnStamped); // page top on the desk, wherever the officer looks (SeatedLook)
            else if (!hasHit || !SetDownHeld(hit))
                PutBackHeld(); // a click on something else puts the stamp back where it last stood
        }

        /// <summary>P139: a click on the bare desk sets the held stamp down there; it stays until it is picked up again.</summary>
        private bool SetDownHeld(RaycastHit hit)
        {
            if (documents == null || hit.collider.GetComponentInParent<CounterClickable>() != null
                || hit.collider.GetComponentInParent<StampTool>() != null) return false;
            if (!documents.StampSpot(hit, _held.DeskTolerance, out var spot)) return false;
            _held.SetDown(spot);
            _held = null;
            return true;
        }

        /// <summary>P139: stamps are drawn with the papers while seated, so a paper under a stamp never shows over it.</summary>
        private void SetStampsSharp(bool sharp)
        {
            if (_stamps == null) return;
            foreach (var stamp in _stamps)
                if (stamp != null) stamp.ShowSharp(sharp);
        }

        /// <summary>P138: the hand pointer over what a click would use: a stamp target with a stamp in hand, else stamps, buttons and the papers.</summary>
        private void MarkHover(Vector2 screenPos)
        {
            bool hover;
            if (_held != null)
                hover = !_held.IsPressing && RaycastPointer(screenPos, out var aim) && aim.collider.GetComponentInParent<StampTarget>() != null;
            else if (documents != null && (documents.IsMovingPaper || documents.PointerOnField))
                hover = true;
            else
                hover = RaycastPointer(screenPos, out var under) && Clickable(under.collider);
            if (hover) GameOff.UI.GameCursor.MarkHover();
        }

        private bool Clickable(Collider collider) =>
            collider.GetComponentInParent<StampTool>() != null || collider.GetComponentInParent<CounterClickable>() != null
            || (documents != null && documents.IsClickable(collider));

        private void OnStamped(CounterAction action)
        {
            Debug.Log($"[Counter] Stamp {action}");
            if (action == CounterAction.Approve) AudioManager.Play(s => s.stampApprove);
            else AudioManager.Play(s => s.stampDeny);
            ActionTriggered?.Invoke(action);
        }

        private void PutBackHeld()
        {
            if (_held == null) return;
            _held.PutBack();
            _held = null;
        }

        /// <summary>Ray from the desk camera through the mouse position (screen pixels).</summary>
        public Ray PointerRay(Vector2 screenPos)
        {
            // Viewport coordinates: the camera may render into a low-res texture (RetroLook), the mouse is in screen pixels.
            var viewport = new Vector3(screenPos.x / Mathf.Max(1, Screen.width), screenPos.y / Mathf.Max(1, Screen.height), 0f);
            return viewCamera.ViewportPointToRay(viewport);
        }

        /// <summary>Closest hit under the mouse; ignores triggers and the player's own body.</summary>
        public bool RaycastPointer(Vector2 screenPos, out RaycastHit closest)
        {
            Ray ray = PointerRay(screenPos);
            closest = default;
            float best = float.MaxValue;
            int count = Physics.RaycastNonAlloc(ray, _hits, clickRange, ~0, QueryTriggerInteraction.Ignore); // no garbage per frame (WebGL)
            for (int i = 0; i < count; i++)
            {
                var hit = _hits[i];
                if (hit.distance >= best || (player != null && hit.collider.transform.IsChildOf(player.transform))) continue;
                best = hit.distance;
                closest = hit;
            }
            return best < float.MaxValue;
        }

        /// <summary>Shows a short message on the desk HUD for a few seconds (P07; spoken announcements come with P41).</summary>
        public void ShowNotice(string message, float seconds = 3f)
        {
            if (notice == null) { Debug.Log("[Counter] " + message); return; }
            if (_noticeRoutine != null) StopCoroutine(_noticeRoutine);
            _noticeRoutine = StartCoroutine(NoticeRoutine(message, seconds));
        }

        private IEnumerator NoticeRoutine(string message, float seconds)
        {
            notice.text = message;
            notice.gameObject.SetActive(true);
            yield return new WaitForSecondsRealtime(seconds);
            notice.gameObject.SetActive(false);
            _noticeRoutine = null;
        }

        private void SetHud(bool seated)
        {
            if (seatedHud != null) seatedHud.SetActive(seated);
            foreach (var go in hideWhileSeated)
                if (go != null) go.SetActive(!seated);
        }
    }
}
