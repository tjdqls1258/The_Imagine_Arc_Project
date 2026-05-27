using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 범위형 타겟팅
/// </summary>
[System.Serializable]
public class RoundTargetModule : TargetingModule
{
    public enum Shape { Circle, Line }
    public Shape AreaShape;
    public LayerMask TargetLayer;

    [Header("Visual Effects")]
    public SkillEffectObject EffectPrefab;

    public override bool ExecuteTargeting(SkillContext context, List<EffectModule> logicEffects)
    {
        context.DetectedTargets.Clear();
        Vector3 centerPos = ClamSkillPos(context);

        if (AreaShape == Shape.Circle)
        {
            Collider[] hits = Physics.OverlapSphere(centerPos, Range, TargetLayer);
            foreach (var hit in hits)
            {
                if (hit.TryGetComponent(out ITargetable target))
                    context.DetectedTargets.Add(target);
            }
        }
        else if (AreaShape == Shape.Line)
        {
            Vector3 startPos = context.Caster.GetTransform().position;
            Vector3 dir = (centerPos - startPos).normalized;
            dir.y = 0;

            Vector3 halfExtents = new Vector3(1f, 2f, Range / 2f);
            Vector3 center = startPos + dir * (Range / 2f);

            Collider[] hits = Physics.OverlapBox(center, halfExtents, Quaternion.LookRotation(dir), TargetLayer);
            foreach (var hit in hits)
            {
                if (hit.TryGetComponent(out ITargetable target))
                    context.DetectedTargets.Add(target);
            }
        }


        if (EffectPrefab != null && EffectPoolManager.Instance != null)
        {
            context.SkillRange = Range;
            EffectPoolManager.Instance.SpawnEffect(EffectPrefab, context);
        }

        if (logicEffects != null)
        {
            foreach (var logic in logicEffects)
            {
                foreach (var target in context.DetectedTargets)
                {
                    logic.Apply(context, target);
                }
            }
        }

        return true;
    }

    private Vector3 ClamSkillPos(SkillContext context)
    {
        Vector3 casterPos = context.Caster.GetTransform().position;
        Vector3 targetPos = context.TargetPosition;

        casterPos.y = targetPos.y;
        Vector3 offset = targetPos - casterPos;
        Vector3 clampedOffset = Vector3.ClampMagnitude(offset, MaxCastRange);
        Vector3 finalPos = casterPos + clampedOffset;

        context.TargetPosition = finalPos;

        return finalPos;
    }
}