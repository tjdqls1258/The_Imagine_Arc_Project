using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class GameBootStart
{
    private readonly AddressableManager m_addressableManager;
    private readonly CSVHelper m_csvHelper;
    private readonly PopupManager m_popupManager;
    private readonly SceneLoadManager m_sceneLoadManager;
    private readonly UIManager m_uiManager;
    private readonly UserDataManager m_dataManager;
    private readonly SoundManager m_soundManager;

    [Inject]
    public GameBootStart(
        AddressableManager addressableManager,
        CSVHelper csvHelper,
        PopupManager popupManager,
        SceneLoadManager sceneLoadManager,
        UIManager uiManager,
        UserDataManager dataManager,
        SoundManager soundManager)
    {
        m_addressableManager = addressableManager;
        m_csvHelper = csvHelper;
        m_popupManager = popupManager;
        m_sceneLoadManager = sceneLoadManager;
        m_uiManager = uiManager;
        m_dataManager = dataManager;
        m_soundManager = soundManager;
    }


    private Transform m_managerParent;

    public void InitBaseSystems(Transform parent)
    {
        m_managerParent = parent;
        m_sceneLoadManager.Init();
        m_popupManager.Init();
        m_soundManager.Init();
        m_uiManager.Init();
        m_dataManager.Init();

        //로컬 데이터 (O, X 팝업, 에러 팝업 등)
        m_csvHelper.InitLocalData();
        m_popupManager.InitLocalData();
    }

    public async UniTask StartAsyncBeforeAddressableLoad(CancellationToken cancelToken)
    {
        await m_popupManager.SettingPopupDataAsync();
        await m_csvHelper.InitCSVDataAsync();
        await m_uiManager.LoadMasterCanvasAsync(m_managerParent);
        await AsyncLoadUserDataAsync();
        await m_uiManager.AutoUIManager.LoadJsonAsync();
    }

    public async UniTask AddressableInitAsync()
    {
        await m_addressableManager.InitAsync();
    }

    //Data Load
    private async UniTask AsyncLoadUserDataAsync()
    {
        if (m_dataManager.hasSaveData)
            m_dataManager.LoadUserData();
        else
            m_dataManager.InitDefaultData();

        await m_dataManager.AsyncLoadUserData();

        (m_dataManager.GetUserData<UserSettingData>() as UserSettingData).SetSoundData(m_soundManager);
    }
}
