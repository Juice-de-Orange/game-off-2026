using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static GameOff.EditorTools.MenuSceneBuilder;

namespace GameOff.EditorTools
{
    /// <summary>
    /// Gives the player's HUD the menu look (martin 05.11.): the interaction prompt "[E]  SLEEP" looks like a menu button
    /// (button grey, light Orbitron capitals, width hugs the text) under the crosshair. Idempotent, only touches
    /// PF_Player/HUD/Prompt. Menu: GameOff → Apply HUD Style.
    /// </summary>
    public static class HudStyleBuilder
    {
        private const string PlayerPrefab = "Assets/_Project/Prefabs/Player/PF_Player.prefab";

        [MenuItem("GameOff/Apply HUD Style")]
        public static void ApplyFromMenu() => Debug.Log(Apply());

        public static string Apply()
        {
            var font = TitleFont();
            var root = PrefabUtility.LoadPrefabContents(PlayerPrefab);
            try
            {
                var prompt = root.transform.Find("HUD/Prompt");
                if (prompt == null) return "[HudStyleBuilder] PF_Player has no HUD/Prompt";
                var label = prompt.GetComponentInChildren<TextMeshProUGUI>(true);

                // Button look: same grey as the menu buttons, opaque.
                prompt.GetComponent<Image>().color = ButtonIdle;

                // Size follows the text: padding like a menu button.
                var layout = prompt.GetComponent<HorizontalLayoutGroup>() ?? prompt.gameObject.AddComponent<HorizontalLayoutGroup>();
                layout.padding = new RectOffset(18, 18, 7, 7);
                layout.childAlignment = TextAnchor.MiddleCenter;
                layout.childControlWidth = true;
                layout.childControlHeight = true;
                layout.childForceExpandWidth = false;
                layout.childForceExpandHeight = false;
                var fitter = prompt.GetComponent<ContentSizeFitter>() ?? prompt.gameObject.AddComponent<ContentSizeFitter>();
                fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

                if (font != null) label.font = font;
                label.fontSize = 18f;
                label.enableAutoSizing = false; // the box grows with the text instead
                label.color = TextLight;
                label.fontStyle = FontStyles.UpperCase;
                label.characterSpacing = 4f;
                label.alignment = TextAlignmentOptions.Center;
                label.textWrappingMode = TextWrappingModes.NoWrap;

                PrefabUtility.SaveAsPrefabAsset(root, PlayerPrefab);
                return "[HudStyleBuilder] prompt styled like a menu button";
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }
    }
}
