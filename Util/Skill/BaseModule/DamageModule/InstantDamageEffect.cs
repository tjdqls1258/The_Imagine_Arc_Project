/// <summary>
/// 즉발 데미지
/// </summary>
[System.Serializable]
public class InstantDamageEffect : EffectModule
{
    public float DamageAmount = 50f;

    public override void Apply(SkillContext context, ITargetable target)
    {
        EffectPayload payload = new EffectPayload
        {
            CasterID = context.Caster.GetCasterID(),
            Category = EffectCategory.InstantDamage,
            Value = DamageAmount
        };

        target.ApplyEffect(payload);
    }
}