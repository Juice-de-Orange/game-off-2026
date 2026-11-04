using System;
using UnityEngine;

namespace GameOff.Counter
{
    /// <summary>
    /// Something the officer can stamp (a document). Needs a Collider. Ink marks become children, so they move with the
    /// document; keep this object unscaled (scale a visual child instead), or rotated marks get sheared.
    /// P06 puts this on the real documents; for now a placeholder sheet on the desk carries it.
    /// </summary>
    public class StampTarget : MonoBehaviour
    {
        private const float MarkLift = 0.001f; // above the surface, against z-fighting

        /// <summary>Raised after a mark was placed (for the document's own look). Game decisions: CounterStation.ActionTriggered.</summary>
        public event Action<CounterAction> Stamped;

        public bool IsStamped { get; private set; }
        public CounterAction LastStamp { get; private set; }

        /// <summary>
        /// Places an ink mark at the hit point, facing out of the surface. textUp: the direction the top of the text should
        /// point to (the viewer's up), so the mark reads upright for whoever stamps it.
        /// </summary>
        public void Stamp(CounterAction action, Material ink, Vector3 point, Vector3 normal, Vector3 textUp, float size, float tilt)
        {
            if (ink != null)
            {
                var mark = GameObject.CreatePrimitive(PrimitiveType.Quad);
                mark.name = $"Ink_{action}";
                Destroy(mark.GetComponent<Collider>());
                mark.GetComponent<MeshRenderer>().sharedMaterial = ink;
                // A quad shows its face toward local -Z: look along -normal so the face points out of the surface.
                // Text up in the surface plane; falls back to the document's axes if the viewer looks along the surface.
                Vector3 up = Vector3.ProjectOnPlane(textUp, normal);
                if (up.sqrMagnitude < 0.01f) up = Mathf.Abs(Vector3.Dot(transform.forward, normal)) > 0.9f ? transform.up : transform.forward;
                var rotation = Quaternion.LookRotation(-normal, up) * Quaternion.Euler(0f, 0f, UnityEngine.Random.Range(-tilt, tilt));
                mark.transform.SetPositionAndRotation(point + normal * MarkLift, rotation);
                mark.transform.localScale = Vector3.one * size;
                mark.transform.SetParent(transform, true);
            }
            IsStamped = true;
            LastStamp = action;
            Stamped?.Invoke(action);
        }
    }
}
