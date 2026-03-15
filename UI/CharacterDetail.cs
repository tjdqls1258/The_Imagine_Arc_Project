using Cysharp.Threading.Tasks;
using DG.Tweening;
using NetExcute;
using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 캐릭터의 상세 정보를 표시하는 UI 패널입니다.
/// 캐릭터의 외형 일러스트, 레벨, 스탯 정보를 로드하고 페이드 연출을 통해 보여줍니다.
/// </summary>
public class CharacterDetail : CachObject
{
    // ====== UI Binding Enums (CachObject 시스템 활용) ======

    enum Images
    {
        CharacterImage, // 캐릭터 전신 일러스트 표시용
    }

    enum Texts
    {
        StateLV_Text,   // 캐릭터 레벨 텍스트
        State_Text      // 캐릭터 이름 및 상세 정보(Cost, Rating 등) 텍스트
    }

    enum Buttons
    {
        Close           // 패널 닫기 버튼
    }

    // ====== Runtime Variables ======

    private CanvasGroup m_group;       // 페이드 인/아웃 연출을 위한 컴포넌트
    private float m_fadeTime = 0.3f;   // UI 연출 시간
    private CharacterData m_characterData; // 현재 표시 중인 캐릭터의 원본 데이터
    private UserCharacterData m_userCharacterData; // 현재 표시 중인 캐릭터의 원본 데이터

    // ----------------------------------------------------------------------
    // ## Initialization (Lifecycle)
    // ----------------------------------------------------------------------

    private void Awake()
    {
        // 1. 필요한 컴포넌트 캐싱 및 바인딩
        m_group = GetComponent<CanvasGroup>();
        Bind<Image>(typeof(Images));
        Bind<TextMeshProUGUI>(typeof(Texts));
        Bind<Button>(typeof(Buttons));

        // 2. 버튼 이벤트 연결
        Get<Button>((int)Buttons.Close).onClick.AddListener(Close);

        // 3. 초기 상태 설정 (비활성화 및 투명도 0)
        m_group.alpha = 0;
        gameObject.SetActive(false);
    }

    // ----------------------------------------------------------------------
    // ## Data Binding & UI Update
    // ----------------------------------------------------------------------

    /// <summary>
    /// 특정 캐릭터 데이터를 전달받아 상세 창을 활성화하고 정보를 갱신합니다.
    /// </summary>
    /// <param name="data">표시할 캐릭터 데이터</param>
    public void OnClickData(UserCharacterData data)
    {
        // UI 활성화 및 페이드 인 시작
        gameObject.SetActive(true);
        m_group.DOFade(1, m_fadeTime);

        m_userCharacterData = data;
        m_characterData = data.GetCharacterData();

        // 비동기로 이미지 로드 시작
        WaitLoadImage().Forget();

        // 텍스트 정보 업데이트
        Get<TextMeshProUGUI>((int)Texts.StateLV_Text).text = $"LV. {m_userCharacterData.level}";
        Get<TextMeshProUGUI>((int)Texts.State_Text).text =
            $"{m_characterData.characterName} data Not Ready\nCost : {m_characterData.cost}\nRating : {m_characterData.rating}\ntest Data : {m_characterData.characterState.maxHp}";
    }

    /// <summary>
    /// [비동기] 어드레서블 시스템을 통해 캐릭터 스프라이트를 로드하고 적용합니다.
    /// </summary>
    private async UniTask WaitLoadImage()
    {
        // 캐릭터 아틀라스에서 스프라이트를 로드하여 Image 컴포넌트에 할당
        await m_characterData.GetCharacterSprite((sp) => Get<Image>((int)Images.CharacterImage).sprite = sp);

        // 이미지 로드 완료 후 다시 한번 페이드 상태를 확인 (부드러운 노출)
        if (gameObject != null && m_group != null)
        {
            gameObject.SetActive(true);
            m_group.DOFade(1, m_fadeTime);
        }
    }

    // ----------------------------------------------------------------------
    // ## UI Control
    // ----------------------------------------------------------------------

    /// <summary>
    /// 상세 패널을 페이드 아웃 연출과 함께 비활성화합니다.
    /// </summary>
    public void Close()
    {
        // 페이드 아웃 연출이 끝난 후 게임 오브젝트를 비활성화하도록 콜백 설정
        m_group.DOFade(0, m_fadeTime).OnComplete(() =>
        {
            m_group.alpha = 0;
            gameObject.SetActive(false);
        });
    }
}