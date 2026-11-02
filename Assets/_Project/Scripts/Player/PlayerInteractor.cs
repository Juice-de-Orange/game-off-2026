using GameOff.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GameOff.Player
{
    /// <summary>Looks for an Interactable in front of the camera, shows its prompt and triggers it on E.</summary>
    public class PlayerInteractor : MonoBehaviour
    {
        [SerializeField] private PlayerTuning tuning;
        [SerializeField] private Camera playerCamera;
        [SerializeField] private FirstPersonController controller;
        [SerializeField] private InteractionPromptUI promptUI;
        [SerializeField] private InputActionReference interactAction;
        [Tooltip("Layers the interaction ray can hit (walls block it).")]
        [SerializeField] private LayerMask rayMask = ~0;

        private Interactable _current;

        public Interactable Current => _current;

        private void OnEnable() => interactAction.action.Enable();

        private void OnDisable()
        {
            interactAction.action.Disable();
            SetCurrent(null);
        }

        private void Update()
        {
            SetCurrent(controller.ControlEnabled ? FindTarget() : null);

            if (_current != null && interactAction.action.WasPressedThisFrame())
                _current.Interact();
        }

        private Interactable FindTarget()
        {
            var ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            if (!Physics.Raycast(ray, out RaycastHit hit, tuning.interactRange, rayMask, QueryTriggerInteraction.Collide))
                return null;
            var target = hit.collider.GetComponentInParent<Interactable>();
            return target != null && target.InteractionEnabled ? target : null;
        }

        private void SetCurrent(Interactable target)
        {
            _current = target;
            if (promptUI == null) return;
            if (target != null) promptUI.Show(target.Prompt);
            else promptUI.Hide();
        }
    }
}
