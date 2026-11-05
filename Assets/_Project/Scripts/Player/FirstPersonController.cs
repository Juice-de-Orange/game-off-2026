using GameOff.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GameOff.Player
{
    /// <summary>
    /// First-person movement (WASD) and mouse look on a CharacterController.
    /// The mouse is captured on the first click (browsers only allow pointer lock after a user gesture).
    /// Other systems (counter view, menus) call SetControlEnabled(false) to take over camera and cursor.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class FirstPersonController : MonoBehaviour
    {
        [SerializeField] private PlayerTuning tuning;
        [SerializeField] private Transform cameraPivot;
        [SerializeField] private InputActionReference moveAction;
        [SerializeField] private InputActionReference lookAction;

        private const float MaxStepSeconds = 0.05f; // a long frame (loading) must not become one huge step through the floor
        private const float MaxFallSpeed = 20f;
        private const float SpawnClearance = 0.05f; // above the floor when placed: touching it exactly can miss it
        private const float RescueAfterFalling = 3f; // meters below the last solid ground; no drop in the station is that deep

        private CharacterController _controller;
        private Vector3 _lastGround;
        private float _sensitivitySetting = 1f;
        private bool _invertY;
        private float _pitch;
        private float _verticalVelocity;
        private bool _controlEnabled = true;

        public bool ControlEnabled => _controlEnabled;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _pitch = cameraPivot.localEulerAngles.x;
            if (_pitch > 180f) _pitch -= 360f;
            _lastGround = transform.position;
        }

        private void OnEnable()
        {
            moveAction.action.Enable();
            lookAction.action.Enable();
            GameSettings.Changed += ReadSettings;
            ReadSettings();
        }

        private void OnDisable()
        {
            moveAction.action.Disable();
            lookAction.action.Disable();
            GameSettings.Changed -= ReadSettings;
        }

        // Read once and on changes: PlayerPrefs are native calls, not for every frame of looking around
        private void ReadSettings()
        {
            _sensitivitySetting = GameSettings.MouseSensitivity;
            _invertY = GameSettings.InvertY;
        }

        /// <summary>
        /// Turn player movement/look on or off, e.g. while sitting at the counter or in a menu. Turning it back on captures the
        /// mouse again right away (P46): this is called just after the click or key press that closed the window, which the
        /// browser still accepts as the user's action, so the camera turns at once instead of waiting for another click.
        /// </summary>
        public void SetControlEnabled(bool enabled)
        {
            _controlEnabled = enabled;
            SetCursorCaptured(enabled);
            if (enabled) PointerLockBridge.Request();
        }

        /// <summary>Moves the player to a spot (e.g. after leaving the counter); yaw from the rotation, camera pitch level.</summary>
        public void Teleport(Vector3 position, Quaternion rotation)
        {
            _controller.enabled = false; // the CharacterController would otherwise overwrite the new position
            Vector3 forward = Vector3.ProjectOnPlane(rotation * Vector3.forward, Vector3.up);
            position += Vector3.up * SpawnClearance;
            transform.SetPositionAndRotation(position, forward.sqrMagnitude > 0.001f ? Quaternion.LookRotation(forward) : transform.rotation);
            Physics.SyncTransforms(); // the controller's next move must see the new position (auto sync is off)
            _controller.enabled = true;
            _lastGround = position;
            _pitch = 0f;
            cameraPivot.localRotation = Quaternion.identity;
            _verticalVelocity = 0f;
        }

        private void Update()
        {
            if (_controlEnabled && !IsCursorCaptured() && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                SetCursorCaptured(true);

            bool canLook = _controlEnabled && IsCursorCaptured();
            if (canLook) Look(lookAction.action.ReadValue<Vector2>());
            Move(_controlEnabled ? moveAction.action.ReadValue<Vector2>() : Vector2.zero);
        }

        private void Look(Vector2 delta)
        {
            float sensitivity = tuning.mouseSensitivity * _sensitivitySetting; // option from the menu
            transform.Rotate(0f, delta.x * sensitivity, 0f);
            float pitchInput = _invertY ? -delta.y : delta.y; // option from the menu (P43)
            _pitch = Mathf.Clamp(_pitch - pitchInput * sensitivity, -tuning.maxPitch, tuning.maxPitch);
            cameraPivot.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
        }

        private void Move(Vector2 input)
        {
            Vector3 horizontal = transform.right * input.x + transform.forward * input.y;
            horizontal = Vector3.ClampMagnitude(horizontal, 1f) * tuning.walkSpeed;

            if (_controller.isGrounded)
            {
                _lastGround = transform.position;
                if (_verticalVelocity < 0f) _verticalVelocity = -2f; // keep grounded on steps
            }
            float dt = Mathf.Min(Time.deltaTime, MaxStepSeconds);
            _verticalVelocity = Mathf.Max(_verticalVelocity - tuning.gravity * dt, -MaxFallSpeed);

            _controller.Move((horizontal + Vector3.up * _verticalVelocity) * dt);

            // Safety net: fell through something after all (B01). Back to where we last stood instead of falling forever.
            if (transform.position.y < _lastGround.y - RescueAfterFalling) Teleport(_lastGround, transform.rotation);
        }

        private static bool IsCursorCaptured() => Cursor.lockState == CursorLockMode.Locked;

        private static void SetCursorCaptured(bool captured)
        {
            Cursor.lockState = captured ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !captured;
        }
    }
}
