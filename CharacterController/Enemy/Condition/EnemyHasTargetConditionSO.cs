using UnityEngine;
using Util_Patten.FSM;

[CreateAssetMenu(menuName = "FSM/Enemy/Conditions/Has Target")]
public class EnemyHasTargetConditionSO : ConditionSO<EnemyContext>
{
    public override bool Evaluate(EnemyContext context)
    {
        return context.currentTarget != null;
    }
}