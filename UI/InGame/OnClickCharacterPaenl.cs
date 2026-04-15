using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OnClickCharacterPaenl : CachObject
{
    enum Images
    {
        CharacterImage,
    }

    enum Buttons
    {
        UpgradButton,
        Back,
    }

    enum TextMeshPros
    {
        UpgradText,
    }

    private InGameCharacterData m_currentCharaterData; 
    private TileClickEvent m_tileEvents;

    private InGameManager m_inGameManager;

    private float m_currentSkillTime;

    private void Awake()
    {
        Bind<Image>(typeof(Images));
        Bind<Button>(typeof(Buttons));
        Bind<TextMeshProUGUI>(typeof(TextMeshPros));

        Get<TextMeshProUGUI>((int)TextMeshPros.UpgradText).text = "UPGRAD";

        Get<Button>((int)Buttons.Back).onClick.AddListener(ClosePanel);
        Get<Button>((int)Buttons.UpgradButton).onClick.AddListener(UpgradeButtonClick);
    }

    public void SetInGameManager(InGameManager inGameManager)
    {
        m_inGameManager = inGameManager;
    }

    public void OnClickCharacter(InGameCharacterData characterData, TileClickEvent tileClickActions)
    {
        gameObject.SetActive(true);
        m_currentCharaterData = characterData;

        m_tileEvents = tileClickActions;

        Time.timeScale = 0f;

        Get<TextMeshProUGUI>((int)TextMeshPros.UpgradText).text = $"UPGRAD\nCost:{characterData.characterData.cost}";
        characterData.characterData.GetCharacterSprite(targetImage: Get<Image>((int)Images.CharacterImage)).Forget();

        m_currentSkillTime = m_tileEvents.GetSkillLastTime() + m_tileEvents.GetSkillCoolTime();
    }

    public void ClosePanel()
    {
        Time.timeScale = 1f;

        gameObject.SetActive(false);

        if(m_tileEvents != null)
            m_tileEvents.OnDeselect();

        m_tileEvents = null;
    }


    private void UpgradeButtonClick()
    {
        int useCost = m_tileEvents.GetUpgradeCost();
        if (m_inGameManager.GetCurrentCost() < m_tileEvents.GetUpgradeCost())
        {
            Logger.Log("업그레이드 불가");
            return;
        }

        m_tileEvents.OnUpgrade();
        m_inGameManager.UseCost(useCost);
    }
}