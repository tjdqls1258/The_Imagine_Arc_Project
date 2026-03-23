using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 버튼 클릭 시 지정된 타입의 팝업을 자동으로 열어주는 범용 UI 컴포넌트입니다.
/// 별도의 코딩 없이 인스펙터 설정만으로 팝업 호출 기능을 구현할 수 있습니다.
/// </summary>
public class ButtonOpenPopup : UIBaseFormMaker
{
    // ====== UI Components ======
    private Button m_button;

    // ====== Inspector Settings ======
    [Header("Popup Settings")]
    [Tooltip("클릭 시 표시할 팝업의 종류를 선택합니다.")]
    [SerializeField] private PopupManager.PopupType popupTarget;

    [Tooltip("팝업 초기화(Init) 시 전달할 파라미터 데이터들입니다.")]
    [SerializeField] private object[] popupData;

    // ----------------------------------------------------------------------
    // ## Initialization
    // ----------------------------------------------------------------------

    protected override void Awake()
    {
        // 1. 버튼 컴포넌트 참조
        m_button = GetComponent<Button>();

        // 2. 버튼 이벤트 초기화 및 연결
        // 중복 등록을 방지하기 위해 기존 리스너를 제거 후 새로 등록합니다.
        m_button.onClick.RemoveAllListeners();
        m_button.onClick.AddListener(OnClickButton);
    }

    // ----------------------------------------------------------------------
    // ## Interaction Logic
    // ----------------------------------------------------------------------

    /// <summary>
    /// 버튼이 클릭되었을 때 실행됩니다.
    /// PopupManager를 통해 비동기로 팝업을 생성하고 데이터를 전달합니다.
    /// </summary>
    private void OnClickButton()
    {
        // Forget()을 사용하여 팝업 생성 비동기 로직의 완료를 기다리지 않고 흐름을 이어갑니다.
        GameMaster.Instance.popupManager.ShowPopup(popupTarget, popupData).Forget();
    }
}