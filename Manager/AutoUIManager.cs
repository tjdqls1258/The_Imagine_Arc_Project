using Cysharp.Threading.Tasks;
using DG.Tweening;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

public class AutoUIManager : MonoBehaviour
{
    public enum UIType
    {
        main,
        inGame
    }

    [Header("Main Canvas Groups")]
    [SerializeField] private CanvasGroup m_commandCanvas;
    [SerializeField] private CanvasGroup m_mainUICanvas;
    [SerializeField] private CanvasGroup m_inGameCanvas;

    [Header("UI Management")]
    [SerializeField] private RectTransform m_closeCanvas;
    [SerializeField] private float m_fadeTime = 0.1f;

    private string m_uiDataJson;
    private bool m_isCommandActive = false;
    private UIType m_currentUIType = UIType.main;

    public Transform CloseCanvas => m_closeCanvas;

    private Action m_lobbyActionOpen;
    private Action m_lobbyActionClose;

    private void Awake()
    {
        m_commandCanvas.alpha = 0;
        m_mainUICanvas.alpha = 0;
        m_inGameCanvas.alpha = 0;
        m_closeCanvas.gameObject.SetActive(false);
    }

    [ContextMenu("Test JsonLoad")]
    public async UniTask LoadJsonAsync()
    {
        var data = await GameMaster.Instance.addressableManager.LoadAssetAndCacheAsync<TextAsset>("UIData");

        if (data == null)
        {
            Debug.LogError("[AutoUIManager] Failed to load UIData TextAsset.");
            return;
        }

        m_uiDataJson = data.text;

        List<UIBaseData> dataList = JsonConvert.DeserializeObject<List<UIBaseData>>(m_uiDataJson);

        await InitUIAsync(dataList);
    }

    private async UniTask InitUIAsync(List<UIBaseData> uiDataList)
    {
        List<UniTask> tasks = new();

        foreach (var data in uiDataList)
        {
            tasks.Add(InstantiateObjectAndSettingAsync(data, GetParent(data.uiType)));
        }

        await UniTask.WhenAll(tasks);
    }

    public async UniTask InstantiateObjectAndSettingAsync(UIBaseData data, Transform parent)
    {
        var obj = await GameMaster.Instance.addressableManager.InstantiateObjectAsync(data.dataName, parent);

        if (obj == null) return;

        RectTransform rect = (RectTransform)obj.transform;

        rect.gameObject.name = data.dataName;

        rect.anchorMin = data.GetAchorMinMax().min;
        rect.anchorMax = data.GetAchorMinMax().max;
        rect.pivot = data.GetPivot();
        rect.anchoredPosition = data.GetAnchorPos();
        rect.sizeDelta = data.GetSizeDetail();

        if (obj.TryGetComponent<UILobbyUpdate>(out var lobbyComponent))
        {
            m_lobbyActionOpen += lobbyComponent.UpdateFormLobby;
            m_lobbyActionClose += lobbyComponent.CloseFormLobby;
        }
    }

    public void SetUIType(UIType type, bool useCommand = true)
    {
        m_currentUIType = type;
        m_isCommandActive = useCommand;

        CanvasSetting(m_commandCanvas, m_isCommandActive);
        CanvasSetting(m_inGameCanvas, m_currentUIType == UIType.inGame);
        CanvasSetting(m_mainUICanvas, m_currentUIType == UIType.main);

        UIAction();

        void CanvasSetting(CanvasGroup canvas, bool active)
        {
            canvas.alpha = active ? 1f : 0f;

            if (canvas.interactable == active)
                return;

            canvas.blocksRaycasts = active;
            canvas.interactable = active;
        }

        void UIAction()
        {
            switch (m_currentUIType)
            {
                case UIType.main:
                    if (m_lobbyActionOpen != null)
                        m_lobbyActionOpen.Invoke();
                    break;
                case UIType.inGame:
                    break;
            }
            if (m_currentUIType != UIType.main)
                m_lobbyActionClose?.Invoke();
        }
    }

    public T GetCompoent<T>(UIBaseData.UIType type) where T : UIBaseFormMaker
    {
        Transform parent = GetParent(type);

        if (parent == null) return null;

        return parent.GetComponentInChildren<T>(true);
    }

    public void PushUI(UIBaseData.UIType type, Transform targetUI)
    {
        targetUI.SetParent(GetParent(type));
    }

    public void PopUI(Transform targetUI)
    {
        targetUI.SetParent(CloseCanvas.transform);
    }

    public Transform GetParent(UIBaseData.UIType type)
    {
        switch (type)
        {
            case UIBaseData.UIType.Command:
                return m_commandCanvas.transform;
            case UIBaseData.UIType.MainUI:
                return m_mainUICanvas.transform;
            case UIBaseData.UIType.InGameUI:
                return m_inGameCanvas.transform;
            default:
                Debug.LogError($"[AutoUIManager] Undefined UIType received: {type}");
                return null;
        }
    }
}