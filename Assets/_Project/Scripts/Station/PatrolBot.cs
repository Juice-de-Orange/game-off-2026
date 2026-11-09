using UnityEngine;

namespace GameOff.Station
{
    /// <summary>P26: the cleaning bot drives a loop through its waypoints and turns on the spot at each one (#31).</summary>
    public class PatrolBot : MonoBehaviour
    {
        [SerializeField] private Vector3[] route;
        [SerializeField] private float speed = 0.35f;
        [SerializeField] private float turnSeconds = 1.2f;

        private int _next;
        private float _turning;

        private void Update()
        {
            if (route == null || route.Length == 0) return;
            Vector3 target = route[_next];
            var heading = target - transform.position;
            if (heading.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(heading), 180f / turnSeconds * Time.deltaTime);
            if (_turning > 0f)
            {
                _turning -= Time.deltaTime;
                return;
            }
            transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
            if ((transform.position - target).sqrMagnitude > 0.0001f) return;
            _next = (_next + 1) % route.Length;
            _turning = turnSeconds;
        }
    }
}
