using UnityEngine;
using Util_Patten.FSM;

[CreateAssetMenu(menuName = "FSM/Enemy/Actions/Die")]
public class EnemyDieActionSO : ActionSO<EnemyContext>
{
    public override void OnEnter(EnemyContext context)
    {
        // 사망 애니메이션 재생 및 풀 반환
        context.animController.PlayAnimation_Bool(CharacterAnimationController.AnimationBool.DIE, true);
        context.disableAction?.Invoke(context.enemyData.id, context.transform.GetComponent<EnemyStateManager>());
    }
}