using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using UniRx;
using UnityEngine;
using VContainer;

public class UIManager
{
    public readonly struct LoadingEvent
    {
        public bool IsShow { get; }

        public LoadingEvent(bool isShow)
        {
            IsShow = isShow;
        }
    }

    private const string UIPATH_FORMAT = "UIPanel/{0}.prefab";

    [Inject] private readonly AddressableManager addressableManager;
    CancellationTokenSource destoryToken = new CancellationTokenSource();

    public enum UISequence
    {
        None = -1,
        StageSeletePanel,
        CharacterListPanel,
        ShopPanel,
        CharacterSelectPanel,
        SettingPanel,
    }

    private Transform MasterCanvas;
    private GameObject m_lodingImage;
    public AutoUIManager AutoUIManager { get; protected set; }

    public readonly Dictionary<UISequence, GameObject> m_openUIPool = new();
    public readonly Dictionary<UISequence, GameObject> m_closeUIPool = new();

    public void Init() 
    {
        SettingMessageEvent();
    }

    private void SettingMessageEvent()
    {
        MessageBroker.Default.Receive<UISequence>().Subscribe(CloseUI).AddTo(destoryToken.Token);
        MessageBroker.Default.Receive<LoadingEvent>().Subscribe(ShowLoadingImage).AddTo(destoryToken.Token);
    }

    public async UniTask LoadMasterCanvasAsync(Transform parent)
    {
        GameObject masterCanvasObj = await addressableManager.InstantiateObjectAsync("MasterCanvas", parent);

        if (masterCanvasObj == null)
        {
            Debug.LogError("[UIManager] Failed to load MasterCanvas.");
            return;
        }

        MasterCanvas = masterCanvasObj.transform;
        AutoUIManager = MasterCanvas.GetComponent<AutoUIManager>();

        if (AutoUIManager == null)
        {
            Debug.LogError("[UIManager] MasterCanvas is missing AutoUIManager component.");
        }
    }

    public async UniTask ShowUI(UISequence type, UIBaseData.UIType uiType = UIBaseData.UIType.MainUI)
    {
        if (m_closeUIPool.ContainsKey(type))
        {
            GameObject ui = m_closeUIPool[type];
            AutoUIManager.PushUI(uiType, ui.transform);

            ChangeItem(m_closeUIPool, m_openUIPool, type);
        }
        else
        {
            string path = string.Format(UIPATH_FORMAT, type.ToString());
            GameObject ui = await addressableManager.InstantiateObjectAsync(path, AutoUIManager.GetParent(uiType));

            if (ui == null)
            {
                Debug.LogError($"[UIManager] Failed to instantiate UI panel at: {path}");
                return;
            }

            m_openUIPool.Add(type, ui);
            ui.GetComponent<UIBase>().SetSequence(type);
        }

        m_openUIPool[type].GetComponent<UIBase>()?.ShowUI();
    }

    public void CloseUI(UISequence type)
    {
        if (m_openUIPool.ContainsKey(type))
        {
            GameObject ui = m_openUIPool[type];

            AutoUIManager.PopUI(ui.transform);

            ChangeItem(m_openUIPool, m_closeUIPool, type);
        }
        else
        {
            Debug.LogError($"{GetType().Name}::Cannot find UI panel in Open Pool: {type}");
        }
    }

    private void ChangeItem(Dictionary<UISequence, GameObject> where, Dictionary<UISequence, GameObject> from, UISequence Item)
    {
        if (where.ContainsKey(Item))
        {
            from.Add(Item, where[Item]);
            where.Remove(Item);
        }
    }

    public UIManager(GameObject lodingImage)
    {
        m_lodingImage = lodingImage;
    }

    public void ShowLoadingImage(LoadingEvent isShow)
    {
        if (m_lodingImage == null)
            return;

        m_lodingImage.SetActive(isShow.IsShow);
    }

    public AutoUIManager GetAutoUIManager() => AutoUIManager;

    ~UIManager()
    {
        destoryToken.Cancel();
    }
}