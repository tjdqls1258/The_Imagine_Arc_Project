using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class LobbyCharacter : UILobbyUpdate
{
    [Inject] private readonly ICSVProvider csvHelper;
    [Inject] private readonly AddressableManager addressable;

    private CancellationTokenSource cancelToken;
    private CharacterData lobbyCharacterData;

    [SerializeField] private float m_delayTime = 3f;
    [SerializeField] private float m_fadeTime = 0.1f;
    private bool isRunTalk = false;

    enum Images
    {
        CharacterImage,
    }

    protected override void Awake()
    {
        Bind<Image>(typeof(Images));
        Bind<TextMeshProUGUI>();
    }

    public override void UpdateFormLobby()
    {
        lobbyCharacterData = csvHelper.GetScriptData<CharacterDataList>().GetData(1);

        lobbyCharacterData.GetCharacterSprite(addressable, targetImage: Get<Image>((int)Images.CharacterImage)).Forget();

        Get<TextMeshProUGUI>().text = $"{lobbyCharacterData.characterName}의 대사";
        RestartChatMessage();
    }

    public override void CloseFormLobby()
    {
        if (cancelToken != null)
        {
            cancelToken.Cancel();
            cancelToken.Dispose();
            cancelToken = null;
        }

        DOTween.Kill(this);
    }

    private async UniTask updateCharacterChatMessage(CancellationToken cancelToken)
    {
        while (cancelToken.IsCancellationRequested == false)
        {
            await UniTask.WaitForSeconds(m_delayTime, cancellationToken: cancelToken);

            SayRandom();

            await UniTask.WaitForSeconds(m_delayTime, cancellationToken: cancelToken);

            Get<TextMeshProUGUI>().DOFade(0, m_fadeTime);
        }
    }

    private void RestartChatMessage()
    {
        if (cancelToken != null)
        {
            cancelToken.Cancel();
            cancelToken.Dispose();
        }

        cancelToken = new CancellationTokenSource();

        updateCharacterChatMessage(cancelToken.Token).Forget();
    }

    public void OnClickCharacter()
    {
        SayRandom();
        RestartChatMessage();
    }

    private void SayRandom()
    {
        int index = Random.Range(0, 5);
        Get<TextMeshProUGUI>().text = $"{lobbyCharacterData.characterName}의 {index}번째 대사";

        Get<TextMeshProUGUI>().DOFade(1, m_fadeTime);
    }
}