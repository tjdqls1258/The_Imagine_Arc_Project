using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 스테이지의 적 생성(Spawn)과 오브젝트 풀링을 관리하는 클래스입니다.
/// 정해진 시간에 맞춰 적을 생성하고, 사망한 적을 재활용하여 메모리 부하를 줄입니다.
/// </summary>
public class EnemySpawnManager : MonoBehaviour
{
    // ====== Runtime Data ======
    private EnemySpawnData[] m_enemySpawnDatas; // 스폰될 적의 정보 및 시간 데이터 리스트
    private MapData.PathData[] m_pathData;         // 이동 경로(Path) 데이터 배열
    private int m_spawnCount = 0;                  // 현재까지 스폰된 적의 개수
    private float m_currentTime = 0;               // 스폰 시작 후 경과 시간

    private int m_totalCount = 0;
    private int m_remmantCount = 0;             // 남은 스폰 개수
    private int m_currentCount = 0;

    // ====== Object Pooling & Async ======
    private CancellationTokenSource m_cancellationTokenSource = new(); // 비동기 루프 취소용 토큰

    /// <summary> [활성 풀] 현재 필드에서 활동 중인 적 리스트 (Key: Enemy ID) </summary>
    private Dictionary<int, List<EnemyController>> m_enemyList = new();

    /// <summary> [비활성 풀] 사망 후 재활용 대기 중인 적 리스트 (Key: Enemy ID) </summary>
    private Dictionary<int, List<EnemyController>> m_disableList = new();

    private Dictionary<int, GameObject> m_enemyModelList = new();

    // ====== Action ======

    /// <summary> 몬스터 사망시 액션 </summary>
    private Action m_enemyDie;

    /// <summary> 몬스터 도착시 액션 </summary>
    private Action m_enemyArriveAction;

    // ----------------------------------------------------------------------
    // ## Initialization
    // ----------------------------------------------------------------------

    /// <summary>
    /// 스테이지 정보를 주입받아 스폰 준비를 합니다.
    /// </summary>
    public void SetEnemyData(EnemySpawnData[] data, MapData.PathData[] pathDatas, Action enemyDieAction, Action enemyArriveAction)
    {
        m_enemySpawnDatas = data;
        m_pathData = pathDatas;

        m_enemyDie = enemyDieAction;
        m_enemyArriveAction = enemyArriveAction;

        m_totalCount = m_enemySpawnDatas.Length;
        m_remmantCount = m_enemySpawnDatas.Length;
        m_currentCount = m_enemySpawnDatas.Length;
    }

    /// <summary>
    /// 외부에서 적 생성을 시작하도록 명령합니다.
    /// </summary>
    public void StartSpawn()
    {
        SpawnStart().Forget(); // 비동기 루프를 별도 대기 없이 실행
    }

    public int GetTotalCount()
    {
        return m_totalCount;
    }

    public void EnemyDie()
    {
        m_currentCount -= 1;
    }

    public int GetCurrentCount() => m_currentCount;

    // ----------------------------------------------------------------------
    // ## Spawn Logic (Async Pipeline)
    // ----------------------------------------------------------------------

    /// <summary>
    /// [비동기] 3초 대기 후 매 프레임 스폰 조건을 체크하는 메인 루프입니다.
    /// </summary>
    private async UniTask SpawnStart()
    {
        m_enemyModelList.Clear();
        float currentTime = Time.realtimeSinceStartup;

        foreach (var enemySpawnData in m_enemySpawnDatas)
        {
            if (m_enemyModelList.ContainsKey(enemySpawnData.enemyDataID))
                continue;
            var enemyData = GameMaster.Instance.csvHelper.GetScripteData<EnemyDataList>().GetData(enemySpawnData.enemyDataID);
            var obj = await GameMaster.Instance.addressableManager.LoadAssetAndCacheAsync<GameObject>(string.Format(Util.ENEMY_MODLED_PATH, enemyData.controllObjectKey)).AttachExternalCancellation(destroyCancellationToken);
            obj.gameObject.SetActive(false);
            m_enemyModelList.Add(enemySpawnData.enemyDataID, obj);
        }

        currentTime = Time.realtimeSinceStartup - currentTime;

        if (currentTime > 0)
        {
            // 1. 게임 시작 전 초기 대기 시간 (예: 준비 시간)
            await UniTask.WaitForSeconds(currentTime, cancellationToken: destroyCancellationToken);
        }

        // 2. 캔슬 토큰이 요청되기 전까지 무한 루프
        while (m_cancellationTokenSource.IsCancellationRequested == false)
        {
            // 물리 업데이트 타이밍 대기 (최적화)
            await UniTask.WaitForFixedUpdate(m_cancellationTokenSource.Token);

            m_currentTime += Time.fixedDeltaTime;
            SpawnEnemy();
        }
    }

    /// <summary>
    /// 경과 시간과 스폰 데이터를 비교하여 적 유닛을 생성 또는 풀에서 꺼내옵니다.
    /// </summary>
    private void SpawnEnemy()
    {
        // 모든 적 스폰이 완료되었다면 루프 종료
        if (m_spawnCount >= m_enemySpawnDatas.Length)
        {
            m_cancellationTokenSource.Cancel();
            return;
        }

        // 현재 경과 시간이 다음 적의 스폰 타임에 도달했는지 체크
        if (m_currentTime >= m_enemySpawnDatas[m_spawnCount].spawnTime)
        {
            EnemyController obj;
            int id = m_enemySpawnDatas[m_spawnCount].enemyDataID;

            m_remmantCount -= 1;

            // --- 오브젝트 풀링 로직 ---
            // 1. 비활성 풀(재활용 리스트)에 해당 ID의 적이 있는지 확인
            if (m_disableList.ContainsKey(id) && m_disableList[id].Count > 0)
            {
                obj = m_disableList[id].First();
                m_disableList[id].Remove(obj);

                // 활성 리스트에 추가 (딕셔너리 키 검사 포함)
                if (!m_enemyList.ContainsKey(id)) m_enemyList.Add(id, new());
                m_enemyList[id].Add(obj);
            }
            // 2. 풀에 없다면 새로 생성 (Instantiate)
            else
            {
                obj = Instantiate(m_enemyModelList[id]).GetComponent<EnemyController>();
                if (m_enemyList.ContainsKey(id) == false)
                {
                    m_enemyList.Add(id, new());
                }
                m_enemyList[id].Add(obj);
            }

            // --- 경로 및 데이터 초기화 ---
            var pathindex = m_enemySpawnDatas[m_spawnCount].pathIndex;
            var pathData = m_pathData.FirstOrDefault(x => x.index == pathindex);

            // 해당 인덱스의 경로가 있다면 적용, 없다면 0번 경로를 기본으로 사용
            if (pathData != null)
            {
                var vectorList = GameUtil.ConvartSerializableVector2IntToVector2Int_List(pathData.path);
                obj.InitEnemyData(GameMaster.Instance.csvHelper.GetScripteData<EnemyDataList>().GetData(m_enemySpawnDatas[m_spawnCount].enemyDataID), vectorList, DieAction, m_enemyDie, m_enemyArriveAction);
            }
            else
            {
                var vectorList = GameUtil.ConvartSerializableVector2IntToVector2Int_List(m_pathData[0].path);
                obj.InitEnemyData(GameMaster.Instance.csvHelper.GetScripteData<EnemyDataList>().GetData(m_enemySpawnDatas[m_spawnCount].enemyDataID), vectorList, DieAction, m_enemyDie, m_enemyArriveAction);
            }

            m_spawnCount++; // 다음 스폰 순서로 인덱스 증가
        }
    }

    // ----------------------------------------------------------------------
    // ## Memory Management (Object Pooling)
    // ----------------------------------------------------------------------

    /// <summary>
    /// [콜백] 적이 사망하거나 끝점에 도달했을 때 호출되어 오브젝트를 비활성 풀로 보냅니다.
    /// </summary>
    private void DieAction(int id, EnemyController enemy)
    {
        // 활성 리스트에서 제거
        if (m_enemyList.ContainsKey(id))
            m_enemyList[id].Remove(enemy);

        // 비활성 풀에 추가하여 나중에 재사용 가능하도록 설정
        if (m_disableList.ContainsKey(id) == false)
        {
            m_disableList.Add(id, new());
        }
        m_disableList[id].Add(enemy);
    }

    /// <summary>
    /// 컴포넌트가 비활성화될 때 실행 중인 비동기 작업을 안전하게 중단합니다.
    /// </summary>
    private void OnDisable()
    {
        m_cancellationTokenSource.Cancel();
    }
}