using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace GameOff.Core
{
    /// <summary>
    /// Renders this camera into a small point-filtered RenderTexture (e.g. 640x360) and shows it full screen
    /// behind all other UI, so the 3D world gets hard pixels while screen-space UI stays sharp.
    /// Touches no project assets (no render-pipeline changes). Put it on the player's real Camera.
    /// Code that converts mouse positions into rays must use viewport coordinates, not Camera pixel coordinates.
    /// Objects on the sharp layer ("Documents": papers on the counter, the held stamp) are drawn by an overlay camera at
    /// full resolution on top of the low-res image, so they stay readable.
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
        private Camera _sharpCamera;
        private int _sharpMask, _baseMask;

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
            _baseMask = _camera.cullingMask;
            _sharpMask = LayerMask.GetMask(Layers.Documents);
        }

        private void OnEnable()
        {
            _retroEnabled = s_playerChoice ?? (settings == null || settings.enabledByDefault);
            RenderPipelineManager.beginCameraRendering += SyncSharpCamera;
            Apply();
        }

        private void OnDisable()
        {
            RenderPipelineManager.beginCameraRendering -= SyncSharpCamera;
            SetSharpOverlay(false);
            _camera.targetTexture = null;
            SetDisplayActive(false);
            ReleaseTexture();
        }

        private void OnDestroy()
        {
            if (_display != null) Destroy(_display);
            if (_screenClearCamera != null) Destroy(_screenClearCamera.gameObject);
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
                SetSharpOverlay(false);
                _camera.targetTexture = null;
                SetDisplayActive(false);
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
            SetDisplayActive(true);
            SetSharpOverlay(true);
        }

        /// <summary>Full-screen image behind all other overlay UI, plus a camera that clears the screen.</summary>
        private void EnsureDisplay()
        {
            if (_display != null) return;

            _display = new GameObject("RetroLook_Display");
            _display.transform.SetParent(transform, false);

            _display.layer = LayerMask.NameToLayer("UI");
            var canvas = _display.AddComponent<Canvas>();

            var imageGo = new GameObject("Image", typeof(RectTransform));
            imageGo.transform.SetParent(_display.transform, false);
            var rect = (RectTransform)imageGo.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            imageGo.layer = _display.layer;
            _image = imageGo.AddComponent<RawImage>();
            _image.raycastTarget = false;

            // Without a camera drawing to the screen, the back buffer is never cleared (and the Editor warns).
            // Its own object: a Screen Space - Camera canvas moves itself in front of its camera, so the camera must not be on
            // (or under) the canvas.
            var screenCam = new GameObject("RetroLook_ScreenCamera");
            screenCam.transform.SetParent(transform, false);
            _screenClearCamera = screenCam.AddComponent<Camera>();
            _screenClearCamera.cullingMask = 1 << _display.layer;
            _screenClearCamera.clearFlags = CameraClearFlags.SolidColor;
            _screenClearCamera.backgroundColor = Color.black;
            // After the main camera has filled the render texture this frame (else the screen shows last frame's world)
            _screenClearCamera.depth = _camera.depth + 1f;
            // Only the image plane at 1 m: no world-space UI on the UI layer gets drawn a second time
            _screenClearCamera.nearClipPlane = 0.5f;
            _screenClearCamera.farClipPlane = 1.5f;
            _screenClearCamera.useOcclusionCulling = false;
            // The low-res image is drawn by this camera (not as an overlay canvas), so the sharp overlay camera can draw on top;
            // overlay-canvas UI (HUD, menus) still comes last.
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = _screenClearCamera;
            canvas.planeDistance = 1f;
            canvas.sortingOrder = -1000;
        }

        private void SetDisplayActive(bool active)
        {
            if (_display != null) _display.SetActive(active);
            if (_screenClearCamera != null) _screenClearCamera.gameObject.SetActive(active);
        }

        /// <summary>Retro on: the main camera skips the sharp layer and the overlay camera draws it in full resolution.</summary>
        private void SetSharpOverlay(bool on)
        {
            if (_sharpMask == 0 || _camera == null) return;
            _camera.cullingMask = on ? _baseMask & ~_sharpMask : _baseMask;
            if (on && _sharpCamera == null)
            {
                var go = new GameObject("RetroLook_SharpCamera");
                go.transform.SetParent(transform, false); // same pose as the main camera
                _sharpCamera = go.AddComponent<Camera>();
                _sharpCamera.cullingMask = _sharpMask;
                _sharpCamera.clearFlags = CameraClearFlags.Depth;
                _sharpCamera.GetUniversalAdditionalCameraData().renderType = CameraRenderType.Overlay;
            }
            if (_sharpCamera == null || _screenClearCamera == null) return;
            var stack = _screenClearCamera.GetUniversalAdditionalCameraData().cameraStack;
            if (on && !stack.Contains(_sharpCamera)) stack.Add(_sharpCamera);
            if (!on) stack.Remove(_sharpCamera);
            _sharpCamera.enabled = on;
        }

        /// <summary>Cinemachine changes the main camera's lens every frame; the overlay camera copies it before drawing.</summary>
        private void SyncSharpCamera(ScriptableRenderContext context, Camera cam)
        {
            if (_sharpCamera == null || cam != _screenClearCamera) return;
            _sharpCamera.fieldOfView = _camera.fieldOfView;
            _sharpCamera.nearClipPlane = _camera.nearClipPlane;
            _sharpCamera.farClipPlane = _camera.farClipPlane;
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
