using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

public class AddressableManager
{
    private readonly Dictionary<string, Object> m_loadedAssets = new();
    private readonly Dictionary<string, List<GameObject>> m_instantiatedGameObjects = new();
    private readonly object _lock = new();

    private Dictionary<string, long> m_checkDownload = new();
    private bool m_isInitialized = false;

    public bool IsInitialized => m_isInitialized;

    public void Init() 
    {
    }

    public async UniTask InitAsync()
    {
        if (m_isInitialized) return;

        await Addressables.InitializeAsync();
        m_isInitialized = true;
    }

    public async UniTask<long> DownloadChecdk(string[] lables)
    {
        long totalByte = 0;
        m_checkDownload.Clear();

        foreach (string label in lables)
        {
            AsyncOperationHandle<long> sizeHandle = Addressables.GetDownloadSizeAsync(label);
            await sizeHandle.Task;

            if (sizeHandle.Status == AsyncOperationStatus.Succeeded)
            {
                long size = sizeHandle.Result;
                if (size > 0)
                {
                    totalByte += size;
                    m_checkDownload.Add(label, size);
                }
            }
            Addressables.Release(sizeHandle);
        }
        return totalByte;
    }

    public async UniTask DownloadAssetsAsync(Action<string, long, long> onDownloading = null,
        Action onSuccess = null, Action onFail = null)
    {
        if (!m_isInitialized) await InitAsync();

        bool isAllSuccess = true;
        foreach (string label in m_checkDownload.Keys)
        {
            AsyncOperationHandle downloadHandle = Addressables.DownloadDependenciesAsync(label);

            while (!downloadHandle.IsDone)
            {
                var status = downloadHandle.GetDownloadStatus();
                if (status.TotalBytes > 0)
                    onDownloading?.Invoke(label, status.DownloadedBytes, status.TotalBytes);

                await UniTask.Delay(100);
            }

            if (downloadHandle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"[AddressableManager] Download failed for label: {label}");
                isAllSuccess = false;
                Addressables.Release(downloadHandle);
                break;
            }

            Addressables.Release(downloadHandle);
        }

        if (isAllSuccess) onSuccess?.Invoke();
        else onFail?.Invoke();
    }

    public async UniTask<T> LoadAssetAndCacheAsync<T>(string key) where T : Object
    {
        if (m_loadedAssets.TryGetValue(key, out Object cached)) return cached as T;

        var loadHandle = Addressables.LoadAssetAsync<T>(key);
        await loadHandle;

        if (loadHandle.Status == AsyncOperationStatus.Succeeded)
        {
            lock (_lock)
            {
                if (!m_loadedAssets.ContainsKey(key))
                    m_loadedAssets[key] = loadHandle.Result;
            }
            return loadHandle.Result;
        }

        Debug.LogError($"[AddressableManager] Failed to load asset: {key}");
        return null;
    }

    public async UniTask<GameObject> InstantiateObjectAsync(string key, Transform parent = null)
    {
        AsyncOperationHandle<GameObject> handle = Addressables.InstantiateAsync(key, parent);
        await handle;

        if (handle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError($"[AddressableManager] Failed to instantiate: {key}");
            return null;
        }

        GameObject result = handle.Result;

        lock (_lock)
        {
            if (!m_instantiatedGameObjects.ContainsKey(key))
                m_instantiatedGameObjects.Add(key, new List<GameObject>());
            m_instantiatedGameObjects[key].Add(result);
        }

        AutoRelease releaseComp = result.GetComponent<AutoRelease>() ?? result.AddComponent<AutoRelease>();
        releaseComp.Init(key, handle, HandleInstanceDestroyed);

        return result;
    }

    public async UniTask<T> InstantiateComponentAsync<T>(string key, Transform parent = null) where T : Component
    {
        GameObject obj = await InstantiateObjectAsync(key, parent);
        return obj != null ? obj.GetComponent<T>() : null;
    }

    private void HandleInstanceDestroyed(string key, GameObject obj)
    {
        lock (_lock)
        {
            if (m_instantiatedGameObjects.TryGetValue(key, out List<GameObject> list))
                list.Remove(obj);
        }
    }

    public void UnloadAsset(string key)
    {
        lock (_lock)
        {
            if (m_loadedAssets.TryGetValue(key, out Object asset))
            {
                m_loadedAssets.Remove(key);
                Addressables.Release(asset);
            }
        }
    }

    public void UnloadAllAssets()
    {
        lock (_lock)
        {
            foreach (var asset in m_loadedAssets.Values)
                Addressables.Release(asset);
            m_loadedAssets.Clear();
        }
    }
}