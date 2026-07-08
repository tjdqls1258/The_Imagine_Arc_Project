using DG.Tweening;
using System;
using System.Collections.Generic;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class InGameUIView : MonoBehaviour
{
    [SerializeField] private UnitButton m_unitButtonBase;
    [SerializeField] private PlayerSkillButton m_playerSkillButton;

    [SerializeField] private Image fadeImage; //시작전 버튼 기본 세팅 안보이도록

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI m_costText;
    [SerializeField] private TextMeshProUGUI m_lifeText;
    [SerializeField] private TextMeshProUGUI m_leftEnemeyText;
    [SerializeField] private TextMeshProUGUI m_timerText;

    private List<UnitButton> m_spawnButtons = new();
    private List<PlayerSkillButton> m_userSkillButton = new();
    private IDisposable m_timer;

    private void Awake()
    {
        ActiveFade(true);
    }

    public void StartGame()
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

    public void CreateUserSkillButtons(UserSkillBase[] userSkills,InGameUIManager inGameUiManager, AddressableManager addressableManager)
    {
        if (userSkills == null) return;
        if(m_userSkillButton.Count == 0)
        {
            m_userSkillButton.Add(m_playerSkillButton);
        }
        for(int count = 0; count < userSkills.Length; count++)
        {
            if (m_userSkillButton.Count < userSkills.Length)
            {
                PlayerSkillButton newButton = Instantiate(m_playerSkillButton, m_playerSkillButton.transform.parent);
                m_userSkillButton.Add(newButton);
            }

            m_userSkillButton[count].SetSkill(userSkills[count], inGameUiManager.inGameManager, addressableManager);
            m_userSkillButton[count].SubscribeCost(inGameUiManager.inGameManager.goodsSystem.CurrentCost);
        }
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
            }

            m_spawnButtons[characterCount].SetCharacter(datas[characterCount], inGameUiManager, addressableManager);
            m_spawnButtons[characterCount].SubscribeCost(inGameUiManager.inGameManager.goodsSystem.CurrentCost);
        }
    }

    public void UISettingDone()
    {
        ActiveFade(false);
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
        ActiveFade(true);
    }

    private void ActiveFade(bool isActive)
    {
        if(isActive)
        {
            fadeImage.gameObject.SetActive(true);
            fadeImage.color = Color.black;
        }
        else
        {
            fadeImage.DOFade(0, 0.2f).
            OnComplete(() => fadeImage.gameObject.SetActive(false)).
            SetLink(gameObject);
        }
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
