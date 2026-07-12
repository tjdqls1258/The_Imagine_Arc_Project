using UnityEngine;

[CreateAssetMenu(fileName = "RootCondition", menuName = "Condition/Root (이동 불가)")]
public class RootConditionSO : ConditionBuffeSO
{
    public override float GetPercentModifier(StatType type, int currentLevel, float value)
    {
        if (type == StatType.MoveSpeed)
        {
            return -1f;
        }

        return 0f;
    }
}