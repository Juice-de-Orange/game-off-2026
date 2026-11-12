using System.Collections.Generic;
using GameOff.Audio;
using GameOff.Economy;
using UnityEngine;

namespace GameOff.Shops
{
    /// <summary>
    /// P32: the open evidence locker. While open, the station's closed locker model is hidden and an open one (shell, shelves, two
    /// swinging doors) shows what was handed in "as ordered" (Confiscation.Locker): one compartment per item type, its model
    /// (EvidenceLockerLayout.ModelFor) or a sealed evidence bag. Only shows; taking things out is the break-in (P83, EvidenceLocker).
    /// P135: shell and doors are cut from the closed model and take its baked lightmap, so the swap does not show; the interior and
    /// the goods are lit by the room's light probes in front of the locker.
    /// </summary>
    public class EvidenceLockerDisplay : MonoBehaviour
    {
        [SerializeField] private EvidenceLockerTuning tuning;
        [SerializeField] private Confiscation confiscation;
        [Tooltip("Shell, shelves, doors and goods; inactive while the locker is shut.")]
        [SerializeField] private GameObject openRoot;
        [Tooltip("Hinge of the left door (opens with +angle around Y).")]
        [SerializeField] private Transform leftDoor;
        [Tooltip("Hinge of the right door (opens with -angle around Y).")]
        [SerializeField] private Transform rightDoor;
        [Tooltip("Bottom centre of each compartment's space, filled in this order.")]
        [SerializeField] private Transform[] slots = new Transform[0];
        [Tooltip("Inactive template for goods without a model.")]
        [SerializeField] private GameObject evidenceBag;
        [Tooltip("Renderers of the station's closed locker model, hidden while the open one is shown.")]
        [SerializeField] private Renderer[] closedRenderers = new Renderer[0];
        [Tooltip("P135: shell and doors cut from the closed model (same lightmap UVs); they get its baked lightmap while shown.")]
        [SerializeField] private Renderer[] sameLightAsClosed = new Renderer[0];
        [Tooltip("P135: the goods take the light probes at this point, in the room in front of the locker.")]
        [SerializeField] private Transform probeAnchor;

        private readonly List<GameObject> _shown = new List<GameObject>();
        private Inventory _locker;
        private float _open;
        private bool _wantOpen;
        private bool _dirty = true;

        public bool IsOpen => _wantOpen;
        public bool IsFullyOpen => _wantOpen && _open >= 1f;
        /// <summary>For the test driver and checks: how many models lie in the locker right now.</summary>
        public int ShownCount => _shown.Count;

        private void Awake()
        {
            if (openRoot != null) openRoot.SetActive(false);
            if (evidenceBag != null) evidenceBag.SetActive(false);
        }

        private void OnEnable()
        {
            _locker = confiscation != null ? confiscation.Locker : null;
            if (_locker != null) _locker.Changed += MarkDirty;
            _dirty = true;
        }

        private void OnDisable()
        {
            if (_locker != null) _locker.Changed -= MarkDirty;
            _locker = null;
        }

        private void MarkDirty() => _dirty = true;

        public void Open()
        {
            if (_wantOpen) return;
            _wantOpen = true;
            SetShown(true);
            if (_dirty) Rebuild();
            AudioManager.Play(s => s.drawerOpen);
        }

        public void Close()
        {
            if (!_wantOpen) return;
            _wantOpen = false;
            AudioManager.Play(s => s.drawerClose);
        }

        /// <summary>Shut at once, without the swing (a new day).</summary>
        public void CloseInstantly()
        {
            _wantOpen = false;
            _open = 0f;
            ApplyDoors();
            SetShown(false);
        }

        private void Update()
        {
            if (openRoot == null || !openRoot.activeSelf) return;
            float seconds = tuning != null ? Mathf.Max(0.01f, tuning.doorSeconds) : 0.6f;
            _open = Mathf.MoveTowards(_open, _wantOpen ? 1f : 0f, Time.deltaTime / seconds);
            ApplyDoors();
            if (!_wantOpen && _open <= 0f) SetShown(false);
            else if (_dirty) Rebuild();
        }

        private void ApplyDoors()
        {
            float angle = (tuning != null ? tuning.doorOpenAngle : 105f) * Mathf.SmoothStep(0f, 1f, _open);
            if (leftDoor != null) leftDoor.localRotation = Quaternion.Euler(0f, angle, 0f);
            if (rightDoor != null) rightDoor.localRotation = Quaternion.Euler(0f, -angle, 0f);
        }

        private void SetShown(bool open)
        {
            if (open) ShareClosedLightmap();
            if (openRoot != null) openRoot.SetActive(open);
            foreach (var r in closedRenderers)
                if (r != null) r.enabled = !open;
        }

        /// <summary>P135: the cut shell and doors use the closed model's lightmap texels, so they look exactly like it.</summary>
        private void ShareClosedLightmap()
        {
            Renderer baked = null;
            foreach (var r in closedRenderers)
                if (r != null && r.lightmapIndex >= 0 && r.lightmapIndex < LightmapSettings.lightmaps.Length) { baked = r; break; }
            if (baked == null) return; // not baked: they keep the light probes
            foreach (var r in sameLightAsClosed)
            {
                if (r == null) continue;
                r.lightmapIndex = baked.lightmapIndex;
                r.lightmapScaleOffset = baked.lightmapScaleOffset;
            }
        }

        /// <summary>Lays the goods out again: one compartment per item type, a few copies for a larger count.</summary>
        private void Rebuild()
        {
            _dirty = false;
            foreach (var go in _shown) Destroy(go);
            _shown.Clear();
            if (_locker == null || slots.Length == 0) return;
            int maxCopies = tuning != null ? tuning.maxCopiesPerSlot : 3;
            var placements = EvidenceLockerLayout.Place(EvidenceLockerLayout.Contents(_locker), slots.Length, maxCopies);
            foreach (var p in placements)
            {
                var slot = slots[p.Slot];
                if (slot == null) continue;
                var model = EvidenceLockerLayout.ModelFor(p.Item, tuning != null ? tuning.tagModels : null);
                for (int i = 0; i < p.Copies; i++) Lay(model, slot, i, p.Copies, p.Slot);
            }
        }

        /// <summary>One copy in its compartment: scaled to fit, standing on the shelf, copies side by side and staggered in depth.</summary>
        private void Lay(GameObject model, Transform slot, int index, int copies, int slotIndex)
        {
            var template = model != null ? model : evidenceBag;
            if (template == null) return;
            var go = Instantiate(template, slot, false);
            go.name = template.name;
            go.SetActive(true);
            foreach (var c in go.GetComponentsInChildren<Collider>()) Destroy(c);
            if (probeAnchor != null)
                foreach (var r in go.GetComponentsInChildren<Renderer>()) r.probeAnchor = probeAnchor;
            var t = go.transform;
            t.localPosition = Vector3.zero;
            // A little turn per copy and compartment, so a row of the same thing doesn't look stamped.
            t.localRotation = Quaternion.Euler(0f, (slotIndex * 37 + index * 23) % 30 - 15f, 0f);
            t.localScale = Vector3.one;

            Vector3 box = tuning != null ? tuning.slotSize : new Vector3(0.4f, 0.42f, 0.36f);
            float step = box.x / copies; // copies overlap a little: every other one stands further back
            if (copies > 1) box = new Vector3(step * 1.4f, box.y, box.z * 0.6f);
            var bounds = Bounds(go);
            float scale = EvidenceLockerLayout.FitScale(bounds.size, box, tuning != null ? tuning.maxUpscale : 1.6f);
            t.localScale = Vector3.one * scale;

            // Copies: side by side, every other one further back.
            float x = (index - (copies - 1) * 0.5f) * step;
            float z = copies > 1 ? (index % 2 == 0 ? -0.2f : 0.2f) * box.z : 0f;
            var target = slot.TransformPoint(new Vector3(x, 0f, z));
            bounds = Bounds(go);
            t.position += new Vector3(target.x - bounds.center.x, target.y - bounds.min.y, target.z - bounds.center.z);
            _shown.Add(go);
        }

        private static Bounds Bounds(GameObject go)
        {
            var renderers = go.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return new Bounds(go.transform.position, Vector3.zero);
            var b = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) b.Encapsulate(renderers[i].bounds);
            return b;
        }
    }
}
