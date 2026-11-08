using System.Collections.Generic;
using UnityEngine;

namespace GameOff.Travelers
{
    /// <summary>
    /// P105: turns a skinned figure into a ragdoll at runtime, for any skeleton. There are no colliders: the figure is blown out of the
    /// lock in a straight line and must not catch on the hatch frame. Without bones the whole figure becomes one body.
    /// </summary>
    public static class Ragdoll
    {
        public static List<Rigidbody> Build(GameObject figure)
        {
            var animator = figure.GetComponent<Animator>();
            if (animator != null) animator.enabled = false;
            var bodies = new Dictionary<Transform, Rigidbody>();
            foreach (var skin in figure.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                skin.updateWhenOffscreen = true; // its bounds no longer follow an animation
                foreach (var bone in skin.bones)
                    if (bone != null && !bodies.ContainsKey(bone)) bodies[bone] = Body(bone.gameObject);
            }
            if (bodies.Count == 0) bodies[figure.transform] = Body(figure);
            foreach (var pair in bodies)
            {
                var parent = pair.Key.parent;
                while (parent != null && parent != figure.transform && !bodies.ContainsKey(parent)) parent = parent.parent;
                if (parent == null || !bodies.TryGetValue(parent, out var parentBody)) continue;
                var joint = pair.Key.gameObject.AddComponent<CharacterJoint>();
                joint.connectedBody = parentBody;
            }
            return new List<Rigidbody>(bodies.Values);
        }

        private static Rigidbody Body(GameObject go)
        {
            var body = go.AddComponent<Rigidbody>();
            body.useGravity = false;
            body.mass = 1f;
            body.linearDamping = 0.05f;
            body.angularDamping = 0.4f;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            return body;
        }
    }
}
