using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance { get; private set; }

    [System.Serializable]
    public struct PoolEntry
    {
        public GameObject prefab;
        public int initialSize;
    }

    public PoolEntry[] prewarmed;

    readonly Dictionary<GameObject, Queue<GameObject>> _pools = new();

    void Awake()
    {
        Instance = this;
        foreach (var entry in prewarmed)
            Prewarm(entry.prefab, entry.initialSize);
    }

    void Prewarm(GameObject prefab, int count)
    {
        if (!_pools.ContainsKey(prefab))
            _pools[prefab] = new Queue<GameObject>();

        for (int i = 0; i < count; i++)
        {
            var go = Instantiate(prefab);
            go.SetActive(false);
            _pools[prefab].Enqueue(go);
        }
    }

    public GameObject Get(GameObject prefab, Vector3 pos, Quaternion rot)
    {
        if (!_pools.ContainsKey(prefab))
            _pools[prefab] = new Queue<GameObject>();

        GameObject go;
        if (_pools[prefab].Count > 0)
            go = _pools[prefab].Dequeue();
        else
            go = Instantiate(prefab);

        go.transform.SetPositionAndRotation(pos, rot);
        go.SetActive(true);
        return go;
    }

    public void Return(GameObject prefab, GameObject instance)
    {
        instance.SetActive(false);
        if (!_pools.ContainsKey(prefab))
            _pools[prefab] = new Queue<GameObject>();
        _pools[prefab].Enqueue(instance);
    }
}
