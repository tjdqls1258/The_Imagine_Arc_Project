using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class Profile : UILobbyUpdate
{
    enum TextMeshProGUIs
    {
        GoodsText,
        GoodsText_1,
        NameText
    }

    enum Buttons
    {
        Setting
    }

    private string m_userName;
    public string UserName
    {
        private set
        {
            m_userName = value;
            Get<TextMeshProUGUI>((int)TextMeshProGUIs.NameText).text = m_userName;
        }
        get => m_userName;
    }

    private long m_goods;
    public long GoodsValue
    {
        private set
        {
            m_goods = value;
            Get<TextMeshProUGUI>((int)TextMeshProGUIs.GoodsText).text = m_goods.ToString("#,0");
        }
        get => m_goods;
    }

    private long m_goods1;
    public long GoodsValue_1
    {
        private set
        {
            m_goods1 = value;
            Get<TextMeshProUGUI>((int)TextMeshProGUIs.GoodsText_1).text = m_goods1.ToString("#,0");
        }
        get => m_goods1;
    }

    protected override void Awake()
    {
        base.Awake();
        Bind<TextMeshProUGUI>(typeof(TextMeshProGUIs));
        Bind<Button>(typeof(Buttons));

        Get<Button>((int)Buttons.Setting).onClick.RemoveListener(OpenSettingPanel);
        Get<Button>((int)Buttons.Setting).onClick.AddListener(OpenSettingPanel);
    }

    public override void UpdateFormLobby()
    {
        base.UpdateFormLobby();
        if (string.IsNullOrEmpty(UserName))
            UserName = "TestUser";

        GoodsValue = 100000;
        GoodsValue_1 = 10;
        //여기부터 웹 통신 필요

    }

    private void OpenSettingPanel()
    {
        uiManager.ShowUI(UIManager.UISequence.SettingPanel, UIBaseData.UIType.Command).Forget();
    }
}
