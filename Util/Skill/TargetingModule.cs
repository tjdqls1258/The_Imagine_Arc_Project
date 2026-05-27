using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[System.Serializable]
public abstract class EffectModule
{
    public SkillEffectObject VfxPrefab; //¿Ã∆Â∆Æ

    public virtual void SetEffect(SkillContext context)
    {//¿Ã∆Â∆Æ ∆˙∏µ « ø‰
        if (VfxPrefab == null) return;

        EffectPoolManager.Instance.SpawnEffect(VfxPrefab, context);
    }

    public abstract void Apply(SkillContext context, ITargetable target);
}

[System.Serializable]
public abstract class TargetingModule
{
    [Header("Range Visuals")]
    public float MaxCastRange = 10f;
    public float Range;

    public IndicatorObject MaxRangeIndicatorPrefab;
    public IndicatorObject ShapeIndicatorPrefab;

    public abstract bool ExecuteTargeting(SkillContext context, List<EffectModule> logicEffects);

    public virtual void ShowIndicator(SkillContext context)
    {
        var maxRangeInd = GetIndcator(context, true);
        var shapeInd = GetIndcator(context, false);
        context.SkillRange = Range;

        if (maxRangeInd != null)
        {
            maxRangeInd.SettingRange(MaxCastRange);
            maxRangeInd.gameObject.SetActive(true);
        }
        if (shapeInd != null)
        {
            shapeInd.SettingRange(Range);
            shapeInd.gameObject.SetActive(true);
        }
    }

    public virtual void UpdateIndicator(SkillContext context)
    {
        var shapeInd = GetIndcator(context, false);
        if (shapeInd != null)
        {
            Vector3 casterPos = context.Caster.GetTransform().position;
            Vector3 targetPos = context.TargetPosition;

            casterPos.y = targetPos.y;
            Vector3 offset = targetPos - casterPos;
            Vector3 clampedOffset = Vector3.ClampMagnitude(offset, MaxCastRange);
            Vector3 finalPos = casterPos + clampedOffset;

            shapeInd.transform.position = finalPos;
            context.TargetPosition = finalPos;
        }

        var maxRangeInd = GetIndcator(context, true);
        if (maxRangeInd != null)
        {
            maxRangeInd.transform.position = context.Caster.GetTransform().position;
        }
    }

    public virtual void HideIndicator(SkillContext context)
    {
        var shapeInd = GetIndcator(context, false);
        if (shapeInd != null) shapeInd.gameObject.SetActive(false);

        var maxRangeInd = GetIndcator(context, true);
        if (maxRangeInd != null) maxRangeInd.gameObject.SetActive(false);
    }

    protected virtual IndicatorObject GetIndcator(SkillContext context, bool isMaxRange)
    {
        if (isMaxRange && MaxRangeIndicatorPrefab != null)
        {
            if (context.MaxRangeIndcatorDic.ContainsKey(this.GetType()) == false)
                context.MaxRangeIndcatorDic.Add(this.GetType(), GameObject.Instantiate(MaxRangeIndicatorPrefab));
            return context.MaxRangeIndcatorDic[GetType()];
        }
        else if (ShapeIndicatorPrefab != null)
        {
            if (context.IndicatorDic.ContainsKey(this.GetType()) == false)
                context.IndicatorDic.Add(this.GetType(), GameObject.Instantiate(ShapeIndicatorPrefab));
            return context.IndicatorDic[GetType()];
        }
        return null;
    }
}