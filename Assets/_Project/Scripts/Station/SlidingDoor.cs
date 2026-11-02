using System.Collections.Generic;
using GameOff.Player;
using UnityEngine;
using UnityEngine.AI;

namespace GameOff.Station
{
    /// <summary>
    /// Sliding door (P27): opens with a hiss when someone allowed comes close, slides sideways into the wall.
    /// Needs a trigger collider on this object (the sensing area). The panel carries its own solid collider.
    /// Travelers count when they have a NavMeshAgent and a collider (P14). Scripted doors react only to Open()/Close().
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class SlidingDoor : MonoBehaviour
    {
        [SerializeField] private DoorAccess access = DoorAccess.Public;
        [Tooltip("The moving door leaf (model or placeholder).")]
        [SerializeField] private Transform panel;
        [Tooltip("How far the leaf slides (normally its width), along its local X axis.")]
        [SerializeField] private float slideDistance = 1.6f;
        [SerializeField] private float openSeconds = 0.45f;
        [SerializeField] private float closeDelay = 0.6f;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip openClip;
        [SerializeField] private AudioClip closeClip;
        [SerializeField] private AudioClip lockedClip;
        [Tooltip("Optional red lamp, shown while the door is locked.")]
        [SerializeField] private GameObject lockedLamp;
        [Tooltip("Carving obstacle that keeps travelers out. Scripted doors: only active while the door is not open.")]
        [SerializeField] private NavMeshObstacle obstacle;

        private readonly HashSet<Collider> _inside = new HashSet<Collider>();
        private Vector3 _closedLocalPos;
        private float _open;          // 0 = closed, 1 = open
        private bool _scriptedOpen;
        private float _lastAllowedTime = -10f;
        private bool _wasOpening;

        public DoorAccess Access => access;
        public bool IsOpen => _open > 0.99f;

        private void Awake()
        {
            if (panel != null) _closedLocalPos = panel.localPosition;
            GetComponent<Collider>().isTrigger = true;
            UpdateLamp();
        }

        /// <summary>Change who may pass (e.g. unlock a room later).</summary>
        public void SetAccess(DoorAccess newAccess)
        {
            access = newAccess;
            UpdateLamp();
        }

        /// <summary>Open a door by code (Scripted doors, or force any door open).</summary>
        public void Open() => _scriptedOpen = true;

        /// <summary>Release a door opened by code; it closes when nobody allowed is near.</summary>
        public void Close() => _scriptedOpen = false;

        private void OnTriggerEnter(Collider other)
        {
            if (!IsRelevant(other)) return;
            _inside.Add(other);
            if (access == DoorAccess.Locked && IsPlayer(other)) Play(lockedClip);
        }

        private void OnTriggerExit(Collider other) => _inside.Remove(other);

        private void Update()
        {
            _inside.RemoveWhere(c => c == null || !c.enabled || !c.gameObject.activeInHierarchy);

            bool wantOpen = _scriptedOpen || AnyoneAllowedInside();
            if (wantOpen) _lastAllowedTime = Time.time;
            bool opening = wantOpen || Time.time - _lastAllowedTime < closeDelay;

            if (opening != _wasOpening)
            {
                Play(opening ? openClip : closeClip);
                _wasOpening = opening;
            }

            float target = opening ? 1f : 0f;
            _open = Mathf.MoveTowards(_open, target, Time.deltaTime / Mathf.Max(0.01f, openSeconds));
            if (panel != null)
            {
                float eased = _open * _open * (3f - 2f * _open);
                panel.localPosition = _closedLocalPos + Vector3.left * (slideDistance * eased);
            }

            // Travelers may only path through a scripted door (inspection lock) while it is open.
            if (obstacle != null && access == DoorAccess.Scripted)
            {
                bool block = _open < 0.95f;
                if (obstacle.enabled != block) obstacle.enabled = block;
            }
        }

        private bool AnyoneAllowedInside()
        {
            if (access == DoorAccess.Locked || access == DoorAccess.Scripted) return false;
            foreach (var c in _inside)
            {
                if (IsPlayer(c)) return true;
                if (access == DoorAccess.Public && IsTraveler(c)) return true;
            }
            return false;
        }

        private static bool IsRelevant(Collider c) => IsPlayer(c) || IsTraveler(c);
        private static bool IsPlayer(Collider c) => c.GetComponentInParent<FirstPersonController>() != null;
        private static bool IsTraveler(Collider c) => c.GetComponentInParent<NavMeshAgent>() != null;

        private void Play(AudioClip clip)
        {
            if (clip != null && audioSource != null) audioSource.PlayOneShot(clip);
        }

        private void UpdateLamp()
        {
            if (lockedLamp != null) lockedLamp.SetActive(access == DoorAccess.Locked);
        }
    }
}
