
using UnityEngine;

/// <summary>
/// 지속 데미지
/// </summary>
[System.Serializable]
public class DamageOverTimeEffect : EffectModule
{
    public float DamageMultiply = 0.5f;
    public float Duration = 5f;
    public float TickRate = 1f; // 1초마다 데미지

    public override void Apply(SkillContext context, ITargetable target)
    {
        EffectPayload payload = new EffectPayload
        {
            CasterID = context.Caster.GetCasterID(),
            Category = EffectCategory.DamageOverTime,
            Value = (context.Damage * DamageMultiply) / (Duration / TickRate), // 1틱당 데미지 계산
            TickRate = TickRate
        };

        target.ApplyEffect(payload);
    }
}