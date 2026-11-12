using System.Collections.Generic;
using GameOff.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace GameOff.UI
{
    /// <summary>
    /// P138: the game's own mouse pointer and the browser mouse rules in one place. One instance survives scene loads (placed in
    /// Boot and Main by MenuSceneBuilder; extra copies remove themselves, like the AudioManager).
    /// - UI_Cursor_Default whenever the pointer is free; UI_Cursor_Hover over an interactable uGUI control and over whatever the
    ///   world reports this frame through MarkHover (the counter desk: stamps, buttons, papers, notes, the drawer).
    /// - The pointer is shown whenever the browser has not locked it, so after Esc, a tab switch or a refused lock it never stays
    ///   invisible, in menus or anywhere else (Unity keeps "Locked" and the hidden pointer after the browser let go).
    /// Looking around drops the jump of a freshly caught pointer: PointerLockBridge.LookDelta.
    /// </summary>
    public class GameCursor : MonoBehaviour
    {
        [Tooltip("UI_Cursor_Default (texture type Cursor: readable, uncompressed, no mipmaps; GameOff/Integrate Asset Deliveries sets that).")]
        [SerializeField] private Texture2D normalTexture;
        [Tooltip("Pixel of the texture that clicks, from its top left.")]
        [SerializeField] private Vector2 normalHotspot = Vector2.zero;
        [Tooltip("UI_Cursor_Hover, over what can be clicked.")]
        [SerializeField] private Texture2D hoverTexture;
        [SerializeField] private Vector2 hoverHotspot = Vector2.zero;

        private static GameCursor _instance;
        private static int _worldHoverFrame = -1;

        private readonly List<RaycastResult> _uiHits = new List<RaycastResult>();
        private PointerEventData _pointer;
        private EventSystem _pointerSystem;
        private Texture2D _applied;
        private bool _wasFree;
        private bool? _lockWanted;

        /// <summary>Something in the world under the pointer reacts to a click: call it in Update of every frame where that is so.</summary>
        public static void MarkHover() => _worldHoverFrame = Time.frameCount;

        /// <summary>The pointer to show: nothing while it is captured, the hand over a control or a clickable thing, else the arrow.</summary>
        public static CursorLook Pick(bool pointerFree, bool overControl, bool overWorld) =>
            !pointerFree ? CursorLook.Hidden : overControl || overWorld ? CursorLook.Hover : CursorLook.Normal;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            if (_instance != this) return;
            _instance = null;
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }

        private void LateUpdate()
        {
            // Whoever wants the mouse captured sets Cursor.lockState (walking, seat, telescope, flight); menus and the counter free it.
            bool wanted = Cursor.lockState == CursorLockMode.Locked;
            if (_lockWanted != wanted)
            {
                PointerLockBridge.SetWanted(wanted); // browser: the next press on the canvas catches the mouse again
                _lockWanted = wanted;
            }

            bool free = !PointerLockBridge.IsLocked;
            if (free && !_wasFree) _applied = null; // just let go: set ours again, the browser may have reset the canvas pointer
            _wasFree = free;
            if (free && !Cursor.visible)
            {
                Cursor.visible = true; // never an invisible free pointer (menus, after Esc or a tab switch)
                _applied = null;
            }
            var look = Pick(free, free && OverControl(), _worldHoverFrame == Time.frameCount);
            if (look == CursorLook.Hidden) return;
            var texture = look == CursorLook.Hover && hoverTexture != null ? hoverTexture : normalTexture;
            if (texture == null || texture == _applied) return;
            Cursor.SetCursor(texture, texture == hoverTexture ? hoverHotspot : normalHotspot, CursorMode.Auto);
            _applied = texture;
        }

        /// <summary>An interactable uGUI control (button, toggle, slider, field ...) is the top thing under the pointer.</summary>
        private bool OverControl()
        {
            var system = EventSystem.current;
            if (system == null || Mouse.current == null || !system.IsPointerOverGameObject()) return false; // cheap check first
            if (_pointer == null || _pointerSystem != system)
            {
                _pointer = new PointerEventData(system); // kept: no garbage per frame
                _pointerSystem = system;
            }
            _pointer.position = Mouse.current.position.ReadValue();
            _uiHits.Clear();
            system.RaycastAll(_pointer, _uiHits);
            if (_uiHits.Count == 0) return false;
            var control = _uiHits[0].gameObject.GetComponentInParent<Selectable>();
            return control != null && control.IsInteractable();
        }
    }
}
