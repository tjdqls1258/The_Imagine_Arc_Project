using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 인게임 전투 화면의 UI 요소를 제어하고 관리하는 클래스입니다.
/// 캐릭터 배치 버튼 생성, 실시간 코스트 표시 업데이트, 상세 정보 패널 호출 등을 담당합니다.
/// </summary>
public class InGameUIManager : UIBaseFormMaker
{
    // ====== Inspector References ======
    [Header("UI Components")]
    [SerializeField] private UnitButton m_unitButtonBase; // 유닛 생성 버튼의 원본 프리팹
    [SerializeField] private TextMeshProUGUI m_costText;  // 코스트 수치를 표시하는 텍스트

    // ====== Runtime State & Caches ======
    private List<UnitButton> m_spawnButton = new();       // 화면에 생성된 유닛 버튼 리스트
    private Camera m_camera;                              // 메인 카메라 캐시용 필드
    private Action<int> m_updateCostAction = null;        // 코스트 변화 시 버튼들의 활성 상태를 갱신하는 멀티캐스트 델리게이트

    // ====== Properties ======
    public InGameManager m_inGameManager { get; private set; }

    /// <summary>
    /// 메인 카메라를 캐싱하여 반환합니다.
    /// </summary>
    public Camera mainCamera
    {
        get
        {
            if (m_camera == null)
                m_camera = Camera.main;

            return m_camera;
        }
    }

    /// <summary>
    /// UI 바인딩 및 이벤트 정의를 위한 열거형입니다.
    /// </summary>
    enum OnClickSettingPanel
    {
        OnClickSettingPanel,
    }

    // ----------------------------------------------------------------------
    // ## Initialization
    // ----------------------------------------------------------------------

    protected override void Awake()
    {
        base.Awake();
        // UI 컴포넌트 자동 바인딩 (UIBaseFormMaker 기능 활용)
        Bind<OnClickCharacterPaenl>(typeof(OnClickSettingPanel));

        // 최초의 기본 버튼 초기화 및 코스트 갱신 액션 등록
        m_spawnButton.Add(m_unitButtonBase);
        m_updateCostAction += m_unitButtonBase.UpdateCostAction;
    }

    /// <summary>
    /// 테스트용 데이터를 기반으로 인게임 UI와 매니저를 초기화합니다.
    /// </summary>
    public void SetInGameDataTest()
    {
        Logger.Log("Game Data Test Setting");

        // CSVHelper를 통해 데이터 시트에서 테스트용 캐릭터 정보 로드
        List<CharacterData> testdatas = new()
        {
            GameMaster.Instance.csvHelper.GetScripteData<CharacterDataList>().GetData(1),
            GameMaster.Instance.csvHelper.GetScripteData<CharacterDataList>().GetData(2),
            GameMaster.Instance.csvHelper.GetScripteData<CharacterDataList>().GetData(3),
            GameMaster.Instance.csvHelper.GetScripteData<CharacterDataList>().GetData(4)
        };

        // UI 버튼 생성 및 캐릭터 데이터 주입
        SetCharacterDatas(testdatas.ToArray());

        // 인게임 매니저 참조 및 게임 시작 로직 연결
        m_inGameManager = FindAnyObjectByType<InGameManager>();
        m_inGameManager.SetChargeAction(ChargeText);
        m_inGameManager.StartGame();

        // 초기 코스트 텍스트 동기화
        ChargeText(m_inGameManager.currentCost);

        // 로컬 함수: 코스트 변화 시 텍스트 갱신 및 버튼들의 상태 업데이트 수행
        void ChargeText(int currentCost)
        {
            m_costText.text = currentCost.ToString();
            m_updateCostAction?.Invoke(currentCost);
        }
    }

    // ----------------------------------------------------------------------
    // ## UI Generation & Management
    // ----------------------------------------------------------------------

    /// <summary>
    /// 전달받은 캐릭터 데이터 목록을 기반으로 배치 버튼들을 생성하고 설정합니다.
    /// </summary>
    /// <param name="characterDatas">배치 리스트에 포함될 캐릭터 데이터 배열</param>
    public void SetCharacterDatas(CharacterData[] characterDatas)
    {
        for (int characterCount = 0; characterCount < GameData.MAX_SETTING_CHARACTERCOUNT; characterCount++)
        {
            // 데이터 범위를 벗어나면 중단
            if (characterCount >= characterDatas.Length)
                break;

            // 필요 시 버튼 프리팹을 추가로 생성(Instantiate)하여 리스트 확장
            if (m_spawnButton.Count <= characterCount)
            {
                UnitButton newButton = Instantiate(m_unitButtonBase, m_unitButtonBase.transform.parent);
                m_spawnButton.Add(newButton);

                // 생성된 버튼의 코스트 갱신 로직을 델리게이트에 등록
                m_updateCostAction += m_spawnButton[characterCount].UpdateCostAction;
            }

            // 버튼에 캐릭터 정보 주입 및 UI 레퍼런스 전달
            m_spawnButton[characterCount].SetCharater(characterDatas[characterCount], this);
        }
    }

    /// <summary>
    /// 맵에 배치된 캐릭터를 클릭했을 때 상세 정보 패널을 엽니다.
    /// </summary>
    public void OnClickCharacter(InGameCharacterData characterData, Action activeAction = null, Action disableAction = null)
    {
        Get<OnClickCharacterPaenl>(0).OnClickCharacter(characterData, activeAction, disableAction);
    }

    // ----------------------------------------------------------------------
    // ## Cleanup
    // ----------------------------------------------------------------------

    /// <summary>
    /// 인게임을 종료하고 관련된 UI 데이터 및 상태를 초기화합니다.
    /// </summary>
    public void ExitGame()
    {
        // 상세 패널 닫기 및 텍스트 초기화
        Get<OnClickCharacterPaenl>(0).ClosePanel();
        m_costText.text = "0";

        // 인게임 로직 종료 및 데이터 리셋
        m_inGameManager.ExitGame();
        ResetCharacterDatas();

        m_inGameManager = null;
    }

    /// <summary>
    /// 모든 유닛 배치 버튼 내부의 데이터를 해제합니다.
    /// </summary>
    private void ResetCharacterDatas()
    {
        foreach (var buttonItem in m_spawnButton)
        {
            buttonItem.DeleteData();
        }
    }
}