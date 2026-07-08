using Cysharp.Threading.Tasks;
using NetExcute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;
using VContainer;

public enum SpawnType
{
    Up,
    Path
}

public class InGameCharacterData : BaseCharacterStat
{
    private NetExcute.UserCharacterData userCharacterDatas;
    protected int upgradeCount;

    public CharacterData characterData;
    public SkillBase nomalAtkSkill;
    public SkillBase passive;
    public SkillBase activeSkill;

    public InGameCharacterData(CharacterData data, NetExcute.UserCharacterData userCharacterData,SkillBase nomalAtk = null, SkillBase passive = null, SkillBase active = null) : base(data.characterState)
    {
        characterData = data;
        userCharacterDatas = userCharacterData;
        upgradeCount = 0;

        nomalAtkSkill = nomalAtk;
        this.passive = passive;
        activeSkill = active;

        ApplyLobbyGrowth();
    }

    private void ApplyLobbyGrowth()
    {
        // 예: "Attacker_Level" 같은 CSV 키값을 생성 (SpawnType이나 클래스 기반)
        //string growthID = $"{characterData.spawnType}_Level";
        //GrowthData growth = DataManager.Instance.GetGrowthData(growthID);

        //if (growth != null)
        //{
        //    int levelUps = userCharacterDatas.level - 1;

        //    if (levelUps > 0)
        //    {
        //        AddStat(StatType.MaxHp, growth.MaxHpAdd * levelUps);
        //        AddStat(StatType.AtkPower, growth.AtkPowerAdd * levelUps);
        //        AddStat(StatType.DefPower, growth.DefPowerAdd * levelUps);
        //    }

        //    // TODO: Enforce(강화)나 Rank(진화)에 따른 추가 스탯 보너스도 여기서 AddStat으로 처리
        //}
    }

    public void UpgradeCharacter(int count = 1)
    {
        upgradeCount += count;

        //string upgradeID = $"{characterData.spawnType}_Upgrade";
        //GrowthData growth = DataManager.Instance.GetGrowthData(upgradeID);

        //if (growth != null)
        //{
        //    // 업그레이드 횟수(count)만큼 CSV 수치를 더함.
        //    AddStat(StatType.MaxHp, growth.MaxHpAdd * count);
        //    AddStat(StatType.AttackDamage, growth.AtkPowerAdd * count);
        //    AddStat(StatType.Defense, growth.DefPowerAdd * count);
        //AddStat(StatType.AttackSpeed, growth.AtkSpeedAdd * count);
        //}
    }

    public float GetAtk()
    {
        return StatCalculator.Calculate(characterData.characterState.atkPower, userCharacterDatas.level, userCharacterDatas.Enforce, userCharacterDatas.Rank, upgradeCount);
    }
}

public class CharacterData : CSVData
{
    [Header("Basic Stats")]
    public int id;
    public int cost;
    public int rating;
    public string characterName;
    public CharacterState characterState;

    public int nomalAtk;
    public int[] passiveSkill;
    public int[] activeSkill;

    [Header("Resource Keys")]
    public string modelObjectName;
    public string modelSpriteName;

    public SpawnType spawnType;

    public int blockCount = 1;

    public string timelineKey = Util.DEFAULT_TIMELINE_PATH;

    [Header("Runtime State (Caching)")]
    private Sprite modelSprite;
    private SpriteAtlas characterSprite;

    private string GetSpriteName => string.Format(Util.CHARACTER_SPRITE_PATH, modelSpriteName);

    public override int GetID() => id;

    public async UniTask<Sprite> GetSpriteAsync(AddressableManager addressableManagerm)
    {
        if (modelSprite == null)
        {
            await LoadSprite(addressableManagerm);
        }
        return modelSprite;
    }

    public async UniTask<GameObject> GetModleObject(AddressableManager addressableManager)
    {
        return await addressableManager.LoadAssetAndCacheAsync<GameObject>(modelObjectName);
    }

    public async UniTask GetCharacterSprite(AddressableManager addressableManagerm, Action<Sprite> loadImage = null, Image targetImage = null)
    {
        if (characterSprite == null)
            await LoadSprite(addressableManagerm);

        Sprite sprite = characterSprite.GetSprite(Util.CHARACTER_IMAGE_NAME);

        if (targetImage != null)
            targetImage.sprite = sprite;

        if (loadImage != null)
            loadImage.Invoke(sprite);
    }

    public async UniTask GetCharacterSpriteFace(AddressableManager addressableManagerm, Action<Sprite> loadImage = null, Image targetImage = null)
    {
        if (characterSprite == null)
            await LoadSprite(addressableManagerm);

        Sprite sprite = characterSprite.GetSprite(Util.CHARACTERFACE_IMAGE_NAME);

        if (targetImage != null)
            targetImage.sprite = sprite;

        if (loadImage != null)
            loadImage.Invoke(sprite);
    }

    public async UniTask LoadSprite(AddressableManager addressableManager)
    {
        if (characterSprite != null) return;

        SpriteAtlas atlas = await addressableManager.LoadAssetAndCacheAsync<SpriteAtlas>(GetSpriteName);
        characterSprite = atlas;
        
        Sprite model = characterSprite.GetSprite(Util.CHARACTER_IMAGE_NAME);
        modelSprite = model;
    }

    public void UnloadAtlas(AddressableManager addressableManager)
    {
        if (characterSprite == null) return;

        addressableManager.UnloadAsset(GetSpriteName);
        characterSprite = null;
    }
}

public class CharacterDataList : CSVDataList<CharacterData>
{
    public string GetName(int id)
    {
        return m_dataList.ContainsKey(id) ? m_dataList[id].characterName : string.Empty;
    }

    public async UniTask<Sprite> GetSpriteAsync(int id, AddressableManager addressableManagerm)
    {
        if (m_dataList.ContainsKey(id))
            return await m_dataList[id].GetSpriteAsync(addressableManagerm);

        return null;
    }

    public List<CharacterData> GetAllList()
    {
        return m_dataList.Values.ToList();
    }

    public List<CharacterData> GetDefaultList()
    {
        List<CharacterData> datas = new();
        foreach (var id in m_dataList.Keys)
            datas.Add(m_dataList[id]);

        return datas;
    }

    public async UniTask LoadAllCharacterSprite(AddressableManager addressableManager)
    {
        var allData = GetAllList();
        List<UniTask> taskList = new();

        foreach (var data in allData)
        {
            taskList.Add(data.LoadSprite(addressableManager));
        }

        await UniTask.WhenAll(taskList);
    }
}