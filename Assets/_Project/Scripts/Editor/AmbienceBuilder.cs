using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

namespace GameOff.EditorTools
{
    /// <summary>
    /// Station ambience in Main (P67, ahead of the per-area room sounds of P61), idempotent:
    /// Prefabs/Audio/PF_Ambience.prefab = one 2D looping AudioSource with SFX_Amb_Station_Loop on the SFX group of GameMixer,
    /// so the SFX volume slider applies and the pause menu pauses it (AudioListener.pause). Replaces an older instance in Main.
    /// Menu: GameOff → Build Ambience into Main (opens and saves Main – claim it first).
    /// </summary>
    public static class AmbienceBuilder
    {
        private const string MainScene = "Assets/_Project/Scenes/Main.unity";
        private const string PrefabPath = "Assets/_Project/Prefabs/Audio/PF_Ambience.prefab";
        private const string ClipPath = "Assets/_Project/Audio/SFX/SFX_Amb_Station_Loop.ogg";
        private const string MixerPath = "Assets/_Project/Audio/GameMixer.mixer";
        // A bed under everything else; P61 balances it against the room sounds by ear.
        private const float Volume = 0.35f;

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

            var root = new GameObject("PF_Ambience");
            var source = root.AddComponent<AudioSource>();
            source.clip = clip;
            source.outputAudioMixerGroup = sfx;
            source.loop = true;
            source.playOnAwake = true;
            source.spatialBlend = 0f;
            source.volume = Volume;
            source.priority = 0; // never culled by other sounds

            Directory.CreateDirectory(Path.GetDirectoryName(PrefabPath));
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);

            foreach (var old in scene.GetRootGameObjects().Where(g => g.name == prefab.name)) Object.DestroyImmediate(old);
            PrefabUtility.InstantiatePrefab(prefab, scene);
            EditorSceneManager.MarkSceneDirty(scene);
            return $"[AmbienceBuilder] {clip.name} ({clip.length:0} s) looping in {scene.name}, volume {Volume}" +
                   (sfx == null ? ", WARNING: no SFX group found" : ", SFX group");
        }
    }
}
