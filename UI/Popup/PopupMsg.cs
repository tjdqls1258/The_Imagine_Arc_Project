using UnityEngine;

/// <summary>
/// 사용자에게 간단한 텍스트 메시지를 보여주는 알림 팝업 클래스입니다.
/// 확인 버튼 하나로 구성된 가장 기본적인 형태의 팝업 구현체입니다.
/// </summary>
public class PopupMsg : PopupBase
{
    // ====== UI Binding Enums (CachObject 시스템 활용) ======

    /// <summary> 팝업 내 버튼 컴포넌트들을 식별하기 위한 Enum입니다. </summary>
    enum Button
    {
        OKButton, // 확인 및 닫기 버튼
    }

    /// <summary> 팝업 내 텍스트 컴포넌트들을 식별하기 위한 Enum입니다. </summary>
    enum Texts
    {
        MessageText // 메시지 내용이 표시될 텍스트
    }

    // ----------------------------------------------------------------------
    // ## Initialization (Overrides)
    // ----------------------------------------------------------------------

    /// <summary>
    /// 팝업에서 사용할 UI 컴포넌트들을 찾아 메모리에 바인딩(캐싱)합니다.
    /// </summary>
    protected override void BindInit()
    {
        // Enum에 정의된 이름을 기반으로 Button과 TextMeshProUGUI 컴포넌트 자동 연결
        Bind<UnityEngine.UI.Button>(typeof(Button));
        Bind<TMPro.TextMeshProUGUI>(typeof(Texts));
    }

    /// <summary>
    /// 버튼에 실제 동작(클릭 이벤트)을 연결합니다.
    /// </summary>
    protected override void AddBtnEvent()
    {
        // OKButton 클릭 시 팝업을 닫는 기능을 할당
        Get<UnityEngine.UI.Button>((int)Button.OKButton).onClick.AddListener(() =>
        {
            Close(); // PopupBase에 정의된 닫기 및 파괴 로직 실행
        });
    }

    // ----------------------------------------------------------------------
    // ## Data Management
    // ----------------------------------------------------------------------

    /// <summary>
    /// 팝업에 표시될 메시지 내용을 설정하는 프로퍼티입니다.
    /// 외부에서 "PopupMsg.Mssage = '내용'" 형태로 사용합니다.
    /// </summary>
    public string Mssage
    {
        set
        {
            // 바인딩된 첫 번째(0번) 텍스트 컴포넌트에 메시지 대입
            Get<TMPro.TextMeshProUGUI>((int)Texts.MessageText).text = value;
        }
    }
}