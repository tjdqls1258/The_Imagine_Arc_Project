using UnityEngine;

[CreateAssetMenu(fileName = "PoisonCondition", menuName = "Condition/Poison (µ¶)")]
public class PoisonConditionSO : ConditionBuffeSO
{
    public override void OnTick(ITargetable target, int currentLevel, float value)
    {
        if (target == null) return;
        float finalTickDamage = value * currentLevel;

        EffectPayload payload = new EffectPayload
        {
            Category = EffectCategory.Debuff,
            Value = finalTickDamage
        };

        target.ApplyEffect(payload);
    }
}