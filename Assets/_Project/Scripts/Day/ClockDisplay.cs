using TMPro;
using UnityEngine;

namespace GameOff.Day
{
    /// <summary>P97: shows GameClock on a wall clock of the station.</summary>
    public class ClockDisplay : MonoBehaviour
    {
        [SerializeField] private TMP_Text text;

        private int _shown = -1;

        private void Update()
        {
            if (_shown == GameClock.Minutes) return;
            _shown = GameClock.Minutes;
            text.text = GameClock.Text;
        }
    }
}
