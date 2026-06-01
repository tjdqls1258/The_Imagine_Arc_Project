using System;
using UnityEngine;
using Util_Patten.FSM;

namespace Character_State
{
    [Serializable]
    public class CharacterContext : Context
    {
        public InGameCharacterData characterData;
        public PlayerCombatController atkController;
        public CharacterAnimationController animController;
        public Transform transform;

        // ====== 런타임 상태 데이터 ======
        public bool isSpawn;
        public bool onClick;
        public float lastSkillTime;
        public float currentAttackDelay;

        // ====== 헬퍼 프로퍼티 ======
        public bool HasTarget => atkController != null && atkController.GetCurrentTarget();
        public bool IsSkillReady => Time.time - lastSkillTime >= characterData.activeSkill.Cooldown;

        public override void Init()
        {
            currentAttackDelay = 0f;
        }
    }

}