using Assets.Scripts.Combat.Interfaces;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Traps.Effects
{
    public class SpawnObjectEffect : MonoBehaviour, ITrapEffect
    {
        [SerializeField] private GameObject prefab;

        [Min(1)]
        [SerializeField] private int countPerTrigger = 1;

        [Min(0)]
        [SerializeField] private int maxAlive = 100;

        [SerializeField] private bool useFirePoint = true;
        [SerializeField] private Transform parentOverride;

        [SerializeField] private float randomRadius = 6f;
        [SerializeField] private bool randomZRotation = true;

        [Header("Pooling")]
        [SerializeField] private bool usePooling = true;
        [Min(0)]
        [SerializeField] private int prewarmCount = 50;

        private bool prewarmed;

        private void Start()
        {
            TryPrewarm();
        }

        public void Execute(TrapContext context, IReadOnlyList<ITargetable> targets)
        {
            if (prefab == null) {
                return;
            }

            TryPrewarm();

            Vector2 basePos2 = useFirePoint ? context.firePoint : (Vector2)transform.position;
            Transform parent = parentOverride;

            for (int i = 0; i < countPerTrigger; i++)
            {
                Vector2 pos2 = basePos2 + Random.insideUnitCircle * Mathf.Max(0f, randomRadius);
                Quaternion rotation = randomZRotation
                    ? Quaternion.Euler(0f, 0f, Random.Range(0f, 360f))
                    : transform.rotation;

                var pos3 = new Vector3(pos2.x, pos2.y, 0f);

                if (usePooling)
                {
                    PrefabPools.Spawn(prefab, pos3, rotation, parent, maxAliveCap: maxAlive);
                }
                else
                {
                    // non-pooled fallback
                    Instantiate(prefab, pos3, rotation, parent);
                }
            }
        }

        private void TryPrewarm()
        {
            if (prewarmed) return;
            if (!usePooling) return;
            if (prewarmCount <= 0) return;
            if (prefab == null) return;

            PrefabPools.Prewarm(prefab, prewarmCount);
            prewarmed = true;
        }
    }
}
