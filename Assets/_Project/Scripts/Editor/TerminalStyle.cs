using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace GameOff.EditorTools
{
    /// <summary>
    /// P95: the delivered terminal look (metal frames, sunken fields, buttons with hover and press) for everything the UI builders make.
    /// Without the sprites the UI keeps its flat colours.
    /// </summary>
    internal static class TerminalStyle
    {
        internal const string Folder = "Assets/_Project/Art/UI/Terminal/";

        private static Sprite Load(string name) =>
            AssetDatabase.LoadAllAssetsAtPath(Folder + name + ".png").OfType<Sprite>().FirstOrDefault();

        /// <summary>Slice borders in pixels of the delivered graphics; 0 = not sliced (also the bubble's tail, P143).</summary>
        internal static int BorderOf(string fileName) =>
            fileName == "UI_Terminal_Panel" ? 24 : fileName == "UI_Terminal_PanelInset" ? 10 : fileName.StartsWith("UI_Terminal_Button") ? 8
            : fileName == BubbleFrameName ? 16 : 0;

        private const string BubbleFrameName = "UI_SpeechBubble";

        /// <summary>P143: the speech bubble's frame, until it is delivered the panel frame; null without either.</summary>
        internal static Sprite BubbleFrame(out int border)
        {
            var sprite = Load(BubbleFrameName);
            string name = sprite != null ? BubbleFrameName : "UI_Terminal_Panel";
            if (sprite == null) sprite = Load(name);
            border = sprite != null ? BorderOf(name) : 0;
            return sprite;
        }

        /// <summary>P143: the bubble's tail (points down-left, open at the top); null until delivered.</summary>
        internal static Sprite BubbleTail() => Load(BubbleFrameName + "_Tail");

        private static void Sliced(Image image, string sprite)
        {
            var s = Load(sprite);
            if (s == null) return;
            image.sprite = s;
            image.type = Image.Type.Sliced;
            image.color = Color.white;
        }

        internal static void Frame(Image image) => Sliced(image, "UI_Terminal_Panel");

        internal static void Inset(Image image) => Sliced(image, "UI_Terminal_PanelInset");

        internal static void Button(Button button)
        {
            var normal = Load("UI_Terminal_Button_Normal");
            if (normal == null) return;
            Sliced(button.image, "UI_Terminal_Button_Normal");
            var pressed = Load("UI_Terminal_Button_Pressed");
            button.transition = Selectable.Transition.SpriteSwap;
            button.spriteState = new SpriteState
            {
                highlightedSprite = Load("UI_Terminal_Button_Hover"),
                selectedSprite = normal,
                pressedSprite = pressed,
                disabledSprite = pressed,
            };
        }

        /// <summary>Back to a flat button tinted by its colours (list rows, the red dev buttons).</summary>
        internal static void Flat(Button button)
        {
            button.image.sprite = null;
            button.image.type = Image.Type.Simple;
            button.transition = Selectable.Transition.ColorTint;
        }

        /// <summary>A frame behind a full-screen page, drawn first.</summary>
        internal static void AddFrame(Transform page, Vector2 size)
        {
            var go = new GameObject("TerminalFrame", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(page, false);
            go.transform.SetAsFirstSibling();
            go.GetComponent<RectTransform>().sizeDelta = size;
            var image = go.GetComponent<Image>();
            image.raycastTarget = false;
            Frame(image);
            if (image.sprite == null) Object.DestroyImmediate(go);
        }

        /// <summary>A tiled line under a window's title.</summary>
        internal static void AddDivider(Transform window, Vector2 position, float width)
        {
            var s = Load("UI_Terminal_Divider");
            if (s == null) return;
            var go = new GameObject("Divider", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(window, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = position;
            rt.sizeDelta = new Vector2(width, 8f);
            var image = go.GetComponent<Image>();
            image.sprite = s;
            image.type = Image.Type.Tiled;
            image.raycastTarget = false;
        }
    }
}
