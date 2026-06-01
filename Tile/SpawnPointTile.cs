using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;

/// <summary>
/// 특정 지점에서 적을 생성하는 스폰 지점 타일 클래스입니다.
/// 타일 기반 맵 시스템(TileBase 상속) 내에서 독립적인 스폰 로직을 수행합니다.
/// </summary>
public class SpawnPointTile : TileBase
{
    public int sapwnIndex = 0;
    private List<EnemySpawnData> m_enemeyData = new();

    private bool m_enemyDataSetDone = false;

    private CancellationTokenSource m_stopToken = new CancellationTokenSource();

    private float currentSpawnTime = 0;

    public void SetEnemyData(List<EnemySpawnData> enemeyDatas)
    {
        m_enemeyData.Clear();

        m_enemeyData.AddRange(enemeyDatas.FindAll((x) => x.pathIndex == sapwnIndex));

        m_enemyDataSetDone = true;
        currentSpawnTime = 0;
    }

    public void StartSpawn()
    {
        SpawnLoop().Forget();
    }

    private async UniTask SpawnLoop()
    {
        foreach (var item in m_enemeyData)
        {
            currentSpawnTime = item.spawnTime - currentSpawnTime;
            await UniTask.WaitForSeconds(currentSpawnTime, cancellationToken: m_stopToken.Token);
        }
    }

    public void FailStageOrClearStage()
    {
        m_stopToken?.Cancel();

        m_stopToken?.Dispose();
    }
}