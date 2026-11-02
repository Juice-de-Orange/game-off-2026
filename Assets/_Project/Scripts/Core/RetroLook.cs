using UnityEngine;
using UnityEngine.UI;

namespace GameOff.Core
{
    /// <summary>
    /// Renders this camera into a small point-filtered RenderTexture (e.g. 640x360) and shows it full screen
    /// behind all other UI, so the 3D world gets hard pixels while screen-space UI stays sharp.
    /// Touches no project assets (no render-pipeline changes). Put it on the player's real Camera.
    /// Code that converts mouse positions into rays must use viewport coordinates, not Camera pixel coordinates.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class RetroLook : MonoBehaviour
    {
        [SerializeField] private RetroLookSettings settings;

        // The player's choice survives scene loads and component re-enables for the whole session.
        private static bool? s_playerChoice;

        private Camera _camera;
        private RenderTexture _texture;
        private GameObject _display;
        private RawImage _image;
        private Camera _screenClearCamera;
        private bool _retroEnabled;
        private int _screenWidth, _screenHeight;

        /// <summary>Turn the retro look on/off at runtime (e.g. from the options menu).</summary>
        public bool RetroEnabled
        {
            get => _retroEnabled;
            set
            {
                _retroEnabled = value;
                s_playerChoice = value;
                Apply();
            }
        }

        private void Awake()
        {
            _camera = GetComponent<Camera>();
        }

        private void OnEnable()
        {
            _retroEnabled = s_playerChoice ?? (settings == null || settings.enabledByDefault);
            Apply();
        }

        private void OnDisable()
        {
            _camera.targetTexture = null;
            if (_display != null) _display.SetActive(false);
            ReleaseTexture();
        }

        private void OnDestroy()
        {
            if (_display != null) Destroy(_display);
        }

        private void Update()
        {
            if (Screen.width != _screenWidth || Screen.height != _screenHeight) Apply();
        }

        private void Apply()
        {
            if (!isActiveAndEnabled) return;
            _screenWidth = Screen.width;
            _screenHeight = Screen.height;

            if (!_retroEnabled)
            {
                _camera.targetTexture = null;
                if (_display != null) _display.SetActive(false);
                ReleaseTexture();
                return;
            }

            int height = Mathf.Min(settings != null ? settings.targetHeight : 360, Mathf.Max(1, _screenHeight));
            int width = Mathf.Max(1, Mathf.RoundToInt(height * (_screenWidth / (float)Mathf.Max(1, _screenHeight))));
            if (_texture == null || _texture.width != width || _texture.height != height)
            {
                ReleaseTexture();
                _texture = new RenderTexture(width, height, 24)
                {
                    name = "RetroLook_RT",
                    filterMode = FilterMode.Point,
                    antiAliasing = 1
                };
                _texture.Create();
            }

            EnsureDisplay();
            _camera.targetTexture = _texture;
            _image.texture = _texture;
            _display.SetActive(true);
        }

        /// <summary>Full-screen image behind all other overlay UI, plus a camera that clears the screen.</summary>
        private void EnsureDisplay()
        {
            if (_display != null) return;

            _display = new GameObject("RetroLook_Display");
            _display.transform.SetParent(transform, false);

            var canvas = _display.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = -1000;

            var imageGo = new GameObject("Image", typeof(RectTransform));
            imageGo.transform.SetParent(_display.transform, false);
            var rect = (RectTransform)imageGo.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            _image = imageGo.AddComponent<RawImage>();
            _image.raycastTarget = false;

            // Without a camera drawing to the screen, the back buffer is never cleared (and the Editor warns).
            _screenClearCamera = _display.AddComponent<Camera>();
            _screenClearCamera.cullingMask = 0;
            _screenClearCamera.clearFlags = CameraClearFlags.SolidColor;
            _screenClearCamera.backgroundColor = Color.black;
            _screenClearCamera.depth = -100;
            _screenClearCamera.useOcclusionCulling = false;
        }

        private void ReleaseTexture()
        {
            if (_texture == null) return;
            if (_camera != null && _camera.targetTexture == _texture) _camera.targetTexture = null;
            _texture.Release();
            Destroy(_texture);
            _texture = null;
        }
    }
}
