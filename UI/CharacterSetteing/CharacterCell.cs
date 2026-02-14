using Cysharp.Threading.Tasks;
using FancyScrollView;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// FancyGridView에서 개별 캐릭터 항목을 표시하는 셀 컴포넌트입니다.
/// 캐릭터의 이미지, 이름 등의 정보를 갱신하고 클릭 이벤트를 처리합니다.
/// </summary>
public class CharacterCell : FancyGridViewCell<CharacterData, CharacterPanelContext>
{
    // ====== Runtime Data ======
    private CharacterData m_data; // 현재 셀에 할당된 캐릭터 데이터

    // ====== UI References ======
    [Header("UI Components")]
    [SerializeField] private Image m_characterImage;      // 캐릭터 일러스트/아이콘 이미지
    [SerializeField] private TextMeshProUGUI m_characterName; // 캐릭터 이름 텍스트
    [SerializeField] private TextMeshProUGUI m_characterLevel; // 캐릭터 레벨 텍스트 (추후 업데이트용)

    // ----------------------------------------------------------------------
    // ## FancyScrollView Overrides
    // ----------------------------------------------------------------------

    /// <summary>
    /// 셀의 내용물을 갱신할 때 호출됩니다. (스크롤 시 리스트 아이템 재사용 시점)
    /// </summary>
    /// <param name="itemData">표시할 캐릭터 데이터 원본</param>
    public override void UpdateContent(CharacterData itemData)
    {
        m_data = itemData;

        // 1. 캐릭터 이미지 비동기 로드 (Addressables 연동)
        // Forget()을 사용하여 로드 완료를 기다리지 않고 다음 로직을 실행합니다.
        itemData.GetCharacterSprite(targetImage: m_characterImage).Forget();

        // 2. 텍스트 정보 업데이트
        m_characterName.text = itemData.characterName;

        // 필요 시 레벨 정보 업데이트 로직 추가 가능
        // m_characterLevel.text = $"Lv. {itemData.level}"; 
    }

    // ----------------------------------------------------------------------
    // ## Interaction
    // ----------------------------------------------------------------------

    /// <summary>
    /// 셀이 클릭되었을 때 실행됩니다. (Button 컴포넌트의 OnClick 등에 연결)
    /// </summary>
    public void OnClick()
    {
        // Context(FancyScrollView의 공유 데이터)를 통해 클릭된 데이터 정보를 알림
        // 선택된 캐릭터의 정보를 상세 창 등에 띄울 때 사용됩니다.
        Context.OnCellClicked.Invoke(m_data);
    }

    // ----------------------------------------------------------------------
    // ## Lifecycle & Optimization
    // ----------------------------------------------------------------------

    /// <summary>
    /// 셀 오브젝트가 비활성화될 때 호출됩니다. (스크롤 범위를 벗어나거나 패널을 닫을 때)
    /// </summary>
    private void OnDisable()
    {
        // 메모리 관리: 로드했던 스프라이트 아틀라스를 언로드하여 비디오 메모리 누수를 방지합니다.
        if (m_data != null)
        {
            m_data.UnloadAtlas();
        }
    }
}