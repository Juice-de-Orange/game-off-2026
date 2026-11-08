using UnityEngine;

namespace GameOff.Station
{
    /// <summary>P26: the cleaning bot drives back and forth along a straight line and turns around at both ends.</summary>
    public class PatrolBot : MonoBehaviour
    {
        [SerializeField] private Vector3 pointA;
        [SerializeField] private Vector3 pointB;
        [SerializeField] private float speed = 0.35f;
        [SerializeField] private float turnSeconds = 1.2f;

        private bool _toB = true;
        private float _turning;

        private void Update()
        {
            Vector3 target = _toB ? pointB : pointA;
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
            _toB = !_toB;
            _turning = turnSeconds;
        }
    }
}
