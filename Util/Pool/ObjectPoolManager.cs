using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class ObjectPoolManager : MonoBehaviour 
{
    private Dictionary<string, GameObject> m_poolBase = new();
    private Dictionary<string, List<GameObject>> m_activePool = new();
    private Dictionary<string, List<GameObject>> m_disablePool = new();

    public bool CheckAddKey(string key) => m_poolBase.ContainsKey(key);

    public void AddKey(string key)
    {
        if (CheckAddKey(key)) return;
        m_poolBase.Add(key, null);
    }

    public void SetPoolObject(string key, GameObject target)
    {
        if (m_poolBase.ContainsKey(key) == false) return;

        var pool = target.AddComponent<PoolObejct>();
        pool.key = key;

        m_activePool.Add(key, new());
        m_disablePool.Add(key, new());
        m_poolBase[key] = pool.gameObject;

        pool.gameObject.SetActive(false);
    }

    public GameObject AddPoolObject(string key, Transform parent = null)
    {
        if (m_poolBase.ContainsKey(key) == false)
        {
            Debug.Log($"찾을 수 없는 키: {key}");
            return null;
        }

        if (m_disablePool.ContainsKey(key) == false)
            m_disablePool.Add(key, new());

        GameObject result;

        if (m_disablePool[key].Count > 0)
        {
            result = m_disablePool[key].First();
            m_disablePool[key].Remove(result);

            if (result == null)
            {
                result = Instantiate(m_poolBase[key].gameObject, parent);
            }
        }
        else
        {
            result = Instantiate(m_poolBase[key].gameObject, parent);
        }

        m_activePool[key].Add(result);
        if (parent != null)
            result.transform.SetParent(parent);

        result.gameObject.SetActive(true);
        return result;
    }

    public void DisablePool(string key, GameObject target)
    {
        if (m_disablePool.ContainsKey(key))
        {
            m_disablePool[key].Add(target);
            if (m_activePool.ContainsKey(key))
                m_activePool[key].Remove(target);
        }
        else
        {
            m_disablePool.Add(key, new List<GameObject>() { target });
        }
    }

    public void DestroyObject(string key, GameObject target)
    {
        if (m_disablePool.ContainsKey(key) && m_disablePool[key].Contains(target))
            m_disablePool[key].Remove(target);

        if (m_activePool.ContainsKey(key) && m_activePool[key].Contains(target))
            m_activePool[key].Remove(target);
    }

    public void RemovePoolObject(string key)
    {
        Action<Dictionary<string, List<GameObject>>> clearAction = (dict) => {
            if (dict.ContainsKey(key))
            {
                for (int i = dict[key].Count - 1; i >= 0; i--)
                    if (dict[key][i] != null) Destroy(dict[key][i]);
                dict.Remove(key);
            }
        };

        clearAction(m_disablePool);
        clearAction(m_activePool);

        if (m_poolBase.ContainsKey(key))
        {
            if (m_poolBase[key] != null) Destroy(m_poolBase[key]);
            m_poolBase.Remove(key);
        }
    }

    public void ClearNullPoolObject()
    {
        foreach (var key in m_disablePool.Keys.ToList())
            m_disablePool[key].RemoveAll(obj => obj.IsUnityNull());

        foreach (var key in m_activePool.Keys.ToList())
            m_activePool[key].RemoveAll(obj => obj.IsUnityNull());
    }
}

public class PoolObejct : MonoBehaviour
{
    public string key = ""; 

    private void OnDisable()
    {
        if (GameMaster.Instance.objectPoolManager != null)
            GameMaster.Instance.objectPoolManager.DisablePool(key, gameObject);
    }

    private void OnDestroy()
    {
        if (GameMaster.Instance.objectPoolManager != null)
            GameMaster.Instance.objectPoolManager.DestroyObject(key, gameObject);
    }
}