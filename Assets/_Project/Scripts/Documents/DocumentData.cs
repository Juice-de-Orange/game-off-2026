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

        public DocumentType Type => type;
        public Texture2D Photo => photo;
        public IReadOnlyList<DocumentField> Fields => fields;

        /// <summary>Value of a field by key, or null if the document doesn't have it.</summary>
        public string Get(string key)
        {
            foreach (var f in fields)
                if (f.key == key) return f.value;
            return null;
        }

#if UNITY_EDITOR
        /// <summary>Editor setup (test data builder).</summary>
        public void EditorSet(DocumentType newType, Texture2D newPhoto, List<DocumentField> newFields)
        {
            type = newType;
            photo = newPhoto;
            fields = newFields;
        }
#endif
    }
}
