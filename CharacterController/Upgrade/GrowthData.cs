using UnityEngine;

[CreateAssetMenu(menuName = "GrowthData")]
public class GrowthData : ScriptableObject
{
    public string ID;
    public float MaxHpAdd;
    public float AtkPowerAdd;
    public float DefPowerAdd;
    public float AtkSpeedAdd;
    public float AtkRangeAdd;
}