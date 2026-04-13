using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public class GameGoodsSystem
{
    private Action<int> m_chargeCostAction;        // 코스트 변동 시 UI를 갱신하기 위해 등록되는 콜백 (Event 역할)
    private float m_costAddTime = 1; // 코스트가 1씩 차오르는 주기 (초)
    private float m_currentTime = 0; // 코스트 증가를 계산하기 위한 내부 타이머 누적값
    public int currentCost { get; private set; } = 0;
    CancellationTokenSource m_cancleToken;
    public void Init()
    {
        m_chargeCostAction = null;
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
                    UpdateCost(1); // 1 코스트 획득
                }
            }

            await UniTask.WaitForFixedUpdate(m_cancleToken.Token);
        }
    }

    public bool UseCost(int cost)
    {
        if (currentCost < cost)
            return false;

        UpdateCost(-cost); // 성공 시 음수값 전달하여 차감
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
        
        m_chargeCostAction = null;
        currentCost = 0;
    }
}