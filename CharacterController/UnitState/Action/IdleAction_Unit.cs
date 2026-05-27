using UnityEngine;
using Util_Patten.FSM;

namespace Character_State
{
    [CreateAssetMenu(fileName = "IdleAction_Unit", menuName = "FSM/Actions/IdleAction_Unit")]
    public class IdleAction_Unit : ActionSO<CharacterContext>
    {
        public override void OnEnter(CharacterContext context)
        {
            context.currentAttackDelay = 0f;
        }

        public override void OnUpdate(CharacterContext context)
        {
            context.atkController.UpdateTargeting();
        }
    }
}
