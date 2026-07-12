using System.Collections.Generic;
using UnityEngine;
using VContainer;

//성장 관련 매니저
public class GrowthManager
{
    private readonly GrowthLibrary m_library;
    private readonly Dictionary<string, GrowthData> m_dataCache = new();

    public GrowthManager(GrowthLibrary library)
    {
        m_library = library;

        foreach (var data in library.growthDataList)
        {
            if (data != null) m_dataCache[data.growthID] = data;
        }
    }

    public GrowthData GetGrowthData(string id)
    {
        if (m_dataCache.TryGetValue(id, out GrowthData growthData))
            return growthData;

        Logger.LogError($"GrowthID : {id}를 찾을 수 없습니다.");
        return null;
    }

    public static float CalculateAddedStat(float currentStat, float growthRate, int step)
    {
        return (currentStat * growthRate) * step;
    }
}