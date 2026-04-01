using Cysharp.Threading.Tasks;
using NetExcute;
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

    // ====== End Game State ======
    [SerializeField] private GameEndPanel m_endPanel;

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
    public void SetInGameData(UserCharacterData[] characterDatas)
    {
        m_endPanel.gameObject.SetActive(false);
        Logger.Log("Game Data Test Setting");

        // CSVHelper를 통해 데이터 시트에서 테스트용 캐릭터 정보 로드 (characterDatas)
        List<InGameCharacterData> characterDeckList = new();

        foreach (var characterData in characterDatas)
        {
            if (characterData == null) continue;
            characterDeckList.Add(SetCharacterData(characterData));
        }

        // UI 버튼 생성 및 캐릭터 데이터 주입
        SetCharacterDatas(characterDeckList.ToArray());

        // 인게임 매니저 참조 및 게임 시작 로직 연결
        m_inGameManager = FindAnyObjectByType<InGameManager>();
        m_inGameManager.SetChargeAction(ChargeText);
        m_inGameManager.StartGame();

        // 초기 코스트 텍스트 동기화
        ChargeText(m_inGameManager.currentCost);

        Get<OnClickCharacterPaenl>(0).SetInGameManager(m_inGameManager);

        // 로컬 함수: 코스트 변화 시 텍스트 갱신 및 버튼들의 상태 업데이트 수행
        void ChargeText(int currentCost)
        {
            m_costText.text = currentCost.ToString();
            m_updateCostAction?.Invoke(currentCost);
        }

        InGameCharacterData SetCharacterData(NetExcute.UserCharacterData data)
        {
            var characterData = data.GetCharacterData();
            InGameCharacterData ingameData = new InGameCharacterData(characterData, data);
            return ingameData;
        }
    }

    // ----------------------------------------------------------------------
    // ## UI Generation & Management
    // ----------------------------------------------------------------------

    /// <summary>
    /// 전달받은 캐릭터 데이터 목록을 기반으로 배치 버튼들을 생성하고 설정합니다.
    /// </summary>
    /// <param name="characterDatas">배치 리스트에 포함될 캐릭터 데이터 배열</param>
    public void SetCharacterDatas(InGameCharacterData[] characterDatas)
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
    public void OnClickCharacter(InGameCharacterData characterData, TileClickEvent tileClickActions)
    {
        Get<OnClickCharacterPaenl>(0).OnClickCharacter(characterData, tileClickActions);
    }

    public void EndGame(bool isWin)
    {
        //TODO 결과 관련 Web통신

        //임시 데이터
        m_endPanel.ResultGame(isWin, new ItemData[]
        {
            new()
            {
                itemID = 0,
                count = 1,
            },
            new()
            {
                itemID = 1,
                count = 2,
            },
            new()
            {
                itemID = 2,
                count = 3,
            }
        });
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
        //m_updateCostAction = null;

        // 홈 씬 로드 (비동기)
        GameMaster.Instance.sceneLoadManager.SceneLoad(SceneInfo.SceneType.HomeScene).Forget();

        // 오브젝트 풀링 내의 유효하지 않은(Null) 객체 정리
        GameMaster.Instance.objectPoolManager.ClearNullPoolObject();
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