using System.Collections.Generic;

public enum StatType
{
    MaxHp,
    AttackSpeed,
    AttackRange,
    AttackDamage,
    Defense,
    MoveSpeed
}

public interface IStatProvider
{
    float GetStat(StatType statType);
    float SetStat(StatType staeType, float value);
    float AddStat(StatType statType, float value);
}

public class BaseCharacterStat : IStatProvider
{
    protected Dictionary<StatType, float> m_baseStats = new Dictionary<StatType, float>();

    public BaseCharacterStat(CharacterState characterData)
    {
        m_baseStats.Add(StatType.MaxHp, characterData.maxHp);
        m_baseStats.Add(StatType.AttackSpeed, characterData.atkSpeed);
        m_baseStats.Add(StatType.AttackRange, characterData.atkRang);
        m_baseStats.Add(StatType.AttackDamage, characterData.atkPower);
        m_baseStats.Add(StatType.Defense, characterData.defPower);
    }

    public virtual float GetStat(StatType statType)
    {
        if (m_baseStats.TryGetValue(statType, out float value))
            return value;

        return 0f;
    }

    public virtual float SetStat(StatType statType, float value)
    {
        if(m_baseStats.ContainsKey(statType))
            m_baseStats[statType] = value;

        return m_baseStats[statType];
    }

    public virtual float AddStat(StatType statType, float value)
    {
        if (m_baseStats.ContainsKey(statType))
            m_baseStats[statType] += value;
        else
            m_baseStats[statType] = value;

        return m_baseStats[statType];
    }
}

public abstract class StatDecorator : IStatProvider
{
    protected IStatProvider m_wrappedProvider;

    public StatDecorator(IStatProvider wrappedProvider)
    {
        m_wrappedProvider = wrappedProvider;
    }

    public virtual float GetStat(StatType statType)
    {
        return m_wrappedProvider.GetStat(statType);
    }
    public virtual float AddStat(StatType statType, float value)
    {
        return m_wrappedProvider.AddStat(statType, value);
    }

    public virtual float SetStat(StatType statType, float value)
    {
        return m_wrappedProvider.SetStat(statType, value);
    }
}

public class FlatStatModifier : StatDecorator
{
    private StatType m_targetStat;
    private float m_amount;

    public FlatStatModifier(IStatProvider wrappedProvider, StatType targetStat, float amount)
        : base(wrappedProvider)
    {
        m_targetStat = targetStat;
        m_amount = amount;
    }

    public override float GetStat(StatType statType)
    {
        float baseValue = base.GetStat(statType);

        if (statType == m_targetStat)
        {
            return baseValue + m_amount;
        }

        return baseValue;
    }
}

public class PercentStatModifier : StatDecorator
{
    private StatType m_targetStat;
    private float m_percent; // 0.1f = 10% 증가, -0.2f = 20% 감소

    public PercentStatModifier(IStatProvider wrappedProvider, StatType targetStat, float percent)
        : base(wrappedProvider)
    {
        m_targetStat = targetStat;
        m_percent = percent;
    }

    public override float GetStat(StatType statType)
    {
        float baseValue = base.GetStat(statType);

        if (statType == m_targetStat)
        {
            return baseValue * (1f + m_percent);
        }

        return baseValue;
    }
}

/*
Test
{
    // 1. 순수 캐릭터 스탯 생성 (기본 공격력 100)
    IStatProvider myStat = new BaseCharacterStat();

    // 버프(공격력 +20)
    myStat = new FlatStatModifier(myStat, StatType.AttackDamage, 20f);

    // 버프 (공격력 10% 증가)
    myStat = new PercentStatModifier(myStat, StatType.AttackDamage, 0.1f);

    //디버프 (공격력 -15)
    myStat = new FlatStatModifier(myStat, StatType.AttackDamage, -15f);


    float finalAtk = myStat.GetStat(StatType.AttackDamage);
    
    // 계산식: ((100 + 20) * 1.1) - 15 = 117
    UnityEngine.Debug.Log($"최종 공격력: {finalAtk}"); 
}
*/