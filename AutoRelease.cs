using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AutoRelease : MonoBehaviour
{
    private AsyncOperationHandle<GameObject> m_handle;

    private Action<string, GameObject> m_onDestroyAction;

    private string m_key;

    public void Init(string key, AsyncOperationHandle<GameObject> handle, Action<string, GameObject> action)
    {
        m_key = key;
        m_handle = handle;
        m_onDestroyAction = action;
    }

    private void OnDestroy()
    {
        m_onDestroyAction?.Invoke(m_key, gameObject);

        if (m_handle.IsValid())
        {
            Addressables.Release(m_handle);
        }
    }
}