using System.Collections;
using GameOff.Audio;
using GameOff.Core;
using GameOff.Day;
using GameOff.Economy;
using GameOff.Player;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GameOff.Shops
{
    /// <summary>
    /// P29: the coin telescope on the deck. One credit, thirty seconds: the view jumps to the eyepiece, the mouse pans, the wheel zooms,
    /// Q or Esc ends it early. It looks at the home planet (HomeLights shows how the family is doing).
    /// </summary>
    public class CoinTelescope : MonoBehaviour
    {
        [SerializeField] private Interactable interactable;
        [SerializeField] private FirstPersonController player;
        [SerializeField] private Camera playerCamera;
        [SerializeField] private DayManager day;
        [Tooltip("Where the eye is, a little behind the eyepiece.")]
        [SerializeField] private Transform eye;
        [SerializeField] private Transform homePlanet;
        [SerializeField] private GameObject overlay;
        [SerializeField] private TMP_Text countdown;
        [SerializeField] private int cost = 1;
        [SerializeField] private float seconds = 30f;
        [SerializeField] private float wideFov = 24f;
        [SerializeField] private float narrowFov = 3f;
        [SerializeField] private float mouseDegrees = 0.03f;
        [SerializeField] private float panLimit = 25f;

        private bool _looking;

        private void OnEnable() => interactable.OnInteract.AddListener(Use);

        private void OnDisable() => interactable.OnInteract.RemoveListener(Use);

        private void Awake() => overlay.SetActive(false);

        /// <summary>Public for the test driver as well.</summary>
        public void Use()
        {
            if (_looking) return;
            var economy = PlayerEconomy.Instance;
            if (economy == null || !economy.Wallet.TrySpend(cost, "Telescope"))
            {
                day.UI.ShowHint($"The slot wants {cost} credit.", 3f);
                return;
            }
            AudioManager.Play(s => s.telescopeCoin);
            StartCoroutine(Look());
        }

        private IEnumerator Look()
        {
            _looking = true;
            player.SetControlEnabled(false);
            yield return null; // the browser needs a frame after the release before it grants the lock again
            Cursor.lockState = CursorLockMode.Locked;
            PointerLockBridge.Request();
            bool held = false;
            var brain = playerCamera.GetComponent<CinemachineBrain>();
            if (brain != null) brain.enabled = false;
            var cam = playerCamera.transform;
            var parent = cam.parent;
            Vector3 position = cam.localPosition;
            Quaternion rotation = cam.localRotation;
            float fov = playerCamera.fieldOfView;
            bool occlusion = playerCamera.useOcclusionCulling;
            playerCamera.useOcclusionCulling = false; // the bake only covers the inside of the station
            cam.SetParent(null, true);
            cam.position = eye.position;
            var centre = Quaternion.LookRotation(homePlanet.position - eye.position);
            overlay.SetActive(true);
            float yaw = 0f, pitch = 0f, zoom = wideFov;
            for (float left = seconds; left > 0f; left -= Time.deltaTime)
            {
                var keys = Keyboard.current;
                if (keys != null && (keys.qKey.wasPressedThisFrame || keys.escapeKey.wasPressedThisFrame)) break;
                held |= PointerLockBridge.IsLocked;
                if (held && !PointerLockBridge.IsLocked) break; // in the browser Esc only releases the mouse
                float scale = mouseDegrees * zoom / wideFov;
                if (keys != null) // the arrow keys and WASD work without the mouse lock
                {
                    float turn = (keys.dKey.isPressed || keys.rightArrowKey.isPressed ? 1f : 0f) - (keys.aKey.isPressed || keys.leftArrowKey.isPressed ? 1f : 0f);
                    float tilt = (keys.wKey.isPressed || keys.upArrowKey.isPressed ? 1f : 0f) - (keys.sKey.isPressed || keys.downArrowKey.isPressed ? 1f : 0f);
                    yaw = Mathf.Clamp(yaw + turn * scale * 600f * Time.deltaTime, -panLimit, panLimit);
                    pitch = Mathf.Clamp(pitch - tilt * scale * 600f * Time.deltaTime, -panLimit, panLimit);
                }
                var mouse = Mouse.current;
                if (mouse != null)
                {
                    // Slower the further it is zoomed in, so the view stays controllable.
                    Vector2 delta = PointerLockBridge.IsLocked ? mouse.delta.ReadValue() * scale : Vector2.zero;
                    yaw = Mathf.Clamp(yaw + delta.x, -panLimit, panLimit);
                    pitch = Mathf.Clamp(pitch - delta.y, -panLimit, panLimit);
                    float wheel = mouse.scroll.ReadValue().y;
                    if (!Mathf.Approximately(wheel, 0f)) zoom = Mathf.Clamp(zoom * (wheel > 0f ? 0.85f : 1.18f), narrowFov, wideFov);
                }
                cam.rotation = centre * Quaternion.Euler(pitch, yaw, 0f);
                playerCamera.fieldOfView = zoom;
                countdown.text = Mathf.CeilToInt(left).ToString("00");
                yield return null;
            }
            overlay.SetActive(false);
            cam.SetParent(parent, false);
            cam.localPosition = position;
            cam.localRotation = rotation;
            playerCamera.fieldOfView = fov;
            playerCamera.useOcclusionCulling = occlusion;
            if (brain != null) brain.enabled = true;
            player.SetControlEnabled(true);
            _looking = false;
        }
    }
}
