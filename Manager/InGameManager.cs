using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.U2D; // SpriteAtlas 사용

public class InGameManager : MonoBehaviour
{
    // ====== Editor References & Managers ======
    [Header("Map References")]
    [Tooltip("맵 타일들을 포함할 부모 GameObject")]
    [SerializeField] private GameObject m_mapObject;

    // ====== Game Data ======
    public int currentCost { get; private set; } = 0;
    private float m_costAddTime = 1;
    private float m_currentTime = 0;
    private bool m_isStartGame = false;
    private int m_arriveCount = 0;

    // 플레이어 및 적 데이터는 외부에서 설정(SetData)하며, 읽기 전용으로 접근 가능
    public PlayerData PlayerData { get; private set; }
    public EnemyData EnemyData { get; private set; }

    [Header("Character Selection")]
    [Tooltip("현재 스테이지에서 사용 가능한 캐릭터 데이터 배열")]
    private CharacterData[] m_currentCharaterArray;
    [SerializeField] private CharacterData currentSeletData; // 현재 선택된 캐릭터 데이터

    // ====== Map Loading State & Constants ======

    private const string MAP_DATAPATH_FORMAT = "MapData-{0}-{1}"; // MapData-{Main}-{Sub} 형식
    private MapData m_mapData; // 로드된 맵 데이터 객체

    private string _currentStageKey; // 현재 로드된 맵 데이터의 Addressable 키
    private string _stageAtlasKey;   // 현재 로드된 스프라이트 아틀라스의 Addressable 키

    // 맵 데이터 Addressables 경로 상수
    private const string MAP_DATA_FOLDER = "MapData";
    private const string SPRITE_ATLAS_FOLDER = "SpriteAltas";

    private EnemySpawnManager m_enemySpawnManager;
    private Action<int> m_chargeCostAction;

    private void Awake()
    {
        // 게임 데이터 싱글톤에서 현재 스테이지 정보를 가져와 맵 로드 시작
        LoadMapDataAsync(GameData.Instance.MainStage, GameData.Instance.SubStage).Forget();
        m_enemySpawnManager = GetComponent<EnemySpawnManager>();  
    }

    private void LateUpdate()
    {
        if (m_isStartGame == false || currentCost > 99) return;

        m_currentTime += Time.deltaTime;
        if (m_currentTime > m_costAddTime)
        {
            m_currentTime = 0;
            UpdateCost(1);
        }
    }

    // ----------------------------------------------------------------------
    // ## Map Loading
    // ----------------------------------------------------------------------

    /// <summary>
    /// 지정된 스테이지 정보에 따라 맵 데이터와 스프라이트 아틀라스를 비동기로 로드하고, 타일을 생성합니다.
    /// </summary>
    /// <param name="mainStage">메인 스테이지 번호</param>
    /// <param name="subStage">서브 스테이지 번호</param>
    public async UniTask LoadMapDataAsync(int mainStage, int subStage)
    {
        // 1. Addressable 키 생성
        _currentStageKey = string.Format(MAP_DATAPATH_FORMAT, mainStage, subStage);
        _stageAtlasKey = $"Stage-{mainStage}-{subStage}";

        string mapDataAddress = $"{MAP_DATA_FOLDER}/{_currentStageKey}.asset";
        string atlasAddress = $"{SPRITE_ATLAS_FOLDER}/{_stageAtlasKey}.spriteatlas";

        // 2. 맵 데이터 및 아틀라스 비동기 로드 및 캐싱
        var mapDataTask = AddressableManager.Instance.LoadAssetAndCacheAsync<MapData>(mapDataAddress);
        var atlasTask = AddressableManager.Instance.LoadAssetAndCacheAsync<SpriteAtlas>(atlasAddress);

        m_mapData = await mapDataTask;
        SpriteAtlas spAtlas = await atlasTask;

        if (m_mapData == null || spAtlas == null)
        {
            Logger.LogError($"Failed to load MapData or SpriteAtlas for Stage {mainStage}-{subStage}.");
            return;
        }

        // 3. 타일 생성 및 초기화 UniTask 리스트 구성
        List<UniTask> tileCreationTasks = new();

        foreach (var data in m_mapData.tileDatas)
        {
            // 'Delete' 타입 타일은 건너뜁니다.
            if (data.type == MapData.MapObject.Delete) continue;

            // 각 타일 데이터에 대해 비동기 생성 및 초기화 작업을 리스트에 추가
            tileCreationTasks.Add(TileCreationTaskHelper(data, spAtlas));
        }

        // 모든 타일 생성 작업이 완료될 때까지 대기
        await UniTask.WhenAll(tileCreationTasks);

        // 카메라 위치, 사이즈 조정
        var tileMaxX = m_mapData.tileDatas.Max(t => t.x);
        var tileMaxY = m_mapData.tileDatas.Max(t => t.y);
        
        GameUtil.mainCamera.transform.position = new Vector3(tileMaxX * 0.5f, tileMaxY * 0.5f, -10);
        GameUtil.mainCamera.orthographicSize =  System.Math.Max(tileMaxX + 1, tileMaxY + 1) * 0.5f;

        GameData.Instance.DefaulteCameraPos = GameUtil.mainCamera.transform.position;

        m_enemySpawnManager.SetEnemyData(m_mapData.enemySpawnDatas, m_mapData.pathDatas, EnemyDieAction, EnemyArriveAction);
        m_enemySpawnManager.StartSpawn();
    }

    /// <summary>
    /// 개별 타일 객체를 비동기로 인스턴스화하고 초기 데이터를 설정하는 헬퍼 함수입니다.
    /// </summary>
    private async UniTask TileCreationTaskHelper(MapData.TileData tileData, SpriteAtlas spAtlas)
    {
        // MapObject.None 타입일 경우 기본 'Wall' 프리팹을 사용하도록 대체 (원본 로직 유지)
        string prefabName = tileData.type == MapData.MapObject.None
            ? MapData.MapObject.Wall.ToString()
            : tileData.type.ToString();

        string tileAddress = $"Tile/{prefabName}.prefab";

        // Addressables를 사용하여 타일 프리팹을 로드하고 TileBase 컴포넌트를 가져옵니다.
        var tileObject = await AddressableManager.Instance.InstantiateComponentAsync<TileBase>(tileAddress, m_mapObject.transform);

        if (tileObject != null)
        {
            // 타일 데이터 및 스프라이트 설정
            tileObject.Init(tileData);
            tileObject.SetTileSprite(spAtlas.GetSprite(tileData.spriteName));
        }
    }

    // ----------------------------------------------------------------------
    // ## Game Data Management
    // ----------------------------------------------------------------------

    public void SetChargeAction(Action<int> charge)
    {
        m_chargeCostAction = charge;
    }

    /// <summary>
    /// 인게임에 필요한 플레이어 및 적 데이터를 설정합니다.
    /// </summary>
    public void SetData(PlayerData playerData, EnemyData enemyData)
    {
        PlayerData = playerData;
        EnemyData = enemyData;
    }

    public void StartGame()
    {
        m_isStartGame = true;
    }

    public bool UseCost(int cost)
    {
        if (currentCost < cost)
            return false;

        UpdateCost(-cost);
        return true;
    }

    private void UpdateCost(int cost)
    {
        currentCost += cost;

        if (m_chargeCostAction != null)
            m_chargeCostAction.Invoke(currentCost);
    }

    private void EnemyDieAction()
    {
        m_enemySpawnManager.EnemyDie();

        if (m_enemySpawnManager.GetCurrentCount() <= 0)
        {
            GameMaster.Instance.uiManager.AutoUIManager.GetCompoent<InGameUIManager>(UIBaseData.UIType.InGameUI).EndGame(true);
            Logger.Log("Enemy Clear");
        }
    }

    private void EnemyArriveAction()
    {
        m_arriveCount++;

        if (m_mapData.m_life <= m_arriveCount)
            GameMaster.Instance.uiManager.AutoUIManager.GetCompoent<InGameUIManager>(UIBaseData.UIType.InGameUI).EndGame(false);
        else
            m_enemySpawnManager.EnemyDie();

        Logger.Log("Enemy Arrive");
    }

    // ----------------------------------------------------------------------
    // ## Game Exit & Cleanup
    // ----------------------------------------------------------------------

    /// <summary>
    /// 게임(스테이지) 종료 시 로드했던 Addressables 에셋을 메모리에서 해제합니다.
    /// </summary>
    public void ExitGame()
    {
        // 맵 데이터 (.asset) 해제
        AddressableManager.Instance.UnloadAsset($"{MAP_DATA_FOLDER}/{_currentStageKey}.asset");
        // 스프라이트 아틀라스 (.spriteatlas) 해제
        AddressableManager.Instance.UnloadAsset($"{SPRITE_ATLAS_FOLDER}/{_stageAtlasKey}.spriteatlas");

        m_isStartGame = false;
        currentCost = 0;
    }
}