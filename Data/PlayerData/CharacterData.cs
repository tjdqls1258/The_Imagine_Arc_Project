using Cysharp.Threading.Tasks;
using NetExcute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

public class InGameCharacterData
{
    private NetExcute.UserCharacterData userCharacterDatas;
    protected int upgradeCount;

    public CharacterData characterData;
    public SkillBase passive;
    public SkillBase activeSkill;

    public InGameCharacterData(CharacterData data, NetExcute.UserCharacterData userCharacterData, SkillBase passive = null, SkillBase active = null)
    {
        characterData = data;
        userCharacterDatas = userCharacterData;
        upgradeCount = 0;

        if (passive.Type != SkillBase.SkillType.Passive)
            Debug.LogError($"{passive.ID} : {passive.name} Is Not Passive");
        else
            this.passive = passive;

        if (active.Type == SkillBase.SkillType.Passive)
            Debug.LogError($"{active.ID} : {active.name} Is Not Active");
        else
            activeSkill = active;
    }

    public float GetAtk()
    {
        return StatCalculator.Calculate(characterData.characterState.atkPower, userCharacterDatas.level, userCharacterDatas.Enforce, userCharacterDatas.Rank, upgradeCount);
    }

    public void UpgradeCharacter(int count)
    {
        upgradeCount = upgradeCount + count;
    }
}

public class CharacterData : CSVData
{
    [Header("Basic Stats")]
    public int id;
    public int cost;
    public int rating;
    public string characterName;
    public MpCharacterState characterState;

    public int[] passiveSkill;
    public int[] activeSkill;

    [Header("Resource Keys")]
    public string modelObjectName;
    public string modelSpriteName;

    [Header("Runtime State (Caching)")]
    private Sprite modelSprite;
    private SpriteAtlas characterSprite;

    private string GetSpriteName => string.Format(Util.CHARACTER_SPRITE_PATH, modelSpriteName);

    public override int GetID() => id;

    public async UniTask<Sprite> GetSpriteAsync()
    {
        if (modelSprite == null)
        {
            await LoadSprite();
        }
        return modelSprite;
    }

    public async UniTask<GameObject> GetModleObject()
    {
        return await GameMaster.Instance.addressableManager.LoadAssetAndCacheAsync<GameObject>(modelObjectName);
    }

    public async UniTask GetCharacterSprite(Action<Sprite> loadImage = null, Image targetImage = null)
    {
        if (characterSprite == null)
            await LoadSprite();

        Sprite sprite = characterSprite.GetSprite(Util.CHARACTER_IMAGE_NAME);

        if (targetImage != null)
            targetImage.sprite = sprite;

        if (loadImage != null)
            loadImage.Invoke(sprite);
    }

    public async UniTask LoadSprite()
    {
        if (characterSprite != null) return;

        SpriteAtlas atlas = await GameMaster.Instance.addressableManager.LoadAssetAndCacheAsync<SpriteAtlas>(GetSpriteName);
        characterSprite = atlas;
    }

    public void UnloadAtlas()
    {
        if (characterSprite == null) return;

        GameMaster.Instance.addressableManager.UnloadAsset(GetSpriteName);
        characterSprite = null;
    }
}

public class CharacterDataList : CSVDataList<CharacterData>
{
    public string GetName(int id)
    {
        return m_dataList.ContainsKey(id) ? m_dataList[id].characterName : string.Empty;
    }

    public async UniTask<Sprite> GetSpriteAsync(int id)
    {
        if (m_dataList.ContainsKey(id))
            return await m_dataList[id].GetSpriteAsync();

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

    public async UniTask LoadAllCharacterSprite()
    {
        var allData = GetAllList();
        List<UniTask> taskList = new();

        foreach (var data in allData)
        {
            taskList.Add(data.LoadSprite());
        }

        await UniTask.WhenAll(taskList);
    }
}