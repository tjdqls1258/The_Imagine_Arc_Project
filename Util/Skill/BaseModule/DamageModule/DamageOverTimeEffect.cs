
using UnityEngine;

/// <summary>
/// 지속 데미지
/// </summary>
[System.Serializable]
public class DamageOverTimeEffect : EffectModule
{
    public float TotalDamage = 100f;
    public float Duration = 5f;
    public float TickRate = 1f; // 1초마다 데미지

    public override void Apply(SkillContext context, ITargetable target)
    {
        EffectPayload payload = new EffectPayload
        {
            CasterID = context.Caster.GetCasterID(),
            Category = EffectCategory.DamageOverTime,
            Value = TotalDamage / (Duration / TickRate), // 1틱당 데미지 계산
            Duration = Duration,
            TickRate = TickRate
        };

        target.ApplyEffect(payload);
    }
}