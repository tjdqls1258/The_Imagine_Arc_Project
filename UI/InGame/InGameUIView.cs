using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InGameUIView : MonoBehaviour
{
    [SerializeField] private UnitButton m_unitButtonBase;
    [SerializeField] private TextMeshProUGUI m_costText;

    private List<UnitButton> m_spawnButtons = new();
    private Action<int> m_updateCostAction = null;

    public void UpdateCostDisplay(int cost)
    {
        m_costText.text = cost.ToString();
        m_updateCostAction?.Invoke(cost);
    }

    public void CreateUnitButtons(InGameCharacterData[] datas, InGameUIManager inGameUiManager)
    {
        if (m_spawnButtons.Count == 0)
        {
            m_spawnButtons.Add(m_unitButtonBase);
            m_updateCostAction += m_unitButtonBase.UpdateCostAction;
        }
        for (int characterCount = 0; characterCount < GameData.MAX_SETTING_CHARACTERCOUNT; characterCount++)
        {
            if (characterCount >= datas.Length)
                break;

            if (m_spawnButtons.Count <= characterCount)
            {
                UnitButton newButton = Instantiate(m_unitButtonBase, m_unitButtonBase.transform.parent);
                m_spawnButtons.Add(newButton);

                m_updateCostAction += m_spawnButtons[characterCount].UpdateCostAction;
            }

            m_spawnButtons[characterCount].SetCharacter(datas[characterCount], inGameUiManager);
        }
    }

    public void Clear()
    {
        m_costText.text = "0";
        ResetCharacterDatas();
    }

    private void ResetCharacterDatas()
    {
        foreach (var buttonItem in m_spawnButtons)
        {
            buttonItem.DeleteData();
        }
    }
}
