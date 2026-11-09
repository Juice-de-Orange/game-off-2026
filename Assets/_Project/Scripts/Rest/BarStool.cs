using GameOff.Player;
using UnityEngine;

namespace GameOff.Rest
{
    /// <summary>
    /// P28: a bar stool. Sitting down opens the barkeeper's conversation (Barkeeper, P65/P93) exactly as talking to him does;
    /// when it closes and gives the controls back, the seat stands the player up again (SeatView.standWhenControlReturns).
    /// </summary>
    public class BarStool : MonoBehaviour
    {
        [SerializeField] private SeatView seat;
        [Tooltip("The barkeeper's own [E] trigger: sitting down uses it.")]
        [SerializeField] private Interactable barkeeper;

        private void OnEnable() => seat.SatDown += Talk;

        private void OnDisable() => seat.SatDown -= Talk;

        private void Talk()
        {
            if (barkeeper != null) barkeeper.Interact();
        }
    }
}
