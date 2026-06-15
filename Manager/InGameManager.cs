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

public class InGameManager : MonoBehaviour
{
    [Header("Map References")]
    [Tooltip("비동기로 생성된 맵 타일들이 배치될 부모 GameObject")]
    [SerializeField] private GameObject m_mapObject;
    [Inject] private readonly AddressableManager addressableManager;
    [Inject] private readonly UIManager uiManager;

    private string m_currentStageKey; 
    private string m_stageAtlasKey;
    public StageRule stageRule { get; private set; }
    public GameGoodsSystem goodsSystem { get; private set; } = new();
    private StageLoader stageLoader;
    public UnityAction<bool, bool> dragCharacter = null;

    private void Awake()
    {
        GameUtil.InjectUtil(this);

        stageLoader = new(uiManager, addressableManager);
        stageRule = new(uiManager);
        LoadMapDataAsync(GameData.Instance.MainStage, GameData.Instance.SubStage).Forget();
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
    }
}