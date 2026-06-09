using Character_State;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PlayerCharacterController : MonoBehaviour, ITargetable
{
    public InGameCharacterData CharacterData { get; private set; }
    public SkillBase ActiveSkill { get; private set; }
    public SkillBase PassiveSkill { get; private set; }

    [Header("Core Modules")]
    public CharacterStateManager stateManager;
    public PlayerCombatController combatController;
    private CharacterAnimationController animController;

    [Header("Block System")]
    public List<EnemyStateManager> blockedEnemies = new();

    private bool isSpawn = false;
    private float lastSkillTime;
    private UnityAction unitDieAction;
    private CancellationTokenSource cancelToken;

    private void Awake()
    {
        stateManager = GetComponent<CharacterStateManager>();
        combatController = GetComponent<PlayerCombatController>();
        animController = GetComponentInChildren<CharacterAnimationController>();
    }

    private void OnDestroy()
    {
        cancelToken?.Cancel();
        cancelToken?.Dispose();
    }

    public void SetCharacter(InGameCharacterData data)
    {
        cancelToken = new CancellationTokenSource();
        CharacterData = data;
        blockedEnemies.Clear();

        ActiveSkill = data.activeSkill;
        PassiveSkill = data.passive;

        // 하위 모듈 초기화 위임
        combatController.InitCombat(data, animController, DieAction);
        stateManager.InitState(data, combatController, animController);
    }

    public void SetSpawn(bool spawn)
    {
        isSpawn = spawn;
        combatController.enabled = spawn;
        stateManager.SetSpawn(spawn);

        if (spawn) PassiveTickLoop().Forget();
    }

    private async UniTask PassiveTickLoop()
    {
        while (isSpawn && cancelToken != null && !cancelToken.IsCancellationRequested)
        {
            await UniTask.WaitForEndOfFrame(this.GetCancellationTokenOnDestroy());
            PassiveSkill?.TryExecutePassive(TriggerType_Passive.OnTick, stateManager.skillContext);
        }
    }

    // --- UI & Actions ---
    public void OnPointerDownAction()
    {
        stateManager.CharacterContext.onClick = true;
        combatController.GetAtkRangeObject().SetActive(true);
    }

    public void OnPointerUpAction()
    {
        stateManager.CharacterContext.onClick = false;
        combatController.GetAtkRangeObject().SetActive(false);
    }

    public void UpgradeCharacter() => combatController.Upgrade();

    public void AtkAreaActive(bool isAtive) => combatController.GetAtkRangeObject().SetActive(isAtive);

    public float GetLastSkillTime() => lastSkillTime;
    public float GetSkillCoolTime() => ActiveSkill.Cooldown;
    public bool IsSpawn() => stateManager.CharacterContext.isSpawn;
    public ITargetable GetSelf() => this;
    public bool IsDie() => stateManager.CharacterContext.isSpawn;
    public Transform GetTransform() => transform;

    public void HighlightTarget(bool show) { }
    public void AddDieAction(UnityAction dieAction) => unitDieAction += dieAction;
    public void RemoveDieAction(UnityAction dieAction) => unitDieAction -= dieAction;
    public void DieAction() => unitDieAction?.Invoke();
    public void ApplyEffect(EffectPayload payload)
    {
        combatController.ApplyEffect(payload);
    }

    // --- Block System ---
    public bool TryBlock(EnemyStateManager enemy)
    {
        if (!isSpawn || CharacterData.characterData.blockCount <= 0) return false;
        if (blockedEnemies.Count >= CharacterData.characterData.blockCount) return false;
        if (blockedEnemies.Contains(enemy)) return true;

        blockedEnemies.Add(enemy);
        return true;
    }

    public void RemoveBlockedEnemy(EnemyStateManager enemy)
    {
        if (blockedEnemies.Remove(enemy) && !enemy.IsDie())
        {
            enemy.Unblock();
        }
    }
}