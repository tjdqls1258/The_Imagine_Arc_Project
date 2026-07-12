using UnityEngine;
using Util_Patten.FSM;

[CreateAssetMenu(menuName = "FSM/Enemy/Actions/Arrive")]
public class EnemyArriveActionSO : ActionSO<EnemyContext>
{
    public override void OnEnter(EnemyContext context)
    {
        context.transform.gameObject.SetActive(false);
        context.disableAction?.Invoke(context.enemyData.id, context.transform.GetComponent<EnemyStateManager>());
        context.arriveAction?.Invoke();
    }
}