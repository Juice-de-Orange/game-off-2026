using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace GameOff.EditorTools
{
    /// <summary>
    /// Makes the delivered traveler models usable in the game (P40): one Animator Controller per species with the clips of its
    /// FBX (Idle, Walk, Talk), and a prefab PF_NPC_&lt;Species&gt;. P145: the travelers' controllers also get "Sit" from the shared
    /// A_NPC_Sit.fbx (the evening guests on benches and bar stools). Idempotent; run again after a new delivery.
    /// </summary>
    public static class TravelerPrefabBuilder
    {
        public const string PrefabFolder = "Assets/_Project/Prefabs/Characters/";
        public static readonly string[] Species = { "Human", "Vellani", "Quobb", "Xirrik", "Sorvak" };
        public static readonly string[] States = { "Idle", "Talk", "Walk" };
        public const string SitState = "Sit";
        public const string ControllerFolder = "Assets/_Project/Art/Animations/Characters/";
        public const string SitClipPath = ControllerFolder + "A_NPC_Sit.fbx";

        private const string ModelFolder = "Assets/_Project/Art/Models/Characters/";

        [MenuItem("GameOff/Build Travelers")]
        public static void BuildFromMenu() => Debug.Log(Build());

        public static string Build()
        {
            var log = new List<string>();
            var sit = SitClip(log);
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
                var controller = Controller(species, fbx, species == "Guard" ? null : sit, log);
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

        /// <summary>
        /// P145: A_NPC_Sit.fbx (animation only, on the skeleton of SK_NPC_Human) as a humanoid with its own avatar, like the SK_
        /// models (DeliveryIntegrator), with one looping clip that plays in place exactly as authored: root rotation, height and
        /// position baked into the pose from the original, so the pelvis stays over the figure's origin at the clip's seat height
        /// (EveningCrowd lifts the figure onto higher seats). Null if the file or its clip is missing.
        /// </summary>
        private static AnimationClip SitClip(List<string> log)
        {
            if (!(AssetImporter.GetAtPath(SitClipPath) is ModelImporter importer))
            {
                log.Add($"WARNING no {SitClipPath}: evening guests only stand");
                return null;
            }
            if (importer.animationType != ModelImporterAnimationType.Human)
            {
                importer.animationType = ModelImporterAnimationType.Human;
                importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
                importer.SaveAndReimport(); // the takes come with the humanoid import
                log.Add("A_NPC_Sit: Humanoid, avatar from this model");
            }
            var clips = importer.clipAnimations.Length > 0 ? importer.clipAnimations : importer.defaultClipAnimations;
            bool changed = importer.clipAnimations.Length == 0;
            foreach (var clip in clips)
            {
                if (clip.loopTime && clip.lockRootRotation && clip.keepOriginalOrientation && clip.lockRootHeightY && clip.keepOriginalPositionY
                    && clip.lockRootPositionXZ && clip.keepOriginalPositionXZ && !clip.heightFromFeet) continue;
                clip.loopTime = true;
                clip.lockRootRotation = clip.keepOriginalOrientation = true;
                clip.lockRootHeightY = clip.keepOriginalPositionY = true;
                clip.lockRootPositionXZ = clip.keepOriginalPositionXZ = true;
                clip.heightFromFeet = false;
                changed = true;
            }
            if (changed && clips.Length > 0)
            {
                importer.clipAnimations = clips;
                importer.SaveAndReimport();
                log.Add($"A_NPC_Sit: {string.Join(", ", clips.Select(c => c.name))} (loop, in place as authored)");
            }
            var avatar = AssetDatabase.LoadAllAssetsAtPath(SitClipPath).OfType<Avatar>().FirstOrDefault();
            if (avatar == null || !avatar.isValid || !avatar.isHuman) log.Add("ERROR A_NPC_Sit: humanoid avatar invalid");
            var takes = AssetDatabase.LoadAllAssetsAtPath(SitClipPath).OfType<AnimationClip>().Where(c => !c.name.StartsWith("__preview__")).ToList();
            var sit = takes.FirstOrDefault(c => c.name == SitState) ?? takes.FirstOrDefault();
            if (sit == null) log.Add($"WARNING {SitClipPath} has no clip: evening guests only stand");
            return sit;
        }

        private static AnimatorController Controller(string species, string fbx, AnimationClip sit, List<string> log)
        {
            string path = ControllerFolder + $"AC_NPC_{species}.controller";
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
            if (controller == null)
            {
                AssetFolders.EnsureForAsset(path);
                controller = AnimatorController.CreateAnimatorControllerAtPath(path);
            }
            var clips = AssetDatabase.LoadAllAssetsAtPath(fbx).OfType<AnimationClip>().Where(c => !c.name.StartsWith("__preview__")).ToList();
            // P157 (#48): the guard's own walk holds the arms up at the shoulders; the humans' walk (humanoid, retargeted) swings them low.
            var humanClips = AssetDatabase.LoadAllAssetsAtPath(ModelFolder + "SK_NPC_Human.fbx").OfType<AnimationClip>()
                .Where(c => !c.name.StartsWith("__preview__")).ToList();
            var machine = controller.layers[0].stateMachine;
            foreach (string name in species == "Guard" ? new[] { "Idle", "Walk", "Grab" } : States)
            {
                var clip = (species == "Guard" && name == "Walk" ? humanClips : clips).FirstOrDefault(c => c.name == name);
                if (clip == null) log.Add($"WARNING {fbx} has no clip {name}");
                var state = machine.states.Select(s => s.state).FirstOrDefault(s => s.name == name) ?? machine.AddState(name);
                state.motion = clip;
                if (name == "Idle") machine.defaultState = state;
            }
            // P145: the evening guests' seat loop. Without its clip the state goes: an empty Sit would T-pose, so they stand instead.
            var sitState = machine.states.Select(s => s.state).FirstOrDefault(s => s.name == SitState);
            if (sit != null)
            {
                if (sitState == null) sitState = machine.AddState(SitState);
                sitState.motion = sit;
            }
            else if (sitState != null) machine.RemoveState(sitState);
            EditorUtility.SetDirty(controller);
            return controller;
        }
    }
}
