using NetExcute;
using System;
using System.Collections.Generic;
using UnityEngine;
using Util_Patten.FSM;

[Serializable]
public class InGameEnemyStat : BaseCharacterStat
{
    protected CharacterState characterState;
    public int spawnLevel;

    public InGameEnemyStat(CharacterState characterData, float MoveSpeed, int level)
        : base(characterData)
    {
        characterState = characterData;
        spawnLevel = level;
        m_baseStats.Add(StatType.MoveSpeed, MoveSpeed);

        ApplyEnemyGrowth();
    }

    private void ApplyEnemyGrowth()
    {
        //string growthID = "Monster_Level"; // 몬스터 전용 CSV 키
        //GrowthData growth = DataManager.Instance.GetGrowthData(growthID);

        //if (growth != null && spawnLevel > 1)
        //{
        //    int levelUps = spawnLevel - 1;

        //    AddStat(StatType.MaxHp, growth.MaxHpAdd * levelUps);
        //    AddStat(StatType.AtkPower, growth.AtkPowerAdd * levelUps);
        //    AddStat(StatType.DefPower, growth.DefPowerAdd * levelUps);

        //    // 디펜스 게임의 경우 후반 웨이브 몬스터의 이속/공속이 
        //    // 미세하게 빨라지게 설계하는 것도 여기서 처리할 수 있습니다.
        //}
    }
}

public class EnemyStateManager : StateMachine<EnemyContext, EnemyState>, ITargetable, ISkillCaster
{
    [Header("FSM Core States")]
    [Tooltip("사망 시 강제 전환할 상태 SO")]
    public EnemyState dieState;

    public ConditionBuffeManager conditionBuffeManager;
    protected InGameEnemyStat inGameEnemyStat;

    [SerializeField] protected float moveSpeed;
    private void Awake()
    {
        if (context.targetReticle != null) context.targetReticle.SetActive(false);
        if (context.animController == null) context.animController = GetComponent<CharacterAnimationController>();
        if (context.hpController == null) context.hpController = GetComponent<HPController>();
        context.transform = this.transform;
    }

    public void InitEnemyData(EnemyData enemyData, List<Vector2Int> movePathList, Action<int, EnemyStateManager> disableAction, Action dieAction, Action arriveAction)
    {
        context.Init();

        context.enemyData = enemyData;
        context.disableAction = disableAction;
        context.dieAction = dieAction;
        context.arriveAction = arriveAction;
        context.enemyStatManager = conditionBuffeManager;
        context.stateManager = this;
        inGameEnemyStat = new(enemyData.characterState, moveSpeed, enemyData.enemyLevel);
        conditionBuffeManager.SetCharacterStat(inGameEnemyStat);

        context.skillcontext = new SkillContext() 
        { 
            Caster = this, 
            Condition = conditionBuffeManager, 
            SkillRange = conditionBuffeManager.GetStat(StatType.AttackRange),
            Damage = conditionBuffeManager.GetStat(StatType.AttackDamage)
            
        };

        foreach (var vec in movePathList)
        {
            context.movePathList.Add(vec);
        }

        context.transform.position = new Vector3(movePathList[0].x, movePathList[0].y, 0);
        gameObject.SetActive(true);

        context.hpController.InitController(context.enemyData.characterState, DieAction_Enemy, context.enemyStatManager);
        context.animController.PlayAnimation_Bool(CharacterAnimationController.AnimationBool.DIE, false);

        if (startState != null)
        {
            ForeChangeState(startState);
        }
    }

    public void ApplyEffect(EffectPayload payload)
    {
        Hit(payload.Value);

        if (payload.conditionBuffes != null && payload.conditionBuffes.Count > 0)
        {
            ApplyBuffeEffectList(payload.conditionBuffes, payload.Value);
        }
    }

    public void Hit(float atk)
    {
        if (context.isDie) return;

        context.hpController.UpdateHp(-atk);
        context.animController.PlayAnimation_Trigger(CharacterAnimationController.AnimationTrigger.HIT);
    }

    public void DieAction_Enemy()
    {
        if (context.isDie) return;

        if (context.currentBlocker != null)
        {
            context.currentBlocker.RemoveBlockedEnemy(this);
            context.currentBlocker = null;
        }

        context.isDie = true;
        context.dieAction?.Invoke();

        if (dieState != null) ForeChangeState(dieState);
    }

    public ITargetable GetSelf() => this;
    public Transform GetTransform() => transform;
    public void DieAction() => gameObject.SetActive(false);

    public void HighlightTarget(bool active)
    {
        if (context.targetReticle != null) context.targetReticle.SetActive(active);
    }

    public void SetBlocked(ITargetable blocker)
    {
        context.isBlocked = true;
        context.currentTarget = blocker;
    }

    public void Unblock()
    {
        context.isBlocked = false;
        context.currentTarget = null;
    }

    public bool IsDie()
    {
        return context.isDie;
    }

    public int GetCasterID()
    {
        return GetInstanceID();
    }

    private void ApplyBuffeEffectList(List<ConditionBuffeSO> effectList, float value)
    {
        foreach(ConditionBuffeSO effect in effectList)
        {
            conditionBuffeManager.ApplyCondition(effect, value);
        }
    }
}