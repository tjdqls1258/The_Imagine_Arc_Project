using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public class GameGoodsSystem
{
    private Action<int> m_chargeCostAction;
    private float m_costAddTime = 1;
    private float m_currentTime = 0;
    public int currentCost { get; private set; } = 0;
    CancellationTokenSource m_cancleToken;
    public void Init()
    {
        currentCost = 0;
        m_cancleToken = new();
    }

    public void StartGame()
    {
        UpdateChargeCost().Forget();
    }

    private async UniTask UpdateChargeCost()
    {
        while (m_cancleToken.IsCancellationRequested == false)
        {
            if (currentCost <= 99)
            {
                m_currentTime += Time.deltaTime;
                if (m_currentTime > m_costAddTime)
                {
                    m_currentTime = 0;
                    UpdateCost(1);
                }
            }

            await UniTask.WaitForFixedUpdate(m_cancleToken.Token);
        }
    }

    public bool UseCost(int cost)
    {
        if (currentCost < cost)
            return false;

        UpdateCost(-cost);
        return true;
    }

    public void AddActionChangeGoods(Action<int> charge)
    {
        m_chargeCostAction = charge;
    }


    private void UpdateCost(int cost)
    {
        if (cost == 0) return;
        currentCost += cost;

        if (m_chargeCostAction != null)
            m_chargeCostAction.Invoke(currentCost);
    }

    public void Clear()
    {
        if (m_cancleToken != null)
        {
            m_cancleToken.Cancel();
            m_cancleToken.Dispose();
            m_cancleToken = null;
        }

        currentCost = 0;
    }
}