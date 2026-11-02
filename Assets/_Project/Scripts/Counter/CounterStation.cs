using System;
using GameOff.Player;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GameOff.Counter
{
    /// <summary>
    /// The officer's workplace. E on the seat → camera blends to the desk view and the mouse is free;
    /// clicks on CounterClickables raise ActionTriggered; Q leaves.
    /// Document logic (P06/P07) listens to ActionTriggered and IsSeated.
    /// </summary>
    public class CounterStation : MonoBehaviour
    {
        [SerializeField] private Interactable seat;
        [SerializeField] private FirstPersonController player;
        [SerializeField] private CinemachineCamera deskCamera;
        [SerializeField] private Camera viewCamera;
        [Tooltip("Shown only while seated (e.g. the \"[Q] Leave counter\" hint).")]
        [SerializeField] private GameObject seatedHud;
        [Tooltip("Hidden while seated (e.g. the player's crosshair).")]
        [SerializeField] private GameObject[] hideWhileSeated = Array.Empty<GameObject>();
        [SerializeField] private InputActionReference leaveAction;
        [SerializeField] private InputActionReference clickAction;
        [SerializeField] private InputActionReference pointAction;
        [SerializeField] private float clickRange = 4f;
        [SerializeField] private int seatedPriority = 20;
        [SerializeField] private int idlePriority = 0;

        private bool _seated;
        private int _seatedFrame = -1;

        /// <summary>Raised when a counter control (stamp/button) is used.</summary>
        public event Action<CounterAction> ActionTriggered;
        /// <summary>Raised with true when the officer sits down, false when they leave.</summary>
        public event Action<bool> SeatedChanged;

        public bool IsSeated => _seated;

        private void Awake()
        {
            deskCamera.Priority = idlePriority;
            if (seatedHud != null) seatedHud.SetActive(false);
        }

        private void OnEnable() => seat.OnInteract.AddListener(Sit);

        private void OnDisable()
        {
            seat.OnInteract.RemoveListener(Sit);
            if (_seated) Leave();
        }

        public void Sit()
        {
            if (_seated) return;
            _seated = true;
            _seatedFrame = Time.frameCount;
            player.SetControlEnabled(false);
            deskCamera.Priority = seatedPriority;
            seat.SetInteractionEnabled(false);
            SetHud(true);
            leaveAction.action.Enable();
            clickAction.action.Enable();
            pointAction.action.Enable();
            SeatedChanged?.Invoke(true);
        }

        public void Leave()
        {
            if (!_seated) return;
            _seated = false;
            leaveAction.action.Disable();
            clickAction.action.Disable();
            pointAction.action.Disable();
            deskCamera.Priority = idlePriority;
            foreach (var c in GetComponentsInChildren<CounterClickable>()) c.ResetCover();
            seat.SetInteractionEnabled(true);
            SetHud(false);
            player.SetControlEnabled(true);
            SeatedChanged?.Invoke(false);
        }

        private void Update()
        {
            if (!_seated || Time.frameCount == _seatedFrame) return;

            if (leaveAction.action.WasPressedThisFrame())
            {
                Leave();
                return;
            }

            if (clickAction.action.WasPressedThisFrame())
                TryClick(pointAction.action.ReadValue<Vector2>());
        }

        private void TryClick(Vector2 screenPos)
        {
            Ray ray = viewCamera.ScreenPointToRay(screenPos);
            if (!Physics.Raycast(ray, out RaycastHit hit, clickRange, ~0, QueryTriggerInteraction.Ignore)) return;
            var clickable = hit.collider.GetComponentInParent<CounterClickable>();
            if (clickable != null && clickable.Click())
            {
                Debug.Log($"[Counter] {clickable.Action}");
                ActionTriggered?.Invoke(clickable.Action);
            }
        }

        private void SetHud(bool seated)
        {
            if (seatedHud != null) seatedHud.SetActive(seated);
            foreach (var go in hideWhileSeated)
                if (go != null) go.SetActive(!seated);
        }
    }
}
