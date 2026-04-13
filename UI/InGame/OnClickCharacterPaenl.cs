using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 인게임 필드에 배치된 캐릭터를 클릭했을 때 나타나는 상세 상호작용 패널입니다.
/// 캐릭터의 외형 확인, 업그레이드, 스킬 관리 기능을 제공하며 게임 일시정지를 제어합니다.
/// </summary>
public class OnClickCharacterPaenl : CachObject
{
    // ====== UI Binding Enums (CachObject 시스템 활용) ======

    enum Images
    {
        CharacterImage, // 캐릭터 전신 일러스트 표시용
    }

    enum Buttons
    {
        UpgradButton,  // 강화/업그레이드 버튼
        Back,          // 패널 닫기 버튼
    }

    enum TextMeshPros
    {
        UpgradText,    // "UPGRADE" 텍스트 레이블
    }

    // ====== Runtime Data ======
    private InGameCharacterData m_currentCharaterData; // 현재 선택된 캐릭터의 데이터
    private TileClickEvent m_tileEvents;

    private InGameManager m_inGameManager;

    private float m_currentSkillTime;



    // ----------------------------------------------------------------------
    // ## Initialization
    // ----------------------------------------------------------------------
    private void Awake()
    {
        // 1. UI 컴포넌트 자동 바인딩 (Enum 기반)
        Bind<Image>(typeof(Images));
        Bind<Button>(typeof(Buttons));
        Bind<TextMeshProUGUI>(typeof(TextMeshPros));

        // 2. 기본 텍스트 초기화
        Get<TextMeshProUGUI>((int)TextMeshPros.UpgradText).text = "UPGRAD";

        // 3. 버튼 이벤트 연결 (인덱스 기반 접근)
        Get<Button>((int)Buttons.Back).onClick.AddListener(ClosePanel);
        Get<Button>((int)Buttons.UpgradButton).onClick.AddListener(UpgradeButtonClick);
    }

    public void SetInGameManager(InGameManager inGameManager)
    {
        m_inGameManager = inGameManager;
    }

    // ----------------------------------------------------------------------
    // ## Panel Control (Open / Close)
    // ----------------------------------------------------------------------

    /// <summary>
    /// 캐릭터를 클릭했을 때 패널을 열고 데이터를 세팅합니다.
    /// </summary>
    /// <param name="characterData">클릭된 캐릭터의 데이터</param>
    /// <param name="activeAction">열릴 때 실행할 콜백 (예: 하이라이트 효과)</param>
    /// <param name="disableAction">닫힐 때 실행할 콜백 (예: 하이라이트 해제)</param>
    public void OnClickCharacter(InGameCharacterData characterData, TileClickEvent tileClickActions)
    {
        // 패널 활성화
        gameObject.SetActive(true);
        m_currentCharaterData = characterData;

        // 콜백 저장 및 실행
        m_tileEvents = tileClickActions;

        // 게임 로직 일시정지 (전투 중단)
        Time.timeScale = 0f;

        // 어드레서블 시스템을 통해 캐릭터 이미지 비동기 로드 및 적용
        Get<TextMeshProUGUI>((int)TextMeshPros.UpgradText).text = $"UPGRAD\nCost:{characterData.characterData.cost}";
        characterData.characterData.GetCharacterSprite(targetImage: Get<Image>((int)Images.CharacterImage)).Forget();

        m_currentSkillTime = m_tileEvents.GetSkillLastTime() + m_tileEvents.GetSkillCoolTime();
    }

    /// <summary>
    /// 패널을 닫고 게임을 다시 진행 상태로 돌립니다.
    /// </summary>
    public void ClosePanel()
    {
        // 게임 속도 정상화
        Time.timeScale = 1f;

        // 패널 비활성화 및 종료 콜백 실행
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
            Logger.Log("요구되는 소모치 부족");
            return;
        }

        m_tileEvents.OnUpgrade();
        m_inGameManager.UseCost(useCost);
    }
}