using UnityEngine;
using System;
using System.Collections.Generic;
using Assets.Scripts.Combat.Interfaces;

namespace Assets.Scripts.Traps.Detectors
{
    public class TimedPulseDetector : MonoBehaviour, ITrapDetector
    {
        public event Action<IReadOnlyList<ITargetable>> OnDetection;

        [Header("Timing")]
        [SerializeField] private float intervalSeconds = 12f;
        [Tooltip("If true, fires immediately when enabled (then continues at interval).")]
        [SerializeField] private bool fireImmediatelyOnEnable = false;

        private bool isEnabled;
        private float nextFireTime;

        private static readonly IReadOnlyList<ITargetable> EmptyTargets = Array.Empty<ITargetable>();

        public void Enable()
        {
            Debug.Log($"TimedPulseDetector on {name} enabled.");
            isEnabled = true;

            var interval = Mathf.Max(0.01f, intervalSeconds);
            nextFireTime = Time.time + interval;

            if (fireImmediatelyOnEnable)
            {
                OnDetection?.Invoke(EmptyTargets);
                nextFireTime = Time.time + interval;
            }
        }

        public void Disable()
        {
            isEnabled = false;
        }

        private void OnEnable()
        {
            // Enable();
        }

        private void OnDisable()
        {
            isEnabled = false;
        }

        private void Update()
        {
            if (!isEnabled)
            {
                return;
            }

            if (Time.time < nextFireTime)
            {
                return;
            }

            var interval = Mathf.Max(0.01f, intervalSeconds);
            nextFireTime = Time.time + interval;

            OnDetection?.Invoke(EmptyTargets);
            Debug.Log($"TimedPulseDetector on {name} Invoked!.");
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (intervalSeconds < 0f)
            {
                intervalSeconds = 0f;
            }
        }
#endif
    }
}
