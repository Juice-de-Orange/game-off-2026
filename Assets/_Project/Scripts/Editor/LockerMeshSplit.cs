using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameOff.EditorTools
{
    /// <summary>
    /// P135: cuts the closed evidence locker (SM_Counter_Locker) into what the open one needs, so that opening it changes nothing but
    /// the doors' angle: the shell (everything except the doors and the panel behind them, plus a frame around the opening where that
    /// panel was) and the two door leaves with their handles and lock, each relative to its hinge and closed with a back face.
    /// Positions, normals, UVs and lightmap UVs come from the model, so the same material and the same baked lightmap fit.
    /// The parts are found by shape, not by name: the doors are the two big pieces in front of the body's front panel.
    /// </summary>
    internal static class LockerMeshSplit
    {
        /// <summary>Mesh data in the model's own space (pivot bottom centre, front toward -Z).</summary>
        internal sealed class Source
        {
            public readonly Vector3[] Positions, Normals;
            public readonly Vector2[] Uv, Uv2; // Uv2 may be null (no lightmap UVs)
            public readonly int[] Triangles;

            public Source(Vector3[] positions, Vector3[] normals, Vector2[] uv, Vector2[] uv2, int[] triangles)
            {
                Positions = positions; Normals = normals; Uv = uv; Uv2 = uv2; Triangles = triangles;
            }
        }

        /// <summary>One mesh in the making.</summary>
        internal sealed class Part
        {
            public readonly List<Vector3> Positions = new List<Vector3>();
            public readonly List<Vector3> Normals = new List<Vector3>();
            public readonly List<Vector2> Uv = new List<Vector2>();
            public readonly List<Vector2> Uv2 = new List<Vector2>();
            public readonly List<int> Triangles = new List<int>();
            private readonly Dictionary<int, int> _fromSource = new Dictionary<int, int>();

            public int Add(Vector3 position, Vector3 normal, Vector2 uv, Vector2 uv2)
            {
                Positions.Add(position);
                Normals.Add(normal);
                Uv.Add(uv);
                Uv2.Add(uv2);
                return Positions.Count - 1;
            }

            /// <summary>A vertex of the source, shared by all its triangles in this part, moved by -offset.</summary>
            public int AddSource(Source s, int index, Vector3 offset)
            {
                if (_fromSource.TryGetValue(index, out int existing)) return existing;
                int added = Add(s.Positions[index] - offset, s.Normals[index], s.Uv[index], s.Uv2 != null ? s.Uv2[index] : Vector2.zero);
                _fromSource[index] = added;
                return added;
            }

            public void Fill(Mesh mesh, bool withUv2)
            {
                mesh.Clear();
                mesh.SetVertices(Positions);
                mesh.SetNormals(Normals);
                mesh.SetUVs(0, Uv);
                if (withUv2) mesh.SetUVs(1, Uv2);
                mesh.SetTriangles(Triangles, 0);
                mesh.RecalculateBounds();
            }
        }

        internal sealed class Result
        {
            public readonly Part Shell = new Part(), LeftDoor = new Part(), RightDoor = new Part();
            /// <summary>Outer front corner of each leaf on the floor; the door parts are relative to these.</summary>
            public Vector3 LeftHinge, RightHinge;
            /// <summary>Z of the body's front face (the plane of the opening) and of its back.</summary>
            public float FrontZ, BackZ;
            /// <summary>The opening in the frame (x/y), where the interior shows.</summary>
            public Rect Opening;
            public bool HasUv2;
        }

        /// <summary>The first mesh of the model, in the model's root space. Null if it has none.</summary>
        internal static Source Read(GameObject model)
        {
            var filter = model != null ? model.GetComponentInChildren<MeshFilter>(true) : null;
            if (filter == null || filter.sharedMesh == null) return null;
            var mesh = filter.sharedMesh;
            var toModel = model.transform.worldToLocalMatrix * filter.transform.localToWorldMatrix;
            var positions = mesh.vertices;
            var normals = mesh.normals;
            var uv = mesh.uv;
            var uv2 = mesh.uv2;
            var triangles = mesh.triangles;
            if (positions.Length == 0 || triangles.Length == 0 || normals.Length != positions.Length) return null;
            for (int i = 0; i < positions.Length; i++)
            {
                positions[i] = toModel.MultiplyPoint3x4(positions[i]);
                normals[i] = toModel.MultiplyVector(normals[i]).normalized;
            }
            if (toModel.determinant < 0f) // mirrored: keep the faces facing out
                for (int t = 0; t < triangles.Length; t += 3) (triangles[t + 1], triangles[t + 2]) = (triangles[t + 2], triangles[t + 1]);
            if (uv.Length != positions.Length) uv = new Vector2[positions.Length];
            return new Source(positions, normals, uv, uv2.Length == positions.Length ? uv2 : null, triangles);
        }

        /// <summary>
        /// Cuts the locker. <paramref name="openingInset"/> narrows the opening on both sides beyond the leaves' outer edges, so the
        /// frame covers the gap between shell and interior. Null if the model does not look like a two-door locker.
        /// </summary>
        internal static Result Split(Source s, float openingInset)
        {
            if (s == null || s.Triangles.Length < 3) return null;
            int triangleCount = s.Triangles.Length / 3;
            var whole = BoundsOf(s.Positions);

            // Pieces: triangles that share a corner position belong to the same piece (a door, a handle, the body ...).
            var weld = new Dictionary<Vector3Int, int>();
            var root = new int[s.Positions.Length];
            var weldOf = new int[s.Positions.Length];
            for (int v = 0; v < s.Positions.Length; v++)
            {
                var key = Vector3Int.RoundToInt(s.Positions[v] * 10000f);
                if (!weld.TryGetValue(key, out int id)) weld[key] = id = v;
                weldOf[v] = id;
                root[v] = v;
            }
            for (int t = 0; t < triangleCount; t++)
            {
                Union(root, weldOf[s.Triangles[t * 3]], weldOf[s.Triangles[t * 3 + 1]]);
                Union(root, weldOf[s.Triangles[t * 3]], weldOf[s.Triangles[t * 3 + 2]]);
            }
            var piece = new int[triangleCount];
            for (int t = 0; t < triangleCount; t++) piece[t] = Find(root, weldOf[s.Triangles[t * 3]]);

            // The front panel: big triangles facing -Z across (nearly) the whole front. The doors stand in front of it.
            var panel = new bool[triangleCount];
            var panelPieces = new HashSet<int>();
            int panelTriangle = -1, panelCount = 0;
            float frontZ = 0f;
            for (int t = 0; t < triangleCount; t++)
            {
                Corners(s, t, out var a, out var b, out var c);
                var n = Vector3.Cross(b - a, c - a).normalized;
                var size = BoundsOf(a, b, c).size;
                if (n.z > -0.9f || size.x < 0.9f * whole.size.x || size.y < 0.5f * whole.size.y) continue;
                panel[t] = true;
                panelPieces.Add(piece[t]);
                panelTriangle = t;
                panelCount++;
                frontZ += (a.z + b.z + c.z) / 3f;
            }
            if (panelCount == 0) return null;
            frontZ /= panelCount;

            var pieceBounds = new Dictionary<int, Bounds>();
            for (int t = 0; t < triangleCount; t++)
            {
                Corners(s, t, out var a, out var b, out var c);
                var tb = BoundsOf(a, b, c);
                if (pieceBounds.TryGetValue(piece[t], out var pb)) { pb.Encapsulate(tb); pieceBounds[piece[t]] = pb; }
                else pieceBounds[piece[t]] = tb;
            }

            // In front of the panel: the two leaves are the biggest pieces, one each side of the middle.
            var inFront = pieceBounds.Where(p => !panelPieces.Contains(p.Key) && p.Value.center.z < frontZ - 0.001f).ToList();
            var leaves = inFront.OrderByDescending(p => p.Value.size.x * p.Value.size.y).Take(2).OrderBy(p => p.Value.center.x).ToList();
            if (leaves.Count < 2) return null;
            var left = leaves[0].Value;
            var right = leaves[1].Value;
            float minLeafArea = 0.15f * whole.size.x * whole.size.y;
            if (left.center.x >= 0f || right.center.x <= 0f || left.size.x * left.size.y < minLeafArea || right.size.x * right.size.y < minLeafArea)
                return null;

            // What hangs on the doors (handles, hasp, padlock) goes with the door it sits on; the label above them stays on the shell.
            var doors = left;
            doors.Encapsulate(right);
            float middle = (left.max.x + right.min.x) * 0.5f;
            var owner = new Dictionary<int, int> { [leaves[0].Key] = 1, [leaves[1].Key] = 2 };
            foreach (var p in inFront)
            {
                if (owner.ContainsKey(p.Key)) continue;
                var centre = p.Value.center;
                if (centre.x < doors.min.x || centre.x > doors.max.x || centre.y < doors.min.y || centre.y > doors.max.y) continue;
                owner[p.Key] = centre.x < middle ? 1 : 2;
            }

            var result = new Result
            {
                LeftHinge = new Vector3(left.min.x, 0f, left.min.z),
                RightHinge = new Vector3(right.max.x, 0f, right.min.z),
                FrontZ = frontZ,
                BackZ = whole.max.z,
                Opening = Rect.MinMaxRect(doors.min.x + openingInset, doors.min.y, doors.max.x - openingInset, doors.max.y),
                HasUv2 = s.Uv2 != null,
            };
            for (int t = 0; t < triangleCount; t++)
            {
                if (panel[t]) continue;
                owner.TryGetValue(piece[t], out int side);
                var part = side == 1 ? result.LeftDoor : side == 2 ? result.RightDoor : result.Shell;
                var offset = side == 1 ? result.LeftHinge : side == 2 ? result.RightHinge : Vector3.zero;
                for (int k = 0; k < 3; k++) part.Triangles.Add(part.AddSource(s, s.Triangles[t * 3 + k], offset));
            }

            // The frame where the panel was: around the opening, and the slit between the leaves. Same UVs as the panel there.
            var panelBounds = BoundsOf(Enumerable.Range(0, triangleCount).Where(t => panel[t]).SelectMany(t => Enumerable.Range(0, 3).Select(k => s.Positions[s.Triangles[t * 3 + k]])));
            float x0 = panelBounds.min.x, x1 = panelBounds.max.x, y0 = panelBounds.min.y, y1 = panelBounds.max.y;
            var o = result.Opening;
            FrontQuad(result.Shell, s, panelTriangle, x0, x1, o.yMax, y1, frontZ);
            FrontQuad(result.Shell, s, panelTriangle, x0, x1, y0, o.yMin, frontZ);
            FrontQuad(result.Shell, s, panelTriangle, x0, o.xMin, o.yMin, o.yMax, frontZ);
            FrontQuad(result.Shell, s, panelTriangle, o.xMax, x1, o.yMin, o.yMax, frontZ);
            FrontQuad(result.Shell, s, panelTriangle, left.max.x, right.min.x, o.yMin, o.yMax, frontZ);

            // The leaves had no back (it was against the panel): the inside of each door, plain metal like the body's side,
            // with the lightmap of its front.
            var plain = SideUvRect(s, panel, owner, piece);
            BackFace(result.LeftDoor, s, left, result.LeftHinge, plain, LargestFront(s, piece, leaves[0].Key));
            BackFace(result.RightDoor, s, right, result.RightHinge, plain, LargestFront(s, piece, leaves[1].Key));
            return result;
        }

        private static int Find(int[] root, int i)
        {
            while (root[i] != i) i = root[i] = root[root[i]];
            return i;
        }

        private static void Union(int[] root, int a, int b)
        {
            a = Find(root, a);
            b = Find(root, b);
            if (a != b) root[b] = a;
        }

        private static void Corners(Source s, int t, out Vector3 a, out Vector3 b, out Vector3 c)
        {
            a = s.Positions[s.Triangles[t * 3]];
            b = s.Positions[s.Triangles[t * 3 + 1]];
            c = s.Positions[s.Triangles[t * 3 + 2]];
        }

        private static Bounds BoundsOf(params Vector3[] points) => BoundsOf((IEnumerable<Vector3>)points);

        private static Bounds BoundsOf(IEnumerable<Vector3> points)
        {
            bool any = false;
            var bounds = new Bounds();
            foreach (var p in points)
            {
                if (any) bounds.Encapsulate(p);
                else { bounds = new Bounds(p, Vector3.zero); any = true; }
            }
            return bounds;
        }

        /// <summary>Weights of p in the triangle a, b, c (x/y only); also outside it, so a flat face's UVs extend linearly.</summary>
        private static Vector3 Barycentric(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
        {
            Vector2 v0 = b - a, v1 = c - a, v2 = p - a;
            float d00 = Vector2.Dot(v0, v0), d01 = Vector2.Dot(v0, v1), d11 = Vector2.Dot(v1, v1);
            float d20 = Vector2.Dot(v2, v0), d21 = Vector2.Dot(v2, v1);
            float denominator = d00 * d11 - d01 * d01;
            if (Mathf.Abs(denominator) < 1e-12f) return new Vector3(1f, 0f, 0f);
            float v = (d11 * d20 - d01 * d21) / denominator;
            float w = (d00 * d21 - d01 * d20) / denominator;
            return new Vector3(1f - v - w, v, w);
        }

        /// <summary>UV (channel 0 or 1) of the flat source triangle t, carried over to the point (x, y) of its plane.</summary>
        private static Vector2 MapUv(Source s, int t, float x, float y, bool lightmap)
        {
            if (t < 0) return Vector2.zero;
            var uvs = lightmap ? s.Uv2 : s.Uv;
            if (uvs == null) return Vector2.zero;
            int ia = s.Triangles[t * 3], ib = s.Triangles[t * 3 + 1], ic = s.Triangles[t * 3 + 2];
            Vector3 pa = s.Positions[ia], pb = s.Positions[ib], pc = s.Positions[ic];
            var w = Barycentric(new Vector2(x, y), new Vector2(pa.x, pa.y), new Vector2(pb.x, pb.y), new Vector2(pc.x, pc.y));
            return uvs[ia] * w.x + uvs[ib] * w.y + uvs[ic] * w.z;
        }

        /// <summary>A rectangle facing -Z at depth z, its UVs taken from the source triangle t of the same plane.</summary>
        private static void FrontQuad(Part part, Source s, int t, float x0, float x1, float y0, float y1, float z)
        {
            if (x1 - x0 < 1e-4f || y1 - y0 < 1e-4f) return;
            int b = part.Positions.Count;
            foreach (var (x, y) in new[] { (x0, y0), (x1, y0), (x0, y1), (x1, y1) })
                part.Add(new Vector3(x, y, z), Vector3.back, MapUv(s, t, x, y, false), MapUv(s, t, x, y, true));
            part.Triangles.AddRange(new[] { b, b + 3, b + 1, b + 3, b, b + 2 }); // same winding as Unity's quad: faces -Z
        }

        /// <summary>The leaf's inside: a rectangle facing +Z at its back, plain metal, with the lightmap of the front (triangle front).</summary>
        private static void BackFace(Part part, Source s, Bounds leaf, Vector3 hinge, Rect plain, int front)
        {
            int b = part.Positions.Count;
            foreach (var (x, y) in new[] { (leaf.min.x, leaf.min.y), (leaf.max.x, leaf.min.y), (leaf.min.x, leaf.max.y), (leaf.max.x, leaf.max.y) })
            {
                var uv = new Vector2(Mathf.Lerp(plain.xMin, plain.xMax, Mathf.InverseLerp(leaf.min.x, leaf.max.x, x)),
                    Mathf.Lerp(plain.yMin, plain.yMax, Mathf.InverseLerp(leaf.min.y, leaf.max.y, y)));
                part.Add(new Vector3(x, y, leaf.max.z) - hinge, Vector3.forward, uv, MapUv(s, front, x, y, true));
            }
            part.Triangles.AddRange(new[] { b, b + 1, b + 3, b + 3, b + 2, b }); // reversed: faces +Z
        }

        /// <summary>The biggest triangle of the piece that faces -Z (a leaf's front), or -1.</summary>
        private static int LargestFront(Source s, int[] piece, int key)
        {
            int best = -1;
            float bestArea = 0f;
            for (int t = 0; t < piece.Length; t++)
            {
                if (piece[t] != key) continue;
                Corners(s, t, out var a, out var b, out var c);
                var cross = Vector3.Cross(b - a, c - a);
                if (cross.normalized.z > -0.9f || cross.magnitude <= bestArea) continue;
                best = t;
                bestArea = cross.magnitude;
            }
            return best;
        }

        /// <summary>UV rectangle of the body's biggest side face (plain painted metal); the panel's if there is none.</summary>
        private static Rect SideUvRect(Source s, bool[] panel, Dictionary<int, int> owner, int[] piece)
        {
            int best = -1;
            float bestArea = 0f;
            for (int t = 0; t < piece.Length; t++)
            {
                if (owner.ContainsKey(piece[t])) continue;
                Corners(s, t, out var a, out var b, out var c);
                var cross = Vector3.Cross(b - a, c - a);
                if (Mathf.Abs(cross.normalized.x) < 0.9f || cross.magnitude <= bestArea) continue;
                best = t;
                bestArea = cross.magnitude;
            }
            if (best < 0) best = System.Array.IndexOf(panel, true);
            if (best < 0) return new Rect(0f, 0f, 0f, 0f);
            Vector2 ua = s.Uv[s.Triangles[best * 3]], ub = s.Uv[s.Triangles[best * 3 + 1]], uc = s.Uv[s.Triangles[best * 3 + 2]];
            return Rect.MinMaxRect(Mathf.Min(ua.x, ub.x, uc.x), Mathf.Min(ua.y, ub.y, uc.y), Mathf.Max(ua.x, ub.x, uc.x), Mathf.Max(ua.y, ub.y, uc.y));
        }
    }
}
