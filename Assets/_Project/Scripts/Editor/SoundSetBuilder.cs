using GameOff.Audio;
using UnityEditor;
using UnityEngine;

namespace GameOff.EditorTools
{
    /// <summary>
    /// Fills Data/Audio/SoundSet.asset with the delivered clips (P61). Volumes are set only when the asset is new, so what was
    /// balanced by ear in the asset survives. The AudioManager prefab from MenuSceneBuilder references the result.
    /// </summary>
    public static class SoundSetBuilder
    {
        private const string AssetPath = "Assets/_Project/Data/Audio/SoundSet.asset";
        private const string ClipFolder = "Assets/_Project/Audio/SFX/";
        private const float FootstepVolume = 0.5f;
        private const float UiVolume = 0.7f;

        [MenuItem("GameOff/Build Sound Set")]
        public static void BuildFromMenu() => Debug.Log($"[SoundSet] Built {Build().name}");

        public static SoundSet Build()
        {
            var set = AssetDatabase.LoadAssetAtPath<SoundSet>(AssetPath);
            bool isNew = set == null;
            if (isNew)
            {
                AssetFolders.EnsureForAsset(AssetPath);
                set = ScriptableObject.CreateInstance<SoundSet>();
                AssetDatabase.CreateAsset(set, AssetPath);
            }

            Assign(set.stampApprove, "SFX_Stamp_Approve");
            Assign(set.stampDeny, "SFX_Stamp_Deny");
            Assign(set.paperSlide, "SFX_Paper_Slide");
            Assign(set.paperPickup, "SFX_Paper_Pickup");
            Assign(set.drawerOpen, "SFX_Drawer_Open");
            Assign(set.drawerClose, "SFX_Drawer_Close");
            Assign(set.printerCitation, "SFX_Printer_Citation");
            Assign(set.buttonArrest, "SFX_Button_Arrest");
            Assign(set.coverFlip, "SFX_Cover_Flip");
            AssignOr(set.shutterDrop, "SFX_Shutter_Drop", "SFX_Door_Close"); // P91: until the delivery
            AssignOr(set.shutterRise, "SFX_Shutter_Rise", "SFX_Door_Open");
            AssignOr(set.arrestScuffle, "SFX_Arrest_Scuffle", "SFX_Button_Arrest");
            Assign(set.airlockFire, "SFX_Airlock_Fire");
            Assign(set.paChime, "SFX_PA_Chime");
            Assign(set.vendorGreet, "SFX_Vendor_Greet");
            Assign(set.tradeBuy, "SFX_Trade_Buy");
            Assign(set.uiClick, "SFX_UI_Click");

            if (set.footsteps == null || set.footsteps.Length != 3)
                set.footsteps = new[] { new Sound { volume = FootstepVolume }, new Sound { volume = FootstepVolume }, new Sound { volume = FootstepVolume } };
            for (int i = 0; i < set.footsteps.Length; i++)
                Assign(set.footsteps[i], $"SFX_Footstep_Metal_0{i + 1}");

            if (isNew) set.uiClick.volume = UiVolume;
            EditorUtility.SetDirty(set);
            AssetDatabase.SaveAssets();
            return set;
        }

        private static void AssignOr(Sound sound, string clipName, string placeholder)
        {
            bool delivered = AssetDatabase.LoadAssetAtPath<AudioClip>($"{ClipFolder}{clipName}.ogg") != null;
            Assign(sound, delivered ? clipName : placeholder);
        }

        private static void Assign(Sound sound, string clipName)
        {
            sound.clip = AssetDatabase.LoadAssetAtPath<AudioClip>($"{ClipFolder}{clipName}.ogg");
            if (sound.clip == null) Debug.LogError($"[SoundSet] Missing clip {clipName}.");
        }
    }
}
