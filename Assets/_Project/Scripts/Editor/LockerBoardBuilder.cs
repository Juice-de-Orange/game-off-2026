using System.Collections.Generic;
using System.Linq;
using GameOff.Counter;
using GameOff.Economy;
using GameOff.Player;
using GameOff.Shops;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static GameOff.EditorTools.MenuSceneBuilder;

namespace GameOff.EditorTools
{
    /// <summary>
    /// P32 + P33 rest, called from EconomyBuilder (GameOff/Build Economy into Main), everything inside PF_Economy:
    /// - the open evidence locker over SM_Counter_Locker: shell and two hinged doors cut from that model (P135, Mesh_LockerOpen.asset),
    ///   the interior with six compartments, a sealed evidence bag for goods without a model (EvidenceLockerDisplay), wired to the
    ///   EvidenceLocker of P83;
    /// - the pinboard close-up while standing: a trigger in front of SM_Counter_Board and a screen with up to four notes (PinBoardCloseUp);
    /// - Data/Economy/EvidenceLockerTuning.asset (created once; the label → model list is only filled while it is empty).
    /// Positions mirror StationGrayboxBuilder.BuildProps: locker 1.0 x 0.5 x 1.8 m, pivot bottom centre at (-4, 0, -6.45), front to -Z;
    /// board 1.0 x 0.7 m centred at (-1.5, 1.75), front at z -6.255.
    /// </summary>
    internal static class LockerBoardBuilder
    {
        private const string TuningPath = "Assets/_Project/Data/Economy/EvidenceLockerTuning.asset";
        private const string TagFolder = "Assets/_Project/Data/Items/Tags/";
        private const string ItemModels = "Assets/_Project/Art/Models/Items/";
        private const string LockerMaterial = "Assets/_Project/Art/Materials/Counter/M_Counter_Locker.mat";
        private const string GrayboxMaterials = "Assets/_Project/Art/Materials/Graybox/";
        private const string BodyFontPath = "Assets/_Project/Fonts/ShareTechMono SDF.asset";
        private const string ClosedLockerName = "SM_Counter_Locker";
        private const string InteriorModel = "Assets/_Project/Art/Models/Counter/SM_Counter_LockerInterior.fbx";
        private const string ClosedModel = "Assets/_Project/Art/Models/Counter/SM_Counter_Locker.fbx";
        private const string OpenMeshPath = "Assets/_Project/Art/Models/Counter/Mesh_LockerOpen.asset"; // P135, generated
        private const float OpeningInset = 0.025f; // the interior's side panels stand ~3 cm inside the shell: the frame covers the gap
        private static readonly Vector3 ProbeAnchorOffset = new Vector3(0f, 1.2f, -1.5f); // in the room, among its light probes
        private const string BagModel = "Assets/_Project/Art/Models/Counter/SM_Item_EvidenceBag.fbx";
        private const string HandleMaterial = "Assets/_Project/Art/Materials/Hangar/M_Hangar_CraneRail.mat";

        private static readonly Vector3 LockerPivot = new Vector3(-4f, 0f, -6.45f);
        private const float LockerWidth = 1.0f, LockerDepth = 0.5f, LockerHeight = 1.8f, Wall = 0.03f;
        private static readonly float[] ShelfTops = { 0.1f, 0.6625f, 1.2125f }; // bottom, middle, top compartment floors

        private static readonly Vector3 BoardCenter = new Vector3(-1.5f, 1.75f, -6.255f);

        private static readonly Color Cork = new Color(0.16f, 0.12f, 0.09f, 0.94f);
        private static readonly Color Paper = new Color(0.89f, 0.87f, 0.81f);
        private static readonly Color Ink = new Color(0.12f, 0.12f, 0.14f);
        private static readonly Color InkDim = new Color(0.35f, 0.35f, 0.38f);
        private static readonly Color PinRed = new Color(0.78f, 0.2f, 0.18f);

        // ---------- Tuning ----------

        /// <summary>The tuning asset; labels of goods without a model get a related model (first match wins), the rest a sealed bag.</summary>
        internal static EvidenceLockerTuning EnsureTuning(List<string> log)
        {
            var tuning = AssetDatabase.LoadAssetAtPath<EvidenceLockerTuning>(TuningPath);
            if (tuning == null)
            {
                tuning = ScriptableObject.CreateInstance<EvidenceLockerTuning>();
                AssetDatabase.CreateAsset(tuning, TuningPath);
                log.Add("EvidenceLockerTuning created");
            }
            if (tuning.tagModels.Count == 0)
            {
                foreach (var (tag, model) in new[]
                         {
                             ("WEAPON", "SM_Item_WeaponParts"), ("MEDICINE", "SM_Item_PillBottle"), ("DRUG", "SM_Item_PillBottle"),
                             ("ALIEN", "SM_Item_AlienSpecimen"), ("TECH", "SM_Item_DataChip"),
                         })
                {
                    var t = AssetDatabase.LoadAssetAtPath<ItemTag>(TagFolder + "Tag_" + tag + ".asset");
                    var m = AssetDatabase.LoadAssetAtPath<GameObject>(ItemModels + model + ".fbx");
                    if (t != null && m != null) tuning.tagModels.Add(new LockerTagModel(t, m));
                }
                EditorUtility.SetDirty(tuning);
                log.Add($"locker label models: {tuning.tagModels.Count}");
            }
            return tuning;
        }

        // ---------- Open locker (P32) ----------

        /// <summary>
        /// Builds the open locker under the EvidenceLocker object and wires display and tuning into it.
        /// P135: shell and doors are cut from the closed model itself (LockerMeshSplit), with its material and lightmap UVs; the display
        /// gives them the closed model's baked light, so opening changes nothing but the doors' angle. The interior and the goods take
        /// the room's light probes in front of the locker; no lamp of its own any more.
        /// </summary>
        internal static void BuildLockerDisplay(Transform lockerRoot, EvidenceLocker locker, Confiscation confiscation,
            EvidenceLockerTuning tuning)
        {
            var go = new GameObject("OpenLocker");
            go.transform.SetParent(lockerRoot, false);
            go.transform.position = LockerPivot;
            var openRoot = new GameObject("Open").transform;
            openRoot.SetParent(go.transform, false);
            // The probe inside the closed locker sees only its inside and is dark; the room's probes in front of it are not.
            var probeAnchor = new GameObject("ProbeAnchor").transform;
            probeAnchor.SetParent(go.transform, false);
            probeAnchor.localPosition = ProbeAnchorOffset;

            var closedModel = AssetDatabase.LoadAssetAtPath<GameObject>(ClosedModel);
            var closedRenderer = closedModel != null ? closedModel.GetComponentInChildren<MeshRenderer>(true) : null;
            var outside = (closedRenderer != null ? closedRenderer.sharedMaterial : null)
                          ?? AssetDatabase.LoadAssetAtPath<Material>(LockerMaterial) ?? Graybox("M_Graybox_Prop");

            var interior = new GameObject("Interior").transform;
            interior.SetParent(openRoot, false);
            var interiorModel = Model(interior, InteriorModel); // P118: back wall, divider and shelves; pivot bottom centre, floors at ShelfTops
            foreach (var r in interiorModel.GetComponentsInChildren<Renderer>(true))
            {
                r.probeAnchor = probeAnchor;
                PrefabUtility.RecordPrefabInstancePropertyModifications(r);
            }

            var cut = LockerMeshSplit.Split(LockerMeshSplit.Read(closedModel), OpeningInset);
            Transform leftDoor, rightDoor;
            var sameLight = new List<Object>();
            float slotZ = 0f;
            if (cut != null)
            {
                var meshes = SaveOpenMeshes(cut);
                sameLight.Add(MeshPiece(openRoot, "Shell", meshes[0], outside, probeAnchor));
                leftDoor = new GameObject("DoorL").transform;
                leftDoor.SetParent(openRoot, false);
                leftDoor.localPosition = cut.LeftHinge;
                sameLight.Add(MeshPiece(leftDoor, "Leaf", meshes[1], outside, probeAnchor));
                rightDoor = new GameObject("DoorR").transform;
                rightDoor.SetParent(openRoot, false);
                rightDoor.localPosition = cut.RightHinge;
                sameLight.Add(MeshPiece(rightDoor, "Leaf", meshes[2], outside, probeAnchor));
                // The interior is deeper than the closed body: squeeze it between the opening and the back, so the closed doors
                // never cut through the shelves' front lips and its back wall stays in front of the station wall.
                float front = cut.FrontZ + 0.003f, back = cut.BackZ - 0.005f;
                FitDepth(interior, openRoot, front, back);
                slotZ = (front + back) * 0.5f;
            }
            else
            {
                Debug.LogWarning("[LockerBoardBuilder] SM_Counter_Locker could not be cut into shell and doors: the open locker falls back to boxes.");
                float w = LockerWidth, d = LockerDepth, h = LockerHeight, halfW = w / 2f, halfD = d / 2f;
                Block(openRoot, "SideL", new Vector3(-halfW + Wall / 2f, h / 2f, 0f), new Vector3(Wall, h, d), outside, probeAnchor);
                Block(openRoot, "SideR", new Vector3(halfW - Wall / 2f, h / 2f, 0f), new Vector3(Wall, h, d), outside, probeAnchor);
                Block(openRoot, "Top", new Vector3(0f, h - Wall / 2f, 0f), new Vector3(w, Wall, d), outside, probeAnchor);
                Block(openRoot, "Plinth", new Vector3(0f, ShelfTops[0] / 2f, 0f), new Vector3(w, ShelfTops[0], d), outside, probeAnchor);
                // Doors: hinged at the front corners, the leaf a little in front of the shell so it never cuts into the sides.
                leftDoor = Hinge(openRoot, "DoorL", new Vector3(-halfW, 0f, -halfD - 0.015f), +1f, outside, probeAnchor);
                rightDoor = Hinge(openRoot, "DoorR", new Vector3(halfW, 0f, -halfD - 0.015f), -1f, outside, probeAnchor);
            }

            // Compartments, filled eye level first: middle, top, bottom; left before right.
            var slots = new List<Transform>();
            float column = (LockerWidth - 2f * Wall - 0.02f) / 4f + 0.01f;
            foreach (int row in new[] { 1, 2, 0 })
                foreach (float side in new[] { -1f, 1f })
                {
                    var slot = new GameObject($"Slot_{row}{(side < 0 ? "L" : "R")}").transform;
                    slot.SetParent(openRoot, false);
                    slot.localPosition = new Vector3(side * column, ShelfTops[row], slotZ);
                    slots.Add(slot);
                }

            var bag = EvidenceBag(go.transform);

            var display = go.AddComponent<EvidenceLockerDisplay>();
            Set(display, "tuning", tuning);
            Set(display, "confiscation", confiscation);
            Set(display, "openRoot", openRoot.gameObject);
            Set(display, "leftDoor", leftDoor);
            Set(display, "rightDoor", rightDoor);
            SetArray(display, "slots", slots.Cast<Object>().ToArray());
            Set(display, "evidenceBag", bag);
            SetArray(display, "sameLightAsClosed", sameLight.ToArray());
            Set(display, "probeAnchor", probeAnchor);
            openRoot.gameObject.SetActive(false);

            Set(locker, "display", display);
            Set(locker, "tuning", tuning);
        }

        /// <summary>P135: the cut meshes as sub-assets of one file, updated in place so the prefab's references and GUID stay.</summary>
        private static Mesh[] SaveOpenMeshes(LockerMeshSplit.Result cut)
        {
            var parts = new[] { (name: "LockerOpen_Shell", part: cut.Shell), (name: "LockerOpen_DoorL", part: cut.LeftDoor), (name: "LockerOpen_DoorR", part: cut.RightDoor) };
            var existing = AssetDatabase.LoadAllAssetsAtPath(OpenMeshPath).OfType<Mesh>().ToList();
            var meshes = new Mesh[parts.Length];
            for (int i = 0; i < parts.Length; i++)
            {
                var mesh = existing.FirstOrDefault(m => m.name == parts[i].name);
                bool created = mesh == null;
                if (created) mesh = new Mesh { name = parts[i].name };
                parts[i].part.Fill(mesh, cut.HasUv2);
                if (created && AssetDatabase.LoadMainAssetAtPath(OpenMeshPath) == null)
                {
                    AssetFolders.EnsureForAsset(OpenMeshPath);
                    AssetDatabase.CreateAsset(mesh, OpenMeshPath);
                }
                else if (created) AssetDatabase.AddObjectToAsset(mesh, OpenMeshPath);
                else EditorUtility.SetDirty(mesh);
                meshes[i] = mesh;
            }
            AssetDatabase.SaveAssets(); // the prefab saved after this refers to the sub-assets: they must be on disk
            return meshes;
        }

        private static Renderer MeshPiece(Transform parent, string name, Mesh mesh, Material material, Transform probeAnchor)
        {
            var go = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
            go.transform.SetParent(parent, false);
            go.GetComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = go.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.probeAnchor = probeAnchor;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; // the closed one's shadows are baked
            return renderer;
        }

        /// <summary>Scales the interior along Z so that its front sits at <paramref name="front"/> and its back at <paramref name="back"/>.</summary>
        private static void FitDepth(Transform interior, Transform space, float front, float back)
        {
            float min = float.MaxValue, max = float.MinValue;
            foreach (var filter in interior.GetComponentsInChildren<MeshFilter>(true))
            {
                if (filter.sharedMesh == null) continue;
                var toSpace = space.worldToLocalMatrix * filter.transform.localToWorldMatrix;
                var b = filter.sharedMesh.bounds;
                for (int c = 0; c < 8; c++)
                {
                    var corner = new Vector3((c & 1) == 0 ? b.min.x : b.max.x, (c & 2) == 0 ? b.min.y : b.max.y, (c & 4) == 0 ? b.min.z : b.max.z);
                    float z = toSpace.MultiplyPoint3x4(corner).z;
                    min = Mathf.Min(min, z);
                    max = Mathf.Max(max, z);
                }
            }
            if (max - min < 0.01f || back - front < 0.01f) return;
            float scale = (back - front) / (max - min);
            interior.localScale = new Vector3(1f, 1f, scale);
            interior.localPosition = new Vector3(0f, 0f, front - min * scale);
        }

        private static Transform Hinge(Transform parent, string name, Vector3 position, float side, Material material, Transform probeAnchor)
        {
            var hinge = new GameObject(name).transform;
            hinge.SetParent(parent, false);
            hinge.localPosition = position;
            float leaf = LockerWidth / 2f - 0.005f;
            Block(hinge, "Leaf", new Vector3(side * leaf / 2f, LockerHeight / 2f, 0f), new Vector3(leaf, LockerHeight - 0.02f, 0.025f), material, probeAnchor);
            Block(hinge, "Handle", new Vector3(side * (leaf - 0.05f), 1.0f, -0.025f), new Vector3(0.02f, 0.16f, 0.025f),
                AssetDatabase.LoadAssetAtPath<Material>(HandleMaterial) ?? material, probeAnchor);
            return hinge;
        }

        /// <summary>Goods without a model: the delivered sealed bag (inactive template).</summary>
        private static GameObject EvidenceBag(Transform parent)
        {
            var bag = Model(parent, BagModel);
            bag.name = "EvidenceBag";
            bag.SetActive(false);
            return bag;
        }

        private static GameObject Model(Transform parent, string path)
        {
            var model = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(path));
            model.transform.SetParent(parent, false);
            return model;
        }

        private static void Block(Transform parent, string name, Vector3 center, Vector3 size, Material material, Transform probeAnchor)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = name;
            Object.DestroyImmediate(cube.GetComponent<Collider>());
            cube.transform.SetParent(parent, false);
            cube.transform.localPosition = center;
            cube.transform.localScale = size;
            var renderer = cube.GetComponent<MeshRenderer>();
            if (material != null) renderer.sharedMaterial = material;
            renderer.probeAnchor = probeAnchor;
        }

        private static Material Graybox(string name) => AssetDatabase.LoadAssetAtPath<Material>(GrayboxMaterials + name + ".mat");

        // ---------- Pinboard close-up (P33) ----------

        /// <summary>Trigger in front of the board, the close-up screen on the economy canvas and PinBoardCloseUp on the trigger.</summary>
        internal static void BuildBoardCloseUp(Transform parent, Transform canvas)
        {
            var go = new GameObject("PinBoardCloseUp");
            go.transform.SetParent(parent, false);
            var trigger = new GameObject("Trigger", typeof(BoxCollider));
            trigger.transform.SetParent(go.transform, false);
            trigger.transform.position = BoardCenter + new Vector3(0f, 0f, -0.12f);
            var box = trigger.GetComponent<BoxCollider>();
            box.isTrigger = true;
            box.size = new Vector3(1.1f, 0.8f, 0.24f);
            var interactable = trigger.AddComponent<Interactable>();
            var iso = new SerializedObject(interactable);
            iso.FindProperty("prompt").stringValue = "Read the pinboard";
            iso.ApplyModifiedPropertiesWithoutUndo();

            var root = new GameObject("PinBoardScreen", typeof(RectTransform), typeof(Image));
            root.transform.SetParent(canvas, false);
            Stretch(root.GetComponent<RectTransform>());
            root.GetComponent<Image>().color = Cork;

            var heading = Label(root.transform, "Heading", "PINBOARD", 20, Amber, new Vector2(0f, 240f), new Vector2(900f, 30f));
            var empty = Label(root.transform, "Empty", "Nothing pinned up yet.", 16, TextDim, Vector2.zero, new Vector2(600f, 40f));

            var cards = new List<Object>();
            for (int i = 0; i < 4; i++) cards.Add(Card(root.transform, i));

            var close = MenuButton(root.transform, "CloseButton", "CLOSE", new Vector2(330f, -240f), new Vector2(150f, 30f), 14);
            // P140: no key line any more; the keys are behind [H] (HelpOverlay).

            var closeUp = trigger.AddComponent<PinBoardCloseUp>();
            Set(closeUp, "interactable", interactable);
            Set(closeUp, "root", root);
            Set(closeUp, "heading", heading);
            Set(closeUp, "empty", empty);
            SetArray(closeUp, "cards", cards.ToArray());
            Set(closeUp, "closeButton", close);
            root.SetActive(false);
        }

        /// <summary>One paper: pin, title, date, text and a row of four photo frames (shown on the wanted list only).</summary>
        private static PinNoteCard Card(Transform parent, int index)
        {
            var bodyFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(BodyFontPath);
            var go = new GameObject("Note" + index, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(-345f + index * 230f, -5f);
            rt.sizeDelta = new Vector2(215f, 420f);
            rt.localRotation = Quaternion.Euler(0f, 0f, index % 2 == 0 ? 1.2f : -0.9f); // pinned by hand, not quite straight
            go.GetComponent<Image>().color = Paper;

            var pin = new GameObject("Pin", typeof(RectTransform), typeof(Image));
            pin.transform.SetParent(go.transform, false);
            var pinRt = pin.GetComponent<RectTransform>();
            pinRt.anchoredPosition = new Vector2(0f, 200f);
            pinRt.sizeDelta = new Vector2(12f, 12f);
            pin.GetComponent<Image>().color = PinRed;

            var title = Label(go.transform, "Title", "", 15, Ink, new Vector2(0f, 178f), new Vector2(195f, 24f));
            var date = Label(go.transform, "Date", "", 10, InkDim, new Vector2(0f, 158f), new Vector2(195f, 16f));
            var body = Label(go.transform, "Body", "", 11, Ink, new Vector2(0f, 30f), new Vector2(195f, 250f));
            body.alignment = TextAlignmentOptions.TopLeft;
            body.fontSizeMin = 6f;
            body.richText = true;
            if (bodyFont != null) { body.font = bodyFont; date.font = bodyFont; }

            var row = new GameObject("Photos", typeof(RectTransform));
            row.transform.SetParent(go.transform, false);
            row.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -155f);
            var photos = new List<Object>();
            var names = new List<Object>();
            var noPhoto = new List<Object>();
            for (int i = 0; i < 4; i++)
            {
                var frame = new GameObject("Frame" + i, typeof(RectTransform), typeof(Image));
                frame.transform.SetParent(row.transform, false);
                var frt = frame.GetComponent<RectTransform>();
                frt.anchoredPosition = new Vector2(-75f + i * 50f, 10f);
                frt.sizeDelta = new Vector2(44f, 56f);
                frame.GetComponent<Image>().color = InkDim;
                var none = Label(frame.transform, "NoPhoto", "NO\nPHOTO", 8, Paper, Vector2.zero, new Vector2(40f, 40f));
                var photo = new GameObject("Photo", typeof(RectTransform), typeof(RawImage));
                photo.transform.SetParent(frame.transform, false);
                photo.GetComponent<RectTransform>().sizeDelta = new Vector2(40f, 52f);
                photo.GetComponent<RawImage>().raycastTarget = false;
                var name = Label(frame.transform, "Name", "", 7, Ink, new Vector2(0f, -36f), new Vector2(48f, 14f));
                if (bodyFont != null) name.font = bodyFont;
                photos.Add(photo.GetComponent<RawImage>());
                names.Add(name);
                noPhoto.Add(none);
            }

            var card = go.AddComponent<PinNoteCard>();
            Set(card, "title", title);
            Set(card, "date", date);
            Set(card, "body", body);
            Set(card, "photoRow", row);
            SetArray(card, "photos", photos.ToArray());
            SetArray(card, "photoNames", names.ToArray());
            SetArray(card, "noPhoto", noPhoto.ToArray());
            return card;
        }

        // ---------- Scene wiring ----------

        /// <summary>
        /// In the scene: the closed locker model of the station (hidden while the open one shows), the player and the counter's pinboard.
        /// Returns what could not be found, empty if all is wired.
        /// </summary>
        internal static string WireInScene(GameObject instance, Scene scene, FirstPersonController player)
        {
            var missing = new List<string>();
            var closed = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Transform>(true))
                .FirstOrDefault(t => t.name == ClosedLockerName);
            if (closed == null) missing.Add(ClosedLockerName);
            foreach (var display in instance.GetComponentsInChildren<EvidenceLockerDisplay>(true))
            {
                SetArray(display, "closedRenderers", closed != null ? closed.GetComponentsInChildren<Renderer>(true).Cast<Object>().ToArray() : new Object[0]);
                PrefabUtility.RecordPrefabInstancePropertyModifications(display);
            }
            var board = Object.FindFirstObjectByType<PinBoard>();
            if (board == null) missing.Add("PinBoard");
            foreach (var closeUp in instance.GetComponentsInChildren<PinBoardCloseUp>(true))
            {
                Set(closeUp, "player", player);
                Set(closeUp, "board", board);
                PrefabUtility.RecordPrefabInstancePropertyModifications(closeUp);
            }
            return missing.Count == 0 ? "" : "missing: " + string.Join(", ", missing);
        }
    }
}
