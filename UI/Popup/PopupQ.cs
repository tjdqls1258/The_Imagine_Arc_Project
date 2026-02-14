using System;
using UnityEngine;

/// <summary>
/// 사용자에게 질문을 던지고 '확인' 또는 '취소' 선택을 받는 질문형 팝업 클래스입니다.
/// 각 버튼 클릭 시 실행될 콜백 액션을 설정하여 후속 처리를 제어할 수 있습니다.
/// </summary>
public class PopupQ : PopupMsg
{
    // ====== UI Binding Enums (CachObject 시스템 활용) ======

    /// <summary> 팝업 내 버튼 컴포넌트들을 식별하기 위한 Enum입니다. </summary>
    private enum Buttons
    {
        OKButton,    // 확인(Yes) 버튼
        CloseButton  // 취소/닫기(No) 버튼
    }

    /// <summary> 팝업 내 텍스트 컴포넌트들을 식별하기 위한 Enum입니다. </summary>
    enum Texts
    {
        MessageText  // 질문 내용이 표시될 텍스트
    }

    // ====== 델리게이트 (Callbacks) ======

    /// <summary> '확인' 버튼을 눌렀을 때 실행될 외부 로직입니다. </summary>
    public Action okAction;

    /// <summary> '취소' 또는 '닫기' 버튼을 눌렀을 때 실행될 외부 로직입니다. </summary>
    public Action noAction;

    // ----------------------------------------------------------------------
    // ## Initialization (Overrides)
    // ----------------------------------------------------------------------

    /// <summary>
    /// 팝업에서 사용할 버튼과 텍스트 컴포넌트들을 찾아 메모리에 바인딩합니다.
    /// </summary>
    protected override void BindInit()
    {
        // 부모 클래스인 PopupMsg의 Bind 로직을 재정의하여 이 팝업에 필요한 버튼들을 바인딩
        Bind<UnityEngine.UI.Button>(typeof(Buttons));
        Bind<TMPro.TextMeshProUGUI>(typeof(Texts));
    }

    /// <summary>
    /// 각 버튼에 클릭 시 실행될 이벤트를 할당합니다.
    /// </summary>
    protected override void AddBtnEvent()
    {
        // 1. 확인(OK) 버튼 이벤트 설정
        Get<UnityEngine.UI.Button>((int)Buttons.OKButton).onClick.AddListener(() =>
        {
            // 등록된 확인 액션이 있다면 실행 후 팝업 닫기
            okAction?.Invoke();
            this.Close();
        });

        // 2. 취소/닫기(Close) 버튼 이벤트 설정
        Get<UnityEngine.UI.Button>((int)Buttons.CloseButton).onClick.AddListener(() =>
        {
            // 등록된 취소 액션이 있다면 실행 후 팝업 닫기
            noAction?.Invoke();
            this.Close();
        });
    }
}