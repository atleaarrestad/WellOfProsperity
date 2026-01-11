using Assets.Scripts.Pooling;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;

public static class PrefabPools
{
    private class PoolData
    {
        public ObjectPool<GameObject> Pool;
        public int Alive;
        public int MaxSize;
    }

    private static readonly Dictionary<GameObject, PoolData> _pools = new();

    public static void Prewarm(GameObject prefab, int count, int defaultCapacity = 32, int maxSize = 512)
    {
        if (prefab == null || count <= 0) return;

        var data = GetOrCreate(prefab, defaultCapacity, maxSize);

        for (int i = 0; i < count; i++)
        {
            var go = data.Pool.Get();
            data.Pool.Release(go);
        }
    }

    public static GameObject Spawn(
        GameObject prefab,
        Vector3 position,
        Quaternion rotation,
        Transform parent = null,
        int maxAliveCap = 0,
        int defaultCapacity = 32,
        int maxSize = 512)
    {
        if (prefab == null) return null;

        var data = GetOrCreate(prefab, defaultCapacity, maxSize);

        if (maxAliveCap > 0 && data.Alive >= maxAliveCap)
            return null;

        var go = data.Pool.Get();

        // Ensure marker knows prefab
        var marker = go.GetComponent<PooledInstance>() ?? go.AddComponent<PooledInstance>();
        marker.Prefab = prefab;

        // Place/parent
        var t = go.transform;
        t.SetParent(parent, false);
        t.SetPositionAndRotation(position, rotation);

        if (!go.activeSelf) go.SetActive(true);

        data.Alive++;

        NotifySpawned(go);
        return go;
    }

    public static void Despawn(GameObject instance)
    {
        if (instance == null) return;

        var marker = instance.GetComponent<PooledInstance>();
        if (marker == null || marker.Prefab == null)
        {
            Object.Destroy(instance);
            return;
        }

        if (!_pools.TryGetValue(marker.Prefab, out var data))
        {
            Object.Destroy(instance);
            return;
        }

        data.Alive = Mathf.Max(0, data.Alive - 1);
        data.Pool.Release(instance);
    }

    public static int GetAlive(GameObject prefab)
    {
        if (prefab == null) return 0;
        return _pools.TryGetValue(prefab, out var data) ? data.Alive : 0;
    }

    private static PoolData GetOrCreate(GameObject prefab, int defaultCapacity, int maxSize)
    {
        if (_pools.TryGetValue(prefab, out var existing))
            return existing;

        var data = new PoolData { Alive = 0, MaxSize = Mathf.Max(1, maxSize) };

        data.Pool = new ObjectPool<GameObject>(
            createFunc: () =>
            {
                var go = Object.Instantiate(prefab);
                var marker = go.GetComponent<PooledInstance>() ?? go.AddComponent<PooledInstance>();
                marker.Prefab = prefab;
                go.SetActive(false);
                return go;
            },
            actionOnGet: go =>
            {
                // no-op (we activate in Spawn after positioning)
            },
            actionOnRelease: go =>
            {
                NotifyDespawned(go);
                go.SetActive(false);
                go.transform.SetParent(null, false);
            },
            actionOnDestroy: go =>
            {
                Object.Destroy(go);
            },
            collectionCheck: false,
            defaultCapacity: Mathf.Max(1, defaultCapacity),
            maxSize: Mathf.Max(1, maxSize)
        );

        _pools[prefab] = data;
        return data;
    }

    private static void NotifySpawned(GameObject go)
    {
        var comps = go.GetComponentsInChildren<MonoBehaviour>(true);
        for (int i = 0; i < comps.Length; i++)
            if (comps[i] is IPoolable p)
                p.OnSpawned();
    }

    private static void NotifyDespawned(GameObject go)
    {
        var comps = go.GetComponentsInChildren<MonoBehaviour>(true);
        for (int i = 0; i < comps.Length; i++)
            if (comps[i] is IPoolable p)
                p.OnDespawned();
    }
}
