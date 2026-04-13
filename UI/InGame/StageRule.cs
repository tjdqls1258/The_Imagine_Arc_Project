using System;
using UnityEngine;

// 승리/패배 규칙 판정 전담
public class StageRule
{
    private EnemySpawnManager m_enemySpawnManager; // 적 웨이브 출현을 제어하는 매니저
    private int m_arriveCount = 0;   // 목표 지점(본진)에 도달한 적의 수 (라이프 차감 기준)
    private int m_life; // 비동기로 로드되어 캐싱된 맵 데이터(SO 또는 JSON 등) 원본

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

    /// <summary> 적 유닛이 유저의 공격에 의해 파괴(사망)했을 때 호출되는 콜백입니다. </summary>
    private void EnemyDieAction()
    {
        m_enemySpawnManager.EnemyDie();

        if (m_enemySpawnManager.GetCurrentCount() <= 0)
        {
            GameMaster.Instance.uiManager.AutoUIManager.GetCompoent<InGameUIManager>(UIBaseData.UIType.InGameUI).EndGame(true);
            Logger.Log("Enemy Clear");
        }
    }

    /// <summary> 적 유닛이 유저의 방어선을 뚫고 목표 지점(본진)에 도착했을 때 호출되는 콜백입니다. </summary>
    private void EnemyArriveAction()
    {
        m_arriveCount++; 
        
        if (m_life <= m_arriveCount)
        {
            GameMaster.Instance.uiManager.AutoUIManager.GetCompoent<InGameUIManager>(UIBaseData.UIType.InGameUI).EndGame(false);
        }
        else
        {
            m_enemySpawnManager.EnemyDie();
        }

        Logger.Log("Enemy Arrive");
    }

    public void Clear()
    {

    }
}
