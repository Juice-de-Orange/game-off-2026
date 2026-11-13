using System.Collections;
using UnityEngine;

namespace GameOff.Counter
{
    /// <summary>
    /// P91: the steel shutter in front of the counter window. It drops when someone is arrested, so the player only hears what
    /// happens behind it (since P105 the airlock happens in view, without the shutter). The kiosk's shutter is one too. Hidden while open.
    /// </summary>
    public class SecurityShutter : MonoBehaviour
    {
        [Tooltip("The part that moves (model or placeholder slats); local Y = height of its top edge.")]
        [SerializeField] private Transform slats;
        [SerializeField] private float closedHeight = 2.8f;
        [SerializeField] private float openHeight = 5f;

        private Coroutine _moving;

        private void Awake() => SetHeight(openHeight);

        public void Drop(float seconds) => MoveTo(closedHeight, seconds);

        public void Raise(float seconds) => MoveTo(openHeight, seconds);

        /// <summary>Open at once (a new day).</summary>
        public void ResetOpen()
        {
            if (_moving != null) StopCoroutine(_moving);
            _moving = null;
            SetHeight(openHeight);
        }

        private void MoveTo(float height, float seconds)
        {
            if (_moving != null) StopCoroutine(_moving);
            slats.gameObject.SetActive(true);
            _moving = StartCoroutine(Move(height, seconds));
        }

        private IEnumerator Move(float height, float seconds)
        {
            float from = slats.localPosition.y;
            for (float t = 0f; t < seconds; t += Time.deltaTime)
            {
                float k = t / seconds;
                SetHeightKeepActive(Mathf.Lerp(from, height, height < from ? k * k : k)); // falls faster and faster, rises evenly
                yield return null;
            }
            SetHeight(height);
            _moving = null;
        }

        private void SetHeight(float height)
        {
            SetHeightKeepActive(height);
            slats.gameObject.SetActive(!Mathf.Approximately(height, openHeight));
        }

        private void SetHeightKeepActive(float height)
        {
            var p = slats.localPosition;
            slats.localPosition = new Vector3(p.x, height, p.z);
        }
    }
}
