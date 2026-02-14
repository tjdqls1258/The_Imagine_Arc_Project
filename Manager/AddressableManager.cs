using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

/// <summary>
/// Unity Addressables 시스템의 리소스 로드, 인스턴스화, 다운로드 및 메모리 해제를 총괄하는 매니저입니다.
/// 모든 작업은 비동기로 처리되며 로드된 에셋의 캐싱을 통해 중복 로드를 방지합니다.
/// </summary>
public class AddressableManager : MonoSingleton<AddressableManager>
{
    // ====== Addressables Caching Dictionaries ======

    // 로드된 에셋 원본(프리팹, 텍스처 등) 저장소 (Key: Addressable Key)
    private readonly Dictionary<string, Object> m_loadedAssets = new();

    // 특정 키로 생성된 모든 인스턴스(GameObject) 추적용 (Key: Addressable Key)
    private readonly Dictionary<string, List<GameObject>> m_instantiatedGameObjects = new();

    // 스레드 안전을 위한 Lock 객체
    private readonly object _lock = new();

    // ====== State ======
    private Dictionary<string, long> m_checkDownload = new(); // 다운로드가 필요한 라벨 정보 저장
    private bool m_isInitialized = false;

    public bool IsInitialized => m_isInitialized;

    public override void Init()
    {
        base.Init();
    }

    // ----------------------------------------------------------------------
    // ## 초기화 및 패치 (Initialization and Patching)
    // ----------------------------------------------------------------------

    /// <summary>
    /// Addressables 시스템을 초기화합니다.
    /// </summary>
    public async UniTask InitAsync()
    {
        if (m_isInitialized) return;

        await Addressables.InitializeAsync();
        m_isInitialized = true;
    }

    /// <summary>
    /// 지정된 라벨들의 총 다운로드 크기를 확인하고 패치가 필요한 리스트를 구성합니다.
    /// </summary>
    /// <returns>총 다운로드 크기 (Bytes)</returns>
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

    /// <summary>
    /// 패치가 필요한 에셋들의 종속성 다운로드를 실행합니다.
    /// </summary>
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
                Logger.LogError($"[AddressableManager] Download failed for label: {label}");
                isAllSuccess = false;
                Addressables.Release(downloadHandle);
                break;
            }

            Addressables.Release(downloadHandle);
        }

        if (isAllSuccess) onSuccess?.Invoke();
        else onFail?.Invoke();
    }

    // ----------------------------------------------------------------------
    // ## 에셋 로딩 (Asset Loading - 원본)
    // ----------------------------------------------------------------------

    /// <summary>
    /// 에셋 원본을 로드하고 캐싱합니다. (중복 로드 시 캐시 반환)
    /// </summary>
    public async UniTask<T> LoadAssetAndCacheAsync<T>(string key) where T : Object
    {
        if (m_loadedAssets.TryGetValue(key, out Object cached)) return cached as T;

        var loadHandle = Addressables.LoadAssetAsync<T>(key);
        await loadHandle;

        if (loadHandle.Status == AsyncOperationStatus.Succeeded)
        {
            lock (_lock)
            {
                m_loadedAssets[key] = loadHandle.Result;
            }
            return loadHandle.Result;
        }

        Logger.LogError($"[AddressableManager] Failed to load asset: {key}");
        return null;
    }

    // ----------------------------------------------------------------------
    // ## 인스턴스화 (Instantiation - 실체)
    // ----------------------------------------------------------------------

    /// <summary>
    /// GameObject를 생성하고 AutoRelease 컴포넌트를 부착하여 자동 메모리 해제를 설정합니다.
    /// </summary>
    public async UniTask<GameObject> InstantiateObjectAsync(string key, Transform parent = null)
    {
        AsyncOperationHandle<GameObject> handle = Addressables.InstantiateAsync(key, parent);
        await handle;

        if (handle.Status != AsyncOperationStatus.Succeeded)
        {
            Logger.LogError($"[AddressableManager] Failed to instantiate: {key}");
            return null;
        }

        GameObject result = handle.Result;

        // 1. 인스턴스 추적 목록에 추가
        lock (_lock)
        {
            if (!m_instantiatedGameObjects.ContainsKey(key))
                m_instantiatedGameObjects.Add(key, new List<GameObject>());
            m_instantiatedGameObjects[key].Add(result);
        }

        // 2. 수명 주기 자동 관리 컴포넌트 추가
        AutoRelease releaseComp = result.GetComponent<AutoRelease>() ?? result.AddComponent<AutoRelease>();
        releaseComp.Init(key, handle, HandleInstanceDestroyed);

        return result;
    }

    /// <summary> 특정 컴포넌트를 포함한 인스턴스를 생성하여 반환합니다. </summary>
    public async UniTask<T> InstantiateComponentAsync<T>(string key, Transform parent = null) where T : Component
    {
        GameObject obj = await InstantiateObjectAsync(key, parent);
        return obj != null ? obj.GetComponent<T>() : null;
    }

    // ----------------------------------------------------------------------
    // ## 해제 및 정리 (Release and Cleanup)
    // ----------------------------------------------------------------------

    /// <summary>
    /// 인스턴스가 파괴될 때 호출되어 내부 리스트를 정리합니다.
    /// </summary>
    private void HandleInstanceDestroyed(string key, GameObject obj)
    {
        lock (_lock)
        {
            if (m_instantiatedGameObjects.TryGetValue(key, out List<GameObject> list))
                list.Remove(obj);
        }
    }

    /// <summary> 특정 키의 로드된 원본 에셋을 메모리에서 해제합니다. </summary>
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

    /// <summary> 모든 캐싱된 원본 에셋을 메모리에서 해제합니다. </summary>
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