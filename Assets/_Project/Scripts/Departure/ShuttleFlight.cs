using System.Collections.Generic;
using GameOff.Characters;
using GameOff.Core;
using GameOff.Day;
using GameOff.Economy;
using GameOff.Player;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GameOff.Departure
{
    /// <summary>
    /// P72/P106: the flight home. The docked shuttle and its cabin are hidden and a movable copy of its model lifts off; the player's camera
    /// becomes a chase camera. W/S thrust, mouse (or A/D) steers, Shift boosts. No collisions. Far enough from the take-off point,
    /// or after maxSeconds, the run ends with a ticket ending that depends on the family (TicketEnding).
    /// </summary>
    public class ShuttleFlight : MonoBehaviour
    {
        [SerializeField] private DepartureTuning tuning;
        [SerializeField] private DayManager day;
        [SerializeField] private FirstPersonController player;
        [SerializeField] private Camera playerCamera;
        [SerializeField] private DayFlowUI ui;
        [Tooltip("The shuttle docked outside the hangar (PF_Exterior); hidden when the copy takes off.")]
        [SerializeField] private Transform dockedShuttle;
        [Tooltip("The cabin inside it (station prefab): its seats would hang in space once the hull has gone.")]
        [SerializeField] private Transform cabin;
        [Tooltip("The shuttle model asset (SM_Ext_Shuttle_Docked.fbx); copied for the flight.")]
        [SerializeField] private GameObject shuttleModel;
        [Tooltip("Optional engine loop, pitch follows the speed.")]
        [SerializeField] private AudioSource engine;

        private InputAction _thrust, _turn, _boost;
        private Transform _body, _visual;
        private Vector3 _start;
        private Quaternion _visualRest;
        private float _time, _speed, _yaw, _startYaw, _pitch, _bank;
        private bool _finished;

        public bool IsFlying { get; private set; }

        private void Awake()
        {
            _thrust = new InputAction("Thrust", InputActionType.Value);
            _thrust.AddCompositeBinding("1DAxis").With("Negative", "<Keyboard>/s").With("Positive", "<Keyboard>/w");
            _thrust.AddCompositeBinding("1DAxis").With("Negative", "<Keyboard>/downArrow").With("Positive", "<Keyboard>/upArrow");
            _turn = new InputAction("Turn", InputActionType.Value);
            _turn.AddCompositeBinding("1DAxis").With("Negative", "<Keyboard>/a").With("Positive", "<Keyboard>/d");
            _turn.AddCompositeBinding("1DAxis").With("Negative", "<Keyboard>/leftArrow").With("Positive", "<Keyboard>/rightArrow");
            _boost = new InputAction("Boost", InputActionType.Button, "<Keyboard>/leftShift");
        }

        private void OnDestroy()
        {
            _thrust.Dispose();
            _turn.Dispose();
            _boost.Dispose();
        }

        /// <summary>Called by the LEAVE button at the ramp.</summary>
        public void Launch()
        {
            if (IsFlying) return;
            if (tuning == null || dockedShuttle == null || shuttleModel == null || playerCamera == null || day == null)
            {
                Debug.LogError("[Departure] ShuttleFlight is not wired (run GameOff/Build Departure into Main) - no flight.");
                if (player != null) player.SetControlEnabled(true);
                return;
            }
            IsFlying = true;
            foreach (var r in dockedShuttle.GetComponentsInChildren<Renderer>()) r.enabled = false;
            if (cabin != null) foreach (var r in cabin.GetComponentsInChildren<Renderer>()) r.enabled = false;

            // The model's nose points along its local -Z.
            _start = dockedShuttle.position;
            _body = new GameObject("FlightShuttle").transform;
            _body.SetPositionAndRotation(_start, Quaternion.LookRotation(-dockedShuttle.forward, Vector3.up));
            _visual = Instantiate(shuttleModel, _body).transform;
            _visual.SetPositionAndRotation(dockedShuttle.position, dockedShuttle.rotation);
            _visual.localScale = dockedShuttle.lossyScale;
            _visualRest = _visual.localRotation; // keeps any import rotation of the model; the lean goes on top
            _yaw = _startYaw = _body.eulerAngles.y;

            // Cinemachine drives the player's camera every frame (counter view, P03); the flight takes it over for good.
            var brain = playerCamera.GetComponent<CinemachineBrain>();
            if (brain != null) brain.enabled = false;
            playerCamera.transform.SetParent(null, true);
            playerCamera.useOcclusionCulling = false; // the bake only covers the inside of the station
            PlaceCamera(1f);

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            PointerLockBridge.Request();
            _thrust.Enable();
            _turn.Enable();
            _boost.Enable();
            if (engine != null) engine.Play();
            if (ui != null) ui.ShowHint("W / S  thrust   ·   Mouse or A / D  steer   ·   Shift  boost", 10f);
            Debug.Log("[Departure] Shuttle launched");
        }

        private void Update()
        {
            if (!IsFlying || _finished) return;
            float dt = Time.deltaTime;
            _time += dt;

            if (_time < tuning.liftOffSeconds)
            {
                float k = Mathf.SmoothStep(0f, 1f, _time / tuning.liftOffSeconds);
                _body.position = _start + Vector3.up * (tuning.liftOffHeight * k);
                _yaw = _startYaw + tuning.undockTurn * k; // away from the station: straight ahead lie its radiators and truss
                _body.rotation = Quaternion.Euler(0f, _yaw, 0f);
            }
            else Steer(dt);

            PlaceCamera(1f - Mathf.Exp(-tuning.cameraFollow * dt));
            if (engine != null) engine.pitch = tuning.enginePitchIdle + _speed / Mathf.Max(1f, tuning.boostSpeed) * tuning.enginePitchRange;

            if (Vector3.Distance(_body.position, _start) > tuning.exitDistance || _time > tuning.maxSeconds) Finish();
        }

        private void Steer(float dt)
        {
            float thrust = _thrust.ReadValue<float>();
            float top = _boost.IsPressed() ? tuning.boostSpeed : tuning.maxSpeed;
            if (thrust > 0f) _speed += tuning.acceleration * thrust * dt;
            else if (thrust < 0f) _speed += tuning.braking * thrust * dt;
            if (_speed > top) _speed = Mathf.MoveTowards(_speed, top, tuning.braking * dt);
            _speed = Mathf.Max(0f, _speed);

            // In the browser Cursor.lockState stays "Locked" after Esc released the lock; ask the browser (PointerLockBridge).
            if (!PointerLockBridge.IsLocked && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                Cursor.lockState = CursorLockMode.Locked;
                PointerLockBridge.Request();
            }
            Vector2 mouse = PointerLockBridge.IsLocked && Mouse.current != null ? Mouse.current.delta.ReadValue() : Vector2.zero;
            float turnKeys = _turn.ReadValue<float>();
            float yawStep = mouse.x * tuning.mouseTurn + turnKeys * tuning.keyTurn * dt;
            _yaw += yawStep;
            _pitch = Mathf.Clamp(_pitch - mouse.y * tuning.mouseTurn, -tuning.maxPitch, tuning.maxPitch);
            float lean = Mathf.Clamp(-yawStep / Mathf.Max(dt, 0.001f) * tuning.bankPerTurn, -tuning.maxBank, tuning.maxBank);
            _bank = Mathf.Lerp(_bank, lean, 1f - Mathf.Exp(-tuning.bankFollow * dt));

            _body.rotation = Quaternion.Euler(_pitch, _yaw, 0f);
            _body.position += _body.forward * (_speed * dt);
            _visual.localRotation = Quaternion.AngleAxis(_bank, Vector3.forward) * _visualRest;
        }

        private void PlaceCamera(float blend)
        {
            var cam = playerCamera.transform;
            Vector3 target = _body.position - _body.forward * tuning.cameraDistance + Vector3.up * tuning.cameraHeight;
            cam.position = Vector3.Lerp(cam.position, target, blend);
            cam.rotation = Quaternion.LookRotation(_body.position + _body.forward * tuning.cameraLookAhead - cam.position, Vector3.up);
        }

        private void Finish()
        {
            _finished = true;
            _thrust.Disable();
            _turn.Disable();
            _boost.Disable();
            var (title, body) = Ending();
            Debug.Log($"[Departure] Left the station after {_time:0.0} s: {title}");
            day.FinishRun(title, body);
        }

        private (string, string) Ending()
        {
            var economy = PlayerEconomy.Instance;
            var conditions = new List<int>();
            var names = new List<string>();
            if (economy != null && economy.Household != null && economy.HouseholdTuning != null)
                for (int i = 0; i < economy.Household.Count; i++)
                {
                    conditions.Add(economy.Household.Condition(i));
                    names.Add(i < economy.HouseholdTuning.members.Count ? economy.HouseholdTuning.members[i].name : "?");
                }
            var (title, body) = TicketEnding.Pick(conditions, names, tuning);
            var conduct = Confiscation.Instance != null ? Confiscation.Instance.Conduct : new Conduct();
            var director = day.GetComponent<CharacterDirector>();
            body = TicketEnding.WithFigures(body, director != null ? director.EndingLines() : null, tuning);
            return (title, body + "\n\n" + TicketEnding.ConductLine(conduct, tuning));
        }
    }
}
