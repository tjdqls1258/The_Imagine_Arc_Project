using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 버프/디버프 모듈
/// </summary>
[System.Serializable]
public class StatModifierEffect : EffectModule
{
    public bool IsBuff = true;
    public float ModifierValue = 20f;

    public List<ConditionBuffeSO> conditionBuffes = new();

    public override void Apply(SkillContext context, ITargetable target)
    {
        if (target == null) return;

        EffectPayload payload = new EffectPayload
        {
            CasterID = context.Caster.GetCasterID(),
            Category = IsBuff ? EffectCategory.Buff : EffectCategory.Debuff,
            Value =  ModifierValue,
            conditionBuffes = conditionBuffes
        };

        target.ApplyEffect(payload);
    }
}