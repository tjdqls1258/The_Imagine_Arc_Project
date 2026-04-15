using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using static PopupManager;

public class PopupManager : MonoBehaviour
{
    [Serializable]
    public enum PopupType
    {
        None = 0,
        PopupMsg,
        PopupQ,
    }

    [Serializable]
    public class PopupData
    {
        public PopupType popupType;
        public string path;
    }

    private readonly Dictionary<PopupType, string> _popupDataMap = new();
    private readonly Stack<PopupBase> _stackPopup = new();
    private PopupBase _currentPopup = null;

    private const string POPUP_DATA_TABLE_KEY = "PopupDataTable";
    [SerializeField] private TextAsset m_localPopupDataText;

    private ToolTipBox m_tooltipBox;

    public void Init()
    {
        m_tooltipBox = GetComponentInChildren<ToolTipBox>(true);
    }

    public async UniTask SettingPopupDataAsync()
    {
        TextAsset popupDataTable =
            await GameMaster.Instance.addressableManager.LoadAssetAndCacheAsync<TextAsset>(POPUP_DATA_TABLE_KEY);

        if (popupDataTable == null)
        {
            Debug.LogError($"[PopupManager] Failed to load popup data table: {POPUP_DATA_TABLE_KEY}");
            return;
        }

        try
        {
            List<PopupData> data = JsonConvert.DeserializeObject<List<PopupData>>(popupDataTable.text);

            foreach (PopupData dataItem in data)
                _popupDataMap.Add(dataItem.popupType, dataItem.path);
        }
        catch (Exception e)
        {
            Debug.LogError($"[PopupManager] Failed to deserialize popup data table: {e.Message}");
        }
        finally
        {
            GameMaster.Instance.addressableManager.UnloadAsset(POPUP_DATA_TABLE_KEY);
        }
    }

    public void SettingLocalData()
    {
        try
        {
            List<PopupData> data = JsonConvert.DeserializeObject<List<PopupData>>(m_localPopupDataText.text);

            foreach (PopupData dataItem in data)
            {
                if (_popupDataMap.ContainsKey(dataItem.popupType) == false)
                    _popupDataMap.Add(dataItem.popupType, dataItem.path);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[PopupManager] Failed to deserialize popup data table: {e.Message}");
        }
    }

    public async UniTask<PopupBase> ShowPopup(PopupType popupType, params object[] par)
    {
        if (!_popupDataMap.TryGetValue(popupType, out string path))
        {
            Debug.LogError($"[PopupManager] PopupType {popupType} not found in map.");
            return null;
        }

        PopupBase popup = await GameMaster.Instance.addressableManager.InstantiateComponentAsync<PopupBase>(path, transform);

        if (popup == null)
        {
            Debug.LogError($"[PopupManager] Failed to instantiate popup at path: {path}");
            return null;
        }

        _stackPopup.Push(popup);
        _currentPopup = popup;

        popup.Init(par);

        return popup;
    }

    public void CloseCurrentPopup()
    {
        if (_stackPopup.Count <= 0 || _currentPopup == null)
        {
            Debug.LogError($"{GetType().Name}::No active Popup to close.");
            return;
        }

        PopupBase closedPopup = _stackPopup.Pop();

        Destroy(closedPopup.gameObject);

        if (_stackPopup.Count <= 0)
        {
            _currentPopup = null;
        }
        else
        {
            _currentPopup = _stackPopup.Peek();
        }
    }

    public void ClosePopupAll()
    {
        while (_currentPopup != null)
        {
            CloseCurrentPopup();
        }
        _stackPopup.Clear();
    }

    public void ShowToolTipPopup(IToolTip toolTip)
    {
        if (toolTip == null || m_tooltipBox == null)
            Debug.LogError($"toolTip is Null = {toolTip == null}, Box is Null = {m_tooltipBox == null}");

        m_tooltipBox.ShowToolTip(toolTip);
    }
}