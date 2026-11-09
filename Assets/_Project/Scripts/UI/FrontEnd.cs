using System;
using System.Collections;
using System.Collections.Generic;
using GameOff.Day;
using GameOff.Player;
using Unity.Cinemachine;
using UnityEngine;

namespace GameOff.UI
{
    /// <summary>
    /// P48: the main menu lives in Main, over the real station. While it is up the player's camera (the only one, as in ShuttleFlight)
    /// drifts around the station, the HUD and the fog are off and no day has begun. START flies the camera to the porthole of room 103,
    /// fades out and hands over: DayManager begins day 1 on Started.
    /// </summary>
    public class FrontEnd : MonoBehaviour
    {
        [SerializeField] private FirstPersonController player;
        [SerializeField] private Camera playerCamera;
        [SerializeField] private DayFlowUI dayUi;
        [SerializeField] private GameObject menuCanvas;
        [Header("Menu camera")]
        [SerializeField] private Vector3 orbitTarget = new Vector3(2f, 9f, 5f);
        [Tooltip("Keeps the station right of the menu panel.")]
        [SerializeField] private Vector3 lookOffset = new Vector3(-28f, 0f, 0f);
        [Header("Flight to room 103 (one Bezier curve, west of the station)")]
        [SerializeField] private float flightSeconds = 4f;
        [SerializeField] private float fadeSeconds = 0.5f;
        [SerializeField] private Vector3 controlA = new Vector3(-75f, 20f, -45f);
        [SerializeField] private Vector3 controlB = new Vector3(-44f, 3f, 5f);
        [Tooltip("Just outside the porthole of room 103.")]
        [SerializeField] private Vector3 flightEnd = new Vector3(-21.3f, 1.5f, 5f);
        [SerializeField] private Vector3 flightLookAt = new Vector3(-17f, 1.5f, 5f);
        [Tooltip("P114: rooms the menu can't see into; their renderers are off until START (the menu runs without occlusion).")]
        [SerializeField] private Bounds[] hiddenRooms;

        private readonly List<Canvas> _hidden = new List<Canvas>();
        private readonly List<Renderer> _hiddenRenderers = new List<Renderer>();
        private MenuCameraDrift _drift;
        private CinemachineBrain _brain;
        private Transform _cameraParent;
        private Vector3 _cameraLocalPosition;
        private Quaternion _cameraLocalRotation;
        private bool _fog, _occlusion, _begun;

        public static FrontEnd Instance { get; private set; }

        /// <summary>The flight is over and the screen is black: the run can begin.</summary>
        public event Action Started;

        private void Awake() => Instance = this;

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void Start()
        {
            player.SetControlEnabled(false);
            var menu = menuCanvas.GetComponent<Canvas>();
            foreach (var canvas in FindObjectsByType<Canvas>(FindObjectsSortMode.None))
                if (canvas != menu && canvas.isRootCanvas && canvas.enabled && canvas.renderMode == RenderMode.ScreenSpaceOverlay) // the retro look's display canvas stays on
                {
                    canvas.enabled = false;
                    _hidden.Add(canvas);
                }

            foreach (var r in FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                if (!r.enabled || r is ParticleSystemRenderer) continue;
                foreach (var room in hiddenRooms)
                    if (room.Contains(r.bounds.center))
                    {
                        r.enabled = false;
                        _hiddenRenderers.Add(r);
                        break;
                    }
            }

            _fog = RenderSettings.fog;
            RenderSettings.fog = false; // Main's indoor fog would swallow the station at this distance

            var cam = playerCamera.transform;
            _cameraParent = cam.parent;
            _cameraLocalPosition = cam.localPosition;
            _cameraLocalRotation = cam.localRotation;
            _occlusion = playerCamera.useOcclusionCulling;
            playerCamera.useOcclusionCulling = false; // the bake only covers the inside of the station
            _brain = playerCamera.GetComponent<CinemachineBrain>();
            if (_brain != null) _brain.enabled = false;
            cam.SetParent(null, true);
            _drift = playerCamera.gameObject.AddComponent<MenuCameraDrift>();
            _drift.Configure(orbitTarget, lookOffset);
        }

        /// <summary>Called by the START SHIFT button.</summary>
        public void Begin()
        {
            if (_begun) return;
            _begun = true;
            menuCanvas.SetActive(false);
            foreach (var r in _hiddenRenderers) if (r != null) r.enabled = true;
            _hiddenRenderers.Clear();
            StartCoroutine(Flight());
        }

        private IEnumerator Flight()
        {
            Destroy(_drift);
            var cam = playerCamera.transform;
            Vector3 start = cam.position;
            Quaternion startRotation = cam.rotation;
            for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime / Mathf.Max(0.1f, flightSeconds))
            {
                float k = Mathf.SmoothStep(0f, 1f, t);
                cam.position = Bezier(start, controlA, controlB, flightEnd, k);
                // Looks at the porthole from the start of the approach; the blend keeps the first frame where the menu left it.
                var toTarget = Quaternion.LookRotation(flightLookAt - cam.position, Vector3.up);
                cam.rotation = Quaternion.Slerp(startRotation, toTarget, Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t * 2.5f)));
                yield return null;
            }
            cam.SetPositionAndRotation(flightEnd, Quaternion.LookRotation(flightLookAt - flightEnd, Vector3.up));

            // The day UI holds the fade; the rest of the HUD stays off until the screen is black.
            if (dayUi != null) // missing only if Build Day Flow ran without Build Menu Scenes after it: the run must still start
            {
                dayUi.SetBlack(false); // its fade rests fully black
                var dayCanvas = dayUi.GetComponentInParent<Canvas>(true);
                if (dayCanvas != null) dayCanvas.enabled = true;
                dayUi.SetDayLabel("");
                yield return dayUi.FadeTo(1f, fadeSeconds);
            }
            foreach (var canvas in _hidden) if (canvas != null) canvas.enabled = true;
            _hidden.Clear();

            RenderSettings.fog = _fog;
            cam.SetParent(_cameraParent, false);
            cam.localPosition = _cameraLocalPosition;
            cam.localRotation = _cameraLocalRotation;
            playerCamera.useOcclusionCulling = _occlusion;
            if (_brain != null) _brain.enabled = true;
            Started?.Invoke();
        }

        private static Vector3 Bezier(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t)
        {
            float u = 1f - t;
            return u * u * u * a + 3f * u * u * t * b + 3f * u * t * t * c + t * t * t * d;
        }
    }
}
