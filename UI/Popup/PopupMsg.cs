using UnityEngine;

public class PopupMsg : PopupBase
{
    enum Button
    {
        OKButton, 
    }

    enum Texts
    {
        MessageText 
    }

    protected override void BindInit()
    {
        Bind<UnityEngine.UI.Button>(typeof(Button));
        Bind<TMPro.TextMeshProUGUI>(typeof(Texts));
    }

    protected override void AddBtnEvent()
    {
        Get<UnityEngine.UI.Button>((int)Button.OKButton).onClick.AddListener(() =>
        {
            Close(); 
        });
    }

    public string Mssage
    {
        set
        {
            Get<TMPro.TextMeshProUGUI>((int)Texts.MessageText).text = value;
        }
    }
}