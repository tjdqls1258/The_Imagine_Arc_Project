using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

public class DownloaderCheck : MonoBehaviour
{
    [SerializeField] private LoadingPanel m_loadingPanel;
    [Inject] private readonly PopupManager popupManager;

    public async UniTask<bool> HandleAddressablesDownloadAsync(AddressableManager addressable)
    {
        var addressManager = addressable;

        long checkDownlSize = await addressManager.DownloadChecdk(Util.ADDRESSABLE_LABEL);

        if (checkDownlSize > 0)
        {
            var popup = await popupManager.ShowPopup(PopupManager.PopupType.PopupQ) as PopupQ;
            if (popup != null)
            {
                popup.Mssage = $"{checkDownlSize} Byte의 추가 다운로드가 필요합니다. 다운로드 하시겠습니까?";

                // 비동기 흐름을 제어하기 위한 신호기
                var tcs = new UniTaskCompletionSource<bool>();

                popup.okAction += () =>
                {
                    StartDownloadProcess(addressManager, tcs).Forget();
                };

                popup.noAction += () =>
                {
                    popupManager.ExitGamePopup().Forget();
                    tcs.TrySetResult(false); // 취소 신호 전달
                };

                // 사용자의 선택과 다운로드가 완료될 때까지 여기서 대기
                return await tcs.Task;
            }
        }

        return true; // 다운로드할 용량이 없으면 바로 통과
    }

    private async UniTaskVoid StartDownloadProcess(AddressableManager addressManager, UniTaskCompletionSource<bool> tcs)
    {
        // 로딩 패널 띄우기
        m_loadingPanel.ShowPanel(LoadingPanel.CanvasGroups.DownloadPaenl);

        // 실제 다운로드 실행 (LoadingText 콜백 연결)
        await addressManager.DownloadAssetsAsync(
            onDownloading: m_loadingPanel.LoadingText,
            null
        );

        // 다운로드 무사 완료 신호 전달
        tcs.TrySetResult(true);
    }
}