using System;
using UnityEngine;

public class PopupQ : PopupMsg
{
    private enum Buttons
    {
        OKButton,
        CloseButton
    }

    enum Texts
    {
        MessageText
    }

    public Action okAction;
    public Action noAction;

    protected override void BindInit()
    {
        Bind<UnityEngine.UI.Button>(typeof(Buttons));
        Bind<TMPro.TextMeshProUGUI>(typeof(Texts));
    }

    protected override void AddBtnEvent()
    {
        Get<UnityEngine.UI.Button>((int)Buttons.OKButton).onClick.AddListener(() =>
        {
            okAction?.Invoke();
            this.Close();
        });

        Get<UnityEngine.UI.Button>((int)Buttons.CloseButton).onClick.AddListener(() =>
        {
            noAction?.Invoke();
            this.Close();
        });
    }
}