using Cysharp.Threading.Tasks;
using DG.Tweening;
using NetExcute;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class CharacterDetail : CacheObject
{
    enum Images { CharacterImage }
    enum Texts { StateLV_Text, State_Text }
    enum Buttons { Close }
    enum Transforms { PassiveLayout, ActiveLayout }

    [Inject] private AddressableManager addressable;
    [Inject] private GrowthManager growthManager;
    [Inject] private ICSVProvider csvHelper;

    private CharacterDetailViewModel m_viewModel;

    private CanvasGroup m_group;
    private float m_fadeTime = 0.3f;

    private CharacterSkillCell m_baseSkillCellPrefab;
    private List<CharacterSkillCell> m_activeSkillCellList = new();
    private List<CharacterSkillCell> m_passiveSkillCellList = new();

    private void Awake()
    {
        m_group = GetComponent<CanvasGroup>();
        Bind<Image>(typeof(Images));
        Bind<TextMeshProUGUI>(typeof(Texts));
        Bind<Button>(typeof(Buttons));
        Bind<Transform>(typeof(Transforms));
        Bind<CharacterSkillCell>();

        Get<Button>((int)Buttons.Close).onClick.AddListener(Close);
        m_baseSkillCellPrefab = Get<CharacterSkillCell>();
        m_activeSkillCellList.Add(m_baseSkillCellPrefab);

        m_group.alpha = 0;
        gameObject.SetActive(false);

        m_viewModel = new CharacterDetailViewModel();
        BindUI();
    }

    private void BindUI()
    {
        m_viewModel.LevelText.Bind(text => Get<TextMeshProUGUI>((int)Texts.StateLV_Text).text = text);
        m_viewModel.InfoText.Bind(text => Get<TextMeshProUGUI>((int)Texts.State_Text).text = text);

        m_viewModel.CharacterSprite.Bind(sprite =>
        {
            if (sprite != null)
            {
                Get<Image>((int)Images.CharacterImage).sprite = sprite;
                if (gameObject.activeSelf) m_group.DOFade(1, m_fadeTime);
            }
        });

        m_viewModel.ActiveSkills.Bind(skills => SetSkillList(skills, Get<Transform>((int)Transforms.ActiveLayout), m_activeSkillCellList).Forget());
        m_viewModel.PassiveSkills.Bind(skills => SetSkillList(skills, Get<Transform>((int)Transforms.PassiveLayout), m_passiveSkillCellList).Forget());
    }

    public void OnClickData(UserCharacterData data)
    {
        gameObject.SetActive(true);
        m_group.DOFade(1, m_fadeTime);

        m_viewModel.LoadDataAsync(data, addressable, growthManager, csvHelper).Forget();
    }

    public void Close()
    {
        m_group.DOFade(0, m_fadeTime).OnComplete(() =>
        {
            m_group.alpha = 0;
            gameObject.SetActive(false);
        });
    }


    private async UniTaskVoid SetSkillList(int[] skillList, Transform parent, List<CharacterSkillCell> baseList)
    {
        if (skillList == null) return;

        List<UniTask> taskList = new();

        for (int a = 0; a < skillList.Length; a++)
        {
            if (baseList.Count <= a)
            {
                var newCell = Instantiate(m_baseSkillCellPrefab, parent);
                baseList.Add(newCell);
            }

            baseList[a].transform.gameObject.SetActive(true);
            taskList.Add(baseList[a].SetSkill(skillList[a], addressable));
        }

        for (int i = skillList.Length; i < baseList.Count; i++)
        {
            baseList[i].gameObject.SetActive(false);
        }

        await UniTask.WhenAll(taskList);
    }
}