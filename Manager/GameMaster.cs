using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

public class GameMaster : MonoSingleton<GameMaster>
{
    // ====== Constants & Labels ======
    public readonly string[] ADDRESSABLE_LABEL = { "InGameData", "SpriteAltas", "JsonData", "UI", "CharacterSprite" };

    // ====== Members ======
    private CSVHelper m_csvHelper = new();

    // ====== Managers ======
    public SoundManager soundManager;
    public AddressableManager addressableManager;
    public SceneLoadManager sceneLoadManager;
    public UIManager uiManager;
    public PopupManager popupManager;
    public UserDataManager dataManager;
    [HideInInspector] public ObjectPoolManager objectPoolManager;
    public CSVHelper csvHelper => m_csvHelper;

    // ----------------------------------------------------------------------
    // [단계 1] 어드레서블 이전 초기화 (로컬 데이터 기반)
    // ----------------------------------------------------------------------
    public void InitBaseSystems()
    {
        base.Init();

        // 어드레서블과 무관하게 최초로 세팅되어야 하는 로컬 팝업/데이터
        popupManager.Init();
        soundManager.Init();
        popupManager.SettingLocalData();
    }

    // ----------------------------------------------------------------------
    // [단계 2] 어드레서블 단독 초기화
    // ----------------------------------------------------------------------
    public async UniTask InitAddressableSystemAsync()
    {
        addressableManager = new AddressableManager();
        await addressableManager.InitAsync();
    }

    // ----------------------------------------------------------------------
    // [단계 3] 어드레서블 이후 초기화 (에셋 로드 종속성)
    // ----------------------------------------------------------------------
    public async UniTask InitAssetDependentSystemsAsync()
    {
        Logger.Log("Starting GameMaster System Initialization...");

        sceneLoadManager.Init();
        uiManager = new UIManager();
        dataManager = new UserDataManager();
        objectPoolManager = gameObject.AddComponent<ObjectPoolManager>();

        // 어드레서블 에셋(CSV, 프리팹 등)을 실제로 로드하는 구간
        await popupManager.SettingPopupDataAsync();
        await csvHelper.InitCSVDataAsync();
        await LoadBaseResource();
        await AsyncLoadUserData();
        await uiManager.AutoUIManager.LoadJsonAsync();

        Logger.Log("GameMaster Initialization Complete.");
    }

    private async UniTask LoadBaseResource()
    {
        List<UniTask> loadingTasks = new List<UniTask>();
        var masterCanvasLoader = uiManager.LoadMasterCanvasAsync(transform);
        loadingTasks.Add(masterCanvasLoader);
        await UniTask.WhenAll(loadingTasks);
    }

    // ----------------------------------------------------------------------
    // ## Data & Exit Logic
    // ----------------------------------------------------------------------
    private async UniTask AsyncLoadUserData()
    {
        dataManager.Init();

        if (dataManager.hasSaveData)
            dataManager.LoadUserData();
        else
            dataManager.InitDefaultData();

        await dataManager.AsyncLoadUserData();
    }

    public async UniTask ExitGamePopup()
    {
        var popup = await popupManager.ShowPopup(PopupManager.PopupType.PopupMsg);
        if (popup != null)
        {
            popup.closeAction += ExitGame;
        }
    }

    public void ExitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public string GetUUID() => PlayerPrefs.GetString("UUID", Guid.NewGuid().ToString());
}