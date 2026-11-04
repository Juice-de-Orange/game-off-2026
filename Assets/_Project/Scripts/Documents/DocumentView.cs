using TMPro;
using UnityEngine;

namespace GameOff.Documents
{
    /// <summary>
    /// A document lying on the desk: fills the prefab's "Field_&lt;key&gt;" texts and the photo from a DocumentData.
    /// The root stays unscaled and carries the BoxCollider (and a StampTarget), so it can be dragged and stamped.
    /// Local frame: X = page right, Z = page top, Y = out of the page.
    /// </summary>
    public class DocumentView : MonoBehaviour
    {
        private const string FieldPrefix = "Field_";

        [SerializeField] private DocumentType type;
        [SerializeField] private Renderer photo;
        [Tooltip("Page size in meters (width along local X, height along local Z).")]
        [SerializeField] private Vector2 pageSize = new Vector2(0.15f, 0.21f);

        private MaterialPropertyBlock _block;

        public DocumentType Type => type;
        public DocumentData Data { get; private set; }
        public Vector2 PageSize => pageSize;

        public void Bind(DocumentData data)
        {
            Data = data;
            foreach (var text in GetComponentsInChildren<TMP_Text>(true))
                if (text.name.StartsWith(FieldPrefix))
                    text.text = data != null ? data.Get(text.name.Substring(FieldPrefix.Length)) ?? "" : "";
            if (photo != null && data != null && data.Photo != null)
            {
                _block ??= new MaterialPropertyBlock();
                photo.GetPropertyBlock(_block);
                _block.SetTexture("_BaseMap", data.Photo);
                photo.SetPropertyBlock(_block);
            }
        }
    }
}
