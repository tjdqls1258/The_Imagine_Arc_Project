using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class InGameOptionPanel : UIBaseFormMaker
{
    [Inject] private readonly UserDataManager userDataManager;
    enum Buttons
    {
        OptionButton,
        SpeedButton
    }

    enum TextMeshProUGUIs
    {
        SpeedText,
    }

    enum CanvasGroups
    {
        OptionPanel,
    }

    private UserGameSettingData gameOption => userDataManager.GetUserData<UserGameSettingData>() as UserGameSettingData;

    private Sequence m_activeOptionSequence;
    private Sequence m_deactiveOptionSequence;

    protected override void Awake()
    {
        base.Awake();

        Bind<CanvasGroup>(typeof(CanvasGroups));
        Bind<Button>(typeof(Buttons));
        Bind<TextMeshProUGUI>(typeof(TextMeshProUGUIs));

        Get<Button>((int)Buttons.OptionButton).OnClickAsObservable().Subscribe(_ => 
        { OnClickOption(); 
        }).AddTo(this);

        Get<Button>((int)Buttons.SpeedButton).OnClickAsObservable().Subscribe(_ =>
        {
            OnClickGameSpeed();
        }).AddTo(this);

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

        Get<TextMeshProUGUI>((int)TextMeshProUGUIs.SpeedText).text = $"x{GameUtil.GetGameTimeScale(gameOption.userGameSettingOption.GameSpeedIndex):F1}";

        void CanvasClose_TweenEnd()
        {
            Get<CanvasGroup>((int)CanvasGroups.OptionPanel).gameObject.SetActive(false);
            GameUtil.SetTimeScale(gameOption.userGameSettingOption.GameSpeedIndex);
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

    public void OnClickGameSpeed()
    {
        gameOption.userGameSettingOption.GameSpeedIndex++;
        if (GameUtil.SetTimeScale(gameOption.userGameSettingOption.GameSpeedIndex) == false)
            gameOption.userGameSettingOption.GameSpeedIndex = 0;

        gameOption.SaveData();
        Get<TextMeshProUGUI>((int)TextMeshProUGUIs.SpeedText).text = $"x{Time.timeScale:F1}";
    }

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
        uiManager.GetAutoUIManager().GetCompoent<InGameUIManager>(UIBaseData.UIType.InGameUI).ExitGame();

        CanvasAtive(false);
    }

    public void ShowSettingPanel()
    {
        uiManager.ShowUI(UIManager.UISequence.SettingPanel, UIBaseData.UIType.Command).Forget();
    }

    #endregion
}