using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class ButtonOpenPopup : UIBaseFormMaker
{
    [Inject] private readonly PopupManager popupManager;
    private Button m_button;

    [Header("Popup Settings")]
    [Tooltip("클릭 시 표시할 팝업의 종류를 선택합니다.")]
    [SerializeField] private PopupManager.PopupType popupTarget;

    [Tooltip("팝업 초기화(Init) 시 전달할 파라미터 데이터들입니다.")]
    [SerializeField] private object[] popupData;

    protected override void Awake()
    {
        m_button = GetComponent<Button>();

        m_button.onClick.RemoveAllListeners();
        m_button.onClick.AddListener(OnClickButton);
    }

    private void OnClickButton()
    {
        popupManager.ShowPopup(popupTarget, popupData).Forget();
    }
}