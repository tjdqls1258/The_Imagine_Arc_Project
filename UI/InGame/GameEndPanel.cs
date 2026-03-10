using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// 게임 종료 시 승리/패배 결과와 보상 목록을 표시하는 UI 패널입니다.
/// </summary>
public class GameEndPanel : UIBase
{
    // ====== UI Binding Enums (CachObject 시스템 활용) ======
    enum TextMeshProUGUIs
    {
        Title, // "복구 성공" 또는 "복구 실패" 타이틀
    }

    enum GameObjects
    {
        Content, // 보상 아이템 리스트가 배치될 부모 컨텐츠 영역
        Reward   // 보상 영역 전체 부모 (승리 시에만 활성화)
    }

    // ====== Properties (편의를 위한 캐싱 접근자) ======
    private GameObject RewardObject => Get<GameObject>((int)GameObjects.Reward);
    private TextMeshProUGUI Title => Get<TextMeshProUGUI>((int)TextMeshProUGUIs.Title);

    /// <summary> 생성된 보상 아이템 스크립트들을 관리하는 리스트 (재사용 목적) </summary>
    private List<RewardItem> m_itemList = new();

    // ----------------------------------------------------------------------
    // ## Initialization
    // ----------------------------------------------------------------------

    protected override void Awake()
    {
        // 1. UI 요소 및 컴포넌트 자동 바인딩
        Bind<TextMeshProUGUI>(typeof(TextMeshProUGUIs));
        Bind<RewardItem>(); // 리스트 항목 템플릿 바인딩
        Bind<GameObject>(typeof(GameObjects));

        // 2. 템플릿으로 사용될 원본 아이템은 비활성화 처리
        Get<RewardItem>().gameObject.SetActive(false);
    }

    // ----------------------------------------------------------------------
    // ## Result Logic
    // ----------------------------------------------------------------------

    /// <summary>
    /// 게임 결과를 판단하여 패널을 활성화합니다.
    /// </summary>
    /// <param name="isWin">승리 여부</param>
    /// <param name="itemsCount">획득한 아이템 데이터 배열</param>
    public void ResultGame(bool isWin, ItemData[] itemsCount)
    {
        gameObject.SetActive(true);

        // 게임 정지 (결과 화면 연출을 위해 타임스케일 조절)
        Time.timeScale = 0;

        if (isWin)
            ResultWin(itemsCount);
        else
            ResultLose();
    }

    /// <summary> 승리 시 처리: 타이틀 변경 및 보상 목록 출력 </summary>
    private void ResultWin(ItemData[] itemsCount)
    {
        Title.text = "복구 성공";
        RewardObject.SetActive(true);

        ShowRewardList(itemsCount);
    }

    /// <summary> 패배 시 처리: 타이틀 변경 및 보상 영역 숨김 </summary>
    private void ResultLose()
    {
        Title.text = "복구 실패";
        RewardObject.SetActive(false);
    }

    // ----------------------------------------------------------------------
    // ## Reward List UI Management
    // ----------------------------------------------------------------------

    /// <summary>
    /// 보상 아이템 목록을 생성하거나 기존 객체를 재사용하여 UI를 갱신합니다.
    /// </summary>
    private void ShowRewardList(ItemData[] itemsCount)
    {
        for (int i = 0; i < itemsCount.Length; i++)
        {
            // 1. 이미 리스트에 생성된 객체가 있다면 재사용
            if (m_itemList.Count > i)
            {
                m_itemList[i].gameObject.SetActive(true);
                m_itemList[i].SetItem(itemsCount[i]);
            }
            // 2. 부족하다면 새로 생성(Instantiate)하여 리스트에 추가
            else
            {
                var item = Instantiate(Get<RewardItem>(), Get<GameObject>((int)GameObjects.Content).transform);
                item.gameObject.SetActive(true);
                item.SetItem(itemsCount[i]);
                m_itemList.Add(item);
            }
        }

        // 3. (선택 사항) 만약 이전 게임보다 보상이 적다면 남는 객체는 비활성화 처리하는 로직을 추가할 수 있습니다.
    }

    // ----------------------------------------------------------------------
    // ## Lifecycle & Transition
    // ----------------------------------------------------------------------

    /// <summary>
    /// 결과 화면을 닫고 인게임 매니저를 통해 종료 처리를 수행합니다.
    /// </summary>
    public override void CloseUI(bool isClosetAll = false)
    {
        // 시간 흐름 복구
        Time.timeScale = 1;

        // 인게임 UI 매니저를 경유하여 게임 나가기 처리 실행
        GameMaster.Instance.uiManager.AutoUIManager
            .GetCompoent<InGameUIManager>(UIBaseData.UIType.InGameUI)
            .ExitGame();
    }
}