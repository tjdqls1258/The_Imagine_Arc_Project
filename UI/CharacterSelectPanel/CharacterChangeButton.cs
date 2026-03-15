using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// 덱 설정 화면의 캐릭터 슬롯 버튼을 관리하는 클래스입니다.
/// 캐릭터의 이미지 표시, 데이터 바인딩 및 클릭 이벤트를 처리합니다.
/// </summary>
public class CharacterChangeButton : CachObject
{
    // ====== 컴포넌트 캐싱 (Lazy Loading) ======

    private Button m_button;
    /// <summary> 버튼 컴포넌트가 없을 경우 자동으로 찾아 캐싱하는 프로퍼티 </summary>
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
    /// <summary> 이미지 컴포넌트가 없을 경우 자동으로 찾아 캐싱하는 프로퍼티 </summary>
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

    // ====== 상태 및 데이터 ======

    /// <summary> 이 버튼 슬롯에 할당된 유저 캐릭터 데이터 </summary>
    private NetExcute.UserCharacterData m_userCharacterData;

    /// <summary> 덱 내에서 이 슬롯이 위치한 고유 인덱스 번호 </summary>
    public int prefabIndex { get; private set; } = 0;

    // ----------------------------------------------------------------------
    // ## Initialization (초기화)
    // ----------------------------------------------------------------------

    private void Awake()
    {
        // 필요 시 추가 텍스트(레벨, 이름) 바인딩 로직 구현 가능
    }

    /// <summary>
    /// 버튼에 인덱스를 부여하고 클릭 시 실행할 액션을 등록합니다.
    /// </summary>
    /// <param name="action">클릭 시 실행될 외부 콜백 (CharacterSelectPanel 등에서 주입)</param>
    /// <param name="index">슬롯 번호</param>
    public void Init(UnityAction<CharacterChangeButton> action, int index)
    {
        prefabIndex = index;
        SettingButtonAction(action);
    }

    /// <summary>
    /// 버튼의 클릭 이벤트를 설정합니다. 기존 리스너를 제거하고 새로운 액션을 할당합니다.
    /// </summary>
    private void SettingButtonAction(UnityAction<CharacterChangeButton> action)
    {
        button.onClick.RemoveListener(OnClick);
        button.onClick.AddListener(OnClick);

        // 로컬 함수를 통한 클릭 이벤트 캡슐화
        void OnClick()
        {
            action?.Invoke(this);
        }
    }

    // ----------------------------------------------------------------------
    // ## UI Update (데이터 바인딩 및 비동기 연출)
    // ----------------------------------------------------------------------

    /// <summary>
    /// 슬롯에 캐릭터 데이터를 설정하고 외형(이미지)을 갱신합니다.
    /// </summary>
    /// <param name="characterData">할당할 캐릭터 데이터 (null일 경우 빈 슬롯)</param>
    public async UniTask SettingPrefab(NetExcute.UserCharacterData characterData)
    {
        m_userCharacterData = characterData;
        await SettingImage(); // 이미지 비동기 로드 시작
    }

    /// <summary>
    /// [비동기] 캐릭터 데이터를 기반으로 스프라이트를 로드하여 이미지 컴포넌트에 적용합니다.
    /// </summary>
    private async UniTask SettingImage()
    {
        // 데이터가 없는 경우 빈 슬롯 처리
        if (m_userCharacterData == null)
        {
            EmptyCharacter();
            return;
        }

        // 마스터 데이터(CharacterData)를 통해 Addressables 스프라이트를 비동기 로드 및 할당
        await m_userCharacterData.GetCharacterData().GetCharacterSprite(targetImage: Image);
    }

    /// <summary>
    /// 슬롯이 비어있을 때의 연출을 처리합니다 (이미지 제거 등).
    /// </summary>
    private void EmptyCharacter()
    {
        Image.sprite = null;
    }

    /// <summary>
    /// 현재 슬롯에 할당된 유저 캐릭터 데이터를 반환합니다.
    /// </summary>
    public NetExcute.UserCharacterData GetCharacterData()
    {
        return m_userCharacterData;
    }
}