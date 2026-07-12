using System;
using UnityEngine;
using VContainer;

/// <summary>
/// 모든 UI 패널 및 요소의 근간이 되는 최상위 베이스 클래스입니다.
/// UI의 초기화, 애니메이션 재생, 열기/닫기 콜백 및 UIManager와의 통신을 관리합니다.
/// </summary>
public class UIBase : CacheObject
{
    [Inject] protected readonly UIManager uiManager;

    public Animation m_UIOpenAnim;
    public Action m_onShow;
    public Action m_onClose;
    protected UIManager.UISequence m_UISequence;

    public void SetSequence(UIManager.UISequence sequence)
        => m_UISequence = sequence;

    protected virtual void Awake()
    {
        Init();
    }

    public virtual void Init(Transform parent = null)
    {
        Logger.Log($"{GetType()}::Init");

        m_onShow = m_onClose = null;

        if (parent != null)
            transform.SetParent(parent);
    }

    public virtual void SetInfo(BaseUIData uiData)
    {
        Logger.Log($"{GetType()}::SetInfo");

        m_onShow = uiData.OnShow;
        m_onClose = uiData.OnClose;
    }

    public virtual void ShowUI()
    {
        if (m_UIOpenAnim)
            m_UIOpenAnim.Play();

        m_onShow?.Invoke();
        m_onShow = null;
    }

    public virtual void CloseUI(bool isClosetAll = false)
    {
        if (isClosetAll == false)
            m_onClose?.Invoke();

        m_onClose = null;

        uiManager.CloseUI(m_UISequence);
    }

    public virtual void OnClickClosetButton()
    {
        // 필요 시 사운드 재생 등을 추가할 수 있습니다.
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