using GameOff.Documents;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GameOff.Counter
{
    /// <summary>
    /// Looking around while seated (P41): A/D or the arrow keys turn the desk camera. It levels out while turning, because the
    /// desk view looks down and the pinboard hangs at eye height. Held still while a paper is read or moved or a stamp is held.
    /// Back to the desk view on the next sit-down, not on standing up, so the camera blend doesn't jump.
    /// </summary>
    public class SeatedLook : MonoBehaviour
    {
        [SerializeField] private CounterStation station;
        [SerializeField] private DocumentDesk desk;
        [SerializeField] private Transform deskCamera;
        [Tooltip("Player/Move: enabled by the FirstPersonController and ignored by it while seated.")]
        [SerializeField] private InputActionReference moveAction;
        [SerializeField] private SeatedLookTuning tuning;

        private float _restPitch, _restYaw, _yaw;

        private void Awake()
        {
            var rest = deskCamera.localEulerAngles;
            _restPitch = rest.x;
            _restYaw = rest.y;
        }

        private void OnEnable() => station.SeatedChanged += OnSeatedChanged;

        private void OnDisable() => station.SeatedChanged -= OnSeatedChanged;

        private void OnSeatedChanged(bool seated)
        {
            if (!seated) return;
            _yaw = 0f;
            Apply();
        }

        private void Update()
        {
            if (!station.IsSeated || station.IsHoldingStamp || desk.IsHandlingPaper) return;
            float input = moveAction.action.ReadValue<Vector2>().x;
            if (Mathf.Approximately(input, 0f)) return;
            _yaw = Mathf.Clamp(_yaw + input * tuning.turnSpeed * Time.deltaTime, -tuning.yawLimit, tuning.yawLimit);
            Apply();
        }

        private void Apply()
        {
            float pitch = Mathf.Lerp(_restPitch, tuning.pitchAtLimit, Mathf.Abs(_yaw) / tuning.yawLimit);
            deskCamera.localRotation = Quaternion.Euler(pitch, _restYaw + _yaw, 0f);
        }
    }
}
