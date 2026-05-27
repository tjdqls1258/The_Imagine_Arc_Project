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

    public class CharacterStateManager : StateMachine<CharacterContext, CharacterStateScriptableObjcetBase>, ISkillCaster
    {
        public SkillContext skillContext { get; private set; } = new();
        private CharacterState currentCharacterState = CharacterState.DisableSpawn;
        public CharacterState GetCurrentCharacterState { get  { return currentCharacterState; } }

        public void SetCharacter(InGameCharacterData data)
        {
            context.characterData = data;
            skillContext.Caster = this;
        }

        public void SetSpawn(bool isSpawn) => context.isSpawn = isSpawn;

        public void OnPointerDownAction()
        {
            context.onClick = true;
            context.atkController.GetAtkRangeObject().SetActive(true);
        }

        public void OnPointerUpAction()
        {
            context.onClick = false;
            context.atkController.GetAtkRangeObject().SetActive(false);
        }

        public void UpgradeCharacter()
        {
            context.atkController.Upgrade();
        }

        // 패시브 스킬 등 상시 로직은 Update에서 별도로 돌리거나 전용 ActionSO를 만듭니다.
        protected override void Update()
        {
            if (!context.isSpawn) return;

            base.Update();
        }

        protected override void ForeChangeState(CharacterStateScriptableObjcetBase state)
        {
            base.ForeChangeState(state);
            currentCharacterState = state.StateType;
        }

#if UNITY_EDITOR
        [ContextMenu("Setting Init Editor")]
        public void SettingEditor()
        {
            context.transform = this.transform;

            context.atkController = GetComponent<PlayerAttackController>();

            context.animController = GetComponentInChildren<CharacterAnimationController>();

            if (context.atkController == null)
                Debug.LogWarning($"[{gameObject.name}] PlayerAttackController를 찾을 수 없습니다!");

            if (context.animController == null)
                Debug.LogWarning($"[{gameObject.name}] CharacterAnimationController를 찾을 수 없습니다!");

            UnityEditor.EditorUtility.SetDirty(this);
        }

        public Transform GetTransform()
        {
            return transform;
        }

        public int GetCasterID()
        {
            return GetInstanceID();
        }
#endif
    }
}