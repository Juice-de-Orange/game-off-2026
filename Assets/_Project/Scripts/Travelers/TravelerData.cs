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
        [Tooltip("Story marker for authors, e.g. \"P15, day 9\" (only shown here, the game doesn't read it).")]
        [SerializeField] private string storyTag;
        [Header("Dialogue (P40)")]
        [TextArea] [SerializeField] private string greeting;
        [SerializeField] private List<FieldAnswer> answers = new List<FieldAnswer>();
        [Tooltip("P100: said after the stamp when let through; empty = a line from the species' pool.")]
        [SerializeField] private string reactionAdmitted;
        [Tooltip("P100: said after the stamp when turned away; empty = a line from the species' pool.")]
        [SerializeField] private string reactionDenied;

        public string DisplayName => displayName;
        public string Species => species;
        public bool IsAlien => !string.Equals(species, "Human", System.StringComparison.OrdinalIgnoreCase);
        public GameObject Model => model;
        public IReadOnlyList<DocumentData> Documents => documents;
        public Verdict IntendedVerdict => intendedVerdict;
        public string IntendedReason => intendedReason;
        public bool TrickIntended => trickIntended;
        public string Greeting => greeting;
        public IReadOnlyList<FieldAnswer> Answers => answers;
        public string ReactionAdmitted => reactionAdmitted;
        public string ReactionDenied => reactionDenied;

        public void EditorSetReactions(string admitted, string denied)
        {
            reactionAdmitted = admitted;
            reactionDenied = denied;
        }

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

        /// <summary>P73: a traveler made by the day generator (endless days), not a project asset.</summary>
        public static TravelerData CreateGenerated(string name, string species, List<DocumentData> documents, Verdict verdict, string reason,
            string greeting, GameObject model, bool trick, List<FieldAnswer> fieldAnswers = null)
        {
            var t = CreateInstance<TravelerData>();
            if (fieldAnswers != null) t.answers = fieldAnswers;
            t.name = "T_Gen_" + name.Replace(' ', '_');
            t.displayName = name;
            t.species = species;
            t.documents = documents;
            t.intendedVerdict = verdict;
            t.intendedReason = reason;
            t.greeting = greeting;
            t.model = model;
            t.trickIntended = trick;
            t.storyTag = "P73 generated";
            return t;
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
