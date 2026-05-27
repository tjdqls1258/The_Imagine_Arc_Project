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
    public IGamePlayCharacter GetSelf();
    public void DieAction();
}

/// <summary>
/// 적 캐릭터의 이동, 피격, 사망 로직을 제어하는 컨트롤러 클래스입니다.
/// </summary>
public class EnemyController : MonoBehaviour, IGamePlayCharacter, ITargetable
{
    [Header("Enemy Settings")]
    [SerializeField] protected EnemyData m_enemyData;     // 적의 능력치 및 기본 정보 데이터
    [SerializeField] protected float stopDistance = 0.01f; // 경로 지점에 도착했다고 판단할 거리 임계값

    [Header("Component References")]
    [SerializeField] protected HPController m_hpController; // 체력 UI 및 수치 제어
    [SerializeField] protected CharacterAnimationController m_characterAnimationController; // 애니메이션 제어

    public bool isDie = false; // 현재 사망 상태 여부
    protected List<Vector2> m_movePathList = new(); // 이동해야 할 월드 좌표 리스트
    protected int m_currentPathIndex = 0;           // 현재 목표로 하는 경로의 인덱스
    protected float m_moveSpeed = 2f;               // 이동 속도

    private CancellationTokenSource m_cancellation = new(); // 비동기 작업(이동) 취소를 위한 토큰
    protected Action<int, EnemyController> m_disableAction; // 비활성화 시 오브젝트 풀에 반환하기 위한 콜백
    protected Action m_dieAction;
    protected Action m_arriveAction;

    [Tooltip("타겟팅 되었을 때 켜질 십자선이나 발밑의 마법진 UI")]
    public GameObject targetReticle;

    private void Awake()
    {
        targetReticle?.SetActive(false);
        if (m_characterAnimationController == null)
            m_characterAnimationController = GetComponent<CharacterAnimationController>();
    }

    public virtual void InitEnemyData(EnemyData enemyData, List<Vector2Int> movePathList, Action<int, EnemyController> disableAction, Action dieAction, Action arriveAction)
    {
        m_enemyData = enemyData;
        this.m_disableAction = disableAction;
        m_dieAction = dieAction;
        m_arriveAction = arriveAction;

        isDie = false;
        m_currentPathIndex = 0;
        m_movePathList.Clear();
        transform.position = new Vector3(movePathList[0].x, movePathList[0].y, 0);

        m_cancellation = new CancellationTokenSource();
        gameObject.SetActive(true);

        SetPath(movePathList);

        m_hpController.InitController(m_enemyData.characterState, DieAction_Enemy);
        m_characterAnimationController.PlayAnimation_Bool(CharacterAnimationController.AnimationBool.DIE, false);
    }

    protected virtual void SetPath(List<Vector2Int> pathData)
    {
        foreach (var vec in pathData)
            m_movePathList.Add(vec);

        LateUpdateAsync().Forget();
    }

    protected virtual async UniTask LateUpdateAsync()
    {
        while (!m_cancellation.IsCancellationRequested)
        {
            await UniTask.WaitForFixedUpdate();

            if (!m_cancellation.IsCancellationRequested)
                EnemyMove();
        }
    }

    protected virtual void EnemyMove()
    {
        if (m_movePathList.Count == 0 || isDie) return;

        if (CheckArraivePath(m_movePathList[m_currentPathIndex]))
        {
            m_currentPathIndex = System.Math.Min(m_movePathList.Count, m_currentPathIndex + 1);

            if (m_movePathList.Count == m_currentPathIndex)
                ArraiveEndPosition();
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, m_movePathList[m_currentPathIndex], m_moveSpeed * Time.deltaTime);
        }
    }

    protected bool CheckArraivePath(Vector2 path)
    {
        if (transform == null) return false;
        return Vector2.Distance(path, transform.position) < stopDistance;
    }

    public virtual void Hit(float atk)
    {
        if (isDie) return;

        m_hpController.UpdateHp(-atk);
        m_characterAnimationController.PlayAnimation_Trigger(CharacterAnimationController.AnimationTrigger.HIT);
    }

    public virtual void ArraiveEndPosition()
    {
        m_cancellation.Cancel();
        gameObject.SetActive(false);
        m_disableAction?.Invoke(m_enemyData.id, this); // 풀에 반환
        m_arriveAction?.Invoke(); //플레이어 라이프 차감 등의 액션
    }

    public virtual void DieAction_Enemy()
    {
        isDie = true;
        m_cancellation.Cancel(); // 이동 정지
        m_disableAction?.Invoke(m_enemyData.id, this); // 풀에 상태 알림
        m_characterAnimationController.PlayAnimation_Bool(CharacterAnimationController.AnimationBool.DIE, true);

        m_dieAction?.Invoke();
    }

    public virtual void DieAction()
    {
        gameObject.SetActive(false);
    }

    public virtual IGamePlayCharacter GetSelf() => this;

    private void OnDisable()
    {
        m_cancellation.Cancel();
    }

    public Transform GetTransform()
    {
        return transform;
    }

    public void ApplyEffect(EffectPayload payload)
    {
        Hit(payload.Value);
    }

    public void HighlightTarget(bool active)
    {
        if (targetReticle != null)
        {
            targetReticle.SetActive(active);
        }
    }
}