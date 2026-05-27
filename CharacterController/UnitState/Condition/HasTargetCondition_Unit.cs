using UnityEngine;
using Util_Patten.FSM;

namespace Character_State
{
    [CreateAssetMenu(fileName = "HasTargetCondition", menuName = "FSM/Conditions/HasTargetCondition")]
    public class HasTargetCondition : ConditionSO<CharacterContext>
    {
        [Tooltip("체크 해제 시 '타겟이 없을 때(False)'를 감지합니다.")]
        public bool isTrueCondition = true;

        public override bool Evaluate(CharacterContext context)
        {
            bool hasTarget = context.HasTarget;

            return isTrueCondition ? hasTarget : !hasTarget;
        }
    }
}