using System.Collections.Generic;
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
        private const string UvMarkName = "UvMark";

        [SerializeField] private DocumentType type;
        [SerializeField] private Renderer photo;
        [Tooltip("Page size in meters (width along local X, height along local Z).")]
        [SerializeField] private Vector2 pageSize = new Vector2(0.15f, 0.21f);

        private MaterialPropertyBlock _block;
        private readonly List<(TMP_Text text, string key, Color color)> _fields = new List<(TMP_Text, string, Color)>();
        private static readonly Color FieldHover = new Color(0.1f, 0.42f, 0.62f);
        private static readonly Color FieldContradiction = new Color(0.86f, 0.2f, 0.12f);
        private static readonly Color UvGenuine = new Color(0.25f, 0.85f, 0.75f, 0.8f);
        private static readonly Color UvForged = new Color(1f, 0.1f, 0.45f, 0.9f);
        private readonly HashSet<string> _contradictions = new HashSet<string>();
        private string _hovered;
        private TMP_Text _uvMark;
        private const float FieldMargin = 0.003f; // meters around a field's text that still count as on it

        public DocumentType Type => type;
        public DocumentData Data { get; private set; }
        public Vector2 PageSize => pageSize;
        /// <summary>A traveler's paper: the officer can ask about its fields (P40). Not slips and pinboard notes.</summary>
        public bool AcceptsQuestions => type != DocumentType.Citation && type != DocumentType.Notice;

        public void Bind(DocumentData data)
        {
            Data = data;
            _fields.Clear();
            _contradictions.Clear();
            _hovered = null;
            foreach (var text in GetComponentsInChildren<TMP_Text>(true))
                if (text.name == UvMarkName)
                {
                    _uvMark = text;
                    text.gameObject.SetActive(false);
                }
                else if (text.name.StartsWith(FieldPrefix))
                {
                    string key = text.name.Substring(FieldPrefix.Length);
                    text.text = data != null ? data.Get(key) ?? "" : "";
                    _fields.Add((text, key, text.color));
                }
            if (photo != null && data != null && data.Photo != null)
            {
                _block ??= new MaterialPropertyBlock();
                photo.GetPropertyBlock(_block);
                _block.SetTexture("_BaseMap", data.Photo);
                photo.SetPropertyBlock(_block);
            }
        }

        /// <summary>The key of the printed field under a ray (from the officer's mouse), or null.</summary>
        public string FieldAt(Ray ray)
        {
            if (!new Plane(transform.up, transform.position).Raycast(ray, out float distance)) return null;
            Vector3 point = ray.GetPoint(distance);
            foreach (var (text, key, _) in _fields)
            {
                if (string.IsNullOrEmpty(text.text)) continue;
                Vector3 local = text.transform.InverseTransformPoint(point);
                var bounds = text.textBounds;
                if (Mathf.Abs(local.x - bounds.center.x) <= bounds.extents.x + FieldMargin
                    && Mathf.Abs(local.y - bounds.center.y) <= bounds.extents.y + FieldMargin)
                    return key;
            }
            return null;
        }

        /// <summary>Tints the field the mouse is on, so it reads as clickable. Null clears it.</summary>
        public void HighlightField(string key)
        {
            _hovered = key;
            Recolor();
        }

        /// <summary>P16: the scanner found that this field contradicts another paper on the desk.</summary>
        public void MarkContradiction(string key, bool on)
        {
            if (on ? _contradictions.Add(key) : _contradictions.Remove(key)) Recolor();
        }

        /// <summary>P62: under the UV lamp the paper's seal shows; only a gadget can tell a forged one.</summary>
        public void ShowUv(bool on)
        {
            if (_uvMark == null || _uvMark.gameObject.activeSelf == on) return;
            if (on)
            {
                bool forged = Data != null && Data.ForgedSeal;
                _uvMark.text = forged ? "SEAL FORGED" : "SEAL VALID";
                _uvMark.color = forged ? UvForged : UvGenuine;
            }
            _uvMark.gameObject.SetActive(on);
        }

        private void Recolor()
        {
            foreach (var (text, fieldKey, color) in _fields)
                text.color = fieldKey == _hovered ? FieldHover : _contradictions.Contains(fieldKey) ? FieldContradiction : color;
        }
    }
}
