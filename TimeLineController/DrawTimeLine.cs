using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

public class DrawTimeLine : TimeLineController
{
    [SerializeField] private Image m_backImage;
    [SerializeField] private Button m_skipButton;
    private int currentIndex = 0;
    private CharacterData[] m_drawCharacterList;
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

        m_targetTimeLine.stopped += EndTimeLine;
    }

    public override void Init()
    {
        if(m_skipButton != null)
            m_skipButton.onClick.AddListener(OnClickNextCharacter);
    }

    public void DrawCharacter(CharacterData[] characterDatas)
    {
        currentIndex = 0;
        m_drawCharacterList = characterDatas;

        SetCharacterData(characterDatas[currentIndex]);
        StartTimeLine();
    }

    public void SetCharacterData(CharacterData data)
    {
        Get<Image>((int)DrawImage.Character).sprite = data.GetCharacterSprite();
        Get<TextMeshProUGUI>((int)DrawTextMeshProUGUI.SayWhat).text = $"{data.characterName} 추출 성공";
    }

    public void FadeInBackObject()
    {
        SetTimeLineTween(m_backImage.DOFade(1, 0.1f));
    }

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
        if(m_drawCharacterList.Length > currentIndex + 1)
        {
            currentIndex += 1;
            StopTimeLine();
            m_targetTimeLine.time = 0;
            SetCharacterData(m_drawCharacterList[currentIndex]);
            StartTimeLine();
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private void EndTimeLine(PlayableDirector pt)
    {
        if(pt == m_targetTimeLine)
        {
            StopTimeLine();
        }
    }
}
