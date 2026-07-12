using System;
using System.Collections.Generic;
using UnityEngine;
using Util_Patten.FSM;

public enum EnemyAttackType { Melee, Ranged }

[Serializable]
public class EnemyContext : Context
{
    [Header("Component References")]
    public Transform transform;
    public HPController hpController;
    public CharacterAnimationController animController;
    public GameObject targetReticle;
    public EnemyStateManager stateManager;
    public SkillContext skillcontext;

    [Header("Enemy Settings")]
    public EnemyData enemyData;
    public SkillBase nomalSkill;
    public float stopDistance = 0.01f;
    public bool isDie = false;

    [Header("Path Data")]
    public List<Vector2> movePathList = new();
    public int currentPathIndex = 0;

    [Header("Callbacks")]
    public Action<int, EnemyStateManager> disableAction;
    public Action dieAction;
    public Action arriveAction;

    [Header("Combat Settings")]
    public EnemyAttackType attackType = EnemyAttackType.Melee;
    public float lastAttackTime = 0f;
    public float nextScanTime;

    [Header("Targeting & Block")]
    public LayerMask targetLayer; // 원거리 적이 탐색할 아군 레이어
    public bool isBlocked = false;
    public ITargetable currentTarget;

    public PlayerCharacterController currentBlocker;

    public ConditionBuffeManager enemyStatManager;

    public override void Init()
    {
        isBlocked = false;
        currentTarget = null;
        lastAttackTime = 0f;
        nextScanTime = 0f;
    }
}