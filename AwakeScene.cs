using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

public class AwakeScene : MonoBehaviour
{
    [SerializeField] private LoadingPanel m_loadingPanel;
    [SerializeField] private DownloaderCheck m_downloader;
    [SerializeField] private Transform m_managerParnet;
    [Inject] private readonly GameBootStart m_bootStart;
    [Inject] private readonly SceneLoadManager sceneLoadManager;
    [Inject] private readonly SoundManager soundManager;
    [Inject] private readonly AddressableManager addressableManager;

    private void Start()
    {
        Application.targetFrameRate = 60;
        StartAppFlowAsync().Forget();
    }

    private async UniTask StartAppFlowAsync()
    {
        m_bootStart.InitBaseSystems(m_managerParnet);

        await m_bootStart.AddressableInitAsync();

        bool canProceed = await m_downloader.HandleAddressablesDownloadAsync(addressableManager);
        if (!canProceed)
        {
            return;
        }

        await m_bootStart.StartAsync(this.destroyCancellationToken);

        m_loadingPanel.ShowPanel(LoadingPanel.CanvasGroups.StartPanel);

        m_loadingPanel.AddOnClickAction(() =>
        {
            LoadNextSceneAsync().Forget();
        });
    }

    private async UniTask LoadNextSceneAsync()
    {
        await sceneLoadManager.SceneLoad(SceneInfo.SceneType.HomeScene);
        soundManager.Play(SoundPath.BGM_Title, SoundType.BGM).Forget();
    }
}