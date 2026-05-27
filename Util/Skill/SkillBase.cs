using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Skill System/Skill Template")]
public class SkillBase : ScriptableObject, IToolTip
{
    public int SkillID;
    [Header("Skill Classification")]
    public string SkillName;
    public SkillType Type;

    [PreviewImage]
    public Sprite SkillIcon;

    public float Cooldown = 5.0f;

    // 패시브 전용 설정
    [Header("Passive Settings")]
    [Tooltip("패시브 발생 조건 (공격시, 초마다 등) ")]
    public TriggerType_Passive ActivationTrigger;
    [Tooltip("패시브 발생 확률 (Ex 공격시 10% 확률 -> 0.1)")]
    public float ProcChance = 1.0f; 

    [Header("Modules")]
    [Tooltip("타겟 설정 방식")]
    [SerializeReference] public TargetingModule TargetingMode;
    [Tooltip("효과")]
    [SerializeReference] public List<EffectModule> EffectModes = new List<EffectModule>();

    [TextArea(3, 10)]
    public string SkillDesc;

    // 액티브 스킬 실행
    public bool ExecuteActive(SkillContext context)
    {
        if (Type != SkillType.Active) return false;

        return ExecutePipeline(context);
    }

    // 패시브 스킬 실행
    public void TryExecutePassive(TriggerType_Passive triggerOccurred, SkillContext context)
    {
        if (Type != SkillType.Passive) return;
        if (ActivationTrigger != triggerOccurred) return;

        if (Random.value <= ProcChance)
        {
            ExecutePipeline(context);
        }
    }

    // 내부 로직
    private bool ExecutePipeline(SkillContext context)
    {
        if(Type != SkillType.Passive)
            Debug.Log($"스킬 사용! [{SkillName}] - 시전자: {context.Caster.GetTransform().gameObject.name}");

        bool skillResult = false;
        if (TargetingMode != null)
        {
            skillResult = TargetingMode.ExecuteTargeting(context, EffectModes);
        }
        else
        {
            foreach (var effect in EffectModes)
            {
                if (context.Caster is ITargetable selfTarget)
                {
                    effect.Apply(context, selfTarget);
                }
            }
        }

        return skillResult;
    }

    public void BeginAiming(SkillContext context)
    {
        if (TargetingMode != null) 
            TargetingMode.ShowIndicator(context);
    }

    public void UpdateAiming(SkillContext context)
    {
        if (TargetingMode != null) 
            TargetingMode.UpdateIndicator(context);
    }

    public bool EndAimingAndExecute(SkillContext context)
    {
        if (TargetingMode != null)
            TargetingMode.HideIndicator(context);

        return ExecuteActive(context);
    }

    public void CancelAiming(SkillContext context)
    {
        if (TargetingMode != null) TargetingMode.HideIndicator(context);
    }

    public string GetTitle()
    {
        return SkillName;
    }

    public string GetDescription()
    {
        return SkillDesc;
    }

    public string GetCoolTime()
    {
        return Cooldown.ToString("N1");
    }
}