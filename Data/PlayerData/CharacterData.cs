using Cysharp.Threading.Tasks;
using NetExcute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

/// <summary>
/// [Instance Data] 인게임 전투 중 실제로 사용되는 캐릭터 데이터 객체입니다.
/// 원본 마스터 데이터(CharacterData)는 참조만 하며, 강화 횟수와 같은 유동적인 상태값을 독립적으로 관리합니다.
/// </summary>
public class InGameCharacterData
{
    private NetExcute.UserCharacterData userCharacterDatas; // 서버/유저 DB에서 가져온 기본 성장 정보 (레벨 등)
    protected int upgradeCount;          // 인게임 내에서 진행된 실시간 강화 횟수
    
    public CharacterData characterData; // 변하지 않는 캐릭터 원본 정보 (CSV 데이터) 참조
    public SkillBase passive;
    public SkillBase activeSkill;

    /// <summary>
    /// 원본 데이터와 유저 성장 데이터를 조합하여 인게임용 인스턴스를 생성합니다.
    /// </summary>
    public InGameCharacterData(CharacterData data, NetExcute.UserCharacterData userCharacterData, SkillBase passive = null, SkillBase other = null)
    {
        characterData = data;
        userCharacterDatas = userCharacterData;
        upgradeCount = 0; // 전투 시작 시 강화 횟수 초기화

        this.passive = passive;
        activeSkill = other;
    }

    /// <summary>
    /// 인게임 최종 공격력을 계산하여 반환합니다.
    /// 계산 공식: 기본 공격력 + (기본 공격력 * 실시간 강화 횟수 * 10%)
    /// </summary>
    public float GetAtk()
    {
        // TODO: 캐릭터 장비, 유저 레벨, 시너지 효과 등을 추가 반영 가능
        return StatCalculator.Calculate(characterData.characterState.atkPower, userCharacterDatas.level, userCharacterDatas.Enforce, userCharacterDatas.Rank, upgradeCount);
    }

    /// <summary>
    /// 인게임 내에서 캐릭터의 강화 수치를 누적시킵니다.
    /// </summary>
    public void UpgradeCharacter(int count)
    {
        upgradeCount = upgradeCount + count;
    }
}

/// <summary>
/// [Master Data] CSV로부터 로드되는 캐릭터의 고유 정보 및 리소스 로딩 로직을 담고 있는 클래스입니다.
/// 모든 인스턴스가 공유하는 '원판' 역할을 수행합니다.
/// </summary>
public class CharacterData : CSVData
{
    [Header("Basic Stats")]
    public int id;                  // 캐릭터 고유 식별 ID
    public int cost;                // 배치 시 소모되는 자원량
    public int rating;              // 캐릭터 등급 (예: 1~5성)
    public string characterName;    // 캐릭터 이름
    public MpCharacterState characterState; // 캐릭터의 기본 스탯(HP, ATK, MP 등) 객체

    public int[] passiveSkill;
    public int[] activeSkill;

    [Header("Resource Keys")]
    public string modelObjectName;  // Addressables: 캐릭터 프리팹 에셋 주소
    public string modelSpriteName;  // Addressables: 캐릭터 아틀라스 에셋 주소

    [Header("Runtime State (Caching)")]
    private Sprite modelSprite;             // 캐싱된 단일 스프라이트
    private SpriteAtlas characterSprite;    // 캐싱된 스프라이트 아틀라스 (메모리 절약용)

    /// <summary> 유틸 규칙에 정의된 경로로 스프라이트 아틀라스 전체 경로를 생성합니다. </summary>
    private string GetSpriteName => string.Format(Util.CHARACTER_SPRITE_PATH, modelSpriteName);

    public override int GetID() => id;

    /// <summary>
    /// [비동기] 캐릭터 스프라이트를 안전하게 가져옵니다. 메모리에 없다면 즉시 로드합니다.
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
    /// [비동기] Addressables를 통해 캐릭터 모델(3D/2D Prefab) 원본 에셋을 로드하고 캐싱합니다.
    /// </summary>
    public async UniTask<GameObject> GetModleObject()
    {
        return await GameMaster.Instance.addressableManager.LoadAssetAndCacheAsync<GameObject>(modelObjectName);
    }

    /// <summary>
    /// [비동기] 캐릭터 아틀라스에서 특정 스프라이트를 추출하여 UI 이미지에 즉시 적용하거나 콜백을 보냅니다.
    /// </summary>
    /// <param name="loadImage">로드 완료 후 호출될 콜백</param>
    /// <param name="targetImage">이미지를 바로 할당할 UI Image 컴포넌트</param>
    public async UniTask GetCharacterSprite(Action<Sprite> loadImage = null, Image targetImage = null)
    {
        if (characterSprite == null)
            await LoadSprite();

        // 아틀라스에서 프로젝트 규칙(Util)에 맞는 이름으로 스프라이트를 꺼내옴
        Sprite sprite = characterSprite.GetSprite(Util.CHARACTER_IMAGE_NAME);

        if (targetImage != null)
            targetImage.sprite = sprite;

        if (loadImage != null)
            loadImage.Invoke(sprite);
    }

    /// <summary>
    /// [비동기] 어드레서블 매니저를 호출하여 실제 아틀라스 리소스를 메모리에 올립니다.
    /// </summary>
    public async UniTask LoadSprite()
    {
        if (characterSprite != null) return; // 이미 로드되어 있다면 중단

        SpriteAtlas atlas = await GameMaster.Instance.addressableManager.LoadAssetAndCacheAsync<SpriteAtlas>(GetSpriteName);
        characterSprite = atlas;
    }

    /// <summary>
    /// 참조 카운트를 관리하여 사용하지 않는 아틀라스 리소스를 메모리에서 완전히 해제합니다.
    /// </summary>
    public void UnloadAtlas()
    {
        if (characterSprite == null) return;

        GameMaster.Instance.addressableManager.UnloadAsset(GetSpriteName);
        characterSprite = null;
    }
}

/// <summary>
/// [Registry/Manager] CSV로부터 로드된 모든 캐릭터 데이터 목록을 보관하고 검색을 담당하는 컬렉션입니다.
/// </summary>
public class CharacterDataList : CSVDataList<CharacterData>
{
    /// <summary> 특정 ID를 가진 캐릭터의 이름을 딕셔너리에서 검색하여 반환합니다. </summary>
    public string GetName(int id)
    {
        return m_dataList.ContainsKey(id) ? m_dataList[id].characterName : string.Empty;
    }

    /// <summary>
    /// [비동기] 특정 ID 캐릭터의 스프라이트 리소스를 로드하고 반환합니다.
    /// </summary>
    public async UniTask<Sprite> GetSpriteAsync(int id)
    {
        if (m_dataList.ContainsKey(id))
            return await m_dataList[id].GetSpriteAsync();

        return null;
    }

    /// <summary> 딕셔너리에 담긴 모든 캐릭터 데이터를 리스트로 변환하여 반환합니다. </summary>
    public List<CharacterData> GetAllList()
    {
        return m_dataList.Values.ToList();
    }

    /// <summary> 모든 캐릭터 데이터가 포함된 기본 리스트 객체를 생성합니다. </summary>
    public List<CharacterData> GetDefaultList()
    {
        List<CharacterData> datas = new();
        foreach (var id in m_dataList.Keys)
            datas.Add(m_dataList[id]);

        return datas;
    }

    /// <summary>
    /// [병렬 비동기 최적화] 등록된 모든 캐릭터의 스프라이트 리소스를 한꺼번에 로드합니다.
    /// 게임 시작 시 로딩 화면 등에서 프리로딩(Pre-loading) 용도로 사용하기 적합합니다.
    /// </summary>
    public async UniTask LoadAllCharacterSprite()
    {
        var allData = GetAllList();
        List<UniTask> taskList = new();

        foreach (var data in allData)
        {
            // 각 캐릭터의 로딩 작업을 리스트에 추가 (병렬 실행 준비)
            taskList.Add(data.LoadSprite());
        }

        // 모든 로딩 작업이 동시에 완료될 때까지 대기하여 시간 단축
        await UniTask.WhenAll(taskList);
    }
}