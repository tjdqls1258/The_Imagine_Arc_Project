using UnityEngine;

[CreateAssetMenu(fileName = "AddPowerBuffe", menuName = "Condition/Add Power (데미지 증가)")]
public class AddPowerBuffe : ConditionBuffeSO
{
    public override float GetPercentModifier(StatType type, int currentLevel, float value)
    {
        if (type == StatType.AttackDamage)
        {
            return value;
        }

        return 0f;
    }
}
