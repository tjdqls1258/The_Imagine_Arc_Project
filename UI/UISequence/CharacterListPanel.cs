using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 캐릭터 목록 화면을 총괄 관리하는 패널 클래스입니다.
/// 캐릭터 리스트(스크롤 뷰)와 선택된 캐릭터의 상세 정보창 사이의 상호작용을 제어합니다.
/// </summary>
public class CharacterListPanel : UIBase
{
    // ====== Inspector References ======

    [Header("UI Components")]
    [Tooltip("캐릭터 목록을 그리드 형태로 표시하는 스크롤 뷰 컨트롤러")]
    [SerializeField] private CharacterPanelScroll m_characterScrollView;

    [Tooltip("선택된 캐릭터의 상세 수치 및 정보를 표시하는 UI 창")]
    [SerializeField] private CharacterDetail m_characterDetail;

    // ----------------------------------------------------------------------
    // ## Initialization (Lifecycle)
    // ----------------------------------------------------------------------

    protected override void Awake()
    {
        // 1. UIManager에서 관리할 수 있도록 이 패널의 시퀀스(ID)를 등록합니다.
        m_UISequence = UIManager.UISequence.CharacterListPanel;

        // 2. [이벤트 연결] 리스트의 셀이 클릭되었을 때 실행될 콜백 등록
        // 셀이 클릭되면 해당 캐릭터의 데이터를 상세 정보창(CharacterDetail)에 전달하여 갱신합니다.
        m_characterScrollView.OnCellClicked(index =>
        {
            m_characterDetail.OnClickData(index);
        });
    }

    public override void ShowUI()
    {
        base.ShowUI();
        WaitLoadImage().Forget();
    }

    private async UniTask WaitLoadImage()
    {
        // CSV 데이터 헬퍼를 통해 로드된 전체 캐릭터 리스트를 가져와 스크롤 뷰에 채워 넣습니다.
        var characterDataList = GameMaster.Instance.csvHelper.GetScripteData<CharacterDataList>();
        if (characterDataList != null)
        {
            await characterDataList.LoadAllCharacterSprite();
            m_characterScrollView.UpdateContents(characterDataList.GetDefaultList());
        }
    }
}