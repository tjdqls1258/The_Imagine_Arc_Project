using Cysharp.Threading.Tasks;
using NetExcute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEditor.U2D.Animation;
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

    protected GrowthManager growthManager;
    public CharacterData characterData;
    public SkillBase nomalAtkSkill;
    public SkillBase passive;
    public SkillBase activeSkill;

    public InGameCharacterData(CharacterData data, NetExcute.UserCharacterData userCharacterData, SkillBase nomalAtk = null, SkillBase passive = null, SkillBase active = null, GrowthManager growthManager = null) : base(data.characterState)
    {
        upgradeCount = 0;

        nomalAtkSkill = nomalAtk;
        this.passive = passive;
        activeSkill = active;

        characterData = data;
        userCharacterDatas = userCharacterData;

        InitializeStats();
    }

    private void InitializeStats()
    {
        GrowthData growth = growthManager.GetGrowthData(characterData.GrowthDataID);
        if (growth == null) return;

        int levelUps = userCharacterDatas.Level - 1;

        foreach (StatType type in Enum.GetValues(typeof(StatType)))
        {
            float levelRate = growth.GetValue(GrowthType.Level, type);
            float enforceRate = growth.GetValue(GrowthType.Enforce, type);

            float multiplier = (levelRate * levelUps) + enforceRate;

            if (multiplier > 0)
            {
                float baseVal = GetInitialBaseValue(type);
                this.AddStat(type, baseVal * multiplier);
            }
        }
    }

    private float GetInitialBaseValue(StatType type)
    {
        return type switch
        {
            StatType.MaxHp => characterData.characterState.maxHp,
            StatType.AttackDamage => characterData.characterState.atkPower,
            StatType.Defense => characterData.characterState.defPower,
            StatType.AttackRange => characterData.characterState.atkRang,
            StatType.AttackSpeed => characterData.characterState.atkSpeed,
            _ => 0f
        };
    }

    public void UpgradeCharacter(int count = 1)
    {
        upgradeCount += count;

        GrowthData growth = growthManager.GetGrowthData(characterData.GrowthDataID);

        if (growth != null)
        {
            foreach (StatType statType in Enum.GetValues(typeof(StatType)))
            {
                float rate = growth.GetValue(GrowthType.Upgrade, statType);

                if (rate <= 0) continue;

                float currentStat = GetStat(statType);
                float addValue = GrowthManager.CalculateAddedStat(currentStat, rate, count);

                AddStat(statType, addValue);
            }
        }
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
    public string GrowthDataID;

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