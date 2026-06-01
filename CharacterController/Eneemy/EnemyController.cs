using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Util_Patten;

/// <summary>
/// 게임 내 모든 적 유닛의 공통 기능을 정의하는 인터페이스입니다.
/// </summary>

public class EnemyController : MonoBehaviour
{
    [SerializeField] private EnemyStateManager m_stateManager;
    public EnemyStateManager StateManager { get { return m_stateManager; } }
    [Header("Enemy Settings")]
    [SerializeField] protected EnemyData m_enemyData;     // 적의 능력치 및 기본 정보 데이터
    protected Action<int, EnemyController> m_disableAction; // 비활성화 시 오브젝트 풀에 반환하기 위한 콜백

    public virtual void InitEnemyData(EnemyData enemyData, List<Vector2Int> movePathList, Action<int, EnemyController> disableAction, Action dieAction, Action arriveAction)
    {
        this.m_disableAction = disableAction;
        m_stateManager.InitEnemyData(enemyData, movePathList, InvokeDisableAction, dieAction, arriveAction);
    }

    public virtual void Hit(float atk)
    {
        m_stateManager.Hit(atk);
    }

    protected virtual void InvokeDisableAction(int id, EnemyStateManager mamger)
    {
        m_disableAction?.Invoke(id, this);
    }

    public ITargetable GetTarget() => m_stateManager;
}