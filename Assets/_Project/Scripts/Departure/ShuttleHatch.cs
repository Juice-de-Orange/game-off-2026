using System.Collections;
using GameOff.Day;
using GameOff.Player;
using UnityEngine;

namespace GameOff.Departure
{
    /// <summary>
    /// P106: the hatch between the hangar and the docked shuttle. E fades out, moves the player to the other side and fades in. The
    /// delivered cabin has its exit at the short end while the shuttle docks with its long side, so the two are not walked through.
    /// </summary>
    public class ShuttleHatch : MonoBehaviour
    {
        [SerializeField] private Interactable interactable;
        [SerializeField] private FirstPersonController player;
        [SerializeField] private DayFlowUI dialogs;
        [SerializeField] private ShuttleFlight flight;
        [Tooltip("Where the player ends up (position and facing).")]
        [SerializeField] private Transform destination;
        [SerializeField] private float fadeSeconds = 0.3f;

        private bool _moving;

        private void OnEnable() => interactable.OnInteract.AddListener(OnInteract);
        private void OnDisable() => interactable.OnInteract.RemoveListener(OnInteract);

        private void OnInteract()
        {
            if (_moving || player == null || dialogs == null || destination == null || (flight != null && flight.IsFlying)) return;
            StartCoroutine(Pass());
        }

        private IEnumerator Pass()
        {
            _moving = true;
            player.SetControlEnabled(false);
            yield return dialogs.FadeTo(1f, fadeSeconds);
            player.Teleport(destination.position, destination.rotation);
            yield return dialogs.FadeTo(0f, fadeSeconds);
            player.SetControlEnabled(true);
            _moving = false;
        }
    }
}
