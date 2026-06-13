using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

public class DrawTimeLine : TimeLineController
{
    [Header("UI References")]
    [SerializeField] private Image m_backImage;    // 연출 배경 이미지 (페이드용)
    [SerializeField] private Button m_skipButton; // 다음/스킵 버튼

    private int currentIndex = 0;                  // 현재 보여주고 있는 캐릭터 인덱스
    private CharacterData[] m_drawCharacterList;   // 연출 대상 캐릭터 데이터 배열
    private AddressableManager addressable;


    enum DrawImage
    {
        Character, 
    }

    enum DrawTextMeshProUGUI
    {
        SayWhat   
    }

    private void Awake()
    {
        Bind<Image>(typeof(DrawImage));
        Bind<TextMeshProUGUI>(typeof(DrawTextMeshProUGUI));

        if (m_targetTimeLine != null)
            m_targetTimeLine.stopped += EndTimeLine;
    }

    public override void Init()
    {
        if (m_skipButton != null)
        {
            m_skipButton.onClick.RemoveAllListeners(); // 중복 등록 방지
            m_skipButton.onClick.AddListener(OnClickNextCharacter);
        }
    }

    public void DrawCharacter(CharacterData[] characterDatas, AddressableManager addressable)
    {
        currentIndex = 0;
        m_drawCharacterList = characterDatas;
        this.addressable = addressable;

        SetCharacterData(characterDatas[currentIndex], StartTimeLine).Forget();
    }


    public async UniTask SetCharacterData(CharacterData data, Action SettingDoneAction)
    {
        // 비동기로 캐릭터 스프라이트 로드 및 이미지 적용
        await data.GetCharacterSprite(addressable, targetImage: Get<Image>((int)DrawImage.Character));

        if (SettingDoneAction != null) SettingDoneAction.Invoke();

        // 캐릭터 획득 메시지 설정
        Get<TextMeshProUGUI>((int)DrawTextMeshProUGUI.SayWhat).text = $"{data.characterName} 추출 성공";
    }

    public void FadeInBackObject()
    {
        SetTimeLineTween(m_backImage.DOFade(1, 0.1f));
    }

    /// <summary> 배경 오브젝트를 어둡게 만듭니다 (타임라인 이벤트용). </summary>
    public void FadeOutBackObject()
    {
        SetTimeLineTween(m_backImage.DOFade(0, 0.1f));
    }

    public void OnClickNextCharacter()
    {
        NextCharacter();
    }

    public void OnSkip()
    {
        StopTimeLine();
        gameObject.SetActive(false);
    }

    public void NextCharacter()
    {
        // 다음 보여줄 캐릭터가 리스트에 남아있는지 확인
        if (m_drawCharacterList.Length > currentIndex + 1)
        {
            currentIndex += 1;

            // 현재 연출 중지 및 타임라인 시간 리셋
            StopTimeLine();
            m_targetTimeLine.time = 0;

            // 데이터 갱신 후 타임라인 재시작
            SetCharacterData(m_drawCharacterList[currentIndex], StartTimeLine).Forget();
        }
        else
        {
            // 모든 연출 완료 시 오브젝트 비활성화
            gameObject.SetActive(false);
        }
    }

    private void EndTimeLine(PlayableDirector pt)
    {
        if (pt == m_targetTimeLine)
        {
            // 타임라인 재생 정지 및 내부 상태 정리
            StopTimeLine();
        }
    }
}