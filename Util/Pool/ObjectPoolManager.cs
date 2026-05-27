using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// ������Ʈ�� ������ �ı� ��� ������ �����Ͽ� �޸� �� ����(GC)�� ����ȭ�ϴ� �Ŵ����Դϴ�.
/// </summary>
public class ObjectPoolManager : MonoBehaviour 
{
    // ====== Ǯ ������ ���� ======
    // Key: ������Ʈ�� �̸� Ȥ�� �ĺ���

    /// <summary> Ǯ�� �ٰ��� �Ǵ� ���� ������/��ü�� �����մϴ�. </summary>
    private Dictionary<string, GameObject> m_poolBase = new();

    /// <summary> ���� ������ Ȱ��ȭ(Active)�Ǿ� ��� ���� ��ü ����Ʈ�Դϴ�. </summary>
    private Dictionary<string, List<GameObject>> m_activePool = new();

    /// <summary> ����� ���� ��Ȱ��ȭ(Disable)�Ǿ� ��� ���� ��ü ����Ʈ�Դϴ�. </summary>
    private Dictionary<string, List<GameObject>> m_disablePool = new();

    // ----------------------------------------------------------------------
    // ## Pool Registration (���)
    // ----------------------------------------------------------------------

    public bool CheckAddKey(string key) => m_poolBase.ContainsKey(key);

    public void AddKey(string key)
    {
        if (CheckAddKey(key)) return;
        m_poolBase.Add(key, null);
    }

    /// <summary>
    /// Ư�� Ű���� �����ϴ� ���� ������Ʈ�� �����ϰ� Ǯ �ý����� �ʱ�ȭ�մϴ�.
    /// </summary>
    public void SetPoolObject(string key, GameObject target)
    {
        if (m_poolBase.ContainsKey(key) == false) return;

        // ���� ������Ʈ�� �ڵ� �ݳ��� ������Ʈ �߰�
        var pool = target.AddComponent<PoolObejct>();
        pool.key = key;

        m_activePool.Add(key, new());
        m_disablePool.Add(key, new());
        m_poolBase[key] = pool.gameObject;

        pool.gameObject.SetActive(false); // ���� �� ��Ȱ��ȭ
    }

    // ----------------------------------------------------------------------
    // ## Pool Control (�뿩 �� �ݳ�)
    // ----------------------------------------------------------------------

    /// <summary>
    /// Ǯ���� ������Ʈ�� �����ɴϴ�. ��� ���� ��ü�� ������ ���� ����(Instantiate)�մϴ�.
    /// </summary>
    public GameObject AddPoolObject(string key, Transform parent = null)
    {
        if (m_poolBase.ContainsKey(key) == false)
        {
            Logger.Log($"�ش� Ű�� Ǯ�� �������� �ʽ��ϴ�: {key}");
            return null;
        }

        if (m_disablePool.ContainsKey(key) == false)
            m_disablePool.Add(key, new());

        GameObject result;

        // 1. ��� ���� ������Ʈ�� �ִ� ��� ����
        if (m_disablePool[key].Count > 0)
        {
            result = m_disablePool[key].First();
            m_disablePool[key].Remove(result);

            // ���� Ǯ ���� ��ü�� ����ġ �ʰ� �ı��� ���(Null) ���� ó��
            if (result == null)
            {
                Logger.Log($"��� Ǯ�� ��ü�� ���ǵǾ����ϴ�. ���� �����մϴ�.");
                result = Instantiate(m_poolBase[key].gameObject, parent);
            }
        }
        // 2. ��� ���� ������Ʈ�� ���� ��� �������� ���� ����
        else
        {
            result = Instantiate(m_poolBase[key].gameObject, parent);
        }

        // Ȱ�� ����Ʈ�� �߰� �� ���� ����
        m_activePool[key].Add(result);
        if (parent != null)
            result.transform.SetParent(parent);

        result.gameObject.SetActive(true); // Ȱ��ȭ �� PoolObject�� OnDisable ���� ����

        return result;
    }

    /// <summary>
    /// ������Ʈ�� ��Ȱ��ȭ�� �� ȣ��Ǿ� Ȱ�� Ǯ���� ��Ȱ�� Ǯ�� ��ġ�� �ű�ϴ�.
    /// </summary>
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

    // ----------------------------------------------------------------------
    // ## Cleanup (����)
    // ----------------------------------------------------------------------

    /// <summary> Ư�� Ǯ�� ��� ��ü�� ���������� �ı��ϰ� �޸𸮿��� �����մϴ�. </summary>
    public void RemovePoolObject(string key)
    {
        // ��Ȱ��/Ȱ�� ����Ʈ ��ȸ�ϸ� Destroy ����
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

    /// <summary> ����Ƽ ������ ���� �ı��� ��ü���� ��ųʸ��� �����ִ� ���(Missing)�� û���մϴ�. </summary>
    public void ClearNullPoolObject()
    {
        // IsUnityNull()�� ����Ͽ� Missing Reference ������ ��ü�� ����
        foreach (var key in m_disablePool.Keys.ToList())
            m_disablePool[key].RemoveAll(obj => obj.IsUnityNull());

        foreach (var key in m_activePool.Keys.ToList())
            m_activePool[key].RemoveAll(obj => obj.IsUnityNull());
    }
}

/// <summary>
/// ������Ʈ Ǯ�� ���� ���� ��ü�� �����Ǿ� ���� ��ȭ�� �Ŵ������� �˸��� ������Ʈ�Դϴ�.
/// </summary>
public class PoolObejct : MonoBehaviour
{
    public string key = ""; // �Ҽӵ� Ǯ�� Ű��

    /// <summary> ������Ʈ�� SetActive(false) �� �� �ڵ����� Ǯ�� �ݳ� ó�� </summary>
    private void OnDisable()
    {
        // �Ŵ����� �ı��Ǵ� ����(�� ��ȯ ��)�� �ƴ� ���� �ݳ� ���μ��� ����
        if (GameMaster.Instance.objectPoolManager != null)
            GameMaster.Instance.objectPoolManager.DisablePool(key, gameObject);
    }

    /// <summary> ������Ʈ�� ���������� Destroy �� �� �Ŵ��� ����Ʈ���� ���� </summary>
    private void OnDestroy()
    {
        if (GameMaster.Instance.objectPoolManager != null)
            GameMaster.Instance.objectPoolManager.DestroyObject(key, gameObject);
    }
}