using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

/// <summary>
/// 어드레서블 에셋의 수명 주기를 자동으로 관리하는 컴포넌트입니다.
/// 오브젝트가 파괴될 때 연결된 핸들을 해제하여 메모리 누수를 방지합니다.
/// </summary>
public class AutoRelease : MonoBehaviour
{
    // ====== Private Fields ======

    /// <summary> 에셋 로드 및 인스턴스화에 사용된 핸들입니다. </summary>
    private AsyncOperationHandle<GameObject> m_handle;

    /// <summary> 오브젝트 파괴 시 외부 매니저(예: AddressableManager)에 알리기 위한 콜백입니다. </summary>
    private Action<string, GameObject> m_onDestroyAction;

    /// <summary> 에셋을 식별하기 위한 고유 키(Addressable Key)입니다. </summary>
    private string m_key;

    // ----------------------------------------------------------------------
    // ## Initialization
    // ----------------------------------------------------------------------

    /// <summary>
    /// 자동 해제를 위한 필수 데이터들을 초기화합니다.
    /// </summary>
    /// <param name="key">에셋 식별 키</param>
    /// <param name="handle">해제할 어드레서블 핸들</param>
    /// <param name="action">파괴 시 실행할 추가 로직 (주로 매니저의 캐시 제거용)</param>
    public void Init(string key, AsyncOperationHandle<GameObject> handle, Action<string, GameObject> action)
    {
        m_key = key;
        m_handle = handle;
        m_onDestroyAction = action;
    }

    // ----------------------------------------------------------------------
    // ## Lifecycle (Memory Management)
    // ----------------------------------------------------------------------

    /// <summary>
    /// 오브젝트가 파괴되는 시점에 호출되어 메모리 해제를 수행합니다.
    /// </summary>
    private void OnDestroy()
    {
        // 1. 외부 매니저에 파괴 사실을 알림 (캐시 리스트 정리 등)
        m_onDestroyAction?.Invoke(m_key, gameObject);

        // 2. 어드레서블 핸들이 유효한지 확인 후 메모리 해제
        // 이 단계를 통해 에셋의 참조 카운트(Reference Count)가 감소합니다.
        if (m_handle.IsValid())
        {
            Addressables.Release(m_handle);
        }
    }
}