using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 게임 마스터 (GameMaster)
/// 게임의 최상위 컨트롤러로서 초기화 파이프라인, 어드레서블 다운로드 관리,
/// 핵심 리소스 및 유저 데이터 로드를 총괄하는 MonoSingleton 클래스입니다.
/// </summary>
public class GameMaster : MonoSingleton<GameMaster>
{
    // ====== Constants & Labels ======
    [Header("Addressable Settings")]
    [Tooltip("다운로드 체크가 필요한 어드레서블 그룹 레이블 목록입니다.")]
    private readonly string[] ADDRESSABLE_LABEL = { "InGameData", "SpriteAltas", "JsonData", "UI", "CharacterSprite" };

    // ====== Members ======
    private CSVHelper m_csvHelper = new();

    // ====== Managers Access (Read-Only Properties) ======
    // 싱글톤 관리자들에 대한 단축 접근 프로퍼티
    public SoundManager soundManager => SoundManager.Instance;
    public AddressableManager addressableManager => AddressableManager.Instance;
    public SceneLoadManager sceneLoadManager => SceneLoadManager.Instance;
    public UIManager uiManager => UIManager.Instance;
    public PopupManager popupManager => PopupManager.Instance;
    public CSVHelper csvHelper => m_csvHelper;

    // ----------------------------------------------------------------------
    // ## Initialization Phase 1: Basic Boot
    // ----------------------------------------------------------------------

    /// <summary>
    /// 게임 시작 시 가장 먼저 호출되는 동기 초기화 메서드입니다.
    /// </summary>
    public override void Init()
    {
        base.Init();

        // 시스템 필수 팝업 매니저 초기 로드
        popupManager.Init();
        popupManager.SettingPopupData();
    }

    // ----------------------------------------------------------------------
    // ## Initialization Phase 2: Content Download
    // ----------------------------------------------------------------------

    /// <summary>
    /// 어드레서블 에셋의 업데이트 상태를 확인하고 필요 시 다운로드 팝업을 출력합니다.
    /// </summary>
    /// <param name="showDownloadPanel">다운로드 UI를 표시할 콜백</param>
    /// <param name="downloadAction">다운로드 진행률 업데이트 콜백 (Label, current, total)</param>
    /// <param name="downloadDoneAction">다운로드 완료 시 실행될 콜백</param>
    public async UniTask InitAddress(Action showDownloadPanel, Action<string, long, long> downloadAction, Action downloadDoneAction)
    {
        // 1. 어드레서블 시스템 초기화
        await addressableManager.InitAsync();

        // 2. 패치 크기 체크
        long checkDownlSize = await addressableManager.DownloadChecdk(ADDRESSABLE_LABEL);

        if (checkDownlSize > 0)
        {
            // 3. 다운로드 확인 팝업 출력
            var popup = await popupManager.ShowPopup(PopupManager.PopupType.PopupQ) as PopupQ;
            if (popup != null)
            {
                popup.Mssage = $"{checkDownlSize} Byte의 추가 데이터가 필요합니다.\n다운로드하시겠습니까?";

                // 확인 클릭 시 다운로드 시작
                popup.okAction += () =>
                {
                    showDownloadPanel?.Invoke();
                    addressableManager.DownloadAssetsAsync(onDownloading: downloadAction, downloadDoneAction).Forget();
                };

                // 취소 클릭 시 게임 종료
                popup.noAction += () => ExitGame();
            }
        }
        else
        {
            // 다운로드할 내용이 없으면 즉시 완료 콜백 호출
            downloadDoneAction?.Invoke();
        }
    }

    // ----------------------------------------------------------------------
    // ## Initialization Phase 3: System & Resource Loading
    // ----------------------------------------------------------------------

    /// <summary>
    /// 리소스 다운로드 완료 후, 게임 플레이에 필요한 모든 시스템을 비동기로 초기화합니다.
    /// </summary>
    public async UniTask InitAscy()
    {
        Logger.Log("Starting GameMaster System Initialization...");

        // 1. 하위 매니저 동기 초기화
        soundManager.Init();
        sceneLoadManager.Init();
        uiManager.Init();

        // 2. 데이터 테이블 및 팝업 에셋 로드
        await popupManager.SettingPopupDataAsync();
        await csvHelper.InitCSVDataAsync();

        // 3. UI 최상위 캔버스(Master Canvas) 로드
        await LoadBaseResource();

        // 4. 유저 데이터(세이브 파일) 로드
        await AsyncLoadUserData();

        // 5. 초기 UI 레이아웃 배치 (JSON 기반 자동 생성)
        await uiManager.AutoUIManager.LoadJsonAsync();

        Logger.Log("GameMaster Initialization Complete.");
    }

    /// <summary>
    /// 게임의 기본 뼈대가 되는 MasterCanvas를 로드합니다.
    /// </summary>
    private async UniTask LoadBaseResource()
    {
        List<UniTask> loadingTasks = new();

        // UI 시스템의 부모가 될 MasterCanvas를 비동기로 로드
        var masterCanvasLoader = uiManager.LoadMasterCanvasAsync(transform);
        loadingTasks.Add(masterCanvasLoader);

        await UniTask.WhenAll(loadingTasks);
    }

    // ----------------------------------------------------------------------
    // ## Data & Exit Logic
    // ----------------------------------------------------------------------

    /// <summary>
    /// 세이브 데이터를 로드하거나 신규 유저용 데이터를 생성합니다.
    /// </summary>
    private async UniTask AsyncLoadUserData()
    {
        UserDataManager.Instance.Init();

        if (UserDataManager.Instance.hasSaveData)
            UserDataManager.Instance.LoadUserData();
        else
            UserDataManager.Instance.InitDefaultData();

        await UserDataManager.Instance.AsyncLoadUserData();
    }

    /// <summary>
    /// 게임 종료 확인 팝업을 출력합니다.
    /// </summary>
    public async UniTask ExitGamePopup()
    {
        var popup = await PopupManager.Instance.ShowPopup(PopupManager.PopupType.PopupMsg);
        if (popup != null)
        {
            popup.closeAction += ExitGame;
        }
    }

    /// <summary>
    /// 환경에 따라 플랫폼 종료 또는 에디터 플레이 모드 종료를 수행합니다.
    /// </summary>
    private void ExitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}