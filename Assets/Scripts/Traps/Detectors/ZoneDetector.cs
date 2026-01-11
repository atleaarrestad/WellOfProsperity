using UnityEngine;
using System;
using System.Collections.Generic;
using Assets.Scripts.Combat.Interfaces;

namespace Assets.Scripts.Traps.Detectors
{
    /// <summary>
    /// Uses a 2D trigger collider as a zone. Tracks targets inside and fires OnDetection
    /// when targets enter (and optionally while targets remain inside).
    ///
    /// Setup:
    /// - Add a Collider2D to this GameObject (or a child) and set IsTrigger = true
    /// - Add a Rigidbody2D somewhere in the interaction (usually on the enemy; can also be on the trap)
    /// - Ensure targets implement ITargetable (on collider GO or parent)
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class ZoneDetector : MonoBehaviour, ITrapDetector
    {
        public event Action<IReadOnlyList<ITargetable>> OnDetection;

        [Header("Filtering")]
        [SerializeField] private LayerMask targetMask;
        [Tooltip("If true, will look for ITargetable on parent objects too.")]
        [SerializeField] private bool searchInParent = true;

        [Header("Trigger Behavior")]
        [Tooltip("Fire event when a target enters the zone.")]
        [SerializeField] private bool fireOnEnter = true;

        [Tooltip("Fire event when a target exits the zone (sends remaining targets).")]
        [SerializeField] private bool fireOnExit = false;

        [Tooltip("If > 0, fires repeatedly while at least one target is inside.")]
        [SerializeField] private float whileInsideIntervalSeconds = 0f;

        [Tooltip("If true, while-inside pulses only happen when detector is enabled.")]
        [SerializeField] private bool requireEnabledForWhileInside = true;

        private readonly List<ITargetable> inside = new();
        private readonly HashSet<ITargetable> insideSet = new();

        private bool isEnabled;
        private float nextPulseTime;

        private Collider2D zoneCollider;

        public void Enable()
        {
            isEnabled = true;
            nextPulseTime = 0f;
        }

        public void Disable()
        {
            isEnabled = false;
        }

        private void Awake() {
            zoneCollider = GetComponent<Collider2D>();
            if (zoneCollider != null && !zoneCollider.isTrigger) {
                Debug.LogWarning($"{nameof(ZoneDetector)} on {name}: Collider2D should be marked IsTrigger.");
            }
        }

        private void Update() {
            if (whileInsideIntervalSeconds <= 0f) return;
            if (inside.Count == 0) return;
            if (requireEnabledForWhileInside && !isEnabled) return;
            if (Time.time < nextPulseTime) return;

            nextPulseTime = Time.time + Mathf.Max(0.01f, whileInsideIntervalSeconds);
            OnDetection?.Invoke(inside);
        }

        private void OnTriggerEnter2D(Collider2D other) {
            if (!IsInLayerMask(other.gameObject.layer, targetMask)) {
                return;
            }

            if (!TryGetTarget(other, out var target)) {
                return;
            }

            if (insideSet.Add(target)) {
                inside.Add(target);

                if (fireOnEnter && isEnabled) {
                    OnDetection?.Invoke(inside);
                }
            }
        }

        private void OnTriggerExit2D(Collider2D other) {
            if (!IsInLayerMask(other.gameObject.layer, targetMask)) {
                return;
            }

            if (!TryGetTarget(other, out var target)) {
                return;
            }

            if (insideSet.Remove(target)) {
                inside.Remove(target);

                if (fireOnExit && isEnabled) {
                    OnDetection?.Invoke(inside);
                }
            }
        }

        private bool TryGetTarget(Collider2D col, out ITargetable target) {
            target = null;

            if (col.TryGetComponent<ITargetable>(out var t0)) {
                target = t0;
                return true;
            }

            if (searchInParent && col.GetComponentInParent<MonoBehaviour>() is ITargetable t1) {
                target = t1;
                return true;
            }

            return false;
        }

        private static bool IsInLayerMask(int layer, LayerMask mask)
        {
            return (mask.value & (1 << layer)) != 0;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (whileInsideIntervalSeconds < 0f) whileInsideIntervalSeconds = 0f;
        }
#endif
    }
}
