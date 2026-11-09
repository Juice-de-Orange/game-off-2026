using System;
using System.Collections;
using GameOff.Core;
using GameOff.Day;
using GameOff.Player;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GameOff.Rest
{
    /// <summary>
    /// P28: a place to sit down or look from (hall bench, bar stool, porthole in 103, the photo on the locker). [E] takes the
    /// camera off the player and moves it to the eye; the mouse (or A/D, W/S) turns the view a little; Q or Esc moves it back and
    /// gives the player the controls again. The player's body stays where it stood. Like the coin telescope, Esc in the browser only
    /// releases the mouse lock, so losing a lock that was held also stands up.
    /// HallBench, BarStool and PortholeView add what happens while seated (SatDown / StandingUp).
    /// </summary>
    public class SeatView : MonoBehaviour
    {
        private enum State { Free, Moving, Seated, Leaving }

        [SerializeField] private Interactable interactable;
        [SerializeField] private FirstPersonController player;
        [SerializeField] private Camera playerCamera;
        [Tooltip("Optional: while a dialog is open (the barkeeper's) Q and Esc belong to the dialog.")]
        [SerializeField] private DayFlowUI dialogs;
        [SerializeField] private RestSpotTuning tuning;
        [SerializeField] private RestHud hud;
        [Tooltip("Camera pose while seated.")]
        [SerializeField] private Transform eye;
        [Tooltip("Optional: the camera passes here on the way (the porthole: in front of the glass before it looks out).")]
        [SerializeField] private Transform approach;
        [Tooltip("Optional: the eye turns toward this (the porthole: the home planet).")]
        [SerializeField] private Transform lookAt;
        [Tooltip("Field of view while seated; 0 keeps the player's.")]
        [SerializeField] private float fieldOfView;
        [SerializeField] private bool mouseLook = true;
        [Tooltip("Bar stool: the barkeeper's dialog takes the controls; when it gives them back, the player stands up.")]
        [SerializeField] private bool standWhenControlReturns;
        [Tooltip("The view looks out of the station: occlusion culling off meanwhile (the bake only covers the inside).")]
        [SerializeField] private bool looksOutside;
        [SerializeField] private string keys = "[Q] Stand up";

        private State _state;
        private Transform _cam, _camParent;
        private Vector3 _camLocalPosition;
        private Quaternion _camLocalRotation;
        private float _camFov;
        private bool _occlusion, _brainWasEnabled, _lockHeld, _holdsControl;
        private CinemachineBrain _brain;
        private float _yaw, _pitch;
        private int _seatedFrame;

        /// <summary>The camera has arrived at the eye.</summary>
        public event Action SatDown;
        /// <summary>Leaving has begun (the camera moves back now).</summary>
        public event Action StandingUp;

        public bool IsSeated => _state == State.Seated;
        public RestHud Hud => hud;
        public RestSpotTuning Tuning => tuning;

        private void OnEnable()
        {
            if (interactable != null) interactable.OnInteract.AddListener(Sit);
        }

        private void OnDisable()
        {
            if (interactable != null) interactable.OnInteract.RemoveListener(Sit);
            if (_state == State.Free) return;
            StopAllCoroutines();
            if (hud != null) hud.Hide();
            Restore();
            if (_holdsControl && player != null) player.SetControlEnabled(true);
            _state = State.Free;
        }

        /// <summary>Public for the test driver as well.</summary>
        public void Sit()
        {
            if (_state != State.Free || player == null || playerCamera == null || eye == null || !player.ControlEnabled) return;
            StartCoroutine(SitRoutine());
        }

        /// <summary>Back to the player (Q, Esc, or the bar dialog closing).</summary>
        public void Leave()
        {
            if (_state != State.Seated) return;
            StartCoroutine(LeaveRoutine());
        }

        private IEnumerator SitRoutine()
        {
            _state = State.Moving;
            _holdsControl = true;
            player.SetControlEnabled(false); // stops walking and looking, hides the [E] prompt
            _cam = playerCamera.transform;
            _brain = playerCamera.GetComponent<CinemachineBrain>();
            _brainWasEnabled = _brain != null && _brain.enabled;
            if (_brain != null) _brain.enabled = false;
            _camParent = _cam.parent;
            _camLocalPosition = _cam.localPosition;
            _camLocalRotation = _cam.localRotation;
            _camFov = playerCamera.fieldOfView;
            _occlusion = playerCamera.useOcclusionCulling;
            _cam.SetParent(null, true);
            _yaw = _pitch = 0f;
            if (mouseLook)
            {
                yield return null; // the browser needs a frame after the release before it grants the lock again
                Cursor.lockState = CursorLockMode.Locked;
                PointerLockBridge.Request();
            }
            float fov = fieldOfView > 0f ? fieldOfView : _camFov;
            if (approach != null)
            {
                yield return Move(() => new Pose(approach.position, approach.rotation), _camFov, _camFov, 0.5f);
                if (looksOutside) playerCamera.useOcclusionCulling = false;
                yield return Move(EyePose, _camFov, fov, 0.5f);
            }
            else
            {
                if (looksOutside) playerCamera.useOcclusionCulling = false;
                yield return Move(EyePose, _camFov, fov, 1f);
            }
            _lockHeld = false;
            _seatedFrame = Time.frameCount;
            _state = State.Seated;
            if (hud != null) hud.Show(keys);
            SatDown?.Invoke();
        }

        private IEnumerator LeaveRoutine()
        {
            _state = State.Leaving;
            if (hud != null) hud.Hide();
            StandingUp?.Invoke();
            // Bar stool: the dialog already gave the controls back (and the mouse lock with it); don't take them again.
            _holdsControl = !player.ControlEnabled;
            if (approach != null)
            {
                yield return Move(() => new Pose(approach.position, approach.rotation), playerCamera.fieldOfView, _camFov, 0.5f);
                playerCamera.useOcclusionCulling = _occlusion;
                yield return Move(HeadPose, _camFov, _camFov, 0.5f);
            }
            else yield return Move(HeadPose, playerCamera.fieldOfView, _camFov, 1f);
            Restore();
            _state = State.Free;
            if (_holdsControl) player.SetControlEnabled(true);
            _holdsControl = false;
        }

        private IEnumerator Move(Func<Pose> target, float fovFrom, float fovTo, float share)
        {
            Vector3 fromPosition = _cam.position;
            Quaternion fromRotation = _cam.rotation;
            float seconds = Mathf.Max(0.01f, tuning != null ? tuning.moveSeconds * share : 0.3f);
            for (float t = 0f; t < 1f; t += Time.deltaTime / seconds)
            {
                float k = Mathf.SmoothStep(0f, 1f, t);
                var to = target();
                _cam.SetPositionAndRotation(Vector3.Lerp(fromPosition, to.position, k), Quaternion.Slerp(fromRotation, to.rotation, k));
                playerCamera.fieldOfView = Mathf.Lerp(fovFrom, fovTo, k);
                yield return null;
            }
            var end = target();
            _cam.SetPositionAndRotation(end.position, end.rotation);
            playerCamera.fieldOfView = fovTo;
        }

        private Pose EyePose()
        {
            var rotation = lookAt != null ? Quaternion.LookRotation(lookAt.position - eye.position) : eye.rotation;
            return new Pose(eye.position, rotation);
        }

        // Follows the head while the camera flies back (on the bar stool the player may already turn).
        private Pose HeadPose() => _camParent != null
            ? new Pose(_camParent.TransformPoint(_camLocalPosition), _camParent.rotation * _camLocalRotation)
            : new Pose(_camLocalPosition, _camLocalRotation);

        private void Restore()
        {
            if (_cam == null) return;
            _cam.SetParent(_camParent, false);
            _cam.localPosition = _camLocalPosition;
            _cam.localRotation = _camLocalRotation;
            playerCamera.fieldOfView = _camFov;
            playerCamera.useOcclusionCulling = _occlusion;
            if (_brain != null) _brain.enabled = _brainWasEnabled;
        }

        private void Update()
        {
            if (_state != State.Seated) return;
            if (standWhenControlReturns && player.ControlEnabled)
            {
                Leave();
                return;
            }
            if (Time.frameCount == _seatedFrame || (dialogs != null && dialogs.IsDialogOpen)) return;

            var keyboard = Keyboard.current;
            bool exit = keyboard != null && (keyboard.qKey.wasPressedThisFrame || keyboard.escapeKey.wasPressedThisFrame);
            if (mouseLook)
            {
                _lockHeld |= PointerLockBridge.IsLocked;
                exit |= _lockHeld && !PointerLockBridge.IsLocked; // in the browser Esc only releases the mouse
            }
            if (exit)
            {
                Leave();
                return;
            }
            if (mouseLook) Look(keyboard);
        }

        private void Look(Keyboard keyboard)
        {
            Vector2 delta = Vector2.zero;
            if (keyboard != null) // the arrow keys and A/D, W/S work without the mouse lock
            {
                float turn = (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed ? 1f : 0f) - (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed ? 1f : 0f);
                float tilt = (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed ? 1f : 0f) - (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed ? 1f : 0f);
                delta += new Vector2(turn, tilt) * (tuning.keyDegreesPerSecond * Time.deltaTime);
            }
            var mouse = Mouse.current;
            if (mouse != null && PointerLockBridge.IsLocked)
            {
                Vector2 m = mouse.delta.ReadValue() * (tuning.mouseDegrees * GameSettings.MouseSensitivity);
                if (GameSettings.InvertY) m.y = -m.y;
                delta += m;
            }
            if (delta == Vector2.zero) return;
            _yaw = Mathf.Clamp(_yaw + delta.x, -tuning.lookYaw, tuning.lookYaw);
            _pitch = Mathf.Clamp(_pitch - delta.y, -tuning.lookPitch, tuning.lookPitch);
            var rest = EyePose().rotation.eulerAngles;
            _cam.rotation = Quaternion.Euler(rest.x + _pitch, rest.y + _yaw, 0f);
        }
    }
}
