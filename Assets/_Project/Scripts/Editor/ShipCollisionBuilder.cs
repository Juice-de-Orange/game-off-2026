using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace GameOff.EditorTools
{
    /// <summary>
    /// P158 (#49): the hangar ships get boxes that follow their shape (hull, wings, legs) instead of one box around everything.
    /// The model is cut into columns on the floor plan; each column is solid from its lowest to its highest surface, so the space
    /// under a high wing stays free. Neighbouring columns of about the same height become one box. Built into
    /// Prefabs/Station/PF_HangarShip_*.prefab by EconomyBuilder; HangarShips switches the boxes on when the ship stands.
    /// </summary>
    public static class ShipCollisionBuilder
    {
        public const string PrefabFolder = "Assets/_Project/Prefabs/Station/";
        public const float Cell = 0.4f;
        private const float SampleStep = 0.15f;
        private const float MergeTolerance = 0.3f;

        /// <summary>The ship prefab: the model at the origin and a "Collision" child with the boxes. Returns the saved prefab.</summary>
        public static GameObject BuildPrefab(GameObject model, List<string> log)
        {
            var root = new GameObject("PF_HangarShip_" + model.name.Replace("SM_Hangar_Ship_", ""));
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(model);
            instance.transform.SetParent(root.transform, false);
            foreach (var c in instance.GetComponentsInChildren<Collider>(true)) Object.DestroyImmediate(c);
            var collision = new GameObject("Collision");
            collision.transform.SetParent(root.transform, false);
            var boxes = Boxes(root);
            foreach (var b in boxes)
            {
                var box = collision.AddComponent<BoxCollider>();
                box.center = b.center;
                box.size = b.size;
            }
            Directory.CreateDirectory(PrefabFolder);
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabFolder + root.name + ".prefab");
            Object.DestroyImmediate(root);
            log?.Add($"{prefab.name}: {boxes.Count} collision boxes");
            if (boxes.Count == 0) Debug.LogWarning($"[ShipCollisionBuilder] {model.name}: no readable mesh, the ship gets one fitted box at runtime");
            return prefab;
        }

        /// <summary>Boxes in the root's space that follow the meshes under it; empty if no mesh can be read (HangarShips then fits one box).</summary>
        public static List<Bounds> Boxes(GameObject root)
        {
            var toLocal = root.transform.worldToLocalMatrix;
            var columns = new Dictionary<Vector2Int, Vector2>(); // floor-plan cell -> (lowest, highest) surface
            var all = new Bounds();
            bool any = false;
            foreach (var filter in root.GetComponentsInChildren<MeshFilter>(true))
            {
                var mesh = filter.sharedMesh;
                if (mesh == null) continue;
                var vertices = mesh.vertices;
                var triangles = mesh.triangles;
                if (vertices.Length == 0 || triangles.Length == 0) continue;
                var matrix = toLocal * filter.transform.localToWorldMatrix;
                for (int i = 0; i < vertices.Length; i++) vertices[i] = matrix.MultiplyPoint3x4(vertices[i]);
                for (int t = 0; t + 2 < triangles.Length; t += 3)
                {
                    Vector3 a = vertices[triangles[t]], b = vertices[triangles[t + 1]], c = vertices[triangles[t + 2]];
                    float longest = Mathf.Max((b - a).magnitude, (c - b).magnitude, (a - c).magnitude);
                    int steps = Mathf.Clamp(Mathf.CeilToInt(longest / SampleStep), 1, 200);
                    for (int i = 0; i <= steps; i++)
                        for (int j = 0; j <= steps - i; j++)
                        {
                            var p = a + (b - a) * ((float)i / steps) + (c - a) * ((float)j / steps);
                            var cell = new Vector2Int(Mathf.FloorToInt(p.x / Cell), Mathf.FloorToInt(p.z / Cell));
                            columns[cell] = columns.TryGetValue(cell, out var range) ? new Vector2(Mathf.Min(range.x, p.y), Mathf.Max(range.y, p.y)) : new Vector2(p.y, p.y);
                            if (any) all.Encapsulate(p);
                            else { all = new Bounds(p, Vector3.zero); any = true; }
                        }
                }
            }
            if (columns.Count == 0) return new List<Bounds>();

            // Runs along Z in each X column, then equal runs of neighbouring X columns into one box.
            var runs = new List<(int x, int z0, int z1, float low, float high)>();
            var byX = new SortedDictionary<int, List<int>>();
            foreach (var cell in columns.Keys)
            {
                if (!byX.TryGetValue(cell.x, out var list)) byX[cell.x] = list = new List<int>();
                list.Add(cell.y);
            }
            foreach (var pair in byX)
            {
                pair.Value.Sort();
                bool open = false;
                int z0 = 0, z1 = 0;
                float low = 0f, high = 0f;
                Vector2 anchor = Vector2.zero; // the run's first column: a slope must not creep on cell by cell
                foreach (int z in pair.Value)
                {
                    var range = columns[new Vector2Int(pair.Key, z)];
                    if (open && z == z1 + 1 && Mathf.Abs(range.x - anchor.x) < MergeTolerance && Mathf.Abs(range.y - anchor.y) < MergeTolerance)
                    {
                        z1 = z;
                        low = Mathf.Min(low, range.x);
                        high = Mathf.Max(high, range.y);
                        continue;
                    }
                    if (open) runs.Add((pair.Key, z0, z1, low, high));
                    open = true;
                    z0 = z1 = z;
                    low = range.x;
                    high = range.y;
                    anchor = range;
                }
                if (open) runs.Add((pair.Key, z0, z1, low, high));
            }

            // Across X compared with the box's first run, for the same reason.
            var boxes = new List<(int x0, int x1, int z0, int z1, float low, float high, float aLow, float aHigh)>();
            foreach (var r in runs)
            {
                int k = boxes.FindIndex(b => b.x1 == r.x - 1 && b.z0 == r.z0 && b.z1 == r.z1
                                             && Mathf.Abs(b.aLow - r.low) < MergeTolerance && Mathf.Abs(b.aHigh - r.high) < MergeTolerance);
                if (k >= 0)
                {
                    var b = boxes[k];
                    boxes[k] = (b.x0, r.x, b.z0, b.z1, Mathf.Min(b.low, r.low), Mathf.Max(b.high, r.high), b.aLow, b.aHigh);
                }
                else boxes.Add((r.x, r.x, r.z0, r.z1, r.low, r.high, r.low, r.high));
            }

            var result = new List<Bounds>();
            foreach (var b in boxes)
            {
                // Cells are clipped to the model so the outermost boxes don't stick out by up to a cell.
                float xMin = Mathf.Max(b.x0 * Cell, all.min.x), xMax = Mathf.Min((b.x1 + 1) * Cell, all.max.x);
                float zMin = Mathf.Max(b.z0 * Cell, all.min.z), zMax = Mathf.Min((b.z1 + 1) * Cell, all.max.z);
                float low = b.low, high = Mathf.Max(b.high, b.low + 0.1f); // a flat skin still gets some thickness
                var min = new Vector3(xMin, low, zMin);
                var max = new Vector3(Mathf.Max(xMax, xMin + 0.05f), high, Mathf.Max(zMax, zMin + 0.05f));
                result.Add(new Bounds((min + max) * 0.5f, max - min));
            }
            return result;
        }
    }
}
