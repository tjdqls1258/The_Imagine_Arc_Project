using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// 오브젝트의 생성과 파괴 대신 재사용을 관리하여 메모리 및 성능(GC)을 최적화하는 매니저입니다.
/// </summary>
public class ObjectPoolManager : MonoBehaviour 
{
    // ====== 풀 데이터 관리 ======
    // Key: 오브젝트의 이름 혹은 식별자

    /// <summary> 풀의 근간이 되는 원본 프리팹/객체를 보관합니다. </summary>
    private Dictionary<string, GameObject> m_poolBase = new();

    /// <summary> 현재 씬에서 활성화(Active)되어 사용 중인 객체 리스트입니다. </summary>
    private Dictionary<string, List<GameObject>> m_activePool = new();

    /// <summary> 사용이 끝나 비활성화(Disable)되어 대기 중인 객체 리스트입니다. </summary>
    private Dictionary<string, List<GameObject>> m_disablePool = new();

    // ----------------------------------------------------------------------
    // ## Pool Registration (등록)
    // ----------------------------------------------------------------------

    public bool CheckAddKey(string key) => m_poolBase.ContainsKey(key);

    public void AddKey(string key)
    {
        if (CheckAddKey(key)) return;
        m_poolBase.Add(key, null);
    }

    /// <summary>
    /// 특정 키값에 대응하는 원본 오브젝트를 설정하고 풀 시스템을 초기화합니다.
    /// </summary>
    public void SetPoolObject(string key, GameObject target)
    {
        if (m_poolBase.ContainsKey(key) == false) return;

        // 원본 오브젝트에 자동 반납용 컴포넌트 추가
        var pool = target.AddComponent<PoolObejct>();
        pool.key = key;

        m_activePool.Add(key, new());
        m_disablePool.Add(key, new());
        m_poolBase[key] = pool.gameObject;

        pool.gameObject.SetActive(false); // 시작 시 비활성화
    }

    // ----------------------------------------------------------------------
    // ## Pool Control (대여 및 반납)
    // ----------------------------------------------------------------------

    /// <summary>
    /// 풀에서 오브젝트를 가져옵니다. 대기 중인 객체가 없으면 새로 생성(Instantiate)합니다.
    /// </summary>
    public GameObject AddPoolObject(string key, Transform parent = null)
    {
        if (m_poolBase.ContainsKey(key) == false)
        {
            Logger.Log($"해당 키의 풀이 존재하지 않습니다: {key}");
            return null;
        }

        if (m_disablePool.ContainsKey(key) == false)
            m_disablePool.Add(key, new());

        GameObject result;

        // 1. 대기 중인 오브젝트가 있는 경우 재사용
        if (m_disablePool[key].Count > 0)
        {
            result = m_disablePool[key].First();
            m_disablePool[key].Remove(result);

            // 만약 풀 내의 객체가 예기치 않게 파괴된 경우(Null) 예외 처리
            if (result == null)
            {
                Logger.Log($"대기 풀의 객체가 유실되었습니다. 새로 생성합니다.");
                result = Instantiate(m_poolBase[key].gameObject, parent);
            }
        }
        // 2. 대기 중인 오브젝트가 없는 경우 원본에서 복제 생성
        else
        {
            result = Instantiate(m_poolBase[key].gameObject, parent);
        }

        // 활성 리스트에 추가 및 상태 설정
        m_activePool[key].Add(result);
        if (parent != null)
            result.transform.SetParent(parent);

        result.gameObject.SetActive(true); // 활성화 시 PoolObject의 OnDisable 추적 시작

        return result;
    }

    /// <summary>
    /// 오브젝트가 비활성화될 때 호출되어 활성 풀에서 비활성 풀로 위치를 옮깁니다.
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
    // ## Cleanup (정리)
    // ----------------------------------------------------------------------

    /// <summary> 특정 풀의 모든 객체를 물리적으로 파괴하고 메모리에서 제거합니다. </summary>
    public void RemovePoolObject(string key)
    {
        // 비활성/활성 리스트 순회하며 Destroy 실행
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

    /// <summary> 유니티 엔진에 의해 파괴된 객체들이 딕셔너리에 남아있는 경우(Missing)를 청소합니다. </summary>
    public void ClearNullPoolObject()
    {
        // IsUnityNull()을 사용하여 Missing Reference 상태의 객체들 제거
        foreach (var key in m_disablePool.Keys.ToList())
            m_disablePool[key].RemoveAll(obj => obj.IsUnityNull());

        foreach (var key in m_activePool.Keys.ToList())
            m_activePool[key].RemoveAll(obj => obj.IsUnityNull());
    }
}

/// <summary>
/// 오브젝트 풀에 속한 개별 객체에 부착되어 상태 변화를 매니저에게 알리는 컴포넌트입니다.
/// </summary>
public class PoolObejct : MonoBehaviour
{
    public string key = ""; // 소속된 풀의 키값

    /// <summary> 오브젝트가 SetActive(false) 될 때 자동으로 풀에 반납 처리 </summary>
    private void OnDisable()
    {
        // 매니저가 파괴되는 시점(씬 전환 등)이 아닐 때만 반납 프로세스 실행
        if (GameMaster.Instance.objectPoolManager != null)
            GameMaster.Instance.objectPoolManager.DisablePool(key, gameObject);
    }

    /// <summary> 오브젝트가 물리적으로 Destroy 될 때 매니저 리스트에서 제거 </summary>
    private void OnDestroy()
    {
        if (GameMaster.Instance.objectPoolManager != null)
            GameMaster.Instance.objectPoolManager.DestroyObject(key, gameObject);
    }
}