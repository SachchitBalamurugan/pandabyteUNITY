using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    private readonly Dictionary<UIPageType, Queue<GameObject>> _pools = new();
    private readonly Dictionary<UIPageType, GameObject> _activeInstances = new();

    public void RegisterPool(UIPageType pageType, GameObject prefab)
    {
        if (!_pools.ContainsKey(pageType))
            _pools[pageType] = new Queue<GameObject>();

        prefab.SetActive(false);
        _pools[pageType].Enqueue(prefab);
    }

    public GameObject GetFromPool(UIPageType pageType)
    {
        if (_pools.TryGetValue(pageType, out var queue) && queue.Count > 0)
        {
            var obj = queue.Dequeue();
            obj.SetActive(true);
            return obj;
        }

        return null; // Factory will instantiate if not found
    }

    public void ReturnToPool(UIPageType pageType, GameObject obj)
    {
        obj.SetActive(false);
        if (!_pools.ContainsKey(pageType))
            _pools[pageType] = new Queue<GameObject>();

        _pools[pageType].Enqueue(obj);
    }

    public void Clear()
    {
        _pools.Clear();
    }
}
