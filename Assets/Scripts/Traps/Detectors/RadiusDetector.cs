using System;
using System.Collections.Generic;
using Assets.Scripts.Combat.Interfaces;
using UnityEngine;

namespace Assets.Scripts.Traps.Detectors
{
    public class RadiusDetector : MonoBehaviour, ITrapDetector
    {
        public event Action<IReadOnlyList<ITargetable>> OnDetection;

        [Header("Detection")]
        [SerializeField] private float radius = 3f;
        [SerializeField] private LayerMask targetMask;
        [Tooltip("How many times per second to scan. 0 = every frame.")]
        [SerializeField] private float scansPerSecond = 10f;

        [Header("Behavior")]
        [Tooltip("If true, only fires the event when the detected set changes.")]
        [SerializeField] private bool onlyNotifyOnChange = true;

        // Non-alloc buffer (tweak size depending on expected density)
        [SerializeField] private int maxHits = 32;

        private Collider2D[] _hits;
        private readonly List<ITargetable> _targets = new();
        private readonly HashSet<ITargetable> _lastSet = new();

        private bool _enabled = true;
        private float _nextScanTime;

        private void Awake()
        {
            _hits = new Collider2D[Mathf.Max(1, maxHits)];
        }

        public void Enable() => _enabled = true;
        public void Disable() => _enabled = false;

        private void Update()
        {
            if (!_enabled) return;

            if (scansPerSecond > 0f)
            {
                if (Time.time < _nextScanTime) return;
                _nextScanTime = Time.time + (1f / scansPerSecond);
            }

            Scan();
        }

        private void Scan()
        {
            // Resize buffer if changed in inspector at runtime
            if (_hits == null || _hits.Length != maxHits)
                _hits = new Collider2D[Mathf.Max(1, maxHits)];

            int count = Physics2D.OverlapCircleNonAlloc(transform.position, radius, _hits, targetMask);

            _targets.Clear();

            for (int i = 0; i < count; i++)
            {
                var col = _hits[i];
                if (col == null) continue;

                // Use collider or parent depending on your setup
                if (col.TryGetComponent<ITargetable>(out var t) ||
                    col.GetComponentInParent<MonoBehaviour>() is ITargetable parentT && (t = parentT) != null)
                {
                    _targets.Add(t);
                }

                // clear slots to avoid holding references when fewer hits next scan
                _hits[i] = null;
            }

            if (_targets.Count == 0)
            {
                // Up to you: some traps want a "no targets" event; most don't.
                // If you do want it, call OnDetection?.Invoke(_targets);
                if (onlyNotifyOnChange && _lastSet.Count > 0)
                {
                    _lastSet.Clear();
                    // optionally notify empty:
                    // OnDetection?.Invoke(_targets);
                }
                return;
            }

            if (onlyNotifyOnChange)
            {
                bool changed = HasChanged(_targets, _lastSet);
                if (!changed) return;

                _lastSet.Clear();
                for (int i = 0; i < _targets.Count; i++)
                    _lastSet.Add(_targets[i]);
            }

            OnDetection?.Invoke(_targets);
        }

        private static bool HasChanged(List<ITargetable> current, HashSet<ITargetable> last)
        {
            if (current.Count != last.Count) return true;

            for (int i = 0; i < current.Count; i++)
                if (!last.Contains(current[i]))
                    return true;

            return false;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.15f);
            Gizmos.DrawSphere(transform.position, radius);
        }
    }
}
