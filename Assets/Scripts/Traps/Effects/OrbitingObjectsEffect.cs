using Assets.Scripts.Combat.Interfaces;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Traps.Effects
{
    public class OrbitingObjectsEffect : MonoBehaviour, ITrapEffect
    {
        [Header("Spawn")]
        [SerializeField] private GameObject orbitingPrefab;
        [Min(1)]
        [SerializeField] private int objectCount = 4;
        [SerializeField] private float radius = 1.5f;

        [Header("Rotation")]
        [Tooltip("Degrees per second.")]
        [SerializeField] private float angularSpeedDegPerSec = 180f;
        [SerializeField] private bool clockwise = true;
        [SerializeField] private float startAngleDeg = 0f;

        [Header("Presentation")]
        [Tooltip("If true, each object rotates to face outward from the center.")]
        [SerializeField] private bool faceOutward = false;
        [Tooltip("Parent spawned objects under this effect GameObject.")]
        [SerializeField] private bool parentToThis = true;

        private readonly List<Transform> _spawned = new();
        private Vector2 _center;
        private float _baseAngleDeg;
        private bool _hasCenter;

        private void Awake()
        {
            _baseAngleDeg = startAngleDeg;
        }

        public void Execute(TrapContext context, IReadOnlyList<ITargetable> targets)
        {
            Debug.Log($"OrbitingObjectsEffect on {name} executing!");
            // Center is the trap's firePoint in world-space
            _center = context.firePoint;
            _hasCenter = true;

            if (orbitingPrefab == null)
            {
                Debug.LogWarning($"{nameof(OrbitingObjectsEffect)} on {name} has no prefab assigned.");
                return;
            }

            // Remove missing/destroyed entries
            for (int i = _spawned.Count - 1; i >= 0; i--)
            {
                if (_spawned[i] == null)
                    _spawned.RemoveAt(i);
            }

            // Replenish to desired count
            while (_spawned.Count < objectCount)
            {
                var go = Instantiate(
                    orbitingPrefab,
                    (Vector3)_center,
                    Quaternion.identity,
                    parentToThis ? transform : null
                );

                _spawned.Add(go.transform);
            }

            // If designer lowered objectCount, optionally remove extras:
            while (_spawned.Count > objectCount)
            {
                var t = _spawned[_spawned.Count - 1];
                _spawned.RemoveAt(_spawned.Count - 1);
                if (t != null) Destroy(t.gameObject);
            }

            // Snap positions immediately
            ApplyOrbitPositions(_baseAngleDeg);
        }

        private void Update()
        {
            if (!_hasCenter) return;

            // Keep the orbit running even if Execute isn't called again.
            // If you want the orbit to only run while "active", add a bool and gate here.
            if (_spawned.Count == 0) return;

            // Clean nulls (in case something destroyed orbiters)
            for (int i = _spawned.Count - 1; i >= 0; i--)
            {
                if (_spawned[i] == null)
                    _spawned.RemoveAt(i);
            }
            if (_spawned.Count == 0) return;

            float dir = clockwise ? -1f : 1f;
            _baseAngleDeg += angularSpeedDegPerSec * dir * Time.deltaTime;

            ApplyOrbitPositions(_baseAngleDeg);
        }

        private void ApplyOrbitPositions(float baseAngleDeg)
        {
            if (_spawned.Count == 0) return;

            float step = 360f / Mathf.Max(1, _spawned.Count);

            for (int i = 0; i < _spawned.Count; i++)
            {
                var t = _spawned[i];
                if (t == null) continue;

                float angleDeg = baseAngleDeg + (step * i);
                float rad = angleDeg * Mathf.Deg2Rad;

                Vector2 offset = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * radius;
                Vector2 pos2 = _center + offset;

                t.position = new Vector3(pos2.x, pos2.y, t.position.z);

                if (faceOutward)
                {
                    // Face away from center (2D: rotate around Z)
                    float rotZ = Mathf.Atan2(offset.y, offset.x) * Mathf.Rad2Deg;
                    t.rotation = Quaternion.Euler(0f, 0f, rotZ);
                }
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireSphere(Application.isPlaying ? (Vector3)_center : transform.position, radius);
        }

        private void OnValidate()
        {
            if (objectCount < 1) objectCount = 1;
            if (radius < 0f) radius = 0f;
        }
#endif
    }
}
