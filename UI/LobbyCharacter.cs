using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 로비 화면에서 배치된 캐릭터의 이미지와 대사 시스템을 제어하는 클래스입니다.
/// 일정 시간마다 대사를 변경하거나 클릭 시 반응하는 기능을 담당합니다.
/// </summary>
public class LobbyCharacter : UILobbyUpdate
{
    // ====== Runtime State & Settings ======
    private CancellationTokenSource cancelToken; // 비동기 대사 루프를 취소하기 위한 토큰
    private CharacterData lobbyCharacterData;    // 로비에 표시될 캐릭터 데이터

    [SerializeField] private float m_delayTime = 3f; // 대사 출력 및 유지 간격
    [SerializeField] private float m_fadeTime = 0.1f; // 대사 텍스트 페이드 연출 시간
    private bool isRunTalk = false;

    // ====== UI Binding Enums (CachObject 시스템 활용) ======
    enum Images
    {
        CharacterImage, // 캐릭터 일러스트 Image
    }

    enum TextTMP
    {
        CharacterText,  // 캐릭터 대사 표시용 TextMeshProUGUI
    }

    // ----------------------------------------------------------------------
    // ## Initialization (Lifecycle)
    // ----------------------------------------------------------------------

    protected override void Awake()
    {
        // 1. 컴포넌트 자동 바인딩
        Bind<Image>(typeof(Images));
        Bind<TextMeshProUGUI>(typeof(TextTMP));
    }

    /// <summary>
    /// 로비 패널이 활성화되거나 업데이트될 때 호출되는 초기화 메서드입니다.
    /// </summary>
    public override void UpdateFormLobby()
    {
        // 테스트용으로 ID 1번 캐릭터 데이터를 로드 (추후 유저 설정 메인 캐릭터 ID로 변경 가능)
        lobbyCharacterData = GameMaster.Instance.csvHelper.GetScripteData<CharacterDataList>().GetData(1);

        // 캐릭터 이미지 비동기 로드 및 적용
        lobbyCharacterData.GetCharacterSprite(targetImage: Get<Image>((int)Images.CharacterImage)).Forget();

        // 기본 대사 설정 및 자동 대사 루프 시작
        Get<TextMeshProUGUI>((int)TextTMP.CharacterText).text = $"{lobbyCharacterData.characterName}의 대사";
        RestartChatMessage();
    }

    /// <summary>
    /// 로비 화면이 닫히거나 패널이 전환될 때 호출되어 리소스를 정리합니다.
    /// </summary>
    public override void CloseFormLobby()
    {
        // 실행 중인 비동기 루프 중단 및 메모리 해제
        if (cancelToken != null)
        {
            cancelToken.Cancel();
            cancelToken.Dispose();
            cancelToken = null;
        }

        // 해당 객체에서 동작 중인 모든 트윈 애니메이션 정지
        DOTween.Kill(this);
    }

    // ----------------------------------------------------------------------
    // ## Chat Message Logic (Async Loop)
    // ----------------------------------------------------------------------

    /// <summary>
    /// [비동기] 주기적으로 캐릭터 대사를 랜덤하게 변경하고 페이드 연출을 수행하는 루프입니다.
    /// </summary>
    private async UniTask updateCharacterChatMessage(CancellationToken cancelToken)
    {
        while (cancelToken.IsCancellationRequested == false)
        {
            // 1. 대사 출력 전 대기
            await UniTask.WaitForSeconds(m_delayTime, cancellationToken: cancelToken);

            // 2. 랜덤 대사 출력 및 페이드 인
            SayRandom();

            // 3. 대사 유지 시간 대기
            await UniTask.WaitForSeconds(m_delayTime, cancellationToken: cancelToken);

            // 4. 대사 텍스트 페이드 아웃 (투명하게 만들기)
            Get<TextMeshProUGUI>((int)TextTMP.CharacterText).DOFade(0, m_fadeTime);
        }
    }

    /// <summary>
    /// 기존 대사 루틴을 초기화하고 새롭게 시작합니다.
    /// </summary>
    private void RestartChatMessage()
    {
        // 기존 토큰 취소 및 재생성 (중복 실행 방지)
        if (cancelToken != null)
        {
            cancelToken.Cancel();
            cancelToken.Dispose();
        }

        cancelToken = new CancellationTokenSource();

        // 비동기 루프 시작 (Fire and Forget)
        updateCharacterChatMessage(cancelToken.Token).Forget();
    }

    // ----------------------------------------------------------------------
    // ## Interaction
    // ----------------------------------------------------------------------

    /// <summary>
    /// 유저가 로비의 캐릭터를 클릭했을 때 호출됩니다.
    /// 즉시 대사를 출력하고 자동 대사 타이머를 재설정합니다.
    /// </summary>
    public void OnClickCharacter()
    {
        SayRandom();
        RestartChatMessage();
    }

    /// <summary>
    /// 랜덤한 인덱스의 대사를 선택하여 텍스트를 갱신하고 페이드 인 연출을 보여줍니다.
    /// </summary>
    private void SayRandom()
    {
        int index = Random.Range(0, 5);
        Get<TextMeshProUGUI>((int)TextTMP.CharacterText).text = $"{lobbyCharacterData.characterName}의 {index}번째 대사";

        // 텍스트를 다시 불투명하게 만듦
        Get<TextMeshProUGUI>((int)TextTMP.CharacterText).DOFade(1, m_fadeTime);
    }
}