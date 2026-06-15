using System;
using UniRx;
using UnityEngine;
using VContainer;

public class StageRule
{
    private readonly UIManager uiManager;

    private EnemySpawnManager m_enemySpawnManager;

    public ReactiveProperty<int> lifeEvent { get; private set; } = new();

    public StageRule(UIManager uiManager)
    {
        this.uiManager = uiManager;
    }

    public void Init(EnemySpawnManager enemySpawnManager, MapData mapData)
    {
        m_enemySpawnManager = enemySpawnManager;

        lifeEvent.Value = mapData.m_life;

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
            uiManager.GetAutoUIManager().GetCompoent<InGameUIManager>(UIBaseData.UIType.InGameUI).EndGame(true);
            Logger.Log("Enemy Clear");
        }
    }

    private void EnemyArriveAction()
    {
        lifeEvent.Value -= 1;

        if (lifeEvent.Value <= 0)
        {
            uiManager.GetAutoUIManager().GetCompoent<InGameUIManager>(UIBaseData.UIType.InGameUI).EndGame(false);
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