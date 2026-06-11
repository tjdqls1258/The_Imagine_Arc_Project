using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class EnemySpawnManager : MonoBehaviour
{
    private EnemySpawnData[] m_enemySpawnDatas; 
    private MapData.PathData[] m_pathData;       
    private int m_spawnCount = 0;
    private float m_currentTime = 0;

    private int m_totalCount = 0;
    private int m_remnant = 0;           
    private int m_currentCount = 0;

    private CancellationTokenSource m_cancellationTokenSource = new(); 

    private Dictionary<int, List<EnemyController>> m_enemyList = new();
    private Dictionary<int, List<EnemyController>> m_disableList = new();

    private Dictionary<int, GameObject> m_enemyModelList = new();

    private Action m_enemyDie;

    private Action m_enemyArriveAction;

    public void SetEnemyData(EnemySpawnData[] data, MapData.PathData[] pathDatas, Action enemyDieAction, Action enemyArriveAction)
    {
        m_enemySpawnDatas = data;
        m_pathData = pathDatas;

        m_enemyDie = enemyDieAction;
        m_enemyArriveAction = enemyArriveAction;

        m_totalCount = m_enemySpawnDatas.Length;
        m_remnant = m_enemySpawnDatas.Length;
        m_currentCount = m_enemySpawnDatas.Length;
    }

    public void StartSpawn()
    {
        SpawnStart().Forget(); 
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

            await UniTask.WaitForSeconds(currentTime, cancellationToken: destroyCancellationToken);
        }

        while (m_cancellationTokenSource.IsCancellationRequested == false)
        {
            await UniTask.WaitForFixedUpdate(m_cancellationTokenSource.Token);

            m_currentTime += Time.fixedDeltaTime;
            SpawnEnemy();
        }
    }

    private void SpawnEnemy()
    {
        if (m_spawnCount >= m_enemySpawnDatas.Length)
        {
            m_cancellationTokenSource.Cancel();
            return;
        }

        if (m_currentTime >= m_enemySpawnDatas[m_spawnCount].spawnTime)
        {
            EnemyController obj;
            int id = m_enemySpawnDatas[m_spawnCount].enemyDataID;

            m_remnant -= 1;

            if (m_disableList.ContainsKey(id) && m_disableList[id].Count > 0)
            {
                obj = m_disableList[id].First();
                m_disableList[id].Remove(obj);

                if (!m_enemyList.ContainsKey(id)) m_enemyList.Add(id, new());
                m_enemyList[id].Add(obj);
            }
            else
            {
                obj = Instantiate(m_enemyModelList[id]).GetComponent<EnemyController>();
                if (m_enemyList.ContainsKey(id) == false)
                {
                    m_enemyList.Add(id, new());
                }
                m_enemyList[id].Add(obj);
            }

            var pathindex = m_enemySpawnDatas[m_spawnCount].pathIndex;
            var pathData = m_pathData.FirstOrDefault(x => x.index == pathindex);

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

            m_spawnCount++;
        }
    }

    private void DieAction(int id, EnemyController enemy)
    {
        if (m_enemyList.ContainsKey(id))
            m_enemyList[id].Remove(enemy);

        if (m_disableList.ContainsKey(id) == false)
        {
            m_disableList.Add(id, new());
        }
        m_disableList[id].Add(enemy);
    }

    private void OnDisable()
    {
        m_cancellationTokenSource.Cancel();
    }
}