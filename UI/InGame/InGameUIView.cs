using System;
using System.Collections.Generic;
using TMPro;
using UniRx;
using UnityEngine;

public class InGameUIView : MonoBehaviour
{
    [SerializeField] private UnitButton m_unitButtonBase;
    [SerializeField] private TextMeshProUGUI m_costText;

    [SerializeField] private TextMeshProUGUI m_lifeText;
    [SerializeField] private TextMeshProUGUI m_leftEnemeyText;
    [SerializeField] private TextMeshProUGUI m_timerText;

    private List<UnitButton> m_spawnButtons = new();
    private IDisposable m_timer;

    public void StatGaem()
    {
        m_timer = Observable.Interval(TimeSpan.FromSeconds(1)).
            Select(temp => TimeSpan.FromSeconds(temp)).
            Subscribe(x =>
            {
                m_timerText.text = $"{x:mm\\:ss}";
            });
    }

    public void UpdateCostDisplay(int cost)
    {
        m_costText.text = cost.ToString();
    }

    public void SubjectGameTextValue(ReactiveProperty<int> lifeProperty, ReactiveProperty<int> leftProperty)
    {
        lifeProperty.Subscribe(UpdateLifeText).AddTo(this);
        leftProperty.Subscribe(UpdateLeftEnemyText).AddTo(this);
    }

    public void CreateUnitButtons(InGameCharacterData[] datas, InGameUIManager inGameUiManager, AddressableManager addressableManager)
    {
        if (m_spawnButtons.Count == 0)
        {
            m_spawnButtons.Add(m_unitButtonBase);
        }
        for (int characterCount = 0; characterCount < GameData.MAX_SETTING_CHARACTERCOUNT; characterCount++)
        {
            if (characterCount >= datas.Length)
                break;

            if (m_spawnButtons.Count <= characterCount)
            {
                UnitButton newButton = Instantiate(m_unitButtonBase, m_unitButtonBase.transform.parent);
                m_spawnButtons.Add(newButton);
                m_spawnButtons[characterCount].SubscribeCost(inGameUiManager.inGameManager.goodsSystem.CurrentCost);
            }

            m_spawnButtons[characterCount].SetCharacter(datas[characterCount], inGameUiManager, addressableManager);
            m_spawnButtons[characterCount].SubscribeCost(inGameUiManager.inGameManager.goodsSystem.CurrentCost);
        }
    }

    public void Clear()
    {
        if (m_timer != null)
        {
            m_timer.Dispose();
            m_timer = null;
        }

        m_costText.text = "0";
        m_timerText.text = "00:00";
        ResetCharacterDatas();
    }

    private void ResetCharacterDatas()
    {
        foreach (var buttonItem in m_spawnButtons)
        {
            buttonItem.DeleteData();
        }
    }

    private void UpdateLifeText(int life) => m_lifeText.text = $"라이프 : {life}";

    private void UpdateLeftEnemyText(int leftEnemy) => m_leftEnemeyText.text = $"남은 적 : {leftEnemy}";
}
