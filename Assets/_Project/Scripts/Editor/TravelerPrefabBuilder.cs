using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace GameOff.EditorTools
{
    /// <summary>
    /// Makes the delivered traveler models usable in the game (P40): one Animator Controller per species with the clips of its
    /// FBX (Idle, Walk, Talk), and a prefab PF_NPC_&lt;Species&gt;. Idempotent; run again after a new delivery.
    /// </summary>
    public static class TravelerPrefabBuilder
    {
        public const string PrefabFolder = "Assets/_Project/Prefabs/Characters/";
        public static readonly string[] Species = { "Human", "Vellani", "Quobb", "Xirrik", "Sorvak" };
        public static readonly string[] States = { "Idle", "Talk", "Walk" };

        private const string ModelFolder = "Assets/_Project/Art/Models/Characters/";
        private const string ControllerFolder = "Assets/_Project/Art/Animations/Characters/";

        [MenuItem("GameOff/Build Travelers")]
        public static void BuildFromMenu() => Debug.Log(Build());

        public static string Build()
        {
            var log = new List<string>();
            foreach (string species in System.Linq.Enumerable.Concat(Species, new[] { "Guard" }))
            {
                string fbx = ModelFolder + $"SK_NPC_{species}.fbx";
                var model = AssetDatabase.LoadAssetAtPath<GameObject>(fbx);
                if (model == null && species == "Guard") continue; // P104: ordered, a human stands in until then
                if (model == null)
                {
                    log.Add($"WARNING no {fbx}");
                    continue;
                }
                var controller = Controller(species, fbx, log);
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(model);
                try
                {
                    var animator = instance.GetComponent<Animator>();
                    if (animator == null) animator = instance.AddComponent<Animator>();
                    animator.runtimeAnimatorController = controller;
                    animator.applyRootMotion = false;
                    animator.cullingMode = AnimatorCullingMode.AlwaysAnimate; // keeps the head bone right for the speech bubble
                    AssetFolders.EnsureForAsset(PrefabFolder + "x");
                    PrefabUtility.SaveAsPrefabAsset(instance, PrefabFolder + $"PF_NPC_{species}.prefab");
                }
                finally { Object.DestroyImmediate(instance); }
                log.Add("PF_NPC_" + species);
            }
            AssetDatabase.SaveAssets();
            return "TravelerPrefabBuilder: " + string.Join(", ", log);
        }

        private static AnimatorController Controller(string species, string fbx, List<string> log)
        {
            string path = ControllerFolder + $"AC_NPC_{species}.controller";
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
            if (controller == null)
            {
                AssetFolders.EnsureForAsset(path);
                controller = AnimatorController.CreateAnimatorControllerAtPath(path);
            }
            var clips = AssetDatabase.LoadAllAssetsAtPath(fbx).OfType<AnimationClip>().Where(c => !c.name.StartsWith("__preview__")).ToList();
            var machine = controller.layers[0].stateMachine;
            foreach (string name in species == "Guard" ? new[] { "Idle", "Walk", "Grab" } : States)
            {
                var clip = clips.FirstOrDefault(c => c.name == name);
                if (clip == null) log.Add($"WARNING {fbx} has no clip {name}");
                var state = machine.states.Select(s => s.state).FirstOrDefault(s => s.name == name) ?? machine.AddState(name);
                state.motion = clip;
                if (name == "Idle") machine.defaultState = state;
            }
            EditorUtility.SetDirty(controller);
            return controller;
        }
    }
}
