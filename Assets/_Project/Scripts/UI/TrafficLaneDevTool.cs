using System.Collections.Generic;
using System.Globalization;
using GameOff.Core;
using GameOff.Player;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace GameOff.UI
{
    /// <summary>
    /// Traffic lane dev tool (P56). With Options → DEV → TRAFFIC_LINE_DEV on (its own switch, independent of DEV TOOLS), every lane of the ShipTraffic
    /// shows as a coloured line as thick as its ship, inside a translucent box for the random spread, with a name tag on
    /// screen; lanes closer than minClearance to the station blink red. A panel on the right edits the selected lane live
    /// (new flights use the new values). While the mode is on the mouse is free for the panel (the player stands still);
    /// Tab switches to looking around / walking and back.
    /// SAVE LANE keeps the selected lane: in the editor it writes the tuning asset at once (unsaved edits are also kept
    /// when play mode ends); in a build it copies the lane as JSON for GameOff → Paste Traffic Lanes in Unity.
    /// COPY ALL copies every lane the same way. RESET goes back to the last saved state.
    /// The panel is built in code: it is a dev tool, not part of the game's UI.
    /// </summary>
    public class TrafficLaneDevTool : MonoBehaviour
    {
        [SerializeField] private ShipTraffic traffic;
        [SerializeField] private FirstPersonController player;
        [Tooltip("Material with shader GameOff/DevLine (no fog, drawn on top).")]
        [SerializeField] private Material lineMaterial;
        [Tooltip("Station + exterior bounds, measured by TrafficBuilder (for the clearance warning).")]
        [SerializeField] private Bounds stationBounds;
        [SerializeField] private float minClearance = 15f;
        [SerializeField] private float timeLapse = 4f;

        private static readonly Color PanelColor = new Color(0.08f, 0.09f, 0.11f, 0.92f);
        private static readonly Color ButtonColor = new Color(0.17f, 0.2f, 0.24f);
        private static readonly Color TextColor = new Color(0.89f, 0.87f, 0.81f);
        private static readonly Color DimColor = new Color(0.55f, 0.56f, 0.59f);
        private static readonly Color WarnColor = new Color(1f, 0.25f, 0.2f);

        private struct Snapshot
        {
            public Vector3 from, to, offset;
            public float scale;
            public Vector2 speed, interval;
        }

        private class LaneView
        {
            public TrafficLane lane;
            public LineRenderer line;
            public Transform box;
            public Material boxMaterial;
            public TextMeshProUGUI tag;
            public Image button;
            public float shipWidth;
            public float clearance;
            public Snapshot original;
        }

        private class Control
        {
            public Slider slider;
            public TextMeshProUGUI value;
            public System.Func<TrafficLane, float> get;
            public System.Action<TrafficLane, float> set;
            public string format;
        }

        private readonly List<LaneView> _views = new List<LaneView>();
        private readonly List<Control> _controls = new List<Control>();
        private GameObject _root;
        private CanvasGroup _panelGroup;
        private TextMeshProUGUI _info;
        private TextMeshProUGUI _timeLapseLabel;
        private TextMeshProUGUI _modeHint;
        private InputAction _toggleMouse;
        private int _selected;
        private bool _look; // Tab: player looks around instead of a free mouse
        private bool _fast;
        private bool _active;
        private bool _tookControl; // only give back what this tool took: the counter and the letter own the control otherwise

        private void Awake()
        {
            _toggleMouse = new InputAction("TrafficDevMouse", InputActionType.Button, "<Keyboard>/tab");
            if (traffic == null || traffic.Tuning == null) enabled = false;
            // The panel (about 130 objects) is built the first time the tool is switched on: most players never do.
        }

        private void OnEnable()
        {
            GameSettings.Changed += Apply;
            _toggleMouse.Enable();
            Apply();
        }

        private void OnDisable()
        {
            GameSettings.Changed -= Apply;
            _toggleMouse.Disable();
            SetActive(false);
        }

        private void OnDestroy()
        {
            _toggleMouse.Dispose();
            foreach (var v in _views)
                if (v.boxMaterial != null) Destroy(v.boxMaterial);
#if UNITY_EDITOR
            if (traffic != null && traffic.Tuning != null) UnityEditor.AssetDatabase.SaveAssetIfDirty(traffic.Tuning);
#endif
        }

        private void Apply() => SetActive(GameSettings.TrafficLineDev);

        private void SetActive(bool on)
        {
            if (on == _active) return;
            if (_root == null)
            {
                if (!on) return;
                BuildViews();
                BuildPanel();
            }
            _active = on;
            _root.SetActive(on);
            _look = false;
            if (!on)
            {
                // Paused (switched off in the options): Resume gives the control back itself.
                if (player != null && _tookControl && Time.timeScale > 0f) player.SetControlEnabled(true);
                _tookControl = false;
                _fast = false;
                traffic.SpeedFactor = 1f;
            }
            Refresh();
        }

        private void Update()
        {
            if (!_active) return;
            bool paused = Time.timeScale == 0f; // the pause menu owns the mouse then
            if (player != null && !paused)
            {
                if (_toggleMouse.WasPressedThisFrame())
                {
                    // Only what this tool holds: with the counter or a window in charge, Tab must not hand control to the player.
                    if (_look && player.ControlEnabled)
                    {
                        _look = false;
                        _tookControl = true;
                        player.SetControlEnabled(false);
                    }
                    else if (!_look && _tookControl)
                    {
                        _look = true;
                        _tookControl = false;
                        player.SetControlEnabled(true);
                    }
                }
                // Free mouse whenever the mode is on – also after Resume, a closed window or standing up from the counter.
                else if (!_look && player.ControlEnabled)
                {
                    _tookControl = true;
                    player.SetControlEnabled(false);
                }
            }
            bool mouse = !_look;
            _panelGroup.interactable = mouse;
            _panelGroup.alpha = mouse ? 1f : 0.8f;
            _modeHint.text = mouse ? "TAB: look around / walk" : "TAB: mouse for the panel";

            float blink = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 8f);
            var cam = Camera.main;
            foreach (var v in _views)
            {
                bool warn = v.clearance < minClearance;
                var c = warn ? Color.Lerp(v.lane.devColor, WarnColor, blink) : v.lane.devColor;
                c.a = v == _views[_selected] ? 0.55f : 0.3f;
                v.line.startColor = v.line.endColor = c;
                PlaceTag(v, cam, warn);
            }
        }

        // ---------- lanes in the world ----------

        private void BuildViews()
        {
            _root = new GameObject("TrafficLaneDev");
            _root.transform.SetParent(transform, false);
            foreach (var lane in traffic.Tuning.lanes)
            {
                var v = new LaneView { lane = lane, shipWidth = ShipWidth(lane), original = Take(lane) };
                var lineGo = new GameObject("Line " + lane.name);
                lineGo.transform.SetParent(_root.transform, false);
                v.line = lineGo.AddComponent<LineRenderer>();
                v.line.useWorldSpace = true;
                v.line.positionCount = 2;
                v.line.sharedMaterial = lineMaterial;
                v.line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                v.line.receiveShadows = false;

                var box = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Destroy(box.GetComponent<Collider>());
                box.name = "Spread " + lane.name;
                box.transform.SetParent(_root.transform, false);
                var r = box.GetComponent<MeshRenderer>();
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                r.receiveShadows = false;
                v.boxMaterial = new Material(lineMaterial);
                r.sharedMaterial = v.boxMaterial;
                v.box = box.transform;
                _views.Add(v);
            }
        }

        /// <summary>Widest cross-section of the lane's ship models (their nose points along z), in metres at scale 1.</summary>
        private static float ShipWidth(TrafficLane lane)
        {
            float width = 0f;
            foreach (var ship in lane.ships)
            {
                if (ship == null) continue;
                foreach (var mf in ship.GetComponentsInChildren<MeshFilter>(true))
                {
                    if (mf.sharedMesh == null) continue;
                    var size = Vector3.Scale(mf.sharedMesh.bounds.size, mf.transform.lossyScale);
                    width = Mathf.Max(width, size.x, size.y);
                }
            }
            return Mathf.Max(1f, width);
        }

        private void UpdateView(LaneView v)
        {
            var lane = v.lane;
            float width = v.shipWidth * lane.scale;
            v.line.SetPosition(0, lane.from);
            v.line.SetPosition(1, lane.to);
            v.line.widthMultiplier = width;

            var dir = lane.to - lane.from;
            float sideways = Mathf.Max(lane.randomOffset.x, lane.randomOffset.z);
            v.box.position = (lane.from + lane.to) * 0.5f;
            v.box.rotation = dir.sqrMagnitude > 0.01f ? Quaternion.LookRotation(dir, Vector3.up) : Quaternion.identity;
            v.box.localScale = new Vector3(width + 2f * sideways, width + 2f * lane.randomOffset.y, dir.magnitude);
            var c = lane.devColor;
            c.a = 0.07f;
            v.boxMaterial.color = c;

            float min = float.MaxValue;
            for (int i = 0; i <= 50; i++)
                min = Mathf.Min(min, Mathf.Sqrt(stationBounds.SqrDistance(Vector3.Lerp(lane.from, lane.to, i / 50f))));
            v.clearance = min - Mathf.Max(lane.randomOffset.y, sideways) - width * 0.5f;
        }

        /// <summary>Name tag at the point of the lane closest to the middle of the screen.</summary>
        private void PlaceTag(LaneView v, Camera cam, bool warn)
        {
            if (cam == null) { v.tag.gameObject.SetActive(false); return; }
            var centre = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            Vector3 best = Vector3.zero;
            float bestDistance = float.MaxValue;
            for (int i = 0; i <= 20; i++)
            {
                var p = cam.WorldToScreenPoint(Vector3.Lerp(v.lane.from, v.lane.to, i / 20f));
                if (p.z <= 0f) continue;
                float d = Vector2.Distance(p, centre);
                if (d < bestDistance) { bestDistance = d; best = p; }
            }
            bool visible = bestDistance < float.MaxValue;
            v.tag.gameObject.SetActive(visible);
            if (!visible) return;
            float px = Screen.height / 720f; // canvas scales with the screen height
            v.tag.transform.position = best + Vector3.up * ((24f + 20f * _views.IndexOf(v)) * px); // stacked: lanes often overlap on screen
            v.tag.text = warn ? $"{v.lane.name.ToUpperInvariant()}  TOO CLOSE ({v.clearance:0} m)" : v.lane.name.ToUpperInvariant();
            v.tag.color = warn ? WarnColor : v.lane.devColor;
        }

        // ---------- lane geometry as sliders ----------

        private static Vector3 Centre(TrafficLane l) => (l.from + l.to) * 0.5f;
        private static float Length(TrafficLane l) => new Vector2(l.to.x - l.from.x, l.to.z - l.from.z).magnitude;
        private static float Tilt(TrafficLane l) => l.to.y - l.from.y;
        private static float Yaw(TrafficLane l) => Mathf.Atan2(l.to.z - l.from.z, l.to.x - l.from.x) * Mathf.Rad2Deg;

        /// <summary>Rebuilds from/to: centre, horizontal length, rotation from the east-west axis, height difference.</summary>
        private static void SetShape(TrafficLane l, Vector3 centre, float length, float yaw, float tilt)
        {
            var dir = new Vector3(Mathf.Cos(yaw * Mathf.Deg2Rad), 0f, Mathf.Sin(yaw * Mathf.Deg2Rad));
            var half = dir * (length * 0.5f) + Vector3.up * (tilt * 0.5f);
            l.from = centre - half;
            l.to = centre + half;
        }

        private static void SetCentre(TrafficLane l, int axis, float value)
        {
            var c = Centre(l);
            c[axis] = value;
            SetShape(l, c, Length(l), Yaw(l), Tilt(l));
        }

        private void AddControls(Transform parent)
        {
            AddControl(parent, "DISTANCE (Z)", 30f, 600f, "0", l => Centre(l).z, (l, v) => SetCentre(l, 2, v));
            AddControl(parent, "HEIGHT (Y)", -150f, 350f, "0", l => Centre(l).y, (l, v) => SetCentre(l, 1, v));
            AddControl(parent, "SIDEWAYS (X)", -300f, 300f, "0", l => Centre(l).x, (l, v) => SetCentre(l, 0, v));
            AddControl(parent, "TILT", -150f, 150f, "0", Tilt, (l, v) => SetShape(l, Centre(l), Length(l), Yaw(l), v));
            AddControl(parent, "ROTATION", -60f, 60f, "0", Yaw, (l, v) => SetShape(l, Centre(l), Length(l), v, Tilt(l)));
            AddControl(parent, "LENGTH", 100f, 1500f, "0", Length, (l, v) => SetShape(l, Centre(l), v, Yaw(l), Tilt(l)));
            AddControl(parent, "SHIP SIZE", 0.2f, 6f, "0.0", l => l.scale, (l, v) => l.scale = v);
            AddControl(parent, "SPEED", 2f, 120f, "0", l => l.speed.x, (l, v) => l.speed = new Vector2(v, v + (l.speed.y - l.speed.x)));
            AddControl(parent, "GAP (S)", 2f, 120f, "0", l => l.interval.x, (l, v) => l.interval = new Vector2(v, v + (l.interval.y - l.interval.x)));
            AddControl(parent, "SPREAD UP/DOWN", 0f, 60f, "0", l => l.randomOffset.y, (l, v) => l.randomOffset = new Vector3(l.randomOffset.x, v, l.randomOffset.z));
            AddControl(parent, "SPREAD DEPTH", 0f, 100f, "0", l => l.randomOffset.z, (l, v) => l.randomOffset = new Vector3(l.randomOffset.x, l.randomOffset.y, v));
        }

        private void AddControl(Transform parent, string label, float min, float max, string format,
            System.Func<TrafficLane, float> get, System.Action<TrafficLane, float> set)
        {
            var row = Rect(parent, label, new Vector2(340f, 28f));
            var name = Text(row, label, 13, DimColor, TextAlignmentOptions.Left);
            Place(name.rectTransform, new Vector2(0f, 0f), new Vector2(0.42f, 1f));
            var slider = MakeSlider(row);
            Place((RectTransform)slider.transform, new Vector2(0.43f, 0.25f), new Vector2(0.82f, 0.75f));
            var value = Text(row, "", 14, TextColor, TextAlignmentOptions.Right);
            Place(value.rectTransform, new Vector2(0.83f, 0f), new Vector2(1f, 1f));
            slider.minValue = min;
            slider.maxValue = max;
            var control = new Control { slider = slider, value = value, get = get, set = set, format = format };
            slider.onValueChanged.AddListener(x => OnSlider(control, x));
            _controls.Add(control);
        }

        private void OnSlider(Control control, float value)
        {
            var v = _views[_selected];
            control.set(v.lane, value);
            Changed();
        }

        private void Changed()
        {
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(traffic.Tuning);
#endif
            Refresh();
        }

        private void Refresh()
        {
            foreach (var v in _views) UpdateView(v);
            if (_views.Count == 0) return;
            var lane = _views[_selected].lane;
            foreach (var c in _controls)
            {
                float x = c.get(lane);
                c.slider.SetValueWithoutNotify(x);
                c.value.text = x.ToString(c.format, CultureInfo.InvariantCulture);
            }
            for (int i = 0; i < _views.Count; i++)
            {
                var col = _views[i].lane.devColor;
                _views[i].button.color = i == _selected ? new Color(col.r * 0.55f, col.g * 0.55f, col.b * 0.55f) : ButtonColor;
            }
            var sel = _views[_selected];
            string clearance = sel.clearance < minClearance
                ? $"<color=#FF4033>Clearance {sel.clearance:0} m – too close (min {minClearance:0})</color>"
                : $"Clearance {sel.clearance:0} m";
            _info.text = $"{clearance}\nShip width {sel.shipWidth * lane.scale:0} m · speed {lane.speed.x:0}–{lane.speed.y:0} · gap {lane.interval.x:0}–{lane.interval.y:0} s";
            _timeLapseLabel.text = _fast ? $"TIME x{timeLapse:0}  ON" : $"TIME x{timeLapse:0}";
        }

        // ---------- buttons ----------

        private void Select(int index)
        {
            _selected = index;
            Refresh();
        }

        private void LaunchNow() => traffic.LaunchNow(_views[_selected].lane);

        private void ToggleTimeLapse()
        {
            _fast = !_fast;
            traffic.SpeedFactor = _fast ? timeLapse : 1f;
            Refresh();
        }

        private void ResetLanes()
        {
            foreach (var v in _views) Restore(v.lane, v.original);
            Changed();
        }

        /// <summary>
        /// Keeps the selected lane: in the editor it is written to the tuning asset now; in a build its values go to the
        /// clipboard for GameOff → Paste Traffic Lanes in Unity. The lane's RESET point moves to the saved values.
        /// </summary>
        private void SaveLane()
        {
            var v = _views[_selected];
            v.original = Take(v.lane);
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(traffic.Tuning);
            UnityEditor.AssetDatabase.SaveAssetIfDirty(traffic.Tuning);
            Refresh();
            _info.text = $"{v.lane.name} saved in {traffic.Tuning.name}.";
#else
            Copy(new[] { v.lane });
            _info.text = $"{v.lane.name} copied. In Unity: GameOff → Paste Traffic Lanes.";
#endif
        }

        private void CopyValues()
        {
            Copy(traffic.Tuning.lanes);
            _info.text = "All lanes copied. In Unity: GameOff → Paste Traffic Lanes.";
        }

        private static void Copy(IEnumerable<TrafficLane> lanes)
        {
            string json = TrafficLaneValues.ToJson(lanes);
            ClipboardBridge.Copy(json);
            Debug.Log("[TrafficLaneDevTool] " + json);
        }

        private static Snapshot Take(TrafficLane l) => new Snapshot
        {
            from = l.from, to = l.to, offset = l.randomOffset, scale = l.scale, speed = l.speed, interval = l.interval,
        };

        private static void Restore(TrafficLane l, Snapshot s)
        {
            l.from = s.from; l.to = s.to; l.randomOffset = s.offset; l.scale = s.scale; l.speed = s.speed; l.interval = s.interval;
        }

        // ---------- panel ----------

        private void BuildPanel()
        {
            var canvasGo = new GameObject("TrafficLaneDevCanvas", typeof(RectTransform));
            canvasGo.transform.SetParent(_root.transform, false);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 40;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            scaler.matchWidthOrHeight = 1f;
            canvasGo.AddComponent<GraphicRaycaster>();

            foreach (var v in _views)
            {
                v.tag = Text(canvasGo.transform, v.lane.name, 14, v.lane.devColor, TextAlignmentOptions.Center);
                v.tag.rectTransform.sizeDelta = new Vector2(320f, 24f);
                v.tag.raycastTarget = false;
            }

            var panel = Rect(canvasGo.transform, "Panel", new Vector2(370f, 0f));
            panel.anchorMin = panel.anchorMax = panel.pivot = new Vector2(1f, 1f);
            panel.anchoredPosition = new Vector2(-12f, -48f);
            panel.gameObject.AddComponent<Image>().color = PanelColor;
            _panelGroup = panel.gameObject.AddComponent<CanvasGroup>();
            var layout = panel.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(14, 14, 12, 12);
            layout.spacing = 2f;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandHeight = false;
            panel.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var title = Text(panel, "TRAFFIC_LINE_DEV", 18, TextColor, TextAlignmentOptions.Left);
            title.rectTransform.sizeDelta = new Vector2(0f, 26f);
            _modeHint = Text(panel, "", 12, DimColor, TextAlignmentOptions.Left);
            _modeHint.rectTransform.sizeDelta = new Vector2(0f, 18f);

            var laneRow = Rect(panel, "Lanes", new Vector2(0f, 32f));
            var laneLayout = laneRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            laneLayout.spacing = 6f;
            laneLayout.childControlWidth = laneLayout.childControlHeight = true;
            laneLayout.childForceExpandWidth = laneLayout.childForceExpandHeight = true;
            for (int i = 0; i < _views.Count; i++)
            {
                int index = i;
                var b = MakeButton(laneRow, _views[i].lane.name.ToUpperInvariant(), () => Select(index), out _);
                _views[i].button = b.GetComponent<Image>();
            }

            AddControls(panel);

            _info = Text(panel, "", 12, DimColor, TextAlignmentOptions.Left);
            _info.rectTransform.sizeDelta = new Vector2(0f, 36f);

            var row1 = ButtonRow(panel);
            MakeButton(row1, "SHIP NOW", LaunchNow, out _);
            MakeButton(row1, "", ToggleTimeLapse, out _timeLapseLabel);
            var row2 = ButtonRow(panel);
            MakeButton(row2, "SAVE LANE", SaveLane, out _);
            MakeButton(row2, "COPY ALL", CopyValues, out _);
            MakeButton(row2, "RESET", ResetLanes, out _);
            var row3 = ButtonRow(panel);
            // Leaves the tool without going through the options menu (Martin, 07.11.).
            MakeButton(row3, "RETURN", () => GameSettings.TrafficLineDev = false, out _);
        }

        private static RectTransform ButtonRow(Transform parent)
        {
            var row = Rect(parent, "Buttons", new Vector2(0f, 32f));
            var h = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            h.spacing = 6f;
            h.childControlWidth = h.childControlHeight = true;
            h.childForceExpandWidth = h.childForceExpandHeight = true;
            return row;
        }

        private static RectTransform Rect(Transform parent, string name, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.sizeDelta = size;
            return rt;
        }

        private static void Place(RectTransform rt, Vector2 min, Vector2 max)
        {
            rt.anchorMin = min;
            rt.anchorMax = max;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }

        private static TextMeshProUGUI Text(Transform parent, string text, float size, Color color, TextAlignmentOptions align)
        {
            var rt = Rect(parent, "Text", new Vector2(0f, 20f));
            var t = rt.gameObject.AddComponent<TextMeshProUGUI>();
            t.text = text;
            t.fontSize = size;
            t.color = color;
            t.alignment = align;
            t.textWrappingMode = TextWrappingModes.NoWrap;
            t.raycastTarget = false;
            return t;
        }

        private static Button MakeButton(Transform parent, string label, UnityEngine.Events.UnityAction onClick, out TextMeshProUGUI text)
        {
            var rt = Rect(parent, label, new Vector2(0f, 32f));
            var image = rt.gameObject.AddComponent<Image>();
            image.color = ButtonColor;
            var button = rt.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(onClick);
            text = Text(rt, label, 13, TextColor, TextAlignmentOptions.Center);
            Place(text.rectTransform, Vector2.zero, Vector2.one);
            return button;
        }

        private static Slider MakeSlider(Transform parent)
        {
            var rt = Rect(parent, "Slider", Vector2.zero);
            var track = Rect(rt, "Track", Vector2.zero);
            Place(track, new Vector2(0f, 0.35f), new Vector2(1f, 0.65f));
            track.gameObject.AddComponent<Image>().color = ButtonColor;
            var fillArea = Rect(rt, "Fill Area", Vector2.zero);
            Place(fillArea, new Vector2(0f, 0.35f), new Vector2(1f, 0.65f));
            var fill = Rect(fillArea, "Fill", Vector2.zero);
            Place(fill, Vector2.zero, Vector2.one);
            fill.gameObject.AddComponent<Image>().color = DimColor;
            var handleArea = Rect(rt, "Handle Area", Vector2.zero);
            Place(handleArea, Vector2.zero, Vector2.one);
            var handle = Rect(handleArea, "Handle", new Vector2(10f, 0f));
            handle.anchorMin = new Vector2(0f, 0f);
            handle.anchorMax = new Vector2(0f, 1f);
            var handleImage = handle.gameObject.AddComponent<Image>();
            handleImage.color = TextColor;
            var slider = rt.gameObject.AddComponent<Slider>();
            slider.fillRect = fill;
            slider.handleRect = handle;
            slider.targetGraphic = handleImage;
            slider.direction = Slider.Direction.LeftToRight;
            return slider;
        }
    }
}
