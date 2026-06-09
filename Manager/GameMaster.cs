using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

public class GameMaster : MonoSingleton<GameMaster>
{
#if UNITY_EDITOR
    public bool isAddressableLoad_Start = false;
#endif
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
    [SerializeField] private GameObject lodingObject;

    private string _uuid = string.Empty;
    public string UUID 
    {
        get
        {
            if(_uuid == string.Empty)
            {
                _uuid = PlayerPrefs.GetString("UUID", Guid.NewGuid().ToString());
                PlayerPrefs.SetString("UUID", _uuid);
            }

            return _uuid;
        }
        set
        {
            PlayerPrefs.SetString("UUID", value);
            _uuid = value;
        }
    }
    public CSVHelper csvHelper => m_csvHelper;

    private void Start()
    {
#if UNITY_EDITOR
        if(isAddressableLoad_Start)
            InitAddressableSystemAsync();
#endif
    }

    public void InitBaseSystems()
    {
        base.Init();
        popupManager.Init();
        soundManager.Init();
        popupManager.SettingLocalData();
    }

    public async UniTask InitAddressableSystemAsync()
    {
        addressableManager = new AddressableManager();
        await addressableManager.InitAsync();
    }

    public async UniTask InitAssetDependentSystemsAsync()
    {
        Logger.Log("Starting GameMaster System Initialization...");

        sceneLoadManager.Init();
        uiManager = new UIManager();
        dataManager = new UserDataManager();
        objectPoolManager = gameObject.AddComponent<ObjectPoolManager>();

        uiManager.SetLodingObject(lodingObject);

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

    public string GetUUID()
    {
        if (UUID == string.Empty)
        {
            UUID = PlayerPrefs.GetString("UUID", Guid.NewGuid().ToString());
            PlayerPrefs.SetString("UUID", UUID);
        }

        return UUID;
    }
}