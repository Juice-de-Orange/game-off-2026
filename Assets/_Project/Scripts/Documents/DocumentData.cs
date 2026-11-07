using System.Collections.Generic;
using UnityEngine;

namespace GameOff.Documents
{
    /// <summary>The content of one document (P06): its type, printed fields and photo. Travelers (P07) reference these.</summary>
    [CreateAssetMenu(menuName = "GameOff/Document", fileName = "Doc_")]
    public class DocumentData : ScriptableObject
    {
        [SerializeField] private DocumentType type;
        [Tooltip("Portrait on ID card and entry permit (optional).")]
        [SerializeField] private Texture2D photo;
        [SerializeField] private List<DocumentField> fields = new List<DocumentField>();
        [Tooltip("The seal is forged (only a gadget reveals it, P16). The rules treat it as a crime: arrest.")]
        [SerializeField] private bool forgedSeal;

        public DocumentType Type => type;
        public Texture2D Photo => photo;
        public IReadOnlyList<DocumentField> Fields => fields;
        public bool ForgedSeal => forgedSeal;
        /// <summary>Made at runtime (a citation), not a project asset: destroy it when its paper is gone.</summary>
        public bool IsRuntime { get; private set; }

        /// <summary>A document made at runtime (e.g. a citation slip); not saved as an asset.</summary>
        public static DocumentData CreateRuntime(DocumentType type, List<DocumentField> fields)
        {
            var data = CreateInstance<DocumentData>();
            data.type = type;
            data.fields = fields;
            data.name = type.ToString();
            data.IsRuntime = true;
            return data;
        }

        /// <summary>
        /// P73: a traveler's paper made by the day generator (endless days). Not marked runtime, so the desk doesn't destroy it
        /// while its traveler may still be looked at; it lives as long as the generated day.
        /// </summary>
        public static DocumentData CreateGenerated(DocumentType type, Texture2D photo, List<DocumentField> fields, bool forged = false)
        {
            var data = CreateInstance<DocumentData>();
            data.type = type;
            data.photo = photo;
            data.fields = fields;
            data.forgedSeal = forged;
            data.name = type.ToString();
            return data;
        }

        /// <summary>Value of a field by key, or null if the document doesn't have it.</summary>
        public string Get(string key)
        {
            foreach (var f in fields)
                if (f.key == key) return f.value;
            return null;
        }

#if UNITY_EDITOR
        /// <summary>Editor setup (test data builder).</summary>
        public void EditorSet(DocumentType newType, Texture2D newPhoto, List<DocumentField> newFields, bool forged = false)
        {
            type = newType;
            photo = newPhoto;
            fields = newFields;
            forgedSeal = forged;
        }
#endif
    }
}
