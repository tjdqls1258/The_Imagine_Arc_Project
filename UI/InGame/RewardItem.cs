using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 보상 목록 화면에서 개별 아이템의 아이콘과 수량을 표시하는 UI 컴포넌트입니다.
/// </summary>
public class RewardItem : MonoBehaviour
{
    // ====== UI References (Unity Inspector) ======
    [Header("UI Components")]
    [SerializeField] private TextMeshProUGUI m_itemCount; // 아이템 획득 수량 텍스트
    [SerializeField] private Image m_itemImage;           // 아이템 아이콘 이미지

    // ====== Runtime Data ======
    /// <summary> 현재 슬롯에 할당된 아이템 데이터 </summary>
    private ItemData m_data;

    // ----------------------------------------------------------------------
    // ## Data Binding (데이터 설정)
    // ----------------------------------------------------------------------

    /// <summary>
    /// 전달받은 아이템 데이터를 기반으로 UI를 갱신하고 활성화합니다.
    /// </summary>
    /// <param name="itemData">표시할 아이템 정보(ID, 수량, 이미지 경로 등)</param>
    public void SetItem(ItemData itemData)
    {
        m_data = itemData;

        // 1. 객체 활성화 (풀링 시스템 등에서 재사용 시 필요)
        gameObject.SetActive(true);

        // 2. 수량 텍스트 갱신
        m_itemCount.text = itemData.count.ToString();

        // 3. 아이콘 이미지 설정
        // [참고] 현재는 이미지 로직이 생략되어 있으나, 
        // 프로젝트의 리소스 관리 방식(Addressables 등)에 맞춰 이미지를 로드하는 로직이 들어갈 자리입니다.
        // 예: AddressableManager.Instance.SetSprite(itemData.iconAddress, m_itemImage).Forget();
    }
}