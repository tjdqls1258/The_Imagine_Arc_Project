using Cysharp.Threading.Tasks;
using NetExcute;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 유저의 캐릭터 덱을 페이지별로 관리하고, 캐릭터 배치를 변경/교체하는 UI 패널입니다.
/// 비동기 로딩 중 중복 클릭 방지 및 데이터 무결성을 위한 저장 팝업 시스템이 포함되어 있습니다.
/// </summary>
public class CharacterSelectPanel : UIBase
{
    // ====== UI Binding Enums (CachObject 자동 바인딩 시스템) ======
    private enum CanvasGroups
    {
        PagePanel, // 덱 페이지 전환 시 부드러운 연출(알파값 조절)을 위한 그룹
    }

    private enum GameObjects
    {
        OderCharacterListPanel, // 캐릭터 교체 시 열리는 전체 보유 캐릭터 목록 패널
        PageList,               // 페이지 버튼(Pagination)들이 생성될 부모 컨테이너
    }

    private enum Buttons
    {
        PageItem, // 덱 페이지 선택 버튼의 원본 템플릿
        SaveButton, //저장 버튼
    }

    // ====== 데이터 및 상태 관리 변수 ======

    /// <summary> 전체 유저 데이터를 관리하는 매니저로부터 UserData 인스턴스 참조 </summary>
    private UserData m_userCharacterData => GameMaster.Instance.dataManager.GetUserData<UserData>() as UserData;

    /// <summary> 페이지 전환 버튼들의 활성화 상태를 한꺼번에 제어하기 위한 리스트 </summary>
    private List<Button> m_pageInteractable = new();

    [SerializeField]
    private CharacterChangeButton[] m_characterImages; // 덱 슬롯(최대 12개)을 담당하는 버튼 컴포넌트들
    private CharacterChangeButton m_clickTarget;       // 현재 교체 작업을 위해 선택된 슬롯 버튼
    private UserCharacterData m_targetData;            // 선택된 슬롯에 원래 배치되어 있던 데이터 (비교용)
    private UserCharacterData[] m_currentDeck;         // 현재 화면에서 수정 중인 '임시' 덱 데이터 배열

    private bool isDirtFlag = false; // 데이터 수정 여부 (true일 경우 닫기나 페이지 전환 시 저장 팝업 출력)
    private int m_currentPage = -1;  // 현재 편집 중인 덱 페이지 인덱스 (0, 1, 2...)
    private int m_currentindex = -1; // 현재 선택된 캐릭터 슬롯의 인덱스 번호

    /// <summary> 캐릭터 목록을 그리드 형태로 보여주는 스크롤 뷰 컴포넌트 </summary>
    private CharacterPanelScroll m_characterListPanel => Get<CharacterPanelScroll>();

    // ----------------------------------------------------------------------
    // ## Initialization (초기화)
    // ----------------------------------------------------------------------

    protected override void Awake()
    {
        Init();
    }

    public override void Init(Transform parent = null)
    {
        base.Init(parent);

        // 덱 슬롯 최대 수(12개)만큼 임시 배열 공간 확보
        m_currentDeck = new UserCharacterData[UserData.MAX_CHARACTER_SETTING];

        // UI 요소 및 스크립트 바인딩
        Bind<CanvasGroup>(typeof(CanvasGroups));
        Bind<GameObject>(typeof(GameObjects));
        Bind<CharacterPanelScroll>();
        Bind<Button>(typeof(Buttons));

        SelecteButtonSetting(); // 덱 슬롯 버튼 초기 이벤트 설정
        SettingContext();       // 하위 스크롤 뷰 데이터 연결
        SetPageItem();          // 덱 페이지 번호 버튼(Pagination) 동적 생성

        Get<Button>((int)Buttons.SaveButton).onClick.AddListener(() =>
        {
            SavePopup().Forget();
        });

        // [Local Function] 덱 슬롯 버튼들에 클릭 콜백 주입
        void SelecteButtonSetting()
        {
            int index = 0;
            foreach (var item in m_characterImages)
            {
                item.Init(OnClickChangeCharacter, index);
                index++;
            }
        }

        // [Local Function] 설정된 최대 덱 개수만큼 페이지 이동 버튼 생성
        void SetPageItem()
        {
            var parent = Get<GameObject>((int)GameObjects.PageList);
            for (int i = 1; i < UserData.MAX_DECKCOUNT; i++)
            {
                var page = Instantiate(Get<Button>((int)Buttons.PageItem), parent.transform);
                int buttonindex = i;
                page.onClick.AddListener(() =>
                {
                    OnClickPageAction(buttonindex);
                });
                m_pageInteractable.Add(page); // 리스트에 담아 추후 일괄 비활성화 가능하게 관리
            }

            // 0번 페이지 버튼(원본)에도 이벤트 할당 및 리스트 등록
            Get<Button>((int)Buttons.PageItem).onClick.AddListener(() =>
            {
                OnClickPageAction(0);
            });
            m_pageInteractable.Add(Get<Button>((int)Buttons.PageItem));
        }

        /// <summary>
        /// 페이지 버튼 클릭 시 실행되는 액션. 저장 여부 확인 및 비동기 페이지 로드를 트리거합니다.
        /// </summary>
        void OnClickPageAction(int pageindex)
        {
            // 로딩 중 중복 클릭 방지를 위해 모든 페이지 버튼 비활성화
            SetInteractablePage(false);

            if (isDirtFlag && m_currentPage != pageindex)
            {
                // 변경 사항이 있다면 저장 질문 팝업을 띄우고, 확인/취소 시 다음 페이지로 이동
                SavePopup(() =>
                {
                    OnClickPage(pageindex).Forget();
                }).Forget();
            }
            else
            {
                // 변경 사항이 없다면 즉시 페이지 이동
                OnClickPage(pageindex).Forget();
            }
        }
    }

    /// <summary> 모든 덱 페이지 선택 버튼의 클릭 가능 여부를 제어합니다. </summary>
    private void SetInteractablePage(bool active)
    {
        foreach (var i in m_pageInteractable)
            i.interactable = active;
    }

    // ----------------------------------------------------------------------
    // ## Lifecycle & Interaction Events
    // ----------------------------------------------------------------------

    public override void ShowUI()
    {
        base.ShowUI();

        isDirtFlag = false;
        SetInteractablePage(false);
        OnClickPage(0).Forget(); // 진입 시 기본적으로 0번 덱 페이지 로드
    }

    /// <summary> 닫기 버튼 클릭 시 변경 사항이 있다면 저장 팝업을, 없다면 즉시 종료합니다. </summary>
    public override void OnClickClosetButton()
    {
        if (isDirtFlag)
            SavePopup(base.OnClickClosetButton).Forget();
        else
            base.OnClickClosetButton();
    }

    // ----------------------------------------------------------------------
    // ## Core Logic (Deck & Page Management)
    // ----------------------------------------------------------------------

    /// <summary>
    /// [비동기] 특정 페이지의 덱 데이터를 로드하고 슬롯 UI를 갱신합니다.
    /// </summary>
    public async UniTask OnClickPage(int pageIndex)
    {
        // 이미 해당 페이지라면 버튼만 다시 활성화하고 중단
        if (m_currentPage == pageIndex)
        {
            SetInteractablePage(true);
            return;
        }

        m_currentPage = pageIndex;
        int index = 0;

        // 시각적 연출을 위해 패널 투명화
        Get<CanvasGroup>((int)CanvasGroups.PagePanel).alpha = 0;

        List<UniTask> loadList = new();
        if (m_userCharacterData.characterDeckList.ContainsKey(pageIndex))
        {
            // 중요: 원본 데이터를 임시 수정용 배열(m_currentDeck)로 '깊은 복사'하여 데이터 오염 방지
            Array.Copy(m_userCharacterData.characterDeckList[m_currentPage], m_currentDeck, m_currentDeck.Length);

            // 각 슬롯에 맞는 캐릭터 프리팹/이미지 로드 작업을 병렬로 실행
            foreach (var character in m_characterImages)
            {
                loadList.Add(m_characterImages[index].SettingPrefab(m_currentDeck[index]));
                index++;
            }
        }

        // 모든 캐릭터의 리소스 로딩이 끝날 때까지 대기 (최적화)
        await UniTask.WhenAll(loadList);

        // 로드 완료 후 화면 표시 및 버튼 다시 활성화
        Get<CanvasGroup>((int)CanvasGroups.PagePanel).alpha = 1;
        SetInteractablePage(true);
    }

    /// <summary> 캐릭터 슬롯 클릭 시 선택 목록 패널을 활성화합니다. </summary>
    private void OnClickChangeCharacter(CharacterChangeButton button)
    {
        m_clickTarget = button;
        m_targetData = button.GetCharacterData();
        m_currentindex = button.prefabIndex;

        SettingContext(); // 하단 리스트에 현재 덱 정보 등을 전달하여 갱신
        Get<GameObject>((int)GameObjects.OderCharacterListPanel).gameObject.SetActive(true);
    }

    /// <summary>
    /// 캐릭터 목록에서 특정 캐릭터를 선택했을 때 실행되는 배치 로직 (해제/스왑/신규 배치)
    /// </summary>
    private void OnClickChange(UserCharacterData data)
    {
        Get<GameObject>((int)GameObjects.OderCharacterListPanel).gameObject.SetActive(false);
        isDirtFlag = true; // 변경 사항 발생 기록

        // 1. [해제] 이미 배치된 캐릭터를 같은 슬롯에서 다시 선택한 경우 -> 제거
        if (m_targetData != null && m_targetData.ID == data.ID)
        {
            m_clickTarget.SettingPrefab(null).Forget();
            m_currentDeck[m_currentindex] = null;
        }
        // 2. [스왑] 선택한 캐릭터가 다른 슬롯에 이미 배치되어 있는 경우 -> 위치 교체
        else if (m_currentDeck.Any(x => x != null && x.ID == data.ID))
        {
            int oldIndex = Array.IndexOf(m_currentDeck, data); // 기존 위치 인덱스
            var currentSlotData = m_currentDeck[m_currentindex]; // 현재 슬롯의 데이터 백업

            // 두 슬롯의 UI를 비동기로 교체 갱신
            m_characterImages[m_currentindex].SettingPrefab(data).Forget();
            m_characterImages[oldIndex].SettingPrefab(currentSlotData).Forget();

            // 내부 임시 배열 데이터 교체
            m_currentDeck[m_currentindex] = data;
            m_currentDeck[oldIndex] = currentSlotData;
        }
        // 3. [신규 배치] 덱에 없던 새로운 캐릭터를 선택한 경우 -> 해당 슬롯에 배치
        else
        {
            m_clickTarget.SettingPrefab(data).Forget();
            m_currentDeck[m_currentindex] = data;
        }
    }

    /// <summary> 수정한 임시 덱 배열 데이터를 실제 유저 데이터 원본으로 저장합니다. </summary>
    private void SaveCurrentDeckList()
    {
        if (isDirtFlag == false) return;

        // 최소 한 명 이상의 캐릭터가 배치된 경우에만 저장 성공
        if (m_currentDeck.Any(x => x != null))
        {
            // 임시 배열의 내용을 원본 데이터 딕셔너리로 복사하여 확정
            Array.Copy(m_currentDeck, m_userCharacterData.characterDeckList[m_currentPage], m_currentDeck.Length);

            isDirtFlag = false; // 플래그 초기화
            return;
        }

        // 덱이 완전히 비어있을 경우 경고 메시지 출력
        PopupNotSaveMessage().Forget();
        isDirtFlag = false;
    }

    /// <summary> 하단 캐릭터 목록 스크롤 뷰에 유저 소유 캐릭터와 현재 덱 상태를 동기화합니다. </summary>
    private void SettingContext()
    {
        m_characterListPanel.OnCellClicked(OnClickChange, m_currentDeck, m_targetData);
        m_characterListPanel.UpdateContents(m_userCharacterData.oderCharacter.Values.ToList());
    }

    // ----------------------------------------------------------------------
    // ## Popup & Utility Management
    // ----------------------------------------------------------------------

    /// <summary>
    /// [비동기] 저장 여부를 묻는 팝업을 출력하며, 완료 시 실행할 콜백 액션을 지원합니다.
    /// </summary>
    /// <param name="closetPopupAction">저장 확인(또는 저장 안 함 선택) 후 실행될 후속 동작</param>
    async UniTask SavePopup(Action closetPopupAction = null)
    {
        var popup = await PopupManager.Instance.ShowPopup(PopupManager.PopupType.PopupQ) as PopupQ;

        // [확인] 클릭 시: 데이터 저장 후 등록된 후속 동작 실행
        popup.okAction = () =>
        {
            SaveCurrentDeckList();
            if (closetPopupAction != null)
                closetPopupAction.Invoke();
        };

        // [아니오] 클릭 시: 저장하지 않고 변경 사항 무시(플래그 해제)
        popup.noAction = () => 
        {
            isDirtFlag = false;
            if (closetPopupAction != null)
                closetPopupAction.Invoke();
        };

        popup.Mssage = "현재 기록을 저장하시겠습니까? \n(비워져 있는 경우 저장이 되지 않습니다.)";
    }

    /// <summary> [비동기] 빈 덱 저장 불가 메시지 팝업을 출력합니다. </summary>
    async UniTask PopupNotSaveMessage()
    {
        var Popup = await PopupManager.Instance.ShowPopup(PopupManager.PopupType.PopupMsg) as PopupMsg;
        Popup.Mssage = "기록이 비워져 있어 저장 되지 않았습니다.";
    }
}