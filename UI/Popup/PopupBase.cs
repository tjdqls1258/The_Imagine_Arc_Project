using System;
using UnityEngine;

/// <summary>
/// 모든 팝업 UI의 최상위 베이스 클래스입니다.
/// UI의 초기화, 버튼 이벤트 등록, 열기/닫기 시의 콜백 처리를 담당합니다.
/// </summary>
public class PopupBase : UIBase
{
    protected PopupManager popupManager;
    public Action showAction;
    public Action closeAction;

    protected virtual void BindInit()
    {
    }

    protected virtual void AddBtnEvent()
    {

    }

    public virtual void Init(PopupManager popupManager, params object[] parm)
    {
        this.popupManager = popupManager;
        if (showAction != null)
            showAction.Invoke();

        BindInit();
        AddBtnEvent();
    }

    public virtual void Close()
    {
        if (closeAction != null)
            closeAction.Invoke();

        popupManager.CloseCurrentPopup();

        Destroy(MyObj);
    }
}