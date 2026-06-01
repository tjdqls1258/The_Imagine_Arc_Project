 using UnityEngine;
using Util_Patten.FSM;

[CreateAssetMenu(menuName = "FSM/Enemy/Actions/Move")]
public class EnemyMoveActionSO : ActionSO<EnemyContext>
{
    public override void OnUpdate(EnemyContext context)
    {
        if (context.movePathList.Count == 0 || context.isDie) return;

        if (context.currentPathIndex >= context.movePathList.Count) return;

        Vector2 targetPos = context.movePathList[context.currentPathIndex];
        float distance = Vector2.Distance(targetPos, context.transform.position);

        if (distance < context.stopDistance)
        {
            context.currentPathIndex++;
        }
        else
        {
            context.transform.position = Vector3.MoveTowards(
                context.transform.position,
                targetPos,
                context.enemyStatManager.GetStat(StatType.MoveSpeed) * Time.deltaTime
            );
        }
    }
}
