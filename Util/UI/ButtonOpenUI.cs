using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class ButtonOpenUI : UIBaseFormMaker
{
    private Button m_button;
    [SerializeField] private UIManager.UISequence OpenUI;
    [SerializeField] private UIBaseData.UIType uiType;

    protected override void Awake()
    {
        m_button = GetComponent<Button>();

        m_button.onClick.RemoveAllListeners();
        m_button.onClick.AddListener(OnClickButton);
    }

    private void OnClickButton()
    {
        if (OpenUI == UIManager.UISequence.None)
            return;

        UIManager.Instance.ShowUI(OpenUI, uiType).Forget();
    }
}
