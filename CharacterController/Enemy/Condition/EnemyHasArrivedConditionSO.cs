using UnityEngine;
using Util_Patten.FSM;

[CreateAssetMenu(menuName = "FSM/Enemy/Conditions/Has Arrived")]
public class EnemyHasArrivedConditionSO : ConditionSO<EnemyContext>
{
    public override bool Evaluate(EnemyContext context)
    {
        return context.currentPathIndex >= context.movePathList.Count;
    }
}