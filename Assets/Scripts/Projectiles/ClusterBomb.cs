using UnityEngine;

public class ClusterBomb : MonoBehaviour, IPoolable {
    public void OnDespawned() {
        
    }

    public void OnSpawned() {
        
    }

    public void Explode()
    {
        PrefabPools.Despawn(gameObject);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
