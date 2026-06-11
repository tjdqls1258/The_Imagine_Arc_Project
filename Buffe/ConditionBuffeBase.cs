using UnityEngine;

public abstract class ConditionBuffeSO : ScriptableObject
{
    [Header("Basic Info")]
    public string ConditionID;
    public string ConditionName;
    public float MaxDuration;
    public float TickInterval = 1f;

    [Header("Stack Settings")]
    public int MaxLevel = 5;
    public bool RefreshDurationOnStack = true;
    public float BaseValue = 1;

    public virtual void OnTick(ITargetable target, int currentLevel, float value) { }
    public virtual float GetFlatModifier(StatType type, int currentLevel, float value) => 0f;
    public virtual float GetPercentModifier(StatType type, int currentLevel, float value) => 0f;
}
public class ActiveCondition
{
    public ConditionBuffeSO Data { get; private set; }

    public float Duration { get; private set; }
    public int CurrentLevel { get; private set; }
    public float Value { get; private set; }
    private float m_tickTimer = 0f;

    public ActiveCondition(ConditionBuffeSO soData, float Value)
    {
        Data = soData;
        Duration = soData.MaxDuration;
        CurrentLevel = 1;
        m_tickTimer = 0f;
        this.Value = Value * soData.BaseValue;
    }

    public void UpdateCondition(ITargetable target, float deltaTime)
    {
        Duration -= deltaTime;
        m_tickTimer += deltaTime;

        if (m_tickTimer >= Data.TickInterval)
        {
            m_tickTimer -= Data.TickInterval;

            Data.OnTick(target, CurrentLevel, Value);
        }
    }

    public void AddStack()
    {
        if (CurrentLevel < Data.MaxLevel)
        {
            CurrentLevel++;
            Debug.Log($"[{Data.ConditionName}] 중첩! 현재 레벨: {CurrentLevel}");
        }

        if (Data.RefreshDurationOnStack)
        {
            Duration = Data.MaxDuration;
        }
    }

    public float GetFlatModifier(StatType type) => Data.GetFlatModifier(type, CurrentLevel, Value);
    public float GetPercentModifier(StatType type) => Data.GetPercentModifier(type, CurrentLevel, Value);
}
/*

[CreateAssetMenu(fileName = "StatBoostCondition", menuName = "Condition/Stat Boost")]
public class StatBoostConditionSO : ConditionBuffeSO
{
    [Header("Modifiers Per Stack")]
    public float AtkPowerBonusPerLevel = 20f;   // 1중첩당 공격력 +20
    public float MoveSpeedPenaltyPerLevel = -0.1f; // 1중첩당 이속 -10%

    // 고정치 스탯 합산
    public override float GetFlatModifier(StatType statType, int currentLevel)
    {
        if (statType == StatType.AtkPower) 
            return AtkPowerBonusPerLevel * currentLevel; 
            
        return 0f;
    }

    // 퍼센트 스탯 합산
    public override float GetPercentModifier(StatType statType, int currentLevel)
    {
        if (statType == StatType.MoveSpeed) 
            return MoveSpeedPenaltyPerLevel * currentLevel; 
            
        return 0f;
    }
}

[CreateAssetMenu(fileName = "TickDamageCondition", menuName = "Condition/Tick Damage")]
public class TickDamageConditionSO : ConditionBuffeSO
{
    [Header("Tick Settings")]
    public float BaseTickDamage = 10f;
    public float DamageIncreasePerLevel = 5f; // 중첩될수록 5씩 더 아파짐

    // ActiveCondition이 TickInterval마다 알아서 이 함수를 찔러줍니다.
    public override void OnTick(ITargetable target, int currentLevel)
    {
        if (target == null) return;

        // 최종 틱 데미지 계산 (기본 10 + 중첩당 5)
        float finalDamage = BaseTickDamage + (DamageIncreasePerLevel * (currentLevel - 1));

        EffectPayload payload = new EffectPayload 
        { 
            Category = EffectCategory.Damage,
            Value = finalDamage 
        };

        target.ApplyEffect(payload);
    }
}

[CreateAssetMenu(fileName = "PoisonSlowCondition", menuName = "Condition/Poison Slow")]
public class PoisonSlowConditionSO : ConditionBuffeSO
{
    [Header("Poison Settings")]
    public float TickDamage = 15f;
    
    [Header("Slow Settings")]
    [Tooltip("중첩과 무관하게 적용될 고정 둔화율")]
    public float MoveSpeedPenalty = -0.3f; // -30%

    public override void OnTick(ITargetable target, int currentLevel)
    {
        if (target != null)
        {
            target.ApplyEffect(new EffectPayload 
            { 
                Category = EffectCategory.Damage, 
                Value = TickDamage * currentLevel // 독은 중첩될수록 데미지만 증가
            });
        }
    }

    public override float GetPercentModifier(StatType statType, int currentLevel)
    {
        // 이동속도 감소는 중첩 레벨을 무시하고 무조건 30%만 깎음
        if (statType == StatType.MoveSpeed) 
            return MoveSpeedPenalty;
            
        return 0f;
    }
}
 */