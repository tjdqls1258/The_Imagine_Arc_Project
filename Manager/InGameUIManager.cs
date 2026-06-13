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
    [Inject] private readonly CSVHelper csvHelper;

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

    public async UniTask SetInGameData(UserCharacterData[] characterDatas)
    {
        m_endPanel.gameObject.SetActive(false);
        Debug.Log("Game Data Test Setting");

        List<InGameCharacterData> characterDeckList = new();

        foreach (var characterData in characterDatas)
        {
            if (characterData == null) continue;
            await SetCharacterData(characterData);
        }

        inGameManager = FindAnyObjectByType<InGameManager>();

        inGameManager.goodsSystem.CurrentCost.Subscribe(cost =>
        {
            m_inGameView.UpdateCostDisplay(cost);
        }).AddTo(this);

        m_inGameView.CreateUnitButtons(characterDeckList.ToArray(), this, addressableManager);

        Get<OnClickCharacterPaenl>(0).SetInGameManager(inGameManager);

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