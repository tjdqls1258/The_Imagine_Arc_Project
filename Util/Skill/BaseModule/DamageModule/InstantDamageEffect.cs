/// <summary>
/// 즉발 데미지
/// </summary>
[System.Serializable]
public class InstantDamageEffect : EffectModule
{
    public float DamageMultiply = 1f;

    public override void Apply(SkillContext context, ITargetable target)
    {
        EffectPayload payload = new EffectPayload
        {
            CasterID = context.Caster.GetCasterID(),
            Category = EffectCategory.InstantDamage,
            Value = context.Damage * DamageMultiply
        };

        target.ApplyEffect(payload);
    }
}