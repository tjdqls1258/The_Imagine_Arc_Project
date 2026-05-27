using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 투사체형 타겟팅 (논타겟 스킬샷)
/// </summary>
[System.Serializable]
public class ProjectileTargetModule : TargetingModule
{
    [Header("Projectile Settings")]
    [Tooltip("발사될 투사체 프리팹 (ProjectileEffect 컴포넌트가 필수적입니다)")]
    public SkillEffectObject ProjectilePrefab;

    public override bool ExecuteTargeting(SkillContext context, List<EffectModule> logicEffects)
    {
        context.DetectedTargets.Clear();

        if (ProjectilePrefab != null && EffectPoolManager.Instance != null)
        {
            context.SkillRange = 1f;

            var spawnedEffect = EffectPoolManager.Instance.SpawnEffect(ProjectilePrefab, context);

            if (spawnedEffect is ProjectileEffect projectile)
            {
                projectile.SetupPayload(context, logicEffects);
            }
            else
            {
                Debug.LogWarning($"[ProjectileTargetModule] '{ProjectilePrefab.name}'에 ProjectileEffect 컴포넌트가 없습니다!");
                return false;
            }
        }

        return true;
    }
}