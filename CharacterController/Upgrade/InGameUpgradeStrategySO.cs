using UnityEngine;

public abstract class InGameUpgradeStrategySO : ScriptableObject
{
    [Header("Upgrade Description")]
    public string upgradeType;

    /// <summary>
    /// 인게임에서 재화를 소모해 업그레이드 할 때 호출됩니다.
    /// </summary>
    /// <param name="baseStat">캐릭터의 인게임 베이스 스탯</param>
    /// <param name="upgradeLevel">현재 달성한 업그레이드 단계</param>
    public abstract void ApplyUpgrade(BaseCharacterStat baseStat, int upgradeLevel);
}