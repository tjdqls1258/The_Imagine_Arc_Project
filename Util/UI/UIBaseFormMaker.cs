using System;
using UnityEngine;

/// <summary>
/// 모든 UI 패널 및 요소의 근간이 되는 최상위 베이스 클래스입니다.
/// UI의 초기화, 애니메이션 재생, 열기/닫기 콜백 및 UIManager와의 통신을 관리합니다.
/// </summary>
public class UIBase : CachObject
{
    // ====== UI 연출 및 상태 ======
    /// <summary> UI가 열릴 때 재생할 애니메이션 컴포넌트입니다. </summary>
    public Animation m_UIOpenAnim;

    /// <summary> UI가 화면에 나타난 직후 실행될 액션입니다. </summary>
    public Action m_onShow;

    /// <summary> UI가 닫힐 때 실행될 액션입니다. </summary>
    public Action m_onClose;

    /// <summary> 이 UI를 식별하기 위한 고유 시퀀스 번호입니다. </summary>
    protected UIManager.UISequence m_UISequence;

    // ----------------------------------------------------------------------
    // ## Initialization (초기화)
    // ----------------------------------------------------------------------

    public void SetSequence(UIManager.UISequence sequence)
        => m_UISequence = sequence;

    protected virtual void Awake()
    {
        // 자식 클래스에서 컴포넌트 바인딩 등을 수행하기 위한 가상 메서드
    }

    /// <summary>
    /// UI가 생성되거나 재사용될 때 호출되는 초기화 함수입니다.
    /// </summary>
    /// <param name="parent">배치될 부모 Transform (Canvas 등)</param>
    public virtual void Init(Transform parent = null)
    {
        Logger.Log($"{GetType()}::Init");

        // 이전 호출의 콜백 데이터 초기화
        m_onShow = m_onClose = null;

        // 부모 설정 (레이아웃 배치)
        if (parent != null)
            transform.SetParent(parent);
    }

    /// <summary>
    /// UI 실행에 필요한 데이터(콜백 등)를 설정합니다.
    /// </summary>
    public virtual void SetInfo(BaseUIData uiData)
    {
        Logger.Log($"{GetType()}::SetInfo");

        m_onShow = uiData.OnShow;
        m_onClose = uiData.OnClose;
    }

    // ----------------------------------------------------------------------
    // ## Lifecycle Control (상태 제어)
    // ----------------------------------------------------------------------

    /// <summary>
    /// UI를 화면에 노출하고 애니메이션 및 OnShow 콜백을 실행합니다.
    /// </summary>
    public virtual void ShowUI()
    {
        // 1. 등록된 오픈 애니메이션이 있다면 재생
        if (m_UIOpenAnim)
            m_UIOpenAnim.Play();

        // 2. OnShow 콜백 실행 후 1회성이므로 비움
        m_onShow?.Invoke();
        m_onShow = null;
    }

    /// <summary>
    /// UI를 닫고 UIManager에 상태 변경을 알립니다.
    /// </summary>
    /// <param name="isClosetAll">true일 경우 전체 닫기 과정이므로 개별 OnClose를 무시합니다.</param>
    public virtual void CloseUI(bool isClosetAll = false)
    {
        // 개별 닫기일 때만 OnClose 콜백 실행
        if (isClosetAll == false)
            m_onClose?.Invoke();

        m_onClose = null;

        // UIManager를 통해 관리 스택에서 이 UI를 제거하도록 요청
        UIManager.Instance.CloseUI(m_UISequence);
    }

    /// <summary>
    /// 주로 닫기 버튼(X버튼)에 연결되는 이벤트 함수입니다.
    /// </summary>
    public virtual void OnClickClosetButton()
    {
        // 필요 시 사운드 재생 등을 추가할 수 있습니다.
        CloseUI(false);
    }
}

// ----------------------------------------------------------------------
// ## Derived Classes (파생 클래스)
// ----------------------------------------------------------------------

/// <summary>
/// 에디터 툴(UIMaker)에서 인식하기 위한 일반 UI 폼 마커 클래스입니다.
/// </summary>
public class UIBaseFormMaker : UIBase
{
}

/// <summary>
/// 로비 화면의 동적 갱신이 필요한 UI들을 위한 베이스 클래스입니다.
/// </summary>
public class UILobbyUpdate : UIBaseFormMaker
{
    /// <summary> 로비 데이터가 갱신될 때 호출할 인터페이스 </summary>
    public virtual void UpdateFormLobby() { }

    /// <summary> 로비를 벗어날 때 호출할 정리 인터페이스 </summary>
    public virtual void CloseFormLobby() { }
}

/// <summary>
/// UI 실행 시 전달할 기초 데이터 구조체입니다.
/// </summary>
public class BaseUIData
{
    public Action OnShow;
    public Action OnClose;
}