using System.Collections.Generic;
using GameOff.Documents;
using GameOff.Travelers;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace GameOff.EditorTools
{
    /// <summary>
    /// GameOff/Validate Travelers: runs the TravelerValidator over every day in the project and checks that every required
    /// document field has a text slot ("Field_&lt;key&gt;") in its prefab. Problems go to the console as errors.
    /// The same checks run in the EditMode tests.
    /// </summary>
    public static class TravelerValidatorMenu
    {
        private const string PrefabFolder = "Assets/_Project/Prefabs/Documents/";

        [MenuItem("GameOff/Validate Travelers")]
        public static void ValidateFromMenu()
        {
            var problems = ValidateProject();
            if (problems.Count == 0) Debug.Log("[Validate] All days and document prefabs are consistent.");
            foreach (var p in problems) Debug.LogError("[Validate] " + p);
        }

        public static List<string> ValidateProject()
        {
            var problems = new List<string>();
            foreach (var guid in AssetDatabase.FindAssets("t:DayRules", new[] { "Assets/_Project/Data" }))
                problems.AddRange(TravelerValidator.Validate(AssetDatabase.LoadAssetAtPath<DayRules>(AssetDatabase.GUIDToAssetPath(guid))));
            problems.AddRange(ValidatePrefabFields());
            return problems;
        }

        /// <summary>Every required field of a document type must be printed somewhere on its prefab.</summary>
        public static List<string> ValidatePrefabFields()
        {
            var problems = new List<string>();
            foreach (var pair in TravelerValidator.RequiredFields)
            {
                string path = PrefabFolder + $"PF_Doc_{pair.Key}.prefab";
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null) { problems.Add($"Missing prefab {path}"); continue; }
                var names = new HashSet<string>();
                foreach (var t in prefab.GetComponentsInChildren<TMP_Text>(true)) names.Add(t.name);
                foreach (var key in pair.Value)
                    if (!names.Contains("Field_" + key)) problems.Add($"{path}: no text \"Field_{key}\" (the {key} would never be shown)");
            }
            return problems;
        }
    }
}
