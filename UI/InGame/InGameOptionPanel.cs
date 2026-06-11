using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class InGameOptionPanel : UIBaseFormMaker
{
    enum Buttons
    {
        OptionButton,
    }

    enum CanvasGroups
    {
        OptionPanel,
    }

    private Sequence m_activeOptionSequence;
    private Sequence m_deactiveOptionSequence;

    protected override void Awake()
    {
        base.Awake();

        Bind<CanvasGroup>(typeof(CanvasGroups));
        Bind<Button>(typeof(Buttons));

        Get<Button>((int)Buttons.OptionButton).onClick.AddListener(OnClickOption);

        m_activeOptionSequence = DOTween.Sequence()
            .Append(Get<CanvasGroup>((int)CanvasGroups.OptionPanel).DOFade(1, 0.3f))
            .SetAutoKill(false)
            .SetUpdate(true)
            .Pause();

        m_deactiveOptionSequence = DOTween.Sequence()
            .Append(Get<CanvasGroup>((int)CanvasGroups.OptionPanel).DOFade(0, 0.3f))
            .SetAutoKill(false)
            .OnComplete(CanvasClose_TweenEnd)
            .SetUpdate(true)
            .Pause();

        void CanvasClose_TweenEnd()
        {
            Get<CanvasGroup>((int)CanvasGroups.OptionPanel).gameObject.SetActive(false);
            Time.timeScale = 1f;
        }
    }

    private void CanvasAtive(bool active)
    {
        Get<CanvasGroup>((int)CanvasGroups.OptionPanel).alpha = active ? 0f : 1f;

        if (active)
        {
            Time.timeScale = 0f;
            Get<CanvasGroup>((int)CanvasGroups.OptionPanel).gameObject.SetActive(active);
            m_activeOptionSequence.Restart();
        }
        else
        {
            m_deactiveOptionSequence.Restart();
        }
    }

    private void OnDestroy()
    {
        m_activeOptionSequence.Kill();
        m_deactiveOptionSequence.Kill();
    }

    #region OnClick Func

    public void OnClickBack()
    {
        CanvasAtive(false);
    }

    private void OnClickOption()
    {
        CanvasAtive(true);
    }

    public void OnClickHome()
    {
        GameMaster.Instance.uiManager.GetAutoUIManager().GetCompoent<InGameUIManager>(UIBaseData.UIType.InGameUI).ExitGame();

        CanvasAtive(false);
    }

    public void ShowSettingPanel()
    {
        GameMaster.Instance.uiManager.ShowUI(UIManager.UISequence.SettingPanel, UIBaseData.UIType.Command).Forget();
    }

    #endregion
}