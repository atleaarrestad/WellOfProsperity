using UnityEngine;
using System;
using System.Collections.Generic;
using Assets.Scripts.Combat.Interfaces;

namespace Assets.Scripts.Traps.Detectors
{
    /// <summary>
    /// Detects targets within a max distance that are visible (not blocked by obstacles).
    /// This is a polling detector (runs scans in Update at a configurable rate).
    /// Uses ContactFilter2D + results array (non-alloc) instead of deprecated NonAlloc API.
    /// </summary>
    public class LineOfSightDetector : MonoBehaviour, ITrapDetector
    {
        public event Action<IReadOnlyList<ITargetable>> OnDetection;

        [Header("Range")]
        [SerializeField] private float maxDistance = 8f;

        [Header("Masks")]
        [Tooltip("Only colliders on this mask are considered potential targets.")]
        [SerializeField] private LayerMask targetMask;
        [Tooltip("Colliders on this mask block line of sight (walls, props, etc.).")]
        [SerializeField] private LayerMask obstacleMask;

        [Header("Scan")]
        [Tooltip("How many times per second to scan. 0 = every frame.")]
        [SerializeField] private float scansPerSecond = 10f;
        [Tooltip("If true, only notifies when the visible set changes.")]
        [SerializeField] private bool onlyNotifyOnChange = true;

        [Header("Performance")]
        [SerializeField] private int maxHits = 32;

        [Header("Triggers")]
        [Tooltip("If true, trigger colliders can be detected as targets.")]
        [SerializeField] private bool includeTriggerTargets = true;

        private bool isEnabled;
        private float nextScanTime;

        private Collider2D[] hits;
        private ContactFilter2D targetFilter;

        private readonly List<ITargetable> visible = new();
        private readonly HashSet<ITargetable> lastSet = new();

        private void Awake() {
            EnsureBuffer();
            BuildFilter();
        }

        public void Enable() {
            isEnabled = true;
            nextScanTime = 0f;
        }

        public void Disable() {
            isEnabled = false;
        }

        private void Update() {
            if (!isEnabled){
                return;
            }

            if (scansPerSecond > 0f) {
                if (Time.time < nextScanTime) return;
                nextScanTime = Time.time + (1f / scansPerSecond);
            }

            Scan();
        }

        private void Scan() {
            EnsureBuffer();

            visible.Clear();

            var count = Physics2D.OverlapCircle(transform.position, maxDistance, targetFilter, hits);

            Vector2 origin = transform.position;

            for (int i = 0; i < count; i++)
            {
                var collider = hits[i];
                hits[i] = null;

                if (collider == null) {
                    continue;
                }

                ITargetable target = null;

                if (collider.TryGetComponent<ITargetable>(out var t0)) {
                    target = t0;
                }
                else if (collider.GetComponentInParent<MonoBehaviour>() is ITargetable t1) {
                    target = t1;
                }

                if (target == null) {
                    continue;
                }

                if (target is not Component targetComponent) {
                    continue;
                }

                Vector2 targetPos = targetComponent.transform.position;
                Vector2 dir = targetPos - origin;
                float dist = dir.magnitude;

                if (dist <= 0.0001f) {
                    visible.Add(target);
                    continue;
                }

                dir /= dist; // normalize

                var hit = Physics2D.Raycast(origin, dir, dist, obstacleMask);
                if (hit.collider is not null) {
                    continue;
                }

                visible.Add(target);
            }

            if (visible.Count == 0) {
                if (onlyNotifyOnChange && lastSet.Count > 0) {
                    lastSet.Clear();
                }
                return;
            }

            if (onlyNotifyOnChange) {

                var isChanged = HasChanged(visible, lastSet);
                if (!isChanged) {
                    return;
                }

                lastSet.Clear();
                for (int i = 0; i < visible.Count; i++) {
                    lastSet.Add(visible[i]);
                }
            }

            OnDetection?.Invoke(visible);
        }

        private void EnsureBuffer()
        {
            int size = Mathf.Max(1, maxHits);
            if (hits == null || hits.Length != size) {
                hits = new Collider2D[size];
            }
        }

        private void BuildFilter()
        {
            targetFilter = new ContactFilter2D {
                useLayerMask = true,
                layerMask = targetMask,
                useTriggers = includeTriggerTargets,
                useDepth = false
            };
        }

        private static bool HasChanged(List<ITargetable> current, HashSet<ITargetable> last)
        {
            if (current.Count != last.Count) {
                return true;
            }

            for (int i = 0; i < current.Count; i++)
                if (!last.Contains(current[i])) {
                    return true;
                }

            return false;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireSphere(transform.position, maxDistance);
        }

        private void OnValidate()
        {
            if (maxDistance < 0f) maxDistance = 0f;
            if (maxHits < 1) maxHits = 1;
            if (scansPerSecond < 0f) scansPerSecond = 0f;

            BuildFilter();
            EnsureBuffer();
        }
#endif
    }
}
