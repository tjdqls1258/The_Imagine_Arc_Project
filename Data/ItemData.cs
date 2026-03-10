using System;
using System.Collections.Generic;

[Serializable]
public class ItemOption
{
    public int id;
    public int value;
}

[Serializable]
public class ItemData
{
    // ====== 고유 식별 정보 ======
    public int itemID;
    public string iconAddress; // Addressables Key

    // ====== 상태 정보 (서버/DB 연동) ======
    public int count = 0;
    public int itemLevel = 0;

    // ====== 옵션 정보 (구조화) ======
    // 병렬 배열 대신 리스트/배열 객체를 사용합니다.
    public List<ItemOption> options = new();

    /// <summary>
    /// 특정 ID의 옵션 값을 빠르게 찾기 위한 헬퍼 메서드
    /// </summary>
    public int GetOptionValue(int optionID)
    {
        return options.Find(x => x.id == optionID)?.value ?? 0;
    }
}