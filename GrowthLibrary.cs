using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GrowthLibrary", menuName = "Data/Growth Library")]
public class GrowthLibrary : ScriptableObject
{
    public List<GrowthData> growthDataList = new List<GrowthData>();
}