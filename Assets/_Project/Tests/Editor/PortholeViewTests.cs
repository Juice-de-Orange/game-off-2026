using System.Linq;
using GameOff.EditorTools;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace GameOff.Tests
{
    /// <summary>
    /// P152: the porthole's view of the home planet is free on every day: the sky turns a bit further each day (SkyRig.dayYaw),
    /// and from the eye outside room 103 nothing of the station or its exterior stands in the line of sight.
    /// </summary>
    public class PortholeViewTests
    {
        private const string SkyPrefab = "Assets/_Project/Prefabs/Environment/PF_SkyRig.prefab";
        private const string StationPrefab = "Assets/_Project/Prefabs/Station/PF_Station_Graybox.prefab";
        private const string ExteriorPrefab = "Assets/_Project/Prefabs/Environment/PF_Exterior.prefab";
        private const float CheckMeters = 60f;

        [Test]
        public void PlanetIsInFreeView_OnEveryDay()
        {
            var sky = AssetDatabase.LoadAssetAtPath<GameObject>(SkyPrefab);
            var station = AssetDatabase.LoadAssetAtPath<GameObject>(StationPrefab);
            var exterior = AssetDatabase.LoadAssetAtPath<GameObject>(ExteriorPrefab);
            if (sky == null || station == null) Assert.Ignore("PF_SkyRig or PF_Station_Graybox is not built yet");

            // Renderer bounds are only real on instances, not on prefab assets.
            var made = new System.Collections.Generic.List<GameObject>();
            try
            {
                var skyI = (GameObject)PrefabUtility.InstantiatePrefab(sky);
                made.Add(skyI);
                made.Add((GameObject)PrefabUtility.InstantiatePrefab(station));
                if (exterior != null) made.Add((GameObject)PrefabUtility.InstantiatePrefab(exterior));

                var rig = skyI.GetComponent<Core.SkyRig>();
                var planet = skyI.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == "HomePlanet");
                Assert.IsNotNull(planet, "the sky has the home planet");
                var yawProperty = new SerializedObject(rig).FindProperty("dayYaw");
                var renderers = made.Skip(1).SelectMany(g => g.GetComponentsInChildren<Renderer>(true)).ToList();
                Assert.That(renderers.Count(r => r.bounds.size.sqrMagnitude > 0f), Is.GreaterThan(10), "the station's renderers have bounds");
                var eye = RestSpotBuilder.PortholeEye;

                for (int i = 0; i < yawProperty.arraySize; i++)
                {
                    float yaw = yawProperty.GetArrayElementAtIndex(i).floatValue;
                    skyI.transform.rotation = Quaternion.Euler(0f, yaw, 0f); // what SkyRig.SetDay does
                    var ray = new Ray(eye, (planet.position - eye).normalized);
                    foreach (var r in renderers)
                    {
                        var b = r.bounds;
                        if (!b.IntersectRay(ray, out float distance) || distance > CheckMeters) continue;
                        Assert.Fail($"day {i + 1} (sky turned {yaw}°): {r.name} at {b.center} stands {distance:F1} m in front of the eye");
                    }
                }
            }
            finally
            {
                foreach (var g in made) if (g != null) Object.DestroyImmediate(g);
            }
        }
    }
}
