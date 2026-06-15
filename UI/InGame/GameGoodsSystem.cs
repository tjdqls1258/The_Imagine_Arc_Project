using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UniRx;
using UnityEngine;

public class GameGoodsSystem
{
    public ReactiveProperty<int> CurrentCost = new ReactiveProperty<int>(0);

    private Action<int> m_chargeCostAction;
    private float m_costAddTime = 1;
    private float m_currentTime = 0;
    
    CancellationTokenSource m_cancleToken;
    public void Init()
    {
        m_cancleToken = new();
    }

    public void StartGame()
    {
        Observable.Interval(TimeSpan.FromSeconds(m_costAddTime))
            .Subscribe(_ =>
            {
                if (CurrentCost.Value <= 99)
                {
                    CurrentCost.Value += 1;
                }
            })
            .AddTo(m_cancleToken.Token);
    }

    public bool UseCost(int cost)
    {
        if (CurrentCost.Value < cost)
            return false;

        CurrentCost.Value -= cost;
        return true;
    }

    public void AddActionChangeGoods(Action<int> charge)
    {
        m_chargeCostAction = charge;
    }

    public void Clear()
    {
        if (m_cancleToken != null)
        {
            m_cancleToken.Cancel();
            m_cancleToken.Dispose();
            m_cancleToken = null;
        }

        CurrentCost.Dispose();
    }
}