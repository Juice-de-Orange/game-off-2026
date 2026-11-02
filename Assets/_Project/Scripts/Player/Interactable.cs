using UnityEngine;
using UnityEngine.Events;

namespace GameOff.Player
{
    /// <summary>
    /// Anything the player can use with E. Needs a Collider on this object or a child.
    /// Wire the effect in the inspector (onInteract) or subclass and override Interact().
    /// </summary>
    public class Interactable : MonoBehaviour
    {
        [Tooltip("Shown as \"[E] <prompt>\", e.g. \"Sleep\".")]
        [SerializeField] private string prompt = "Use";
        [SerializeField] private bool interactionEnabled = true;
        [SerializeField] private UnityEvent onInteract = new UnityEvent();

        public string Prompt => prompt;
        public bool InteractionEnabled => interactionEnabled && isActiveAndEnabled;
        public UnityEvent OnInteract => onInteract;

        public void SetPrompt(string newPrompt) => prompt = newPrompt;
        public void SetInteractionEnabled(bool enabled) => interactionEnabled = enabled;

        public virtual void Interact()
        {
            onInteract.Invoke();
        }
    }
}
