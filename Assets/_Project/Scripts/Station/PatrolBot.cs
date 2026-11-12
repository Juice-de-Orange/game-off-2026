using GameOff.Audio;
using UnityEngine;

namespace GameOff.Station
{
    /// <summary>
    /// P26: the cleaning bot drives a loop through its waypoints and turns on the spot at each one (#31).
    /// P137: its brushes hum as a quiet 3D loop that is only heard close by, and it beeps when it turns (#39).
    /// </summary>
    public class PatrolBot : MonoBehaviour
    {
        [SerializeField] private Vector3[] route;
        [SerializeField] private float speed = 0.35f;
        [SerializeField] private float turnSeconds = 1.2f;
        [Tooltip("P137: the looping cleaning sound at the bot (3D, short linear range; set up by the station builder).")]
        [SerializeField] private AudioSource loop;
        [Tooltip("P137: clip and volume of the loop (cleaningBotLoop); the turn beep is cleaningBotTurn.")]
        [SerializeField] private SoundSet sounds;
        [Tooltip("P137: the turn beep fades out over this distance.")]
        [SerializeField] private float beepDistance = 7f;

        private int _next;
        private float _turning;

        private void Start()
        {
            if (loop == null || sounds == null || sounds.cleaningBotLoop.clip == null) return;
            if (loop.outputAudioMixerGroup == null) loop.outputAudioMixerGroup = AudioManager.SfxGroup;
            loop.clip = sounds.cleaningBotLoop.clip;
            loop.volume = sounds.cleaningBotLoop.volume;
            loop.loop = true;
            loop.Play();
        }

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
            AudioManager.PlayAt(s => s.cleaningBotTurn, transform.position, beepDistance);
        }
    }
}
