using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoadManager : MonoBehaviour
{
    [Header("Loading UI")]
    [SerializeField] private GameObject layoutGroupObject;
    [SerializeField] private CanvasGroup layoutGroup;

    private float m_currentProgress = 0;
    private float m_fadeTime = 1;

    private SceneInfo.SceneType m_currentScene = SceneInfo.SceneType.Awake;
    public SceneInfo.SceneType CurrentScene => m_currentScene;

    public void Init()
    {
    }

    public async UniTask SceneLoad(SceneInfo.SceneType type, Action endSceneLoadAction = null)
    {
        m_currentProgress = 0;

        GameMaster.Instance.popupManager.ClosePopupAll();
        Time.timeScale = 1;

        string SceneName = SceneInfo.GetSceneName(type);
        if (string.IsNullOrEmpty(SceneName)) return;

        layoutGroupObject.SetActive(true);

        var doTask = SceneManager.LoadSceneAsync("Empty");

        await layoutGroup.DOFade(1, m_fadeTime).AsyncWaitForCompletion();
        await doTask;

        SceneChangeAction(type);

        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(SceneName);

        while (!asyncOperation.isDone)
        {
            m_currentProgress = asyncOperation.progress;
            await UniTask.WaitForFixedUpdate();
        }

        layoutGroup.DOFade(0, m_fadeTime).OnComplete(() => layoutGroupObject.SetActive(false));

        if (endSceneLoadAction != null)
            endSceneLoadAction.Invoke();

        m_currentScene = type;

        await UniTask.WaitForSeconds(m_fadeTime);
    }

    protected void SceneChangeAction(SceneInfo.SceneType type)
    {
        switch (type)
        {
            case SceneInfo.SceneType.HomeScene:
                GameMaster.Instance.uiManager.GetAutoUIManager().SetUIType(AutoUIManager.UIType.main);
                break;
            case SceneInfo.SceneType.GameScene:
                GameMaster.Instance.uiManager.GetAutoUIManager().SetUIType(AutoUIManager.UIType.inGame);
                break;
            default:
                return;
        }
    }
}

public class SceneInfo
{
    public enum SceneType
    {
        Awake,
        Intro,
        GameScene,
        HomeScene,
    }

    public static string GetSceneName(SceneType scene) => scene switch
    {
        SceneType.Intro => "Intro",
        SceneType.GameScene => "GameScene",
        SceneType.HomeScene => "HomeScene",
        _ => "",
    };
}