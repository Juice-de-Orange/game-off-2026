using System.Text;
using GameOff.Economy;
using UnityEngine;

namespace GameOff.UI
{
    /// <summary>Item tags as colored TextMeshPro chips, e.g. "CONTRABAND" on dark red (P46).</summary>
    public static class ItemTagText
    {
        public static string Chips(ItemData item)
        {
            if (item == null || item.Tags.Count == 0) return "";
            var sb = new StringBuilder();
            foreach (var tag in item.Tags)
            {
                if (tag == null) continue;
                string hex = ColorUtility.ToHtmlStringRGB(tag.Color);
                sb.Append($"<mark=#{hex}DD><color=#E2DDCF> {tag.DisplayName} </color></mark>  ");
            }
            return sb.ToString();
        }
    }
}
