using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 버튼 클릭 시 지정된 특정 UI 패널(화면)을 열어주는 범용 컴포넌트입니다.
/// UIManager의 UISequence를 제어하여 화면 전환을 수행합니다.
/// </summary>
public class ButtonOpenUI : UIBaseFormMaker
{
    // ====== UI Components ======
    private Button m_button;

    // ====== Inspector Settings ======
    [Header("UI Navigation Settings")]
    [Tooltip("클릭 시 열고자 하는 UI 패널의 시퀀스(ID)를 선택합니다.")]
    [SerializeField] private UIManager.UISequence OpenUI;

    [Tooltip("해당 UI가 속한 타입(Main, InGame 등)을 지정합니다.")]
    [SerializeField] private UIBaseData.UIType uiType;

    // ----------------------------------------------------------------------
    // ## Initialization
    // ----------------------------------------------------------------------

    protected override void Awake()
    {
        // 부모 클래스(UIBaseFormMaker)의 초기화 로직이 있다면 호출
        base.Awake();

        // 1. 버튼 컴포넌트 참조
        m_button = GetComponent<Button>();

        // 2. 버튼 이벤트 초기화 및 연결
        m_button.onClick.RemoveAllListeners();
        m_button.onClick.AddListener(OnClickButton);
    }

    // ----------------------------------------------------------------------
    // ## Interaction Logic
    // ----------------------------------------------------------------------

    /// <summary>
    /// 버튼이 클릭되었을 때 실행됩니다.
    /// UIManager를 통해 해당 UI 패널을 비동기로 출력합니다.
    /// </summary>
    private void OnClickButton()
    {
        // 예외 처리: 설정된 UI 시퀀스가 없다면 중단
        if (OpenUI == UIManager.UISequence.None)
            return;

        // UIManager에 UI 출력을 요청 (비동기 호출)
        // Forget()을 사용하여 화면 전환 작업 중 메인 흐름이 멈추지 않도록 처리
        GameMaster.Instance.uiManager.ShowUI(OpenUI, uiType).Forget();
    }
}