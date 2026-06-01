using UnityEngine;
using Util_Patten.FSM;

namespace Character_State
{
    [CreateAssetMenu(menuName = "FSM/Unit/Actions/Attack")]
    public class AttackAction_Unit : ActionSO<CharacterContext>
    {
        public override void OnEnter(CharacterContext context)
        {
            //공격 상태 초기화
        }
        public override void OnUpdate(CharacterContext context)
        {
            context.atkController.UpdateTargeting();

            if (!context.HasTarget)
            {
                return;
            }

            context.atkController.LookAtTarget();

            if (context.currentAttackDelay <= context.atkController.AttackDelay)
            {
                context.currentAttackDelay += Time.deltaTime;
            }
            else
            {
                context.currentAttackDelay = 0f;
                context.animController.PlayAnimation_Trigger(CharacterAnimationController.AnimationTrigger.ATK);
            }
        }
    }
}