using System;
using UnityEngine;

[Serializable]
public class CharacterState
{
    public float maxHp = 1;
    public int atkPower;
    public int defPower;
    public int atkSpeed;
}

[Serializable]
public class MpCharacterState : CharacterState
{
    public float maxMp = 1 ;
}


public static class StatCalculator
{
    // 성장 계수
    private const float LEVEL_GROWTH_RATE = 0.1f;    // 레벨당 기본치의 10% 증가
    private const float ENFORCE_GROWTH_RATE = 0.05f; // 강화당 기본치의 5% 증가
    private const float INGAME_ENFORCE_GROWTH_RATE = 0.1f; // 인게임강화당 기본치의 5% 증가
    private const float RANK_MULTIPLIER = 0.2f;     // 랭크당 20% 추가 보너스

    /// <summary>
    /// 기본값과 성장 지표를 받아 최종 스탯을 계산합니다.
    /// </summary>
    public static float Calculate(float baseValue, int level, int enforce, int rank, int ingameEnforce)
    {
        // 예시 공식: 기본값 * (1 + 레벨성장 + 강화성장) * (1 + 랭크보너스)
        float growthFactor = 1 + ((level - 1) * LEVEL_GROWTH_RATE) + (enforce * ENFORCE_GROWTH_RATE) + (ingameEnforce * INGAME_ENFORCE_GROWTH_RATE);
        float rankFactor = 1 + (rank * RANK_MULTIPLIER);

        return baseValue * growthFactor * rankFactor;
    }
}