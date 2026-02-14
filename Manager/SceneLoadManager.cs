using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 게임 내 씬 전환을 관리하는 매니저 클래스입니다.
/// 페이드 연출, 비동기 로딩, 씬 전환 시 UI 상태 변경 등을 담당합니다.
/// </summary>
public class SceneLoadManager : MonoSingleton<SceneLoadManager>
{
    // ====== Inspector References ======
    [Header("Loading UI")]
    [SerializeField] private GameObject layoutGroupObject; // 로딩 화면 UI 부모 객체
    [SerializeField] private CanvasGroup layoutGroup;       // 페이드 효과를 위한 CanvasGroup

    // ====== Runtime Variables ======
    private float m_currentProgress = 0; // 현재 씬 로딩 진행률 (0~1)
    private float m_fadeTime = 1;        // 페이드 연출 시간 (초)

    /// <summary> 현재 활성화된 씬의 타입 </summary>
    private SceneInfo.SceneType m_currentScene = SceneInfo.SceneType.Awake;
    public SceneInfo.SceneType CurrentScene => m_currentScene;

    // ----------------------------------------------------------------------
    // ## Initialization
    // ----------------------------------------------------------------------

    public override void Init()
    {
        base.Init();
    }

    // ----------------------------------------------------------------------
    // ## Core Logic: Scene Loading
    // ----------------------------------------------------------------------

    /// <summary>
    /// [비동기] 지정된 씬으로 전환을 시작합니다.
    /// </summary>
    /// <param name="type">목표 씬 타입</param>
    /// <param name="endSceneLoadAction">씬 로드 완료 후 실행할 콜백</param>
    public async UniTask SceneLoad(SceneInfo.SceneType type, Action endSceneLoadAction = null)
    {
        m_currentProgress = 0;

        // 1. 씬 전환 전 모든 팝업 닫기 및 타임스케일 초기화
        PopupManager.Instance.ClosePopupAll();
        Time.timeScale = 1;

        // 2. 목표 씬 이름 확인
        string SceneName = SceneInfo.GetSceneName(type);
        if (string.IsNullOrEmpty(SceneName)) return;

        // 3. 로딩 화면 표시 및 암전(Fade In) 시작
        layoutGroupObject.SetActive(true);

        // 중간 정화 단계: Empty 씬을 비동기로 로드하여 이전 씬 리소스를 해제 유도
        var doTask = SceneManager.LoadSceneAsync("Empty");

        // 페이드 연출이 완료될 때까지 대기
        await layoutGroup.DOFade(1, m_fadeTime).AsyncWaitForCompletion();

        // Empty 씬 로드 완료 대기
        await doTask;

        // 4. 씬 타입에 따른 UI 매니저 설정 변경
        SceneChangeAction(type);

        // 5. 목표 씬 로드 시작 (AsyncOperation 활용)
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(SceneName);

        // 씬 로드가 완료될 때까지 프로그래스를 갱신하며 대기
        while (!asyncOperation.isDone)
        {
            m_currentProgress = asyncOperation.progress;
            // fixedUpdate 타이밍마다 체크하여 부하 분산
            await UniTask.WaitForFixedUpdate();
        }

        // 6. 로드 완료 후 화면 밝아짐(Fade Out) 연출
        layoutGroup.DOFade(0, m_fadeTime).OnComplete(() => layoutGroupObject.SetActive(false));

        // 7. 완료 콜백 실행
        if (endSceneLoadAction != null)
            endSceneLoadAction.Invoke();

        m_currentScene = type;

        // 페이드 시간이 완전히 끝날 때까지 대기하여 안전한 조작 보장
        await UniTask.WaitForSeconds(m_fadeTime);
    }

    /// <summary>
    /// 씬 전환 시 씬의 성격(홈/게임)에 따라 UI 매니저의 상태를 자동으로 설정합니다.
    /// </summary>
    protected void SceneChangeAction(SceneInfo.SceneType type)
    {
        switch (type)
        {
            case SceneInfo.SceneType.HomeScene:
                // 홈 씬으로 전환 시 메인 UI 레이아웃 설정
                GameMaster.Instance.uiManager.AutoUIManager.SetUIType(AutoUIManager.UIType.main);
                break;
            case SceneInfo.SceneType.GameScene:
                // 게임 씬으로 전환 시 인게임 UI 레이아웃 설정
                GameMaster.Instance.uiManager.AutoUIManager.SetUIType(AutoUIManager.UIType.inGame);
                break;
            default:
                return;
        }
    }
}

// ----------------------------------------------------------------------
// ## Scene Information Data
// ----------------------------------------------------------------------

/// <summary>
/// 프로젝트의 씬 타입 정의 및 씬 이름 매핑을 담당하는 데이터 클래스입니다.
/// </summary>
public class SceneInfo
{
    public enum SceneType
    {
        Awake,      // 최초 진입
        Intro,      // 로고/인트로
        GameScene,  // 인게임 플레이
        HomeScene,  // 로비/메인 홈
    }

    /// <summary>
    /// Enum 타입을 실제 유니티 빌드 세팅에 등록된 씬 이름으로 변환합니다.
    /// </summary>
    public static string GetSceneName(SceneType scene) => scene switch
    {
        SceneType.Intro => "Intro",
        SceneType.GameScene => "GameScene",
        SceneType.HomeScene => "HomeScene",
        _ => "",
    };
}