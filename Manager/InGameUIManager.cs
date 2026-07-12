using Cysharp.Threading.Tasks;
using NetExcute;
using System.Collections.Generic;
using UnityEngine;
using VContainer;
using UniRx;

public class InGameUIManager : UIBaseFormMaker
{
    [Inject] private readonly AddressableManager addressableManager;
    [Inject] private readonly SceneLoadManager sceneLoadManager;
    [Inject] private readonly ObjectPoolManager objectPoolManager;
    [Inject] private readonly ICSVProvider csvHelper;
    [Inject] private readonly GrowthManager growthManager;

    [Header("UI Components")]
    [SerializeField] private InGameUIView m_inGameView;
    [SerializeField] private GameEndPanel m_endPanel;

    public InGameManager inGameManager { get; private set; }

    private Camera m_camera;
    public Camera mainCamera
    {
        get
        {
            if (m_camera == null)
                m_camera = Camera.main;

            return m_camera;
        }
    }

    enum OnClickSettingPanel
    {
        OnClickSettingPanel,
    }

    protected override void Awake()
    {
        base.Awake();

        Bind<OnClickCharacterPaenl>(typeof(OnClickSettingPanel));
    }

    public void StartGame()
    {
        m_inGameView.StartGame();
    }

    public async UniTask SetInGameData(UserCharacterData[] characterDatas, List<int> userSkillList)
    {
        m_endPanel.gameObject.SetActive(false);
        Logger.Log("Game Data Test Setting");

        List<InGameCharacterData> characterDeckList = new();
        List<UserSkillBase> userSkillBases = new();

        foreach (var characterData in characterDatas)
        {
            if (characterData == null) continue;
            await SetCharacterData(characterData);
        }

        await SetUserSkillData(userSkillList);

        inGameManager = FindAnyObjectByType<InGameManager>();
        inGameManager.goodsSystem.CurrentCost.Subscribe(cost =>
        {
            m_inGameView.UpdateCostDisplay(cost);
        }).AddTo(this);

        m_inGameView.CreateUnitButtons(characterDeckList.ToArray(), this, addressableManager);
        m_inGameView.CreateUserSkillButtons(userSkillBases.ToArray(), this, addressableManager);

        Get<OnClickCharacterPaenl>(0).SetInGameManager(inGameManager);

        m_inGameView.SubjectGameTextValue(inGameManager.stageRule.lifeEvent, inGameManager.GetComponent<EnemySpawnManager>().currentCount);
        
        m_inGameView.UISettingDone();

        async UniTask SetCharacterData(NetExcute.UserCharacterData data)
        {
            var characterData = data.GetCharacterData(csvHelper);
            var activeSkillID = characterData.activeSkill[data.activeSkillID];
            var passiveSkillID = characterData.passiveSkill[data.passiveSkillID];
            var nomalAtkDataID = characterData.nomalAtk;

            var activeSkillData = await addressableManager.LoadAssetAndCacheAsync<SkillBase>(string.Format(Util.CHARACTER_SKILL_PATH, activeSkillID));
            var passiveSkillData = await addressableManager.LoadAssetAndCacheAsync<SkillBase>(string.Format(Util.CHARACTER_SKILL_PATH, passiveSkillID));
            var nomalAtkData = await addressableManager.LoadAssetAndCacheAsync<SkillBase>(string.Format(Util.CHARACTER_SKILL_PATH, nomalAtkDataID));
            InGameCharacterData ingameData = new InGameCharacterData(characterData, data, nomalAtkData, passiveSkillData, activeSkillData);

            characterDeckList.Add(ingameData);
        }

        async UniTask SetUserSkillData(List<int> userSkillIDs)
        {
            foreach (var id in userSkillIDs)
            {
                var skillData = await addressableManager.LoadAssetAndCacheAsync<UserSkillBase>(string.Format(Util.USER_SKILL_PATH, id));
                userSkillBases.Add(skillData);
            }
        }
    }

    public void OnClickCharacter(InGameCharacterData characterData, TileClickEvent tileClickActions)
    {
        Get<OnClickCharacterPaenl>(0).OnClickCharacter(characterData, tileClickActions);
    }

    public void EndGame(bool isWin)
    {
        // TODO: 결과 리포트 Web 통신 등

        m_endPanel.ResultGame(isWin, new ItemData[]
        {
            new() { itemID = 0, count = 1 },
            new() { itemID = 1, count = 2 },
            new() { itemID = 2, count = 3 }
        });
    }

    public void ExitGame()
    {
        Get<OnClickCharacterPaenl>(0).ClosePanel();
        m_inGameView.Clear();

        inGameManager.ExitGame();
        inGameManager = null;

        sceneLoadManager.SceneLoad(SceneInfo.SceneType.HomeScene).Forget();
        objectPoolManager.ClearNullPoolObject();
    }
}