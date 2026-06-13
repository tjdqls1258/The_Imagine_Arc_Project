using Cysharp.Threading.Tasks;
using FancyScrollView;
using NetExcute;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterCell : FancyGridViewCell<UserCharacterData, CharacterPanelContext>
{
    private UserCharacterData m_userCharacterData;
    private CharacterData m_data => m_userCharacterData.GetCharacterData(Context.csvHelper); 

    [Header("UI Components")]
    [SerializeField] private Image m_characterImage;      // 캐릭터 일러스트/아이콘 이미지
    [SerializeField] private TextMeshProUGUI m_characterName; // 캐릭터 이름 텍스트
    [SerializeField] private TextMeshProUGUI m_characterLevel; // 캐릭터 레벨 텍스트 (추후 업데이트용)

    [SerializeField] private GameObject m_blockObjcet;
    [SerializeField] private GameObject m_blockLable;
    [SerializeField] private GameObject m_selecteLable;

    public override void UpdateContent(UserCharacterData itemData)
    {
        m_userCharacterData = itemData;

        m_data.GetCharacterSprite(Context.addressableManager ,targetImage: m_characterImage).Forget();

        m_characterName.text = m_data.characterName;

        // 필요 시 레벨 정보 업데이트 로직 추가 가능
        // m_characterLevel.text = $"Lv. {itemData.level}"; 

        if(Context.userCharacterDatas != null)
        {
            bool setCharacter = Context.userCharacterDatas.Any(x => x != null && m_userCharacterData.ID == x.ID);

            m_blockObjcet.SetActive(setCharacter);

            if (Context.selecteCharacterData != null)
            {
                m_blockLable.SetActive(Context.selecteCharacterData.ID != m_userCharacterData.ID);
                m_selecteLable.SetActive(Context.selecteCharacterData.ID == m_userCharacterData.ID);
            }
            else
            {
                m_blockLable.SetActive(setCharacter);
                m_selecteLable.SetActive(false);
            }
        }
    }

    public void OnClick()
    {
        Context.OnCellClicked.Invoke(m_userCharacterData);
    }

    private void OnDisable()
    {
        if (m_data != null)
        {
            m_data.UnloadAtlas(Context.addressableManager);
        }
        if (this == null) return;


        if(m_blockObjcet!= null)
            m_blockObjcet.SetActive(false);
    }
}