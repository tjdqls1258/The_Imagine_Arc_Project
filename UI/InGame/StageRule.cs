using System;
using UnityEngine;

public class StageRule
{
    private EnemySpawnManager m_enemySpawnManager;
    private int m_arriveCount = 0;
    private int m_life;

    public void Init(EnemySpawnManager enemySpawnManager, MapData mapData)
    {
        m_enemySpawnManager = enemySpawnManager;
        m_life = mapData.m_life;

        m_enemySpawnManager.SetEnemyData(mapData.enemySpawnDatas, mapData.pathDatas, EnemyDieAction, EnemyArriveAction);
    }

    public void StartGame()
    {
        m_enemySpawnManager.StartSpawn();
    }

    private void EnemyDieAction()
    {
        m_enemySpawnManager.EnemyDie();

        if (m_enemySpawnManager.GetCurrentCount() <= 0)
        {
            GameMaster.Instance.uiManager.GetAutoUIManager().GetCompoent<InGameUIManager>(UIBaseData.UIType.InGameUI).EndGame(true);
            Logger.Log("Enemy Clear");
        }
    }

    private void EnemyArriveAction()
    {
        m_arriveCount++;

        if (m_life <= m_arriveCount)
        {
            GameMaster.Instance.uiManager.GetAutoUIManager().GetCompoent<InGameUIManager>(UIBaseData.UIType.InGameUI).EndGame(false);
        }
        else
        {
            m_enemySpawnManager.EnemyDie();
        }

        Debug.Log("Enemy Arrive");
    }

    public void Clear()
    {

    }
}