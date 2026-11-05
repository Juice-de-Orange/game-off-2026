using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameOff.UI
{
    /// <summary>
    /// A simple list of rows (label left, value right) for the trade and inventory windows (P46). Rows are copies of an
    /// inactive template; the selected row is highlighted. Clipped by a RectMask2D, no scrolling yet (enough for a jam).
    /// </summary>
    public class ItemListView : MonoBehaviour
    {
        [SerializeField] private Transform content;
        [SerializeField] private Button rowTemplate;
        [SerializeField] private Color idle = new Color(0.106f, 0.122f, 0.153f);
        [SerializeField] private Color selected = new Color(0.173f, 0.196f, 0.235f);
        [SerializeField] private Color text = new Color(0.886f, 0.867f, 0.812f);
        [SerializeField] private Color selectedText = new Color(1f, 0.827f, 0.42f);

        private readonly List<Button> _rows = new List<Button>();

        public struct Row
        {
            public string Label;
            public string Value;
            public Color? ValueColor;
            public bool Selected;
            public Action OnClick;
        }

        private void Awake() => rowTemplate.gameObject.SetActive(false);

        public void Show(IReadOnlyList<Row> rows)
        {
            while (_rows.Count < rows.Count)
            {
                var copy = Instantiate(rowTemplate, content);
                _rows.Add(copy);
            }
            for (int i = 0; i < _rows.Count; i++)
            {
                bool used = i < rows.Count;
                _rows[i].gameObject.SetActive(used);
                if (!used) continue;
                var row = rows[i];
                var labels = _rows[i].GetComponentsInChildren<TMP_Text>(true);
                labels[0].text = row.Label;
                labels[0].color = row.Selected ? selectedText : text;
                labels[1].text = row.Value;
                labels[1].color = row.ValueColor ?? (row.Selected ? selectedText : text);
                var colors = _rows[i].colors;
                colors.normalColor = row.Selected ? selected : idle;
                colors.selectedColor = colors.normalColor;
                colors.disabledColor = colors.normalColor; // read-only rows (TODAY) must not look greyed out
                _rows[i].colors = colors;
                _rows[i].onClick.RemoveAllListeners();
                if (row.OnClick != null) _rows[i].onClick.AddListener(() => row.OnClick());
                _rows[i].interactable = row.OnClick != null;
            }
        }
    }
}
