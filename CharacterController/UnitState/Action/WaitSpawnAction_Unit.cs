using UnityEngine;
using Util_Patten.FSM;

namespace Character_State
{
    [CreateAssetMenu(fileName = "WaitSpawnAction_Unit", menuName = "FSM/Actions/WaitSpawnAction_Unit")]
    public class WaitSpawnAction_Unit : ActionSO<CharacterContext>
    {
        public override void OnEnter(CharacterContext context)
        {
            context.currentAttackDelay = 0f;
        }
    }
}