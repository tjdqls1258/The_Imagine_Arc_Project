using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.U2D; // SpriteAtlas 사용

/// <summary>
/// 인게임 플레이의 핵심 로직을 총괄하는 매니저 클래스입니다.
/// 맵 생성, 코스트(자원) 관리, 적 스폰 제어 및 승패 판정(게임 오버/클리어)을 담당합니다.
/// </summary>
public class InGameManager : MonoBehaviour
{
    [Header("Map References")]
    [Tooltip("비동기로 생성된 맵 타일들이 배치될 부모 GameObject")]
    [SerializeField] private GameObject m_mapObject;

    private bool m_isStartGame = false;

    private string _currentStageKey; 
    private string _stageAtlasKey;

    private StageRule stageRule;
    private GameGoodsSystem goodsSystem;
    private StageLoader stageLoader;

    private void Awake()
    {
        LoadMapDataAsync(GameData.Instance.MainStage, GameData.Instance.SubStage).Forget();
        stageRule = new();
        goodsSystem = new();
        stageLoader = new();
    }

    public int GetCurrentCost() => goodsSystem.currentCost;

    public async UniTask LoadMapDataAsync(int mainStage, int subStage)
    {
        _currentStageKey = string.Format(Util.MAP_DATAPATH_FORMAT, mainStage, subStage);
        _stageAtlasKey = string.Format(Util.STAGE_NAME, mainStage, subStage);

        string mapDataAddress = string.Format(Util.MAP_DATA_FOLDER, _currentStageKey);
        string atlasAddress = string.Format(Util.SPRITE_ATLAS_FOLDER, _stageAtlasKey);

        var mapDataTask = GameMaster.Instance.addressableManager.LoadAssetAndCacheAsync<MapData>(mapDataAddress);
        var atlasTask = GameMaster.Instance.addressableManager.LoadAssetAndCacheAsync<SpriteAtlas>(atlasAddress);

        var mapData = await mapDataTask.AttachExternalCancellation(destroyCancellationToken);
        SpriteAtlas spAtlas = await atlasTask.AttachExternalCancellation(destroyCancellationToken);

        if (mapData == null || spAtlas == null)
        {
            Logger.LogError($"Failed to load MapData or SpriteAtlas for Stage {mainStage}-{subStage}.");
            return;
        }

        stageRule.Init(GetComponent<EnemySpawnManager>(), mapData);
        goodsSystem.Init();
        await stageLoader.InitTile(mapData, spAtlas, destroyCancellationToken, m_mapObject.transform);
    }

    public void SetChargeAction(Action<int> charge)
    {
        goodsSystem.AddActionChangeGoods(charge);
    }

    public void StartGame()
    {
        m_isStartGame = true;
        stageRule.StartGame();
        goodsSystem.StartGame();
    }

    public bool UseCost(int cost) => goodsSystem.UseCost(cost);

    public void ExitGame()
    {
        // 맵 데이터 (.asset) 해제
        GameMaster.Instance.addressableManager.UnloadAsset(string.Format(Util.MAP_DATA_FOLDER, _currentStageKey));
        // 스프라이트 아틀라스 (.spriteatlas) 해제
        GameMaster.Instance.addressableManager.UnloadAsset(string.Format(Util.SPRITE_ATLAS_FOLDER, _stageAtlasKey));

        m_isStartGame = false;
        stageRule.Clear();
        goodsSystem.Clear();
    }
}