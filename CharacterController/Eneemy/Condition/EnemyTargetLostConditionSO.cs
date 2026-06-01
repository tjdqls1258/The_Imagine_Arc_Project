using UnityEngine;
using Util_Patten.FSM;

[CreateAssetMenu(menuName = "FSM/Enemy/Conditions/Target Lost")]
public class EnemyTargetLostConditionSO : ConditionSO<EnemyContext>
{
    public override bool Evaluate(EnemyContext context)
    {
        if (context.currentTarget == null)
            return true;

        var targetMono = context.currentTarget as MonoBehaviour;
        if (targetMono == null || !targetMono.gameObject.activeInHierarchy)
        {
            context.currentTarget = null;
            context.isBlocked = false;
            return true;
        }

        if (context.attackType == EnemyAttackType.Ranged)
        {
            float distance = Vector2.Distance(context.transform.position, targetMono.transform.position);
            if (distance > context.enemyStatManager.GetStat(StatType.AttackRange) + 0.5f)
            {
                context.currentTarget = null;
                return true;
            }
        }

        return false;
    }
}