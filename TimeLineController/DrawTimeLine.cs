using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

/// <summary>
/// 캐릭터 획득/뽑기 연출을 담당하는 타임라인 컨트롤러 클래스입니다.
/// 여러 캐릭터의 연출을 순차적으로 보여주거나 스킵하는 기능을 포함합니다.
/// </summary>
public class DrawTimeLine : TimeLineController
{
    // ====== Inspector Settings ======
    [Header("UI References")]
    [SerializeField] private Image m_backImage;    // 연출 배경 이미지 (페이드용)
    [SerializeField] private Button m_skipButton; // 다음/스킵 버튼

    // ====== Runtime State ======
    private int currentIndex = 0;                  // 현재 보여주고 있는 캐릭터 인덱스
    private CharacterData[] m_drawCharacterList;   // 연출 대상 캐릭터 데이터 배열

    // ====== UI Binding Enums (CachObject 시스템 활용) ======
    enum DrawImage
    {
        Character, // 캐릭터 일러스트 표시용 Image
    }

    enum DrawTextMeshProUGUI
    {
        SayWhat    // 캐릭터 이름 및 획득 메시지 표시용 Text
    }

    // ----------------------------------------------------------------------
    // ## Initialization
    // ----------------------------------------------------------------------

    private void Awake()
    {
        // 1. CachObject 시스템을 이용한 UI 컴포넌트 자동 바인딩
        Bind<Image>(typeof(DrawImage));
        Bind<TextMeshProUGUI>(typeof(DrawTextMeshProUGUI));

        // 2. 타임라인 종료 이벤트(stopped)에 콜백 등록
        if (m_targetTimeLine != null)
            m_targetTimeLine.stopped += EndTimeLine;
    }

    /// <summary>
    /// 매니저 등에 의해 호출되는 초기화 메서드입니다. 버튼 이벤트를 연결합니다.
    /// </summary>
    public override void Init()
    {
        if (m_skipButton != null)
        {
            m_skipButton.onClick.RemoveAllListeners(); // 중복 등록 방지
            m_skipButton.onClick.AddListener(OnClickNextCharacter);
        }
    }

    // ----------------------------------------------------------------------
    // ## Core Logic: Drawing Character
    // ----------------------------------------------------------------------

    /// <summary>
    /// 캐릭터 뽑기 연출을 시작합니다.
    /// </summary>
    /// <param name="characterDatas">연출할 캐릭터 데이터 배열</param>
    public void DrawCharacter(CharacterData[] characterDatas)
    {
        currentIndex = 0;
        m_drawCharacterList = characterDatas;

        // 첫 번째 캐릭터 데이터 설정 후 타임라인 시작
        SetCharacterData(characterDatas[currentIndex]);
        StartTimeLine();
    }

    /// <summary>
    /// UI 요소를 특정 캐릭터의 데이터로 갱신합니다.
    /// </summary>
    public void SetCharacterData(CharacterData data)
    {
        // 비동기로 캐릭터 스프라이트 로드 및 이미지 적용
        data.GetCharacterSprite(targetImage: Get<Image>((int)DrawImage.Character)).Forget();

        // 캐릭터 획득 메시지 설정
        Get<TextMeshProUGUI>((int)DrawTextMeshProUGUI.SayWhat).text = $"{data.characterName} 추출 성공";
    }

    // ----------------------------------------------------------------------
    // ## Timeline Events (Timeline Signal 등에서 호출)
    // ----------------------------------------------------------------------

    /// <summary> 배경 오브젝트를 밝게 만듭니다 (타임라인 이벤트용). </summary>
    public void FadeInBackObject()
    {
        SetTimeLineTween(m_backImage.DOFade(1, 0.1f));
    }

    /// <summary> 배경 오브젝트를 어둡게 만듭니다 (타임라인 이벤트용). </summary>
    public void FadeOutBackObject()
    {
        SetTimeLineTween(m_backImage.DOFade(0, 0.1f));
    }

    // ----------------------------------------------------------------------
    // ## Navigation & Skip
    // ----------------------------------------------------------------------

    /// <summary> 다음 캐릭터 버튼 클릭 시 호출됩니다. </summary>
    public void OnClickNextCharacter()
    {
        NextCharacter();
    }

    /// <summary> 연출 전체를 즉시 중단하고 닫습니다. </summary>
    public void OnSkip()
    {
        StopTimeLine();
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 다음 캐릭터 연출로 넘어가거나, 더 이상 없으면 연출을 종료합니다.
    /// </summary>
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
            SetCharacterData(m_drawCharacterList[currentIndex]);
            StartTimeLine();
        }
        else
        {
            // 모든 연출 완료 시 오브젝트 비활성화
            gameObject.SetActive(false);
        }
    }

    // ----------------------------------------------------------------------
    // ## Lifecycle & Callbacks
    // ----------------------------------------------------------------------

    /// <summary>
    /// 타임라인 재생이 완전히 끝났을 때 유니티 시스템에 의해 호출되는 콜백입니다.
    /// </summary>
    private void EndTimeLine(PlayableDirector pt)
    {
        if (pt == m_targetTimeLine)
        {
            // 타임라인 재생 정지 및 내부 상태 정리
            StopTimeLine();
        }
    }
}