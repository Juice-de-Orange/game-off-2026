using System.Collections.Generic;
using GameOff.Counter;
using GameOff.Documents;
using GameOff.Travelers;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace GameOff.EditorTools
{
    /// <summary>
    /// P34: the counter terminal in PF_CounterStation, run by GameOff/Build Documents (DocumentPrefabBuilder.WireCounter). A click area over
    /// the CRT built into SM_Counter_Desk with the status line on its screen, and the green terminal window in the seated HUD.
    /// Rebuilt from scratch on every run.
    /// </summary>
    internal static class CounterTerminalBuilder
    {
        private const string DeskModel = "Assets/_Project/Art/Models/Counter/SM_Counter_Desk.fbx";
        private const string MonoFont = "Assets/_Project/Fonts/ShareTechMono SDF.asset";
        private const string TuningPath = "Assets/_Project/Data/Counter/TerminalTuning.asset";
        // Mirrors StationGrayboxBuilder.BuildProps: SM_Counter_Desk at world (-0.7, 0, -9), yaw 0; the counter stands at (-2, 0, -9), unrotated.
        private static readonly Vector3 DeskInCounter = new Vector3(1.3f, 0f, 0f);
        // The desk's Screen slot as delivered (counter space), if the model can't be read: left of the papers, sunk into the desk top.
        private static readonly Bounds FallbackScreen = new Bounds(new Vector3(1.7f, 0.79f, 0.89f), new Vector3(0.087f, 0.064f, 0.517f));
        private static readonly Color Green = new Color(0.61f, 0.88f, 0.35f), Phosphor = new Color(0.02f, 0.06f, 0.03f, 0.96f);

        internal static void Wire(GameObject root, TextMeshProUGUI hint, List<string> log)
        {
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(MonoFont);
            var screen = ScreenBounds(log);
            var normal = ScreenNormal(screen);
            var crt = ClickArea(root.transform, screen, normal);
            var screenStatus = ScreenText(crt, screen, font);

            var hud = hint.transform.parent;
            var old = hud.Find("TerminalWindow");
            if (old != null) Object.DestroyImmediate(old.gameObject);
            var window = new GameObject("TerminalWindow", typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster));
            window.transform.SetParent(hud, false);
            Stretch((RectTransform)window.transform, Vector2.zero, Vector2.one);
            var canvas = window.GetComponent<Canvas>();
            canvas.overrideSorting = true;
            canvas.sortingOrder = 20; // over the seated HUD's own bars

            var panel = Box(window.transform, "Panel", new Vector2(640f, 380f), Vector2.zero);
            panel.color = Phosphor;
            TerminalStyle.Frame(panel);
            var inset = Box(panel.transform, "Screen", new Vector2(600f, 340f), Vector2.zero);
            inset.color = Phosphor;
            TerminalStyle.Inset(inset);

            Label(panel.transform, "Title", "KESTREL-9 BORDER TERMINAL  //  WANTED LIST · ENTRY RECORD", font, 15f,
                new Vector2(560f, 22f), new Vector2(0f, 152f), TextAlignmentOptions.MidlineLeft);
            TerminalStyle.AddDivider(panel.transform, new Vector2(0f, 136f), 560f);

            var listGo = new GameObject("Queries", typeof(RectTransform), typeof(VerticalLayoutGroup));
            listGo.transform.SetParent(panel.transform, false);
            var list = (RectTransform)listGo.transform;
            list.sizeDelta = new Vector2(190f, 240f);
            list.anchoredPosition = new Vector2(-180f, 0f);
            var layout = listGo.GetComponent<VerticalLayoutGroup>();
            layout.spacing = 6f;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            var template = Row(list, font);

            var output = Label(panel.transform, "Output", "", font, 13f, new Vector2(350f, 240f), new Vector2(105f, 0f), TextAlignmentOptions.TopLeft);
            output.richText = true;
            var status = Label(panel.transform, "Status", "", font, 13f, new Vector2(400f, 22f), new Vector2(-80f, -150f), TextAlignmentOptions.MidlineLeft);
            var close = Row(panel.transform, font);
            close.name = "Close";
            close.gameObject.SetActive(true);
            close.GetComponentInChildren<TMP_Text>().text = "CLOSE [Q]";
            var closeRect = (RectTransform)close.transform;
            closeRect.sizeDelta = new Vector2(130f, 26f);
            closeRect.anchoredPosition = new Vector2(225f, -150f);
            window.SetActive(false);

            var terminal = root.GetComponent<CounterTerminal>();
            if (terminal == null) terminal = root.AddComponent<CounterTerminal>();
            var so = new SerializedObject(terminal);
            so.FindProperty("station").objectReferenceValue = root.GetComponent<CounterStation>();
            so.FindProperty("shift").objectReferenceValue = root.GetComponent<CounterShift>();
            so.FindProperty("tuning").objectReferenceValue = Tuning();
            so.FindProperty("window").objectReferenceValue = window;
            so.FindProperty("output").objectReferenceValue = output;
            so.FindProperty("status").objectReferenceValue = status;
            so.FindProperty("queryList").objectReferenceValue = list;
            so.FindProperty("queryTemplate").objectReferenceValue = template;
            so.FindProperty("closeButton").objectReferenceValue = close;
            so.FindProperty("screenStatus").objectReferenceValue = screenStatus;
            so.ApplyModifiedPropertiesWithoutUndo();

            // A paper dropped onto the CRT lands on the first spot instead (like the gadgets).
            var desk = new SerializedObject(root.GetComponent<DocumentDesk>());
            var clear = desk.FindProperty("keepClear");
            clear.arraySize++;
            clear.GetArrayElementAtIndex(clear.arraySize - 1).objectReferenceValue = crt.GetComponent<BoxCollider>();
            desk.ApplyModifiedPropertiesWithoutUndo();

            hint.text += "  ·  click the CRT: terminal";
            log.Add($"counter terminal (P34): CRT at {crt.transform.localPosition}, window in the seated HUD");
        }

        /// <summary>The Screen material slot of the delivered desk in counter space.</summary>
        private static Bounds ScreenBounds(List<string> log)
        {
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(DeskModel);
            if (model != null)
                foreach (var filter in model.GetComponentsInChildren<MeshFilter>(true))
                {
                    var mesh = filter.sharedMesh;
                    var renderer = filter.GetComponent<MeshRenderer>();
                    if (mesh == null || renderer == null) continue;
                    int slot = -1;
                    var materials = renderer.sharedMaterials;
                    for (int i = 0; i < materials.Length && i < mesh.subMeshCount; i++)
                        if (materials[i] != null && materials[i].name.ToLowerInvariant().Contains("screen")) slot = i;
                    if (slot < 0 && mesh.subMeshCount > 1) slot = 1; // delivered order: Desk, Screen
                    if (slot < 0) continue;
                    var local = mesh.GetSubMesh(slot).bounds;
                    var toModel = model.transform.worldToLocalMatrix * filter.transform.localToWorldMatrix;
                    var bounds = new Bounds(toModel.MultiplyPoint3x4(local.center), Vector3.zero);
                    var e = local.extents;
                    for (int c = 0; c < 8; c++)
                        bounds.Encapsulate(toModel.MultiplyPoint3x4(local.center + new Vector3((c & 1) == 0 ? -e.x : e.x, (c & 2) == 0 ? -e.y : e.y, (c & 4) == 0 ? -e.z : e.z)));
                    bounds.center += DeskInCounter;
                    return bounds;
                }
            log.Add("WARNING  no Screen slot found in " + DeskModel + " - terminal click area at the delivered spot");
            return FallbackScreen;
        }

        /// <summary>
        /// The screen is a slanted strip along the desk (long side Z); it faces up and toward the officer (-X, the chair is the origin)
        /// across its bounds' diagonal.
        /// </summary>
        private static Vector3 ScreenNormal(Bounds screen) => new Vector3(-screen.size.y, screen.size.x, 0f).normalized;

        /// <summary>The clickable box over the screen. Thick enough to reach above the desk top, whose collider covers the sunk screen.</summary>
        private static GameObject ClickArea(Transform root, Bounds screen, Vector3 normal)
        {
            var old = root.Find("Terminal_CRT");
            if (old != null) Object.DestroyImmediate(old.gameObject);
            var go = new GameObject("Terminal_CRT");
            go.transform.SetParent(root, false);
            go.transform.localPosition = screen.center;
            // Text on the screen reads left to right for the officer (who looks along +X, so right is -Z).
            var right = Vector3.back;
            var forward = -normal;
            go.transform.localRotation = Quaternion.LookRotation(forward, Vector3.Cross(forward, right));
            float slant = new Vector2(screen.size.x, screen.size.y).magnitude;
            const float thickness = 0.12f;
            var box = go.AddComponent<BoxCollider>();
            box.size = new Vector3(screen.size.z, slant, thickness);
            box.center = new Vector3(0f, 0f, -thickness * 0.5f);
            var clickable = go.AddComponent<CounterClickable>();
            var so = new SerializedObject(clickable);
            so.FindProperty("action").enumValueIndex = (int)CounterAction.Terminal;
            so.ApplyModifiedPropertiesWithoutUndo();
            return go;
        }

        /// <summary>The status line in green on the CRT itself (lane and clock), a few millimetres in front of the glass.</summary>
        private static TMP_Text ScreenText(GameObject crt, Bounds screen, TMP_FontAsset font)
        {
            var go = new GameObject("Status", typeof(TextMeshPro));
            go.transform.SetParent(crt.transform, false);
            go.transform.localPosition = new Vector3(0f, 0f, -0.004f);
            var text = go.GetComponent<TextMeshPro>();
            float slant = new Vector2(screen.size.x, screen.size.y).magnitude;
            text.rectTransform.sizeDelta = new Vector2(screen.size.z * 0.92f, slant * 0.8f);
            if (font != null) text.font = font;
            text.text = "LANE 1 CLOSED";
            text.color = Green;
            text.alignment = TextAlignmentOptions.Center;
            text.enableAutoSizing = true;
            text.fontSizeMin = 0.05f;
            text.fontSizeMax = 0.3f;
            text.textWrappingMode = TextWrappingModes.Normal;
            return text;
        }

        private static Button Row(Transform parent, TMP_FontAsset font)
        {
            var go = new GameObject("QueryRow", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            go.transform.SetParent(parent, false);
            go.GetComponent<LayoutElement>().preferredHeight = 26f;
            var image = go.GetComponent<Image>();
            image.color = new Color(0.08f, 0.16f, 0.08f, 1f);
            var button = go.GetComponent<Button>();
            TerminalStyle.Button(button);
            var label = Label(go.transform, "Text", "", font, 13f, Vector2.zero, Vector2.zero, TextAlignmentOptions.MidlineLeft);
            Stretch(label.rectTransform, Vector2.zero, Vector2.one);
            label.rectTransform.offsetMin = new Vector2(10f, 2f);
            label.rectTransform.offsetMax = new Vector2(-6f, -2f);
            label.enableAutoSizing = true; // long alien names stay in the row
            label.fontSizeMin = 8f;
            label.fontSizeMax = 13f;
            label.raycastTarget = false;
            go.SetActive(false);
            return button;
        }

        private static Image Box(Transform parent, string name, Vector2 size, Vector2 position)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
            return go.GetComponent<Image>();
        }

        private static TextMeshProUGUI Label(Transform parent, string name, string content, TMP_FontAsset font, float size, Vector2 box,
            Vector2 position, TextAlignmentOptions alignment)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var text = go.GetComponent<TextMeshProUGUI>();
            text.rectTransform.sizeDelta = box;
            text.rectTransform.anchoredPosition = position;
            if (font != null) text.font = font;
            text.text = content;
            text.fontSize = size;
            text.color = Green;
            text.alignment = alignment;
            text.raycastTarget = false;
            return text;
        }

        private static void Stretch(RectTransform rect, Vector2 min, Vector2 max)
        {
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }

        private static TerminalTuning Tuning()
        {
            var asset = AssetDatabase.LoadAssetAtPath<TerminalTuning>(TuningPath);
            if (asset != null) return asset;
            AssetFolders.EnsureForAsset(TuningPath);
            asset = ScriptableObject.CreateInstance<TerminalTuning>();
            AssetDatabase.CreateAsset(asset, TuningPath);
            return asset;
        }
    }
}
