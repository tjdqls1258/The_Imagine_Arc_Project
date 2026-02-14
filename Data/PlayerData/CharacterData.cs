using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

/// <summary>
/// 인게임에서 실제로 사용되는 캐릭터 데이터 인스턴스입니다.
/// 원본 데이터(CharacterData)를 참조하며 업그레이드 횟수 등 가변적인 상태를 관리합니다.
/// </summary>
public class InGameCharacterData
{
    public InGameCharacterData(CharacterData data)
    {
        characterData = data;
        upgradeCount = 0;
    }

    public CharacterData characterData; // 원본 데이터 참조
    public int upgradeCount;            // 캐릭터 강화 횟수
}

/// <summary>
/// CSV로부터 로드되는 캐릭터의 고유 정보 및 리소스 로딩 로직을 담고 있는 클래스입니다.
/// </summary>
public class CharacterData : CSVData
{
    [Header("Basic Stats")]
    public int id;                  // 캐릭터 고유 ID
    public int cost;                // 배치 비용
    public int rating;              // 등급 (Rarity)
    public string characterName;    // 캐릭터 이름
    public float maxHp = 1;         // 최대 체력
    public int atkPower;            // 공격력
    public int defPower;            // 방어력
    public int atkSpeed;            // 공격 속도
    public float maxMp = 1;         // 최대 마나

    [Header("Resource Keys")]
    public string modelObjectName;  // 어드레서블 프리팹 경로 키
    public string modelSpriteName;  // 어드레서블 스프라이트 아틀라스 경로 키

    [Header("Runtime State")]
    public MpCharacterState characterState; // 전투 로직에서 사용할 상태 객체
    private Sprite modelSprite;             // 캐싱된 캐릭터 스프라이트
    private SpriteAtlas characterSprite;    // 캐싱된 스프라이트 아틀라스

    /// <summary> 어드레서블 시스템에서 사용할 스프라이트 아틀라스의 전체 경로를 반환합니다. </summary>
    private string GetSpriteName => string.Format(Util.CHARACTER_SPRITE_PATH, modelSpriteName);

    /// <summary>
    /// CSV 필드 값을 바탕으로 실제 전투에서 사용할 상태 객체(characterState)를 생성합니다.
    /// </summary>
    public void SetCharacterState()
    {
        characterState = new()
        {
            maxHp = maxHp,
            atkPower = atkPower,
            atkSpeed = atkSpeed,
            maxMp = maxMp,
            defPower = defPower
        };
    }

    public override int GetID() => id;

    /// <summary>
    /// [비동기] 캐릭터 스프라이트를 반환합니다. 로드되어 있지 않다면 로드 후 반환합니다.
    /// </summary>
    public async UniTask<Sprite> GetSpriteAsync()
    {
        if (modelSprite == null)
        {
            await LoadSprite();
        }

        return modelSprite;
    }

    /// <summary>
    /// [비동기] 어드레서블 매니저를 통해 캐릭터의 모델(Prefab) 원본을 로드하고 캐싱합니다.
    /// </summary>
    public async UniTask<GameObject> GetModleObject()
    {
        return await AddressableManager.Instance.LoadAssetAndCacheAsync<GameObject>(modelObjectName);
    }

    /// <summary>
    /// [비동기] 캐릭터의 일러스트/아이콘을 로드하여 UI 이미지에 적용하거나 콜백으로 전달합니다.
    /// </summary>
    /// <param name="loadImage">이미지 로드 완료 후 실행할 액션</param>
    /// <param name="targetImage">이미지를 적용할 UI Image 컴포넌트</param>
    public async UniTask GetCharacterSprite(Action<Sprite> loadImage = null, Image targetImage = null)
    {
        if (characterSprite == null)
            await LoadSprite();

        // 아틀라스에서 정해진 이름(UTIL 규칙)으로 스프라이트를 추출
        Sprite sprite = characterSprite.GetSprite(Util.CHARACTER_IMAGE_NAME);

        if (targetImage != null)
            targetImage.sprite = sprite;

        if (loadImage != null)
            loadImage.Invoke(sprite);
    }

    /// <summary>
    /// [비동기] 어드레서블 시스템을 통해 캐릭터의 스프라이트 아틀라스를 로드하고 메모리에 캐싱합니다.
    /// </summary>
    public async UniTask LoadSprite()
    {
        if (characterSprite != null) return;

        SpriteAtlas atlas = await AddressableManager.Instance.LoadAssetAndCacheAsync<SpriteAtlas>(GetSpriteName);
        characterSprite = atlas;
    }

    /// <summary>
    /// 사용하지 않는 아틀라스 리소스를 메모리에서 해제합니다.
    /// </summary>
    public void UnloadAtlas()
    {
        if (characterSprite == null) return;

        AddressableManager.Instance.UnloadAsset(GetSpriteName);
        characterSprite = null;
    }
}

/// <summary>
/// 전체 캐릭터 데이터 목록을 관리하는 컬렉션 클래스입니다.
/// </summary>
public class CharacterDataList : CSVDataList<CharacterData>
{
    /// <summary> ID에 해당하는 캐릭터의 이름을 가져옵니다. </summary>
    public string GetName(int id)
    {
        return m_dataList.ContainsKey(id) ? m_dataList[id].characterName : string.Empty;
    }

    /// <summary>
    /// [비동기] 특정 ID 캐릭터의 리소스를 로드하고 스프라이트를 반환합니다.
    /// </summary>
    public async UniTask<Sprite> GetSpriteAsync(int id)
    {
        if (m_dataList.ContainsKey(id))
            return await m_dataList[id].GetSpriteAsync();

        return null;
    }

    /// <summary> 딕셔너리에 저장된 모든 캐릭터 데이터를 리스트 형태로 반환합니다. </summary>
    public List<CharacterData> GetAllList()
    {
        return m_dataList.Values.ToList();
    }

    /// <summary> 모든 캐릭터 데이터가 포함된 기본 리스트를 생성하여 반환합니다. </summary>
    public List<CharacterData> GetDefaultList()
    {
        List<CharacterData> datas = new();

        foreach (var id in m_dataList.Keys)
            datas.Add(m_dataList[id]);

        return datas;
    }
}