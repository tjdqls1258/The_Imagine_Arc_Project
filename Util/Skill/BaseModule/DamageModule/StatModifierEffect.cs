using System.Collections.Generic;

/// <summary>
/// 버프/디버프 모듈
/// </summary>
[System.Serializable]
public class StatModifierEffect : EffectModule
{
    public bool IsBuff = true;
    public string TargetStat = "AttackPower";
    public float ModifierValue = 20f;
    public float Duration = 10f;

    public List<ConditionBuffeBase> conditionBuffes = new();

    public override void Apply(SkillContext context, ITargetable target)
    {
        EffectPayload payload = new EffectPayload
        {
            CasterID = context.Caster.GetCasterID(),
            Category = IsBuff ? EffectCategory.Buff : EffectCategory.Debuff,
            Value =  ModifierValue,
            Duration = Duration,
            TargetStat_Tag = TargetStat
        };

        target.ApplyEffect(payload);
    }
}