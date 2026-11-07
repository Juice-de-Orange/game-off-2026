using System.Collections.Generic;
using System.IO;
using GameOff.Characters;
using GameOff.Documents;
using GameOff.Economy;
using GameOff.Travelers;
using UnityEditor;
using UnityEngine;

namespace GameOff.EditorTools
{
    /// <summary>
    /// P75 test character (acceptance): "Tess Marlo", built from Vesna's papers with her own name.
    ///   Day 1 anywhere: let through = helped; turned away / arrested = betrayed → tomorrow a cold snap (prices, station mood),
    ///   and on day 3 she is on the wanted list (arrest).
    ///   Day 2, third in the queue, only if helped: thanks you.
    ///   Day 3, first in the queue, only if wronged: comes back for revenge – wanted, so arrest is right.
    /// Only active while the dev tools are on (CharacterDirector.devCharacters). Generated content – don't edit by hand.
    /// </summary>
    public static class CharacterBuilder
    {
        private const string Folder = "Assets/_Project/Data/Characters/Test/";
        private const string SourceTraveler = "Assets/_Project/Data/Travelers/Test/T_Test_01_Vesna.asset";
        private const string ColdSnap = "Assets/_Project/Data/Market/Market_ColdSnap.asset";
        public const string CharacterPath = Folder + "Char_Test_TessMarlo.asset";
        private const string Name = "TESS MARLO";

        [MenuItem("GameOff/Build Test Character")]
        public static void BuildFromMenu() => Debug.Log("[CharacterBuilder] " + Build());

        public static string Build()
        {
            var source = AssetDatabase.LoadAssetAtPath<TravelerData>(SourceTraveler);
            if (source == null)
            {
                var guids = AssetDatabase.FindAssets("T_Test_01_Vesna t:TravelerData");
                if (guids.Length > 0) source = AssetDatabase.LoadAssetAtPath<TravelerData>(AssetDatabase.GUIDToAssetPath(guids[0]));
            }
            if (source == null) return "no source traveler (T_Test_01_Vesna) - nothing built";
            Directory.CreateDirectory(Folder);

            var docs = new List<DocumentData>();
            foreach (var d in source.Documents)
            {
                if (d == null) continue;
                var copy = Copy(d, Folder + "Doc_Tess_" + d.Type + ".asset");
                var so = new SerializedObject(copy);
                var fields = so.FindProperty("fields");
                for (int i = 0; i < fields.arraySize; i++)
                {
                    var v = fields.GetArrayElementAtIndex(i).FindPropertyRelative("value");
                    if (v.stringValue == source.DisplayName) v.stringValue = Name;
                }
                so.ApplyModifiedPropertiesWithoutUndo();
                docs.Add(copy);
            }

            var first = Traveler("T_Tess_1_Arrives", source, docs, Verdict.Admit, "", "Hi. First time on Kestrel. Be nice?");
            var thanks = Traveler("T_Tess_2_Thanks", source, docs, Verdict.Admit, "", "You again! I'm back - thanks to you.");
            var revenge = Traveler("T_Tess_3_Revenge", source, docs, Verdict.Arrest, "On the wanted list (arrest)", "Remember me? I remember you.");

            var cold = AssetDatabase.LoadAssetAtPath<MarketEvent>(ColdSnap);
            var ch = AssetDatabase.LoadAssetAtPath<RecurringCharacter>(CharacterPath);
            if (ch == null)
            {
                ch = ScriptableObject.CreateInstance<RecurringCharacter>();
                AssetDatabase.CreateAsset(ch, CharacterPath);
            }
            ch.EditorSet("test_tess", Name, source.Species, new List<Appearance>
            {
                new Appearance
                {
                    traveler = first, dayFrom = 1, dayTo = 1, slot = 0, condition = AppearanceCondition.NeverMet,
                    onAdmit = Outcome.Helped, onDeny = Outcome.Betrayed, onArrest = Outcome.Betrayed,
                    consequences = new List<Consequence>
                    {
                        new Consequence { when = Outcome.Betrayed, marketEvent = cold, eventInDays = 1 },
                        new Consequence { when = Outcome.Betrayed, wanted = true, wantedOrder = WantedOrder.Arrest, wantedInDays = 2, wantedForDays = 1,
                            wantedNote = "Threatened an officer" },
                    },
                },
                new Appearance
                {
                    traveler = thanks, dayFrom = 2, dayTo = 2, slot = 3, condition = AppearanceCondition.LastHelped,
                    greetingAfterHelped = "You let me through yesterday. I won't forget that.",
                    eventLines = new List<EventLine> { new EventLine { marketEvent = cold, line = "And it's freezing back home, they say." } },
                },
                new Appearance
                {
                    traveler = revenge, dayFrom = 3, dayTo = 3, slot = 1, condition = AppearanceCondition.LastWronged,
                    onAdmit = Outcome.Neutral, onDeny = Outcome.Betrayed, onArrest = Outcome.Neutral,
                    greetingAfterBetrayed = "You sent me away. Let's see how you like it.",
                },
            });
            EditorUtility.SetDirty(ch);
            AssetDatabase.SaveAssets();
            return $"test character {Name}: 3 appearances, {docs.Count} papers";
        }

        private static TravelerData Traveler(string file, TravelerData source, List<DocumentData> docs, Verdict verdict, string reason, string greeting)
        {
            var t = Copy(source, Folder + file + ".asset");
            var so = new SerializedObject(t);
            so.FindProperty("displayName").stringValue = Name;
            so.FindProperty("intendedVerdict").enumValueIndex = (int)verdict;
            so.FindProperty("intendedReason").stringValue = reason;
            so.FindProperty("greeting").stringValue = greeting;
            so.FindProperty("storyTag").stringValue = "P75 test character";
            var list = so.FindProperty("documents");
            list.arraySize = docs.Count;
            for (int i = 0; i < docs.Count; i++) list.GetArrayElementAtIndex(i).objectReferenceValue = docs[i];
            so.ApplyModifiedPropertiesWithoutUndo();
            return t;
        }

        /// <summary>Copies an asset to path (overwrites the content of an existing copy, keeps its GUID).</summary>
        private static T Copy<T>(T source, string path) where T : ScriptableObject
        {
            var existing = AssetDatabase.LoadAssetAtPath<T>(path);
            if (existing == null)
            {
                AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(source), path);
                return AssetDatabase.LoadAssetAtPath<T>(path);
            }
            EditorUtility.CopySerialized(source, existing);
            existing.name = Path.GetFileNameWithoutExtension(path);
            return existing;
        }
    }
}
