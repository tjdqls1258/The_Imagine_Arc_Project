using System;
using UnityEngine;

public class UIBase : CachObject
{
    public Animation m_UIOpenAnim;

    public Action m_onShow;
    public Action m_onClose;

    protected UIManager.UISequence m_UISequence;

    protected virtual void Awake()
    {

    }

    public virtual void Init(Transform parent = null)
    {
        Logger.Log($"{GetType()}::Init");

        m_onShow = m_onClose = null;

        if(parent  != null)
            transform.SetParent( parent );  
    }

    public virtual void SetInfo(BaseUIData uiData)
    {
        Logger.Log($"{GetType()}::SetInfo");

        m_onShow = uiData.OnShow;
        m_onClose = uiData.OnClose;
    }

    public virtual void ShowUI()
    {
        if(m_UIOpenAnim)
            m_UIOpenAnim.Play();

        m_onShow?.Invoke();
        m_onShow = null;
    }

    public virtual void CloseUI(bool isClosetAll = false)
    {
        if(isClosetAll == false)
            m_onClose?.Invoke();

        m_onClose = null;
        UIManager.Instance.CloseUI(m_UISequence);
    }

    public virtual void OnClickClosetButton()
    {
        //SoundManager.Instance.Play("", SoundType.EFFECT).Forget();
        CloseUI(false);
    }
}

public class UIBaseFormMaker : UIBase
{
}

public class UILobbyUpdate : UIBaseFormMaker
{
    public virtual void UpdateFormLobby() { }
    public virtual void CloseFormLobby() { }
}

public class BaseUIData
{
    public Action OnShow;
    public Action OnClose;
}