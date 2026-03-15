using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

/// <summary>
/// 게임 내 모든 적 유닛의 공통 기능을 정의하는 인터페이스입니다.
/// </summary>
public interface IGamePlayCharacter
{
    public void DieAction();
}

/// <summary>
/// 적 캐릭터의 이동, 피격, 사망 로직을 제어하는 컨트롤러 클래스입니다.
/// </summary>
public class EnemyController : MonoBehaviour, IGamePlayCharacter
{
    // ====== Inspector Settings & References ======
    [Header("Enemy Settings")]
    [SerializeField] protected EnemyData m_enemyData;     // 적의 능력치 및 기본 정보 데이터
    [SerializeField] protected float stopDistance = 0.01f; // 경로 지점에 도착했다고 판단할 거리 임계값

    [Header("Component References")]
    [SerializeField] protected HPController m_hpController; // 체력 UI 및 수치 제어
    [SerializeField] protected CharacterAnimationController m_characterAnimationController; // 애니메이션 제어

    // ====== Runtime State ======
    public bool isDie = false; // 현재 사망 상태 여부
    protected List<Vector2> m_movePathList = new(); // 이동해야 할 월드 좌표 리스트
    protected int m_currentPathIndex = 0;           // 현재 목표로 하는 경로의 인덱스
    protected float m_moveSpeed = 2f;               // 이동 속도

    // ====== Async & Memory Management ======
    private CancellationTokenSource m_cancellation = new(); // 비동기 작업(이동) 취소를 위한 토큰
    protected Action<int, EnemyController> m_disableAction; // 비활성화 시 오브젝트 풀에 반환하기 위한 콜백
    protected Action m_dieAction;
    protected Action m_arriveAction;

    // ----------------------------------------------------------------------
    // ## Initialization
    // ----------------------------------------------------------------------

    private void Awake()
    {
        // 애니메이션 컨트롤러 자동 캐싱
        if (m_characterAnimationController == null)
            m_characterAnimationController = GetComponent<CharacterAnimationController>();
    }

    /// <summary>
    /// 적 유닛을 초기화하고 게임에 등장시킵니다. (오브젝트 풀링 대응)
    /// </summary>
    /// <param name="enemyData">적 기본 데이터</param>
    /// <param name="movePathList">이동할 경로 리스트</param>
    /// <param name="disableAction">비활성화 시 호출될 콜백 함수</param>
    public virtual void InitEnemyData(EnemyData enemyData, List<Vector2Int> movePathList, Action<int, EnemyController> disableAction, Action dieAction, Action arriveAction)
    {
        m_enemyData = enemyData;
        this.m_disableAction = disableAction;
        m_dieAction = dieAction;
        m_arriveAction = arriveAction;

        // 1. 상태 및 위치 초기화
        isDie = false;
        m_currentPathIndex = 0;
        m_movePathList.Clear();
        transform.position = new Vector3(movePathList[0].x, movePathList[0].y, 0);

        // 2. 비동기 토큰 및 오브젝트 활성화
        m_cancellation = new CancellationTokenSource();
        gameObject.SetActive(true);

        // 3. 경로 설정 및 비동기 이동 시작
        SetPath(movePathList);

        // 4. 컴포넌트 초기화 (체력 및 애니메이션)
        m_hpController.InitController(m_enemyData.characterState, DieAction_Enemy);
        m_characterAnimationController.PlayAnimation_Bool(CharacterAnimationController.AnimationBool.DIE, false);
    }

    /// <summary>
    /// 전달받은 경로 데이터를 내부 리스트에 저장하고 이동 루틴을 실행합니다.
    /// </summary>
    protected virtual void SetPath(List<Vector2Int> pathData)
    {
        foreach (var vec in pathData)
            m_movePathList.Add(vec);

        // 비동기 업데이트 루프 시작 (Forget: 결과를 기다리지 않고 실행)
        LateUpdateAsync().Forget();
    }

    // ----------------------------------------------------------------------
    // ## Movement Logic
    // ----------------------------------------------------------------------

    /// <summary>
    /// 매 프레임 실행되는 비동기 이동 루프입니다. FixedUpdate 타이밍에 맞춰 동작합니다.
    /// </summary>
    protected virtual async UniTask LateUpdateAsync()
    {
        while (!m_cancellation.IsCancellationRequested)
        {
            // 물리 업데이트 타이밍 대기
            await UniTask.WaitForFixedUpdate();

            // 대기 후 토큰 상태 다시 확인 (취소되지 않았다면 이동 실행)
            if (!m_cancellation.IsCancellationRequested)
                EnemyMove();
        }
    }

    /// <summary>
    /// 현재 목표 지점을 향해 이동하며, 도착 시 다음 지점으로 갱신합니다.
    /// </summary>
    protected virtual void EnemyMove()
    {
        if (m_movePathList.Count == 0 || isDie) return;

        // 현재 목표 지점에 도달했는지 확인
        if (CheckArraivePath(m_movePathList[m_currentPathIndex]))
        {
            // 다음 인덱스로 이동 (최대치 제한)
            m_currentPathIndex = System.Math.Min(m_movePathList.Count, m_currentPathIndex + 1);

            // 모든 경로를 다 통과했다면 도착 처리
            if (m_movePathList.Count == m_currentPathIndex)
                ArraiveEndPosition();
        }
        else
        {
            // 목표 지점을 향해 등속 이동
            transform.position = Vector3.MoveTowards(transform.position, m_movePathList[m_currentPathIndex], m_moveSpeed * Time.deltaTime);
        }
    }

    /// <summary>
    /// 특정 좌표와 현재 위치의 거리를 계산하여 도착 여부를 판단합니다.
    /// </summary>
    protected bool CheckArraivePath(Vector2 path)
    {
        if (transform == null) return false;
        return Vector2.Distance(path, transform.position) < stopDistance;
    }

    // ----------------------------------------------------------------------
    // ## Interaction & State Change
    // ----------------------------------------------------------------------

    /// <summary>
    /// 외부(공격 유닛 등)에서 호출하는 피격 메서드입니다.
    /// </summary>
    /// <param name="atk">공격력</param>
    public virtual void Hit(float atk)
    {
        if (isDie) return;

        m_hpController.UpdateHp(-atk);
        m_characterAnimationController.PlayAnimation_Trigger(CharacterAnimationController.AnimationTrigger.HIT);
    }

    /// <summary>
    /// 경로의 마지막 지점에 도착했을 때 호출됩니다. (플레이어 라이프 차감 등의 로직 연결부)
    /// </summary>
    public virtual void ArraiveEndPosition()
    {
        m_cancellation.Cancel();
        gameObject.SetActive(false);
        m_disableAction?.Invoke(m_enemyData.id, this); // 풀에 반환
        m_arriveAction?.Invoke(); //플레이어 라이프 차감 등의 액션
    }

    /// <summary>
    /// 체력이 0이 되어 사망할 때 호출됩니다.
    /// </summary>
    public virtual void DieAction_Enemy()
    {
        isDie = true;
        m_cancellation.Cancel(); // 이동 정지
        m_disableAction?.Invoke(m_enemyData.id, this); // 풀에 상태 알림
        m_characterAnimationController.PlayAnimation_Bool(CharacterAnimationController.AnimationBool.DIE, true);

        m_dieAction?.Invoke();
    }

    /// <summary>
    /// IGamePlayCharacter 인터페이스 구현: 사망 애니메이션 후 완전히 제거될 때 사용합니다.
    /// </summary>
    public virtual void DieAction()
    {
        gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        // 오브젝트가 비활성화될 때 실행 중인 비동기 작업을 안전하게 취소
        m_cancellation.Cancel();
    }
}