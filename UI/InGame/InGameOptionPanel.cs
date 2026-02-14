using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 인게임 환경 설정 및 메뉴 팝업을 관리하는 클래스입니다.
/// 게임 일시정지(TimeScale 제어) 및 DOTween을 이용한 페이드 연출을 처리합니다.
/// </summary>
public class InGameOptionPanel : UIBaseFormMaker
{
    // ====== UI Binding Enums (CachObject 시스템 활용) ======
    enum Buttons
    {
        OptionButton, // 옵션 열기 버튼
    }

    enum CanvasGroups
    {
        OptionPanel,  // 페이드 연출을 위한 옵션 패널 본체
    }

    // ====== DOTween Sequences (최적화를 위해 미리 생성) ======
    private Sequence m_activeOptionSequence;   // 활성화(열기) 연출
    private Sequence m_deactiveOptionSequence; // 비활성화(닫기) 연출

    // ----------------------------------------------------------------------
    // ## Initialization
    // ----------------------------------------------------------------------

    protected override void Awake()
    {
        base.Awake();

        // 1. UI 컴포넌트 자동 바인딩
        Bind<CanvasGroup>(typeof(CanvasGroups));
        Bind<Button>(typeof(Buttons));

        // 2. 버튼 이벤트 연결 (인덱스 0: OptionButton)
        Get<Button>((int)Buttons.OptionButton).onClick.AddListener(OnClickOption);

        // 3. 트윈 시퀀스 초기화 (SetAutoKill(false)를 통해 재사용 가능하게 설정)
        // SetUpdate(true)를 설정하여 Time.timeScale이 0(일시정지)이어도 UI 연출은 작동하게 함
        m_activeOptionSequence = DOTween.Sequence()
            .Append(Get<CanvasGroup>((int)CanvasGroups.OptionPanel).DOFade(1, 0.3f))
            .SetAutoKill(false)
            .SetUpdate(true)
            .Pause();

        m_deactiveOptionSequence = DOTween.Sequence()
            .Append(Get<CanvasGroup>((int)CanvasGroups.OptionPanel).DOFade(0, 0.3f))
            .SetAutoKill(false)
            .OnComplete(CanvaseClose_TweenEnd) // 닫기 연출 완료 후 정리 로직 실행
            .SetUpdate(true)
            .Pause();

        // 내부 로컬 함수: 닫기 연출이 끝난 후 호출
        void CanvaseClose_TweenEnd()
        {
            Get<CanvasGroup>((int)CanvasGroups.OptionPanel).gameObject.SetActive(false);
            Time.timeScale = 1f; // 게임 일시정지 해제
        }
    }

    // ----------------------------------------------------------------------
    // ## Canvas State Control
    // ----------------------------------------------------------------------

    /// <summary>
    /// 옵션 패널의 활성화 상태를 제어하고 게임 일시정지 유무를 결정합니다.
    /// </summary>
    /// <param name="active">true: 열기, false: 닫기</param>
    private void CanvasAtive(bool active)
    {
        // 현재 알파값 초기화 (연출 시작 전 보정)
        Get<CanvasGroup>((int)CanvasGroups.OptionPanel).alpha = active ? 0f : 1f;

        if (active)
        {
            Time.timeScale = 0f; // 게임 로직 일시정지
            Get<CanvasGroup>((int)CanvasGroups.OptionPanel).gameObject.SetActive(active);
            m_activeOptionSequence.Restart(); // 열기 연출 실행
        }
        else
        {
            m_deactiveOptionSequence.Restart(); // 닫기 연출 실행 (OnComplete에서 일시정지 해제됨)
        }
    }

    private void OnDestroy()
    {
        // 메모리 누수 방지: 생성된 시퀀스 파괴
        m_activeOptionSequence.Kill();
        m_deactiveOptionSequence.Kill();
    }

    // ----------------------------------------------------------------------
    // ## UI Button Callbacks
    // ----------------------------------------------------------------------

    #region OnClick Func

    /// <summary> 돌아가기 버튼 클릭 시 패널 닫기 </summary>
    public void OnClickBack()
    {
        CanvasAtive(false);
    }

    /// <summary> 옵션 버튼 클릭 시 패널 열기 </summary>
    private void OnClickOption()
    {
        CanvasAtive(true);
    }

    /// <summary>
    /// 홈(로비) 화면으로 나가는 버튼 클릭 시 호출됩니다.
    /// 인게임 리소스를 정리하고 씬을 전환합니다.
    /// </summary>
    public void OnClickHome()
    {
        // 1. 인게임 매니저를 통해 게임 종료 로직 수행
        GameMaster.Instance.uiManager.AutoUIManager.GetCompoent<InGameUIManager>(UIBaseData.UIType.InGameUI).ExitGame();

        // 2. 홈 씬 로드 (비동기)
        SceneLoadManager.Instance.SceneLoad(SceneInfo.SceneType.HomeScene).Forget();

        // 3. 오브젝트 풀링 내의 유효하지 않은(Null) 객체 정리
        ObjectPoolManager.Instance.ClearNullPoolObject();

        // 4. 패널 닫기 (이때 Time.timeScale이 1로 복구됨)
        CanvasAtive(false);
    }

    #endregion
}