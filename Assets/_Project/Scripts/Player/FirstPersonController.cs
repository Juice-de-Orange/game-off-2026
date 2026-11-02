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

        private CharacterController _controller;
        private float _pitch;
        private float _verticalVelocity;
        private bool _controlEnabled = true;

        public bool ControlEnabled => _controlEnabled;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _pitch = cameraPivot.localEulerAngles.x;
            if (_pitch > 180f) _pitch -= 360f;
        }

        private void OnEnable()
        {
            moveAction.action.Enable();
            lookAction.action.Enable();
        }

        private void OnDisable()
        {
            moveAction.action.Disable();
            lookAction.action.Disable();
        }

        /// <summary>Turn player movement/look on or off, e.g. while sitting at the counter or in a menu.</summary>
        public void SetControlEnabled(bool enabled)
        {
            _controlEnabled = enabled;
            if (!enabled) SetCursorCaptured(false);
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
            transform.Rotate(0f, delta.x * tuning.mouseSensitivity, 0f);
            _pitch = Mathf.Clamp(_pitch - delta.y * tuning.mouseSensitivity, -tuning.maxPitch, tuning.maxPitch);
            cameraPivot.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
        }

        private void Move(Vector2 input)
        {
            Vector3 horizontal = transform.right * input.x + transform.forward * input.y;
            horizontal = Vector3.ClampMagnitude(horizontal, 1f) * tuning.walkSpeed;

            if (_controller.isGrounded && _verticalVelocity < 0f) _verticalVelocity = -2f; // keep grounded on steps
            _verticalVelocity -= tuning.gravity * Time.deltaTime;

            _controller.Move((horizontal + Vector3.up * _verticalVelocity) * Time.deltaTime);
        }

        private static bool IsCursorCaptured() => Cursor.lockState == CursorLockMode.Locked;

        private static void SetCursorCaptured(bool captured)
        {
            Cursor.lockState = captured ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !captured;
        }
    }
}
