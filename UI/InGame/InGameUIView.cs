using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor.U2D.Animation;
using UnityEngine;

public class InGameUIView : MonoBehaviour
{
    [SerializeField] private UnitButton m_unitButtonBase; // 유닛 생성 버튼의 원본 프리팹
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
            // 데이터 범위를 벗어나면 중단
            if (characterCount >= datas.Length)
                break;

            // 필요 시 버튼 프리팹을 추가로 생성(Instantiate)하여 리스트 확장
            if (m_spawnButtons.Count <= characterCount)
            {
                UnitButton newButton = Instantiate(m_unitButtonBase, m_unitButtonBase.transform.parent);
                m_spawnButtons.Add(newButton);

                // 생성된 버튼의 코스트 갱신 로직을 델리게이트에 등록
                m_updateCostAction += m_spawnButtons[characterCount].UpdateCostAction;
            }

            // 버튼에 캐릭터 정보 주입 및 UI 레퍼런스 전달
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
