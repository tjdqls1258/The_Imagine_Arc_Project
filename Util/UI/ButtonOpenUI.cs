using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class ButtonOpenUI : UIBaseFormMaker
{
    private Button m_button;

    [Header("UI Navigation Settings")]
    [Tooltip("클릭 시 열고자 하는 UI 패널의 시퀀스(ID)를 선택합니다.")]
    [SerializeField] private UIManager.UISequence OpenUI;

    [Tooltip("해당 UI가 속한 타입(Main, InGame 등)을 지정합니다.")]
    [SerializeField] private UIBaseData.UIType uiType;

    protected override void Awake()
    {
        base.Awake();

        m_button = GetComponent<Button>();

        m_button.onClick.RemoveAllListeners();
        m_button.onClick.AddListener(OnClickButton);
    }

    private void OnClickButton()
    {
        if (OpenUI == UIManager.UISequence.None)
            return;

        uiManager.ShowUI(OpenUI, uiType).Forget();
    }
}