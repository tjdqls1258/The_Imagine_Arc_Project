using UnityEngine;
using Util_Patten.FSM;

namespace Character_State
{
    [CreateAssetMenu(fileName = "IsSpawnCondition", menuName = "FSM/Conditions/IsSpawnCondition")]
    public class IsSpawnCondition : ConditionSO<CharacterContext>
    {
        public override bool Evaluate(CharacterContext context)
        {
            // PlayerCharacterController에서 SetSpawn(true)를 호출해주면 조건이 충족됨
            return context.isSpawn;
        }
    }
}