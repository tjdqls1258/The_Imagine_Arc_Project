using System.Collections.Generic;
using UnityEngine;
using Util_Patten.FSM;

namespace Character_State
{
    public enum CharacterState
    {
        DisableSpawn,
        Spawn_Idle,
        Spawn_Attack,
        DieAction,
        SpawnCoolTime
    }

    public class CharacterStateManager : StateMachine<CharacterContext, CharacterStateScriptableObjcetBase>
    {
        public CharacterContext CharacterContext { get => context; }
        public SkillContext skillContext { get; private set; } = new();
        public CharacterState CurrentCharacterState { get; private set; } = CharacterState.DisableSpawn;

        public void InitState(InGameCharacterData data, PlayerCombatController combat, CharacterAnimationController anim)
        {
            context.characterData = data;
            context.atkController = combat;
            context.animController = anim;
            context.transform = transform;

            skillContext.Caster = combat;
        }

        public void SetSpawn(bool isSpawn) => context.isSpawn = isSpawn;

        protected override void Update()
        {
            if (!context.isSpawn) return;
            base.Update();
        }

        protected override void ForeChangeState(CharacterStateScriptableObjcetBase state)
        {
            base.ForeChangeState(state);
            CurrentCharacterState = state.StateType;
        }

        public float GetDamage() => context.atkController.ConditionManager.GetStat(StatType.AttackDamage);
#if UNITY_EDITOR
        [ContextMenu("Setting Init Editor")]
        public void SettingEditor()
        {
            context.transform = this.transform;

            context.atkController = GetComponent<PlayerCombatController>();

            context.animController = GetComponentInChildren<CharacterAnimationController>();

            if (context.atkController == null)
                Debug.LogWarning($"[{gameObject.name}] PlayerAttackController를 찾을 수 없습니다!");

            if (context.animController == null)
                Debug.LogWarning($"[{gameObject.name}] CharacterAnimationController를 찾을 수 없습니다!");

            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}