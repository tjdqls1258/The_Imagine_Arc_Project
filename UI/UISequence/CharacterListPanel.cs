using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using VContainer;

/// <summary>
/// 보유 캐릭터 목록 화면을 총괄 관리하는 패널 클래스입니다.
/// 캐릭터 리스트(스크롤 뷰)와 선택된 캐릭터의 상세 정보창 사이의 상호작용을 제어합니다.
/// </summary>
public class CharacterListPanel : UIBase
{
    [Inject] private readonly UserDataManager dataManager;
    [Inject] private readonly AddressableManager addressableManager;
    [Inject] private readonly CSVHelper csvHelper;
    [Header("UI Components")]
    [Tooltip("캐릭터 목록을 그리드 형태로 표시하는 최적화 스크롤 컨트롤러")]
    [SerializeField] private CharacterPanelScroll m_characterScrollView;

    [Tooltip("선택된 캐릭터의 상세 스탯 및 정보를 표시하는 서브 UI 창")]
    [SerializeField] private CharacterDetail m_characterDetail;

    private UserData OwnUserCharacterData => dataManager.GetUserData<UserData>() as UserData;

    protected override void Awake()
    {
        m_characterScrollView.OnCellClicked(data =>
        {
            m_characterDetail.OnClickData(data);
        }, addressableManager, csvHelper);
    }

    public override void ShowUI()
    {
        base.ShowUI();
        WaitLoadImage().Forget();
    }

    private async UniTask WaitLoadImage()
    {
        var ownCharacterList = OwnUserCharacterData.oderCharacter.Values.ToList();

        List<UniTask> tasks = new();
        foreach (var character in ownCharacterList)
        {
            tasks.Add(character.GetCharacterData(csvHelper).LoadSprite(addressableManager));
        }

        await UniTask.WhenAll(tasks);

        m_characterScrollView.UpdateContents(ownCharacterList);
    }
}