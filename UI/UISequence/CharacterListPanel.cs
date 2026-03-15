using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

/// <summary>
/// 보유 캐릭터 목록 화면을 총괄 관리하는 패널 클래스입니다.
/// 캐릭터 리스트(스크롤 뷰)와 선택된 캐릭터의 상세 정보창 사이의 상호작용을 제어합니다.
/// </summary>
public class CharacterListPanel : UIBase
{
    // ====== UI 컴포넌트 참조 (Inspector 할당) ======

    [Header("UI Components")]
    [Tooltip("캐릭터 목록을 그리드 형태로 표시하는 최적화 스크롤 컨트롤러")]
    [SerializeField] private CharacterPanelScroll m_characterScrollView;

    [Tooltip("선택된 캐릭터의 상세 스탯 및 정보를 표시하는 서브 UI 창")]
    [SerializeField] private CharacterDetail m_characterDetail;

    /// <summary> 
    /// 데이터 매니저를 통해 현재 로그인한 유저의 캐릭터 데이터를 가져오는 읽기 전용 프로퍼티입니다. 
    /// </summary>
    private UserData OwnUserCharacterData => GameMaster.Instance.dataManager.GetUserData<UserData>() as UserData;

    // ----------------------------------------------------------------------
    // ## Initialization (초기 설정)
    // ----------------------------------------------------------------------

    protected override void Awake()
    {
        // [이벤트 바인딩] 리스트의 개별 셀(캐릭터 항목)이 클릭되었을 때 실행될 콜백을 등록합니다.
        // FancyScrollView의 컨텍스트를 통해 전달된 클릭 이벤트가 이곳에서 처리됩니다.
        m_characterScrollView.OnCellClicked(data =>
        {
            // 클릭된 캐릭터 데이터를 상세 정보창(CharacterDetail)에 전달하여 정보를 갱신합니다.
            m_characterDetail.OnClickData(data);
        });
    }

    // ----------------------------------------------------------------------
    // ## Lifecycle Management (화면 제어)
    // ----------------------------------------------------------------------

    /// <summary>
    /// 패널이 활성화될 때 호출됩니다. 리소스 로딩 과정을 비동기로 실행합니다.
    /// </summary>
    public override void ShowUI()
    {
        base.ShowUI();

        // 리소스 로드 및 UI 갱신 프로세스를 시작합니다. (비동기 흐름 분리)
        WaitLoadImage().Forget();
    }

    /// <summary>
    /// [비동기] 캐릭터 리소스를 로드한 후 스크롤 뷰의 내용을 갱신합니다.
    /// </summary>
    private async UniTask WaitLoadImage()
    {
        // 1. 유저가 보유한 전체 캐릭터 리스트를 가져옵니다.
        var ownCharacterList = OwnUserCharacterData.oderCharacter.Values.ToList();

        // 2. 각 캐릭터가 사용할 이미지(Sprite) 로드 작업들을 리스트에 담습니다.
        List<UniTask> tasks = new();
        foreach (var character in ownCharacterList)
        {
            // 개별 CharacterData 내부에 정의된 비동기 로드 함수를 호출합니다.
            tasks.Add(character.GetCharacterData().LoadSprite());
        }

        // 3. 모든 캐릭터의 리소스 로딩이 완료될 때까지 병렬로 대기합니다. (최적화 포인트)
        // 개별 로드를 하나씩 기다리는 것보다 훨씬 빠른 로딩 속도를 보장합니다.
        await UniTask.WhenAll(tasks);

        // 4. 리소스 준비가 완료되면 스크롤 뷰에 데이터를 전달하여 화면을 구성합니다.
        m_characterScrollView.UpdateContents(ownCharacterList);
    }
}