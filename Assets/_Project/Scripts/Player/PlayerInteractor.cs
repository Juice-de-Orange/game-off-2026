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
        private readonly RaycastHit[] _hits = new RaycastHit[16];

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
            int count = Physics.RaycastNonAlloc(ray, _hits, tuning.interactRange, rayMask, QueryTriggerInteraction.Collide);
            // Nearest hit that isn't the player's own body: looking down steeply (e.g. at the bed) the ray starts in or
            // runs through the player's capsule, which would otherwise block every interactable below.
            Collider nearest = null;
            float nearestDistance = float.MaxValue;
            for (int i = 0; i < count; i++)
            {
                var c = _hits[i].collider;
                if (c.transform.IsChildOf(controller.transform) || _hits[i].distance >= nearestDistance) continue;
                nearest = c;
                nearestDistance = _hits[i].distance;
            }
            if (nearest == null) return null;
            var target = nearest.GetComponentInParent<Interactable>();
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
