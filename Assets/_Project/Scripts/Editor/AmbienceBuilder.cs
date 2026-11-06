using System.IO;
using System.Linq;
using GameOff.Audio;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

namespace GameOff.EditorTools
{
    /// <summary>
    /// Station ambience in Main (P67, ahead of the per-area room sounds of P61), idempotent:
    /// Prefabs/Audio/PF_Ambience.prefab = AmbienceZones with three 2D looping AudioSources (station, bar, hangar) on the SFX group,
    /// so the SFX volume slider applies and the pause menu pauses it (AudioListener.pause). Replaces an older instance in Main.
    /// Menu: GameOff → Build Ambience into Main (opens and saves Main – claim it first).
    /// </summary>
    public static class AmbienceBuilder
    {
        private const string MainScene = "Assets/_Project/Scenes/Main.unity";
        private const string PrefabPath = "Assets/_Project/Prefabs/Audio/PF_Ambience.prefab";
        private const string ClipPath = "Assets/_Project/Audio/SFX/SFX_Amb_Station_Loop.ogg";
        private const string BarClipPath = "Assets/_Project/Audio/SFX/SFX_Amb_Bar_Loop.ogg";
        private const string HangarClipPath = "Assets/_Project/Audio/SFX/SFX_Amb_Hangar_Loop.ogg";
        private const string MixerPath = "Assets/_Project/Audio/GameMixer.mixer";

        [MenuItem("GameOff/Build Ambience into Main")]
        public static void BuildIntoMainFromMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var scene = EditorSceneManager.OpenScene(MainScene, OpenSceneMode.Single);
            Debug.Log(Build(scene));
            EditorSceneManager.SaveScene(scene);
        }

        public static string Build(Scene scene)
        {
            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(ClipPath);
            if (clip == null) return "[AmbienceBuilder] " + ClipPath + " missing - nothing built";
            var mixer = AssetDatabase.LoadAssetAtPath<AudioMixer>(MixerPath);
            var sfx = mixer != null ? mixer.FindMatchingGroups("SFX").FirstOrDefault() : null;

            var tuning = SoundSetBuilder.Build();
            var barClip = AssetDatabase.LoadAssetAtPath<AudioClip>(BarClipPath);
            var hangarClip = AssetDatabase.LoadAssetAtPath<AudioClip>(HangarClipPath);

            var root = new GameObject("PF_Ambience");
            AudioSource Source(string name, AudioClip sourceClip, float volume)
            {
                var go = new GameObject(name);
                go.transform.SetParent(root.transform, false);
                var source = go.AddComponent<AudioSource>();
                source.clip = sourceClip;
                source.outputAudioMixerGroup = sfx;
                source.loop = true;
                source.playOnAwake = true;
                source.spatialBlend = 0f;
                source.volume = volume;
                source.priority = 0; // never culled by other sounds
                return source;
            }
            var zones = root.AddComponent<AmbienceZones>();
            var prefabLink = new SerializedObject(zones);
            prefabLink.FindProperty("station").objectReferenceValue = Source("Station", clip, tuning.ambienceVolume);
            prefabLink.FindProperty("bar").objectReferenceValue = Source("Bar", barClip, 0f);
            prefabLink.FindProperty("hangar").objectReferenceValue = Source("Hangar", hangarClip, 0f);
            prefabLink.FindProperty("barArea").boundsValue = StationGrayboxBuilder.RoomBounds("Bar");
            prefabLink.FindProperty("hangarArea").boundsValue = StationGrayboxBuilder.RoomBounds("Hangar");
            prefabLink.FindProperty("tuning").objectReferenceValue = tuning;
            prefabLink.ApplyModifiedPropertiesWithoutUndo();

            Directory.CreateDirectory(Path.GetDirectoryName(PrefabPath));
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);

            foreach (var old in scene.GetRootGameObjects().Where(g => g.name == prefab.name)) Object.DestroyImmediate(old);
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
            // Follows the player in this scene; the prefab can't hold that reference.
            var player = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<GameOff.Player.FirstPersonController>(true)).FirstOrDefault();
            var sceneLink = new SerializedObject(instance.GetComponent<AmbienceZones>());
            sceneLink.FindProperty("listener").objectReferenceValue = player != null ? player.transform : null;
            sceneLink.ApplyModifiedPropertiesWithoutUndo();
            EditorSceneManager.MarkSceneDirty(scene);
            string warnings = (sfx == null ? ", WARNING: no SFX group found" : ", SFX group")
                + (player == null ? ", WARNING: no player, the room sound stays on the hum" : "")
                + (barClip == null || hangarClip == null ? ", WARNING: bar or hangar clip missing" : "");
            return $"[AmbienceBuilder] {clip.name} ({clip.length:0} s) with bar and hangar rooms in {scene.name}, volume {tuning.ambienceVolume:0.##}{warnings}";
        }
    }
}
