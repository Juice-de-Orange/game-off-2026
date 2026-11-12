using System.Collections.Generic;
using UnityEngine;

namespace GameOff.Documents
{
    /// <summary>
    /// P139: how things lie on the counter desk, as plain math (tested in EditMode). Every thing has a flat footprint (a turned
    /// rectangle on the desk) and a thickness. In pile order (bottom first) each lies one stack step above the highest thing below
    /// it that it overlaps, so what was touched last is on top and nothing sinks into the 1.5 cm bribe envelope.
    /// Papers count as having no thickness: one step apart is enough against z-fighting.
    /// </summary>
    public static class DeskLayout
    {
        /// <summary>A turned rectangle on the desk, in world X/Z.</summary>
        public struct Footprint
        {
            public Vector2 Center;
            /// <summary>Unit direction of the thing's local X on the desk.</summary>
            public Vector2 Right;
            /// <summary>Unit direction of the thing's local Z on the desk.</summary>
            public Vector2 Forward;
            /// <summary>Half extents along Right and Forward.</summary>
            public Vector2 Half;

            public static Footprint Of(Vector3 center, Vector3 right, Vector3 forward, Vector2 half) => new Footprint
            {
                Center = new Vector2(center.x, center.z),
                Right = OnDesk(right, Vector2.right),
                Forward = OnDesk(forward, Vector2.up),
                Half = half,
            };

            private static Vector2 OnDesk(Vector3 direction, Vector2 fallback)
            {
                var flat = new Vector2(direction.x, direction.z);
                return flat.sqrMagnitude > 1e-8f ? flat.normalized : fallback;
            }
        }

        /// <summary>True if the two rectangles overlap; edges that only touch don't count (separating axis test).</summary>
        public static bool Overlap(Footprint a, Footprint b)
        {
            Vector2 d = b.Center - a.Center;
            return !Apart(a.Right, d, a, b) && !Apart(a.Forward, d, a, b) && !Apart(b.Right, d, a, b) && !Apart(b.Forward, d, a, b);
        }

        private static bool Apart(Vector2 axis, Vector2 d, Footprint a, Footprint b) =>
            Mathf.Abs(Vector2.Dot(d, axis)) >= Reach(a, axis) + Reach(b, axis);

        private static float Reach(Footprint f, Vector2 axis) =>
            f.Half.x * Mathf.Abs(Vector2.Dot(f.Right, axis)) + f.Half.y * Mathf.Abs(Vector2.Dot(f.Forward, axis));

        /// <summary>
        /// The bottom height of every thing, in pile order (bottom first): one step above the top of the highest thing below it
        /// that it overlaps, else one step above the desk. Writes into bottoms, which must be at least as long as footprints.
        /// </summary>
        public static void Bottoms(IReadOnlyList<Footprint> footprints, IReadOnlyList<float> thickness, float deskTop, float step, IList<float> bottoms)
        {
            for (int i = 0; i < footprints.Count; i++)
            {
                float under = deskTop;
                for (int j = 0; j < i; j++)
                    if (Overlap(footprints[i], footprints[j])) under = Mathf.Max(under, bottoms[j] + thickness[j]);
                bottoms[i] = under + step;
            }
        }

        /// <summary>A point in the desk area's own space, pulled inside the area (size along its local X and Z); the height stays.</summary>
        public static Vector3 ClampToArea(Vector3 local, Vector2 size) => new Vector3(
            Mathf.Clamp(local.x, -size.x * 0.5f, size.x * 0.5f),
            local.y,
            Mathf.Clamp(local.z, -size.y * 0.5f, size.y * 0.5f));

        /// <summary>A mouse hit on the bare desk top: a surface facing up, at the desk's height (within the tolerance).</summary>
        public static bool OnDeskTop(float hitHeight, float normalUp, float deskTop, float tolerance) =>
            normalUp > 0.9f && Mathf.Abs(hitHeight - deskTop) <= tolerance;
    }
}
