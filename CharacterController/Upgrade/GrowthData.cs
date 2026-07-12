using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum GrowthType
{
    Level,
    Enforce,
    Rank,
    Upgrade
}

[System.Serializable]
public class StatGrowthRule
{
    public StatType statType;     // 대상 스탯
    public float value;           // 증가치
}

[CreateAssetMenu(fileName = "GrowthData", menuName = "Data/Growth Data")]
public class GrowthData : ScriptableObject
{
    public string growthID; // 식별자
    public GrowthType growthType;

    public List<StatGrowthRule> levelRules = new List<StatGrowthRule>();
    public List<StatGrowthRule> enforceRules = new List<StatGrowthRule>();
    public List<StatGrowthRule> rankRules = new List<StatGrowthRule>();
    public List<StatGrowthRule> upgradeRules = new List<StatGrowthRule>();

    public float GetValue(GrowthType type, StatType stat)
    {
        var targetList = type switch
        {
            GrowthType.Level => levelRules,
            GrowthType.Enforce => enforceRules,
            GrowthType.Rank => rankRules,
            GrowthType.Upgrade => upgradeRules,
            _ => null
        };

        return targetList?.FirstOrDefault(x => x.statType == stat)?.value ?? 0f;
    }
}