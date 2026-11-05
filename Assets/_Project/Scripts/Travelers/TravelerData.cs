using System.Collections.Generic;
using GameOff.Documents;
using UnityEngine;

namespace GameOff.Travelers
{
    /// <summary>
    /// One traveler at the counter (P07): who they are, the papers they hand over, what they say (P40), and the decision the
    /// author intends. The correct decision is computed by RuleEvaluator from the papers and the day's rules; the validator
    /// and the tests make sure it matches intendedVerdict, so content mistakes never reach the game.
    /// </summary>
    [CreateAssetMenu(menuName = "GameOff/Traveler", fileName = "T_")]
    public class TravelerData : ScriptableObject
    {
        [SerializeField] private string displayName;
        [Tooltip("Human, Vellani, Quobb, Xirrik, Sorvak. Non-human species speak in alien glyphs that get translated (P40).")]
        [SerializeField] private string species = "Human";
        [Tooltip("Character prefab PF_NPC_<Species> shown in the inspection lock (P40; GameOff/Build Travelers).")]
        [SerializeField] private GameObject model;
        [SerializeField] private List<DocumentData> documents = new List<DocumentData>();
        [Header("Author's intention (checked against the rules)")]
        [SerializeField] private Verdict intendedVerdict;
        [TextArea] [SerializeField] private string intendedReason;
        [Tooltip("This traveler is meant to almost match a rule (a trick, e.g. The Shifter): the validator doesn't report near misses. The decision itself is still checked.")]
        [SerializeField] private bool trickIntended;
        [Tooltip("Story marker, e.g. \"shifter\" for The Shifter (P15).")]
        [SerializeField] private string storyTag;
        [Header("Dialogue (P40)")]
        [TextArea] [SerializeField] private string greeting;
        [SerializeField] private List<FieldAnswer> answers = new List<FieldAnswer>();

        public string DisplayName => displayName;
        public string Species => species;
        public bool IsAlien => !string.Equals(species, "Human", System.StringComparison.OrdinalIgnoreCase);
        public GameObject Model => model;
        public IReadOnlyList<DocumentData> Documents => documents;
        public Verdict IntendedVerdict => intendedVerdict;
        public string IntendedReason => intendedReason;
        public string StoryTag => storyTag;
        public bool TrickIntended => trickIntended;
        public string Greeting => greeting;
        public IReadOnlyList<FieldAnswer> Answers => answers;

        /// <summary>The traveler's answer to a question about a document field, or null.</summary>
        public string AnswerFor(string fieldKey)
        {
            foreach (var a in answers)
                if (a.key == fieldKey) return a.line;
            return null;
        }

        /// <summary>The first document of a type, or null.</summary>
        public DocumentData Document(DocumentType type)
        {
            foreach (var d in documents)
                if (d != null && d.Type == type) return d;
            return null;
        }

#if UNITY_EDITOR
        /// <summary>Editor setup (content builders and tests).</summary>
        public void EditorSet(string newName, string newSpecies, List<DocumentData> newDocuments, Verdict verdict, string reason,
            string tag = "", string newGreeting = "", List<FieldAnswer> newAnswers = null, GameObject newModel = null, bool trick = false)
        {
            trickIntended = trick;
            displayName = newName;
            species = newSpecies;
            documents = newDocuments;
            intendedVerdict = verdict;
            intendedReason = reason;
            storyTag = tag;
            greeting = newGreeting;
            answers = newAnswers ?? new List<FieldAnswer>();
            model = newModel;
        }
#endif
    }
}
