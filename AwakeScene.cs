using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using UnityEngine;

public class AwakeScene : MonoBehaviour
{
    [SerializeField] private LoadingPanel m_loadingPanel;

    private void Start()
    {
        StartAscy().Forget();
    }

    async UniTask StartAscy()
    {
        GameMaster.Instance.Init();

        await GameMaster.Instance.InitAddress(
            ()=>m_loadingPanel.ShowPanel(LoadingPanel.CanvasGroups.DownloadPaenl),
            m_loadingPanel.LoadingText, 
            () => InitStart().Forget());
    }

    async UniTask InitStart()
    {
        await GameMaster.Instance.InitAscy();
        m_loadingPanel.ShowPanel(LoadingPanel.CanvasGroups.StartPaenl);

        m_loadingPanel.AddOnClickAction(() =>
        {
            TaskHelp().Forget();

            async UniTask TaskHelp()
            {
                await GameMaster.Instance.sceneLoadManager.SceneLoad(SceneInfo.SceneType.HomeScene);
                GameMaster.Instance.uiManager.AutoUIManager.SetUIType(AutoUIManager.UIType.main, true);
                SoundManager.Instance.Play(SoundPath.BGM_Title, SoundType.BGM).Forget();
            }
        });
    }
}
