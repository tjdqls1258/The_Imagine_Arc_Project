using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CharacterChangeButton : CachObject
{
    private AddressableManager addressable;
    private ICSVProvider csvHelper;
    private Button m_button;
    private Button button
    {
        set { m_button = value; }
        get
        {
            if (m_button == null)
                m_button = GetComponent<Button>();
            return m_button;
        }
    }

    private Image m_Image;
    private Image Image
    {
        set { m_Image = value; }
        get
        {
            if (m_Image == null)
                m_Image = GetComponent<Image>();
            return m_Image;
        }
    }

    private NetExcute.UserCharacterData m_userCharacterData;

    public int prefabIndex { get; private set; } = 0;

    public void Init(AddressableManager addressable, ICSVProvider csvHelper, UnityAction<CharacterChangeButton> action, int index)
    {
        this.csvHelper = csvHelper;
        this.addressable = addressable;
        prefabIndex = index;
        SettingButtonAction(action);
    }

    private void SettingButtonAction(UnityAction<CharacterChangeButton> action)
    {
        button.onClick.RemoveListener(OnClick);
        button.onClick.AddListener(OnClick);

        void OnClick()
        {
            action?.Invoke(this);
        }
    }

    public async UniTask SettingPrefab(NetExcute.UserCharacterData characterData)
    {
        m_userCharacterData = characterData;
        await SettingImage(); // 이미지 비동기 로드 시작
    }

    private async UniTask SettingImage()
    {
        if (m_userCharacterData == null)
        {
            EmptyCharacter();
            return;
        }

        await m_userCharacterData.GetCharacterData(csvHelper).GetCharacterSprite(addressable, targetImage: Image);
    }

    private void EmptyCharacter()
    {
        Image.sprite = null;
    }

    public NetExcute.UserCharacterData GetCharacterData()
    {
        return m_userCharacterData;
    }
}