using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 단일 지정 타겟팅 (타겟팅 스킬, 확정타, 버프/힐 등)
/// </summary>
[System.Serializable]
public class SingleTargetModule : TargetingModule
{
    [Tooltip("타겟으로 인식할 레이어 (적군 or 아군)")]
    public LayerMask TargetLayer;
    [Tooltip("마우스 주변 타겟 인식 반경 (터치/마우스 조작 편의성 보정)")]
    public float SearchRadius = 1.0f;

    [Header("Visual Effects")]
    [Tooltip("타겟의 머리 위나 몸에 즉시 발생하는 시각 이펙트 (낙뢰, 징표 등)")]
    public SkillEffectObject TargetVfxPrefab;

    [Header("Validation")]
    [Tooltip("시전 완료 시점에 타겟이 사거리를 벗어났다면 스킬을 취소할 것인가?")]
    public bool CancelIfOutOfRange = true;

    public override void UpdateIndicator(SkillContext context)
    {
        base.UpdateIndicator(context);

        Collider2D hit = Physics2D.OverlapCircle(context.TargetPosition, SearchRadius, TargetLayer);

        if (hit != null)
        {
            ITargetable foundTarget = hit.GetComponent<ITargetable>();

            if (foundTarget != null)
            {
                if (context.PrimaryTarget == null)
                {
                    context.PrimaryTarget = foundTarget;
                }
                else if (foundTarget != context.PrimaryTarget)
                {
                    context.PrimaryTarget.HighlightTarget(false);
                    context.PrimaryTarget = foundTarget;
                }

                context.PrimaryTarget.HighlightTarget(true);
            }
        }
        else
        {
            if (context.PrimaryTarget != null)
            {
                context.PrimaryTarget.HighlightTarget(false);
                context.PrimaryTarget = null;
            }
        }
    }

    public override bool ExecuteTargeting(SkillContext context, List<EffectModule> logicEffects)
    {
        context.DetectedTargets.Clear();

        // 타겟 유효성 검증
        if (context.PrimaryTarget != null)
        {
            if (CancelIfOutOfRange)
            {
                float distance = Vector3.Distance(
                    context.Caster.GetTransform().position,
                    context.PrimaryTarget.GetTransform().position
                );

                if (distance > MaxCastRange + 1.0f)
                {
                    context.PrimaryTarget.HighlightTarget(false);
                    return false; // 사거리 밖 취소
                }
            }

            context.DetectedTargets.Add(context.PrimaryTarget);

            if (TargetVfxPrefab != null && EffectPoolManager.Instance != null)
            {
                context.TargetPosition = context.PrimaryTarget.GetTransform().position;
                context.SkillRange = 1f; 

                EffectPoolManager.Instance.SpawnEffect(TargetVfxPrefab, context);
            }

            if (logicEffects != null)
            {
                foreach (var logic in logicEffects)
                {
                    logic.Apply(context, context.PrimaryTarget);
                }
            }

            context.PrimaryTarget.HighlightTarget(false);
        }
        else
        {
            Debug.LogWarning("[SingleTargetModule] 지정된 PrimaryTarget이 없습니다!");
            return false;
        }

        return true;
    }
}