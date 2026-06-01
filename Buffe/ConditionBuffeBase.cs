using UnityEngine;

public abstract class ConditionBuffeBase
{
    public string ConditionID; // 동일 버프인지 식별하기 위한 고유 ID
    public string ConditionName;

    public float Duration;          // 현재 남은 시간
    public float MaxDuration;       // 최대 지속 시간 (초기화용)
    public float TickInterval = 1f;
    private float m_tickTimer = 0f;

    [Header("Stack Settings")]
    public int CurrentLevel = 1;
    public int MaxLevel = 5; // 최대 중첩 횟수
    public bool RefreshDurationOnStack = true; // 중첩 시 지속 시간 초기화 여부

    public void UpdateCondition(ITargetable target, float deltaTime)
    {
        Duration -= deltaTime;
        m_tickTimer += deltaTime;

        if (m_tickTimer >= TickInterval)
        {
            m_tickTimer -= TickInterval;
            OnTick(target);
        }
    }

    public void AddStack()
    {
        if (CurrentLevel < MaxLevel)
        {
            CurrentLevel++;
            Debug.Log($"[{ConditionName}] 중첩! 현재 레벨: {CurrentLevel}");
        }

        // bool 값에 따라 시간 초기화 여부 결정
        if (RefreshDurationOnStack)
        {
            Duration = MaxDuration;
        }
    }

    protected virtual void OnTick(ITargetable target) { }
    public virtual float GetFlatModifier(StatType type) => 0f;
    public virtual float GetPercentModifier(StatType type) => 0f;
}

/*

public class AttackBoostCondition : ConditionBuffeBase
{
    public override float GetFlatModifier(StatType statType)
    {
        if (statType == StatType.AtkPower) return 20f; // 공격력 고정치 +20
        return 0f;
    }

    public override float GetPercentModifier(StatType statType)
    {
        if (statType == StatType.MoveSpeed) return -0.1f; // 이동속도 -10% (페널티)
        return 0f;
    }
}


public class TickDamageCondition : ConditionBuffeBase
{
    private float m_tickInterval; // 몇 초마다 데미지를 줄 것인가 (예: 1초)
    private float m_tickDamage;   // 1틱당 데미지
    private float m_timer;        // 내부 타이머

    public TickDamageCondition(string id, string name, float duration, float interval, float damage)
    {
        ConditionID = id;
        ConditionName = name;
        Duration = duration;
        
        m_tickInterval = interval;
        m_tickDamage = damage;
        m_timer = 0f;
    }

    public override void UpdateCondition(ITargetable target, float dt)
    {
        base.UpdateCondition(target, dt); // (Duration -= dt; 가 구현되어 있다고 가정)

        m_timer += dt;

        if (m_timer >= m_tickInterval)
        {
            m_timer -= m_tickInterval;

            if (target != null)
            {
                EffectPayload payload = new EffectPayload { Value = m_tickDamage };
                target.ApplyEffect(payload); 
                
                // Debug.Log($"[{ConditionName}] 틱 데미지 발동! {m_tickDamage} 피해량 적용");
            }
        }
    }

    public override float GetFlatModifier(StatType statType) => 0f;
    public override float GetPercentModifier(StatType statType) => 0f;
}
 */