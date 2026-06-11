using System;
using System.Collections.Generic;
using UnityEngine;

public enum SkillType { Active, Passive }
public enum TriggerType_Passive { None, OnAttack, OnDamaged, OnTick, OnKill } //패시브 터지는 타이밍
public enum EffectCategory { InstantDamage, DamageOverTime, Buff, Debuff, Heal } //효과

public interface ISkillCaster
{
    Transform GetTransform();
    int GetCasterID();
}

public interface ITargetable
{
    Transform GetTransform();
    void ApplyEffect(EffectPayload payload);
    void HighlightTarget(bool show);
    public void DieAction();
    public ITargetable GetSelf();
    public bool IsDie();
}

[Serializable]
public class SkillContext
{
    public ISkillCaster Caster { get; set; }

    // [타겟팅 정보]
    public Vector3 TargetPosition { get; set; }
    public ITargetable PrimaryTarget { get; set; } // 단일 타겟 (논타겟팅/타겟팅 구분용)
    public List<ITargetable> DetectedTargets { get; set; } = new List<ITargetable>();
    public Vector3 Direction { get; set; }
    public float SkillRange { get; set; } = 1f;
    
    // 패시브 발동 정보
    public float Damage { get; set; }
    public ConditionBuffeManager Condition { get; set; }

    public Dictionary<Type, IndicatorObject> IndicatorDic = new();
    public Dictionary<Type, IndicatorObject> MaxRangeIndcatorDic = new();
}

public struct EffectPayload 
{
    public int CasterID;
    public EffectCategory Category;
    
    public float Value;
    public float TickRate;

    public List<ConditionBuffeSO> conditionBuffes;
}