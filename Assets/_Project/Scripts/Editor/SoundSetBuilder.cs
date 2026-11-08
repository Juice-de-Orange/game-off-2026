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
            set.paperSlide.volume = set.paperPickup.volume = PaperVolume; // P102
            set.paperPickups = Variants("SFX_Paper_Pickup_0{0}", 4, PaperVolume);
            set.paperSlides = Variants("SFX_Paper_Slide_0{0}", 4, PaperVolume);
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
            Assign(set.telescopeCoin, "SFX_Telescope_Coin");
            Assign(set.guardBoots, "SFX_Guard_Boots");
            Assign(set.airlockAlarm, "SFX_Airlock_Alarm");
            Assign(set.airlockDecompress, "SFX_Airlock_Decompress");
            Assign(set.shipLand, "SFX_Ship_Land");
            Assign(set.tradeBuy, "SFX_Trade_Buy");
            Assign(set.uiClick, "SFX_UI_Click");

            if (set.clownFootsteps == null || set.clownFootsteps.Length != 3)
                set.clownFootsteps = new[] { new Sound { volume = FootstepVolume }, new Sound { volume = FootstepVolume }, new Sound { volume = FootstepVolume } };
            for (int i = 0; i < set.clownFootsteps.Length; i++)
                Assign(set.clownFootsteps[i], $"SFX_ClownShoe_Squeak_0{i + 1}");
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

        private const float PaperVolume = 0.32f;

        /// <summary>P102: the delivered variants; empty as long as there are none.</summary>
        private static Sound[] Variants(string format, int count, float volume)
        {
            var list = new System.Collections.Generic.List<Sound>();
            for (int i = 1; i <= count; i++)
            {
                var clip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/SFX/" + string.Format(format, i) + ".ogg");
                if (clip != null) list.Add(new Sound { clip = clip, volume = volume });
            }
            return list.ToArray();
        }

        private static void Assign(Sound sound, string clipName)
        {
            sound.clip = AssetDatabase.LoadAssetAtPath<AudioClip>($"{ClipFolder}{clipName}.ogg");
            if (sound.clip == null) Debug.LogError($"[SoundSet] Missing clip {clipName}.");
        }
    }
}
