using System;
using UnityEngine;

/// <summary>
/// 모든 팝업 UI의 최상위 베이스 클래스입니다.
/// UI의 초기화, 버튼 이벤트 등록, 열기/닫기 시의 콜백 처리를 담당합니다.
/// </summary>
public class PopupBase : UIBase
{
    // ====== 델리게이트 (Callbacks) ======

    /// <summary> 팝업이 화면에 나타날 때 실행될 추가 액션입니다. </summary>
    public Action showAction;

    /// <summary> 팝업이 닫힐 때 실행될 추가 액션입니다. </summary>
    public Action closeAction;

    // ----------------------------------------------------------------------
    // ## 가상 메서드 (상속받아 재정의 가능)
    // ----------------------------------------------------------------------

    /// <summary>
    /// CachObject의 Bind 시스템을 사용하여 UI 컴포넌트들을 연결하는 로직을 구현합니다.
    /// </summary>
    protected virtual void BindInit()
    {
        // 상속받은 자식 클래스에서 Enum을 활용해 Bind<T> 호출
    }

    /// <summary>
    /// 버튼 컴포넌트들에 onClick 이벤트를 등록하는 로직을 구현합니다.
    /// </summary>
    protected virtual void AddBtnEvent()
    {
        // 상속받은 자식 클래스에서 Get<Button>(idx).onClick.AddListener 호출
    }

    // ----------------------------------------------------------------------
    // ## 팝업 생명주기 제어 (Lifecycle)
    // ----------------------------------------------------------------------

    /// <summary>
    /// 팝업이 생성될 때 PopupManager에 의해 호출되는 초기화 메서드입니다.
    /// </summary>
    /// <param name="parm">팝업 생성 시 전달할 가변 인자 데이터 (ID, 수치 등)</param>
    public virtual void Init(params object[] parm)
    {
        // 1. 팝업이 열릴 때 실행해야 할 외부 로직이 있다면 실행
        if (showAction != null)
            showAction.Invoke();

        // 2. 컴포넌트 바인딩 및 이벤트 등록 수행
        BindInit();
        AddBtnEvent();
    }

    /// <summary>
    /// 팝업을 닫고 메모리에서 제거합니다.
    /// </summary>
    public virtual void Close()
    {
        // 1. 팝업이 닫힐 때 실행해야 할 외부 로직(데이터 저장, 알림 등) 실행
        if (closeAction != null)
            closeAction.Invoke();

        // 2. 관리자(PopupManager)에게 현재 팝업이 닫혔음을 알림 (스택 관리)
        GameMaster.Instance.popupManager.CloseCurrentPopup();

        // 3. 실제 게임 오브젝트 파괴 (CachObject의 MyObj 사용)
        Destroy(MyObj);
    }
}