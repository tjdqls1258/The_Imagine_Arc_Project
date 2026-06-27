using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.VirtualTexturing;
using UnityEngine.U2D;
using VContainer;
using VContainer.Unity;

public class InGameManager : MonoBehaviour, ISkillCaster
{
    [Header("Map References")]
    [Tooltip("비동기로 생성된 맵 타일들이 배치될 부모 GameObject")]
    [SerializeField] private GameObject m_mapObject;
    [Inject] private readonly AddressableManager addressableManager;
    [Inject] private readonly UIManager uiManager;
    [Inject] private readonly UserDataManager userDataManager;

    private InGameTimeScaleManager gameTimeScaleManager = new();

    private string m_currentStageKey; 
    private string m_stageAtlasKey;
    public StageRule stageRule { get; private set; }
    public GameGoodsSystem goodsSystem { get; private set; } = new();
    private StageLoader stageLoader;
    public UnityAction<bool, bool> dragCharacter = null;
    public SkillContext userSkillcontext { get; private set; } = new();

    private void Awake()
    {
        GameUtil.InjectUtil(this);

        stageLoader = new(uiManager, addressableManager);
        stageRule = new(uiManager);
        LoadMapDataAsync(GameData.Instance.MainStage, GameData.Instance.SubStage).Forget();

        userSkillcontext.Caster = this;
        userSkillcontext.SkillRange = float.MaxValue;
        userSkillcontext.Damage = 0;
    }

    public async UniTask LoadMapDataAsync(int mainStage, int subStage)
    {
        m_currentStageKey = string.Format(Util.MAP_DATAPATH_FORMAT, mainStage, subStage);
        m_stageAtlasKey = string.Format(Util.STAGE_NAME, mainStage, subStage);

        string mapDataAddress = string.Format(Util.MAP_DATA_FOLDER, m_currentStageKey);
        string atlasAddress = string.Format(Util.SPRITE_ATLAS_FOLDER, m_stageAtlasKey);

        var mapDataTask = addressableManager.LoadAssetAndCacheAsync<MapData>(mapDataAddress);
        var atlasTask = addressableManager.LoadAssetAndCacheAsync<SpriteAtlas>(atlasAddress);

        var mapData = await mapDataTask.AttachExternalCancellation(destroyCancellationToken);
        SpriteAtlas spAtlas = await atlasTask.AttachExternalCancellation(destroyCancellationToken);

        if (mapData == null || spAtlas == null)
        {
            Logger.LogError($"Failed to load MapData or SpriteAtlas for Stage {mainStage}-{subStage}.");
            return;
        }

        stageRule.Init(GetComponent<EnemySpawnManager>(), mapData);
        goodsSystem.Init();
        await stageLoader.InitTile(mapData, spAtlas, destroyCancellationToken, m_mapObject.transform, this);

        StartGame();
    }

    public void StartGame()
    {
        gameTimeScaleManager.Init(GameUtil.GetGameTimeScale((userDataManager.GetUserData<UserGameSettingData>() as UserGameSettingData).userGameSettingOption.GameSpeedIndex));
        stageRule.StartGame();
        goodsSystem.StartGame();
        uiManager.GetAutoUIManager().GetCompoent<InGameUIManager>(UIBaseData.UIType.InGameUI).StartGame();
    }

    public bool UseCost(int cost) => goodsSystem.UseCost(cost);

    public void ExitGame()
    {
        addressableManager.UnloadAsset(string.Format(Util.MAP_DATA_FOLDER, m_currentStageKey));
        addressableManager.UnloadAsset(string.Format(Util.SPRITE_ATLAS_FOLDER, m_stageAtlasKey));

        stageRule.Clear();
        goodsSystem.Clear();
        gameTimeScaleManager.Dispose();
    }

    public Transform GetTransform() => transform;

    public int GetCasterID() => GetInstanceID();

    public UniTask<Sprite> GetCutsceneSpriteAsync(AddressableManager am)
    {
        return UniTask.FromResult<Sprite>(null); // 플레이어 스킬 연출 이미지 정해지면 교체
    }

    public string GetTimelineKey()
    {
        return string.Empty; // 플레이어 전용 스킬 연출 정해지면 교체
    }
}