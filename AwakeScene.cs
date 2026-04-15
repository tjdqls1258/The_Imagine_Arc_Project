using Cysharp.Threading.Tasks;
using UnityEngine;

public class AwakeScene : MonoBehaviour
{
    [SerializeField] private LoadingPanel m_loadingPanel;
    [SerializeField] private DownloaderCheck m_downloader;

    private void Start()
    {
        StartAppFlowAsync().Forget();
    }

    private async UniTask StartAppFlowAsync()
    {
        // 1. [어드레서블 이전] 뼈대 및 로컬 데이터 세팅
        GameMaster.Instance.InitBaseSystems();

        // 2. [어드레서블 단독] 매니저 생성 및 초기화
        await GameMaster.Instance.InitAddressableSystemAsync();

        // 3. [다운로드 체크] 팝업 발생 및 대기
        bool canProceed = await m_downloader.HandleAddressablesDownloadAsync();
        if (!canProceed)
        {
            return; // 사용자가 다운로드를 취소했으므로 이후 진행 중단
        }

        // 4. [어드레서블 이후] 패치된 최신 데이터(CSV, Prefab 등)를 기반으로 매니저 초기화
        await GameMaster.Instance.InitAssetDependentSystemsAsync();

        // 5. 완료 후 터치 대기 UI 노출
        m_loadingPanel.ShowPanel(LoadingPanel.CanvasGroups.StartPaenl);

        m_loadingPanel.AddOnClickAction(() =>
        {
            LoadNextSceneAsync().Forget();
        });
    }

    private async UniTask LoadNextSceneAsync()
    {
        await GameMaster.Instance.sceneLoadManager.SceneLoad(SceneInfo.SceneType.HomeScene);
        GameMaster.Instance.soundManager.Play(SoundPath.BGM_Title, SoundType.BGM).Forget();
    }
}