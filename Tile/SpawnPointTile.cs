using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;

/// <summary>
/// 특정 지점에서 적을 생성하는 스폰 지점 타일 클래스입니다.
/// 타일 기반 맵 시스템(TileBase 상속) 내에서 독립적인 스폰 로직을 수행합니다.
/// </summary>
public class SpawnPointTile : TileBase
{
    // ====== Inspector & Data ======

    /// <summary> 이 스폰 지점이 담당할 경로(Path)의 인덱스 번호입니다. </summary>
    public int sapwnIndex = 0;

    /// <summary> 이 지점에서 스폰될 적들의 데이터 리스트입니다. </summary>
    private List<EnemySpawnData> m_enemeyData = new();

    /// <summary> 적 데이터 설정이 완료되었는지 확인하는 플래그입니다. </summary>
    private bool m_enemyDataSetDone = false;

    // ====== Runtime Control ======

    /// <summary> 비동기 스폰 루프를 중단하거나 자원을 해제하기 위한 토큰입니다. </summary>
    private CancellationTokenSource m_stopToken = new CancellationTokenSource();

    /// <summary> 현재까지 경과된 스폰 대기 시간입니다. </summary>
    private float currentSpawnTime = 0;

    // ----------------------------------------------------------------------
    // ## Data Initialization
    // ----------------------------------------------------------------------

    /// <summary>
    /// 전체 스폰 데이터 중 이 타일의 인덱스(sapwnIndex)와 일치하는 데이터만 필터링하여 설정합니다.
    /// </summary>
    /// <param name="enemeyDatas">스테이지 전체 적 스폰 데이터 목록</param>
    public void SetEnemyData(List<EnemySpawnData> enemeyDatas)
    {
        m_enemeyData.Clear();

        // 전체 데이터 중 pathIndex가 자신의 sapwnIndex와 같은 데이터만 추출하여 추가
        m_enemeyData.AddRange(enemeyDatas.FindAll((x) => x.pathIndex == sapwnIndex));

        m_enemyDataSetDone = true;
        currentSpawnTime = 0;
    }

    // ----------------------------------------------------------------------
    // ## Spawn Logic (Async)
    // ----------------------------------------------------------------------

    /// <summary>
    /// 적 생성을 시작하도록 명령합니다. UniTask를 사용하여 비동기로 루프를 돌립니다.
    /// </summary>
    public void StartSpawn()
    {
        // 스폰 루프 실행 (Fire and Forget 방식)
        SpawnLoop().Forget();
    }

    /// <summary>
    /// [비동기] 설정된 스폰 시간에 맞춰 적을 순차적으로 생성합니다.
    /// </summary>
    private async UniTask SpawnLoop()
    {
        foreach (var item in m_enemeyData)
        {
            // 다음 적 스폰까지 대기해야 할 시간 계산 (상대적 대기 시간)
            currentSpawnTime = item.spawnTime - currentSpawnTime;

            // 정해진 시간만큼 대기 (토큰을 통해 중단 가능)
            await UniTask.WaitForSeconds(currentSpawnTime, cancellationToken: m_stopToken.Token);

            // TODO: 실제 적 생성 로직 호출부
            // 예: EnemyManager.Instance.DoSpawn(item);
        }
    }

    // ----------------------------------------------------------------------
    // ## Lifecycle Management
    // ----------------------------------------------------------------------

    /// <summary>
    /// 스테이지 클리어 또는 실패 시 호출되어 실행 중인 스폰 루프를 즉시 중단합니다.
    /// </summary>
    public void FailStageOrClearStage()
    {
        // 토큰 취소를 통해 WaitForSeconds 대기 중인 루틴을 즉시 종료
        m_stopToken?.Cancel();

        // 사용이 끝난 토큰 소스 메모리 해제
        m_stopToken?.Dispose();
    }
}