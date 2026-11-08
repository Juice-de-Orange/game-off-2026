using UnityEngine;

namespace GameOff.Station
{
    /// <summary>P26: turns around its own Y axis (ceiling fans).</summary>
    public class Spinner : MonoBehaviour
    {
        [SerializeField] private float degreesPerSecond = 120f;

        private void Update() => transform.Rotate(0f, degreesPerSecond * Time.deltaTime, 0f, Space.Self);
    }
}
