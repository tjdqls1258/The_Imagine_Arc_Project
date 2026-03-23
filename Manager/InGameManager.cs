using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.U2D; // SpriteAtlas 사용

/// <summary>
/// 인게임 플레이의 핵심 로직을 총괄하는 매니저 클래스입니다.
/// 맵 생성, 코스트(자원) 관리, 적 스폰 제어 및 승패 판정(게임 오버/클리어)을 담당합니다.
/// </summary>
public class InGameManager : MonoBehaviour
{
    // ====== Editor References & Managers ======
    [Header("Map References")]
    [Tooltip("비동기로 생성된 맵 타일들이 배치될 부모 GameObject")]
    [SerializeField] private GameObject m_mapObject;

    // ====== Game State Data ======

    /// <summary> 현재 유저가 보유한 인게임 재화(코스트) 수치입니다. 타워 배치/업그레이드에 사용됩니다. </summary>
    public int currentCost { get; private set; } = 0;

    private float m_costAddTime = 1; // 코스트가 1씩 차오르는 주기 (초)
    private float m_currentTime = 0; // 코스트 증가를 계산하기 위한 내부 타이머 누적값
    private bool m_isStartGame = false; // 게임이 본격적으로 시작되었는지 여부 플래그
    private int m_arriveCount = 0;   // 목표 지점(본진)에 도달한 적의 수 (라이프 차감 기준)

    // 플레이어 및 적 관련 기본 스탯 데이터 (외부에서 주입되며 읽기 전용으로 활용)
    public PlayerData PlayerData { get; private set; }
    public EnemyData EnemyData { get; private set; }

    [Header("Character Selection")]
    [Tooltip("현재 스테이지에서 유저가 사용할 수 있도록 편성된 캐릭터 덱 배열")]
    private CharacterData[] m_currentCharaterArray;
    [SerializeField] private CharacterData currentSeletData; // 유저가 배치를 위해 현재 선택(포커싱)한 캐릭터 데이터

    // ====== Map Loading State & Constants ======

    private const string MAP_DATAPATH_FORMAT = "MapData-{0}-{1}"; // Addressable 에셋 검색용 포맷 규칙 (MapData-{Main}-{Sub})
    private MapData m_mapData; // 비동기로 로드되어 캐싱된 맵 데이터(SO 또는 JSON 등) 원본

    private string _currentStageKey; // 현재 플레이 중인 맵 데이터의 Addressable 키값 보관용
    private string _stageAtlasKey;   // 현재 맵에서 사용하는 스프라이트 아틀라스의 Addressable 키값 보관용

    // 맵 리소스가 들어있는 Addressables 폴더/그룹 경로 상수
    private const string MAP_DATA_FOLDER = "MapData";
    private const string SPRITE_ATLAS_FOLDER = "SpriteAltas";

    private EnemySpawnManager m_enemySpawnManager; // 적 웨이브 출현을 제어하는 매니저
    private Action<int> m_chargeCostAction;        // 코스트 변동 시 UI를 갱신하기 위해 등록되는 콜백 (Event 역할)

    // ----------------------------------------------------------------------
    // ## Initialization (초기화 및 로드)
    // ----------------------------------------------------------------------

    private void Awake()
    {
        // 1. 유저가 선택한 스테이지 정보를 싱글톤에서 가져와 맵 로딩 파이프라인 시작
        LoadMapDataAsync(GameData.Instance.MainStage, GameData.Instance.SubStage).Forget();

        // 2. 같은 오브젝트에 부착된 적 스폰 매니저 캐싱
        m_enemySpawnManager = GetComponent<EnemySpawnManager>();
    }

    private void LateUpdate()
    {
        // 게임이 시작되지 않았거나, 코스트가 최대치(99)에 도달했으면 계산 중지
        if (m_isStartGame == false || currentCost > 99) return;

        // 시간에 따른 자연 코스트 회복 로직
        m_currentTime += Time.deltaTime;
        if (m_currentTime > m_costAddTime)
        {
            m_currentTime = 0;
            UpdateCost(1); // 1 코스트 획득
        }
    }

    // ----------------------------------------------------------------------
    // ## Map Loading (비동기 리소스/오브젝트 파이프라인)
    // ----------------------------------------------------------------------

    /// <summary>
    /// [비동기] 지정된 스테이지 정보에 따라 맵 데이터와 아틀라스를 로드하고, 모든 타일을 월드에 생성합니다.
    /// </summary>
    /// <param name="mainStage">메인 스테이지 챕터 번호</param>
    /// <param name="subStage">해당 챕터의 세부 스테이지 번호</param>
    public async UniTask LoadMapDataAsync(int mainStage, int subStage)
    {
        // 1. Addressable 시스템에 질의할 키 조합 생성
        _currentStageKey = string.Format(MAP_DATAPATH_FORMAT, mainStage, subStage);
        _stageAtlasKey = $"Stage-{mainStage}-{subStage}";

        string mapDataAddress = $"{MAP_DATA_FOLDER}/{_currentStageKey}.asset";
        string atlasAddress = $"{SPRITE_ATLAS_FOLDER}/{_stageAtlasKey}.spriteatlas";

        // 2. 맵 데이터 객체와 텍스쳐 아틀라스를 비동기 병렬 로드 및 메모리 캐싱
        var mapDataTask = GameMaster.Instance.addressableManager.LoadAssetAndCacheAsync<MapData>(mapDataAddress);
        var atlasTask = GameMaster.Instance.addressableManager.LoadAssetAndCacheAsync<SpriteAtlas>(atlasAddress);

        m_mapData = await mapDataTask;
        SpriteAtlas spAtlas = await atlasTask;

        // 예외 처리: 리소스 로드 실패 시 게임 진행 불가
        if (m_mapData == null || spAtlas == null)
        {
            Logger.LogError($"Failed to load MapData or SpriteAtlas for Stage {mainStage}-{subStage}.");
            return;
        }

        // 3. 맵 데이터에 정의된 타일들을 생성하기 위한 비동기 작업 리스트 준비
        List<UniTask> tileCreationTasks = new();

        foreach (var data in m_mapData.tileDatas)
        {
            // 'Delete' 타입은 맵 툴에서 지워진 타일이므로 무시
            if (data.type == MapData.MapObject.Delete) continue;

            // 각 타일을 비동기로 인스턴스화하는 작업을 리스트에 적재
            tileCreationTasks.Add(TileCreationTaskHelper(data, spAtlas));
        }

        // 4. 모든 타일 오브젝트가 한꺼번에 생성 완료될 때까지 대기 (로딩 멈춤 현상 최소화)
        await UniTask.WhenAll(tileCreationTasks);

        // 5. 생성된 맵의 최대 X, Y 범위를 계산하여 카메라 뷰포트 자동 조절 (정중앙 포커싱 및 크기 맞춤)
        var tileMaxX = m_mapData.tileDatas.Max(t => t.x);
        var tileMaxY = m_mapData.tileDatas.Max(t => t.y);

        GameUtil.mainCamera.transform.position = new Vector3(tileMaxX * 0.5f, tileMaxY * 0.5f, -10);
        GameUtil.mainCamera.orthographicSize = System.Math.Max(tileMaxX + 1, tileMaxY + 1) * 0.5f;

        // UI에서 유닛을 해제했을 때 돌아갈 기본 카메라 위치 저장
        GameData.Instance.DefaulteCameraPos = GameUtil.mainCamera.transform.position;

        // 6. 맵 로딩이 모두 완료되었으므로, 적 스폰 매니저에 웨이브 정보 및 콜백 주입 후 스폰 시작
        m_enemySpawnManager.SetEnemyData(m_mapData.enemySpawnDatas, m_mapData.pathDatas, EnemyDieAction, EnemyArriveAction);
        m_enemySpawnManager.StartSpawn();
    }

    /// <summary>
    /// 개별 타일 객체를 비동기로 인스턴스화하고, 초기 데이터를 주입하는 헬퍼 함수입니다.
    /// </summary>
    private async UniTask TileCreationTaskHelper(MapData.TileData tileData, SpriteAtlas spAtlas)
    {
        // 타일 타입이 지정되지 않은 경우(None) 기본 'Wall' 프리팹 경로를 사용
        string prefabName = tileData.type == MapData.MapObject.None
            ? MapData.MapObject.Wall.ToString()
            : tileData.type.ToString();

        string tileAddress = $"Tile/{prefabName}.prefab";

        // Addressables를 통해 해당 타일 프리팹을 로드하고 게임 월드(m_mapObject 하위)에 배치
        var tileObject = await GameMaster.Instance.addressableManager.InstantiateComponentAsync<TileBase>(tileAddress, m_mapObject.transform);

        if (tileObject != null)
        {
            // 타일의 좌표 정보와 시각적 요소(아틀라스에서 잘라온 스프라이트) 초기 설정
            tileObject.Init(tileData);
            tileObject.SetTileSprite(spAtlas.GetSprite(tileData.spriteName));
        }
    }

    // ----------------------------------------------------------------------
    // ## Game Logic & Rules (재화 및 승패 판정)
    // ----------------------------------------------------------------------

    /// <summary> 코스트 획득/소모 시 실행될 외부 UI 갱신 이벤트 등을 등록합니다. </summary>
    public void SetChargeAction(Action<int> charge)
    {
        m_chargeCostAction = charge;
    }

    /// <summary> 인게임 플레이어 버프 및 적 스탯 데이터 원본을 주입합니다. </summary>
    public void SetData(PlayerData playerData, EnemyData enemyData)
    {
        PlayerData = playerData;
        EnemyData = enemyData;
    }

    /// <summary> 컷신이나 로딩 완료 후 본격적인 게임 타이머 및 로직을 가동합니다. </summary>
    public void StartGame()
    {
        m_isStartGame = true;
    }

    /// <summary>
    /// 타워 건설이나 업그레이드 시 코스트를 지불합니다.
    /// </summary>
    /// <returns>가진 코스트가 충분하여 지불에 성공하면 true, 부족하면 false</returns>
    public bool UseCost(int cost)
    {
        if (currentCost < cost)
            return false;

        UpdateCost(-cost); // 성공 시 음수값 전달하여 차감
        return true;
    }

    /// <summary> 내부적으로 코스트 수치를 변동시키고, 등록된 이벤트 리스너(UI 등)에 알립니다. </summary>
    private void UpdateCost(int cost)
    {
        if (cost == 0) return;
        currentCost += cost;

        if (m_chargeCostAction != null)
            m_chargeCostAction.Invoke(currentCost);
    }

    /// <summary> 적 유닛이 유저의 공격에 의해 파괴(사망)했을 때 호출되는 콜백입니다. </summary>
    private void EnemyDieAction()
    {
        m_enemySpawnManager.EnemyDie(); // 스폰 매니저에 사망 사실 통보 (웨이브 카운트 갱신 등)

        // 화면 상의 적이 모두 죽었고 남은 스폰 예정 웨이브도 없다면 -> [Stage Clear]
        if (m_enemySpawnManager.GetCurrentCount() <= 0)
        {
            GameMaster.Instance.uiManager.AutoUIManager.GetCompoent<InGameUIManager>(UIBaseData.UIType.InGameUI).EndGame(true);
            Logger.Log("Enemy Clear");
        }
    }

    /// <summary> 적 유닛이 유저의 방어선을 뚫고 목표 지점(본진)에 도착했을 때 호출되는 콜백입니다. </summary>
    private void EnemyArriveAction()
    {
        m_arriveCount++; // 도달한 적 카운트 누적

        // 도달한 적의 수가 맵에 설정된 최대 라이프 수명을 넘었다면 -> [Game Over]
        if (m_mapData.m_life <= m_arriveCount)
        {
            GameMaster.Instance.uiManager.AutoUIManager.GetCompoent<InGameUIManager>(UIBaseData.UIType.InGameUI).EndGame(false);
        }
        else
        {
            // 라이프가 남았다면 일단 해당 적 유닛만 화면에서 제거
            m_enemySpawnManager.EnemyDie();
        }

        Logger.Log("Enemy Arrive");
    }

    // ----------------------------------------------------------------------
    // ## Game Exit & Cleanup (메모리 해제)
    // ----------------------------------------------------------------------

    /// <summary>
    /// 게임(스테이지) 종료 시 로드했던 무거운 Addressables 에셋을 메모리에서 해제하여 누수(Leak)를 방지합니다.
    /// </summary>
    public void ExitGame()
    {
        // 맵 데이터 (.asset) 해제
        GameMaster.Instance.addressableManager.UnloadAsset($"{MAP_DATA_FOLDER}/{_currentStageKey}.asset");
        // 스프라이트 아틀라스 (.spriteatlas) 해제
        GameMaster.Instance.addressableManager.UnloadAsset($"{SPRITE_ATLAS_FOLDER}/{_stageAtlasKey}.spriteatlas");

        m_isStartGame = false;
        currentCost = 0; // 다음 판을 위해 코스트 초기화
    }
}