using GameOff.Day;
using GameOff.Economy;
using TMPro;
using UnityEngine;

namespace GameOff.Rest
{
    /// <summary>
    /// P29: the morning letter lies on the table in room 103 all day, the same text the player read when waking up. Its letters
    /// wobble and tilt a little, more the worse the family is doing: the hand that writes them shakes.
    /// </summary>
    public class TableLetter : MonoBehaviour
    {
        [SerializeField] private TMP_Text text;
        [SerializeField] private DayManager day;
        [SerializeField] private RestSpotTuning tuning;

        private float _shake;
        private int _seed;

        private void OnEnable()
        {
            if (day != null) day.LetterWritten += OnLetter;
            TMPro_EventManager.TEXT_CHANGED_EVENT.Add(OnTextChanged);
        }

        private void OnDisable()
        {
            if (day != null) day.LetterWritten -= OnLetter;
            TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(OnTextChanged);
        }

        private void OnLetter(string title, string body)
        {
            var economy = PlayerEconomy.Instance;
            _shake = FamilyMood.Shake(FamilyMood.Of(economy != null ? economy.Household : null));
            _seed = day.Day;
            text.text = string.IsNullOrWhiteSpace(title) ? body : $"<b>{title}</b>\n\n{body}";
        }

        // TMP rebuilt the mesh (new text, or the object came back on): shake the fresh vertices once.
        private void OnTextChanged(Object changed)
        {
            if (changed == text) Wobble();
        }

        private void Wobble()
        {
            if (_shake <= 0f || tuning == null) return;
            var info = text.textInfo;
            for (int i = 0; i < info.characterCount; i++)
            {
                var c = info.characterInfo[i];
                if (!c.isVisible) continue;
                var vertices = info.meshInfo[c.materialReferenceIndex].vertices;
                int v = c.vertexIndex;
                Vector3 centre = (vertices[v] + vertices[v + 2]) * 0.5f; // bottom left + top right
                float height = Mathf.Abs(vertices[v + 1].y - vertices[v].y);
                Vector3 shift = Handwriting.Offset(i, _seed, _shake) * (tuning.letterJitter * height);
                var turn = Quaternion.Euler(0f, 0f, Handwriting.Tilt(i, _seed, _shake) * tuning.letterTilt);
                for (int k = 0; k < 4; k++) vertices[v + k] = centre + turn * (vertices[v + k] - centre) + shift;
            }
            text.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);
        }
    }
}
