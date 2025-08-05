using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Zenject;

public class UIFactory
{
    private readonly DiContainer _container;
    private readonly PoolManager _poolManager;

    public UIFactory(DiContainer container, PoolManager poolManager)
    {
        _container = container;
        _poolManager = poolManager;
    }

    public async UniTask<GameObject> CreatePageAsync(UIPageConfigSO config, Transform parent)
    {
        GameObject instance;

        if (config.usePooling)
        {
            instance = _poolManager.GetFromPool(config.pageType);
            if (instance == null)
            {
                instance = _container.InstantiatePrefab(config.prefab, parent);
                _poolManager.RegisterPool(config.pageType, instance);
            }
        }
        else
        {
            instance = _container.InstantiatePrefab(config.prefab, parent);
        }

        instance.transform.SetParent(parent, false);
        instance.SetActive(true);
        await UniTask.Yield(); // async-safe instantiation

        return instance;
    }
}
