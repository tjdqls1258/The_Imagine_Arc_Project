using Character_State;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PlayerCharacterContrroller : MonoBehaviour
{
    public SkillBase m_activeSkill { private set; get; }
    public SkillBase m_passiveSkill { private set; get; }
    private InGameCharacterData m_characterData;
    private PlayerAttackController m_atkController;
    private CharacterAnimationController m_characterAniumationController;
    private UnityAction m_unitDieAction;

    private bool m_onClick = false;
    private bool m_isSpawn = false;
    private float m_lastSkillTime;
    private CancellationTokenSource cancel;

    public CharacterStateManager stateManager; // FSM 제어기

    private Image m_skillRange;
    List<IGamePlayCharacter> m_target;

    private void Awake()
    {
        stateManager = GetComponent<CharacterStateManager>();
        m_atkController = GetComponent<PlayerAttackController>();
        m_characterAniumationController = GetComponentInChildren<CharacterAnimationController>();
    }

    private void OnDestroy()
    {
        cancel?.Cancel();
        cancel?.Dispose();
    }

    public void SetCharacter(InGameCharacterData characterData)
    {
        cancel = new CancellationTokenSource();
        m_characterData = characterData;

        // 공격 컨트롤러 세팅
        if(m_target == null)
            m_target = new() { m_atkController };
        m_target.Clear();

        m_target.Add(m_atkController);
        m_atkController.InitCharacterData(m_characterData, m_characterAniumationController);
        SetSkill(characterData.activeSkill, characterData.passive);

        // FSM에 캐릭터 데이터 주입 (내부적으로 Context 세팅)
        stateManager.SetCharacter(characterData);
    }

    public void SetSkill(SkillBase active, SkillBase passive)
    {
        m_activeSkill = active;
        m_passiveSkill = passive;
    }

    public void SetSpawn(bool isSpawn)
    {
        m_atkController.enabled = isSpawn;
        m_isSpawn = isSpawn;

        stateManager.SetSpawn(isSpawn);

        UpdateFunc().Forget();
    }

    // 패시브 스킬은 상태와 무관하게 계속 돌아야 하므로 UniTask 루프
    protected async UniTask UpdateFunc()
    {
        while (m_isSpawn && cancel != null && !cancel.IsCancellationRequested)
        {
            await UniTask.WaitForEndOfFrame(this.GetCancellationTokenOnDestroy());
            m_passiveSkill?.TryExecutePassive(TriggerType_Passive.OnTick, stateManager.skillContext);
        }
    }

    public InGameCharacterData GetCharacterData() => m_characterData;
    public void OnPointerDownAction() { m_onClick = true; AtkAreaActive(m_onClick); }
    public void OnPointerUpAction() { m_onClick = false; AtkAreaActive(m_onClick); }
    public void AtkAreaActive(bool Active) { if (m_atkController != null) m_atkController.GetAtkRangeObject().SetActive(Active); }
    public void UpgradeCharacter() { m_atkController.Upgrade(); }
    public bool Skill() 
    {
        m_lastSkillTime = Time.time + m_atkController.SkillLastTime(); 
        return m_atkController.UseSkill(stateManager.skillContext); 
    }

    public float GetLastSkillTime() => m_lastSkillTime;
    public float GetSkillCoolTime() => m_activeSkill.Cooldown;
    public bool IsSpwan() => m_isSpawn;

    public void AddDieAction(UnityAction dieAction)
    {
        m_unitDieAction += dieAction;
    }
}