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
using UnityEngine.UI;

namespace GameOff.Shops
{
    /// <summary>
    /// P29: the coin telescope on the deck. One credit, thirty seconds: the view jumps to the eyepiece, the mouse pans, the wheel zooms,
    /// Q or Esc ends it early. It looks at the home planet (HomeLights shows how the family is doing).
    /// P111: zoomed all the way in on the planet, a short focus blink takes the view down to the home street diorama, where the
    /// family's windows show how each of them is doing (HomeWindows); zooming out of the street blinks back to the planet.
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

        [Header("P111: home street")]
        [SerializeField] private HomeViewTuning homeTuning;
        [Tooltip("The home street diorama, far below the station; only switched on while the telescope shows it.")]
        [SerializeField] private GameObject homeDiorama;
        [Tooltip("Where the view looks at the house from, inside the diorama.")]
        [SerializeField] private Transform homeEye;
        [Tooltip("Black image under the eyepiece frame for the focus blink.")]
        [SerializeField] private Image blink;

        private bool _looking;

        private void OnEnable() => interactable.OnInteract.AddListener(Use);

        private void OnDisable() => interactable.OnInteract.RemoveListener(Use);

        private void Awake()
        {
            overlay.SetActive(false);
            ShowStreet(false);
            SetBlink(0f, false);
        }

        private void ShowStreet(bool on)
        {
            if (homeDiorama != null) homeDiorama.SetActive(on);
        }

        private void SetBlink(float alpha, bool on)
        {
            if (blink == null) return;
            blink.gameObject.SetActive(on);
            blink.color = new Color(0f, 0f, 0f, alpha);
        }

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
            SetBlink(0f, true);
            bool canStreet = homeTuning != null && homeDiorama != null && homeEye != null;
            bool street = false;
            float blinkAt = -1f; // focus blink progress 0..1, below 0 while none runs
            float yaw = 0f, pitch = 0f, zoom = wideFov;
            float streetYaw = 0f, streetPitch = 0f, streetZoom = canStreet ? homeTuning.streetWideFov : wideFov;
            for (float left = seconds; left > 0f; left -= Time.deltaTime)
            {
                var keys = Keyboard.current;
                if (keys != null && (keys.qKey.wasPressedThisFrame || keys.escapeKey.wasPressedThisFrame)) break;
                held |= PointerLockBridge.IsLocked;
                if (held && !PointerLockBridge.IsLocked) break; // in the browser Esc only releases the mouse
                // Slower the further it is zoomed in, so the view stays controllable.
                float scale = mouseDegrees * (street ? streetZoom : zoom) / wideFov;
                Vector2 pan = Vector2.zero;
                if (keys != null) // the arrow keys and WASD work without the mouse lock
                {
                    float turn = (keys.dKey.isPressed || keys.rightArrowKey.isPressed ? 1f : 0f) - (keys.aKey.isPressed || keys.leftArrowKey.isPressed ? 1f : 0f);
                    float tilt = (keys.wKey.isPressed || keys.upArrowKey.isPressed ? 1f : 0f) - (keys.sKey.isPressed || keys.downArrowKey.isPressed ? 1f : 0f);
                    pan += new Vector2(turn, tilt) * (scale * 600f * Time.deltaTime);
                }
                float wheel = 0f;
                var mouse = Mouse.current;
                if (mouse != null)
                {
                    if (PointerLockBridge.IsLocked) pan += mouse.delta.ReadValue() * scale;
                    wheel = mouse.scroll.ReadValue().y;
                }

                if (blinkAt >= 0f)
                {
                    float before = blinkAt;
                    blinkAt += Time.deltaTime / Mathf.Max(0.05f, homeTuning.blinkSeconds);
                    if (before < 0.5f && blinkAt >= 0.5f) // eyes shut: switch the view
                    {
                        street = !street;
                        ShowStreet(street);
                        if (street) { streetYaw = 0f; streetPitch = 0f; streetZoom = homeTuning.streetWideFov; }
                        else zoom = TelescopeFocus.PlanetFovAfterStreet(homeTuning, wideFov);
                    }
                    SetBlink(TelescopeFocus.BlinkAlpha(blinkAt), true);
                    if (blinkAt >= 1f) { blinkAt = -1f; SetBlink(0f, true); }
                    wheel = 0f; // no zooming while the eye refocuses
                }

                if (!street)
                {
                    yaw = Mathf.Clamp(yaw + pan.x, -panLimit, panLimit);
                    pitch = Mathf.Clamp(pitch - pan.y, -panLimit, panLimit);
                    if (!Mathf.Approximately(wheel, 0f)) zoom = Mathf.Clamp(zoom * (wheel > 0f ? 0.85f : 1.18f), narrowFov, wideFov);
                    if (canStreet && blinkAt < 0f && TelescopeFocus.EntersStreet(zoom, yaw, pitch, homeTuning)) blinkAt = 0f;
                    cam.position = eye.position;
                    cam.rotation = centre * Quaternion.Euler(pitch, yaw, 0f);
                    playerCamera.fieldOfView = zoom;
                }
                else
                {
                    float limit = homeTuning.streetPanLimit;
                    streetYaw = Mathf.Clamp(streetYaw + pan.x, -limit, limit);
                    streetPitch = Mathf.Clamp(streetPitch - pan.y, -limit, limit);
                    if (!Mathf.Approximately(wheel, 0f))
                    {
                        float next = streetZoom * (wheel > 0f ? 0.85f : 1.18f);
                        if (blinkAt < 0f && TelescopeFocus.LeavesStreet(next, homeTuning)) blinkAt = 0f;
                        streetZoom = Mathf.Clamp(next, homeTuning.streetNarrowFov, homeTuning.streetWideFov);
                    }
                    cam.position = homeEye.position;
                    cam.rotation = homeEye.rotation * Quaternion.Euler(streetPitch, streetYaw, 0f);
                    playerCamera.fieldOfView = streetZoom;
                }
                countdown.text = Mathf.CeilToInt(left).ToString("00");
                yield return null;
            }
            ShowStreet(false);
            SetBlink(0f, false);
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
