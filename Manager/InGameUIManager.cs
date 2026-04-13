using Cysharp.Threading.Tasks;
using NetExcute;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 인게임 전투 화면의 UI 요소를 제어하고 관리하는 클래스입니다.
/// 캐릭터 배치 버튼 생성, 실시간 코스트 표시 업데이트, 상세 정보 패널 호출 등을 담당합니다.
/// </summary>
public class InGameUIManager : UIBaseFormMaker
{
    [Header("UI Components")]
    [SerializeField] private InGameUIView m_inGameView;
    [SerializeField] private GameEndPanel m_endPanel;

    public InGameManager m_inGameManager { get; private set; }

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

    public async Task SetInGameData(UserCharacterData[] characterDatas)
    {
        m_endPanel.gameObject.SetActive(false);
        Logger.Log("Game Data Test Setting");

        List<InGameCharacterData> characterDeckList = new();

        foreach (var characterData in characterDatas)
        {
            if (characterData == null) continue;
            await SetCharacterData(characterData);
        }

        m_inGameManager = FindAnyObjectByType<InGameManager>();

        m_inGameManager.SetChargeAction(m_inGameView.UpdateCostDisplay);
        m_inGameManager.StartGame();

        m_inGameView.CreateUnitButtons(characterDeckList.ToArray(), this);
        m_inGameView.UpdateCostDisplay((m_inGameManager.GetCurrentCost()));

        Get<OnClickCharacterPaenl>(0).SetInGameManager(m_inGameManager);

        async UniTask SetCharacterData(NetExcute.UserCharacterData data)
        {
            var characterData = data.GetCharacterData();
            var activeSkill = await GameMaster.Instance.addressableManager.LoadAssetAndCacheAsync<SkillBase>(string.Format(Util.CHARACTER_SKILL_PATH, data.activeSkillID));
            var passiveSkill = await GameMaster.Instance.addressableManager.LoadAssetAndCacheAsync<SkillBase>(string.Format(Util.CHARACTER_SKILL_PATH, data.passiveSkillID));

            InGameCharacterData ingameData = new InGameCharacterData(characterData, data, passiveSkill, activeSkill);

            characterDeckList.Add(ingameData);
        }
    }

    public void OnClickCharacter(InGameCharacterData characterData, TileClickEvent tileClickActions)
    {
        Get<OnClickCharacterPaenl>(0).OnClickCharacter(characterData, tileClickActions);
    }

    public void EndGame(bool isWin)
    {
        //TODO 결과 관련 Web통신

        //임시 데이터
        m_endPanel.ResultGame(isWin, new ItemData[]
        {
            new()
            {
                itemID = 0,
                count = 1,
            },
            new()
            {
                itemID = 1,
                count = 2,
            },
            new()
            {
                itemID = 2,
                count = 3,
            }
        });
    }

    public void ExitGame()
    {
        Get<OnClickCharacterPaenl>(0).ClosePanel();
        m_inGameView.Clear();

        m_inGameManager.ExitGame();

        m_inGameManager = null;

        GameMaster.Instance.sceneLoadManager.SceneLoad(SceneInfo.SceneType.HomeScene).Forget();
        GameMaster.Instance.objectPoolManager.ClearNullPoolObject();
    }
}