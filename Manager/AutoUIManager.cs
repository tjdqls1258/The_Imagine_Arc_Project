using Cysharp.Threading.Tasks;
using DG.Tweening;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// UI 캔버스 및 레이아웃 관리자
/// 게임의 메인 UI 구조(캔버스 그룹)와 레이아웃을 관리하는 클래스입니다.
/// JSON 데이터를 기반으로 UI 요소를 비동기로 로드 및 배치하며, CanvasGroup과 DOTween을 사용하여 화면 전환 페이드 처리를 담당합니다.
/// </summary>
public class AutoUIManager : MonoBehaviour
{
    // ====== Data Structures ======

    /// <summary>
    /// 현재 게임에서 사용 중인 주요 UI 화면 타입을 정의합니다. (Main Scene vs InGame Scene)
    /// </summary>
    public enum UIType
    {
        main,
        inGame
    }

    // Note: UIBaseData 클래스는 외부에 정의되어 있으며, UI의 레이아웃 정보를 제공합니다.

    // ====== Canvas References & Settings ======
    [Header("Main Canvas Groups")]
    [Tooltip("모든 UI 화면에 걸쳐 표시되는 공통 명령/메뉴 캔버스")]
    [SerializeField] private CanvasGroup m_commandCanvas;
    [Tooltip("메인 로비/타이틀 화면 UI 캔버스")]
    [SerializeField] private CanvasGroup m_mainUICanvas;
    [Tooltip("인게임 플레이 화면 UI 캔버스")]
    [SerializeField] private CanvasGroup m_inGameCanvas;

    [Header("UI Management")]
    [Tooltip("비활성화된 UI 객체들을 보관하는 RectTransform (UI 풀링을 위한 임시 부모)")]
    [SerializeField] private RectTransform m_closeCanvas; // 명칭은 유지하되, 역할에 대한 주석을 명확히 함
    [Tooltip("캔버스 페이드 인/아웃에 걸리는 시간")]
    [SerializeField] private float m_fadeTime = 0.1f;

    // ====== Runtime State ======
    private string m_uiDataJson; // 로드된 UI 데이터 JSON 문자열
    private bool m_isCommandActive = false; // Command 캔버스가 활성화(알파 1) 상태인지 여부
    private UIType m_currentUIType = UIType.main; // 현재 활성화된 메인 UI 타입

    /// <summary>
    /// 비활성 UI 객체를 보관하는 캔버스(임시 보관 영역)의 Transform에 접근합니다.
    /// </summary>
    public Transform CloseCanvas => m_closeCanvas;

    private Action m_lobbyActionOpen;
    private Action m_lobbyActionClose;

    // ----------------------------------------------------------------------
    // ## Initialization and Data Loading
    // ----------------------------------------------------------------------

    private void Awake()
    {
        // 모든 캔버스를 초기 상태(투명, 비활성)로 설정하여 화면에 보이지 않도록 합니다.
        m_commandCanvas.alpha = 0;
        m_mainUICanvas.alpha = 0;
        m_inGameCanvas.alpha = 0;
        // 임시 보관소 캔버스는 비활성화 상태 유지
        m_closeCanvas.gameObject.SetActive(false);
    }

    /// <summary>
    /// Addressables에서 UI 레이아웃 JSON 데이터를 로드하고 파싱하여 UI 생성을 시작합니다.
    /// </summary>
    [ContextMenu("Test JsonLoad")]
    public async UniTask LoadJsonAsync()
    {
        // 1. JSON TextAsset 로드 및 데이터 캐싱
        var data = await AddressableManager.Instance.LoadAssetAndCacheAsync<TextAsset>("UIData");

        if (data == null)
        {
            Logger.LogError("[AutoUIManager] Failed to load UIData TextAsset.");
            return;
        }

        m_uiDataJson = data.text;

        // 2. JSON 파싱
        List<UIBaseData> dataList = JsonConvert.DeserializeObject<List<UIBaseData>>(m_uiDataJson);

        // 3. UI 객체 비동기 초기화 및 배치 시작
        await InitUIAsync(dataList);
    }

    /// <summary>
    /// 로드된 UIBaseData 목록을 기반으로 모든 UI 프리팹을 비동기로 인스턴스화하고 배치합니다.
    /// </summary>
    private async UniTask InitUIAsync(List<UIBaseData> uiDataList)
    {
        List<UniTask> tasks = new();

        foreach (var data in uiDataList)
        {
            // 각 UI 데이터를 기반으로 객체 인스턴스화 및 레이아웃 설정 작업을 UniTask로 생성
            // GetParent(data.uiType)을 통해 해당 UI가 소속될 캔버스 그룹을 결정합니다.
            tasks.Add(InstantiateObjectAndSettingAsync(data, GetParent(data.uiType)));
        }

        // 모든 UI 생성 작업이 완료될 때까지 병렬적으로 대기
        await UniTask.WhenAll(tasks);
    }

    /// <summary>
    /// Addressables를 사용하여 UI 프리팹을 인스턴스화하고, RectTransform 속성을 설정합니다.
    /// </summary>
    public async UniTask InstantiateObjectAndSettingAsync(UIBaseData data, Transform parent)
    {
        // Addressables를 통해 GameObject를 비동기로 인스턴스화
        var obj = await AddressableManager.Instance.InstantiateObjectAsync(data.dataName, parent);

        if (obj == null) return;

        RectTransform rect = (RectTransform)obj.transform;

        // 1. 이름 설정
        rect.gameObject.name = data.dataName;

        // 2. RectTransform 레이아웃 설정 (JSON 데이터 기반)
        rect.anchorMin = data.GetAchorMinMax().min;
        rect.anchorMax = data.GetAchorMinMax().max;
        rect.pivot = data.GetPivot();
        rect.anchoredPosition = data.GetAnchorPos();
        rect.sizeDelta = data.GetSizeDetail();

        if(obj.TryGetComponent<UILobbyUpdate>(out var lobbyComponent))
        {
            m_lobbyActionOpen += lobbyComponent.UpdateFormLobby;
            m_lobbyActionClose += lobbyComponent.CloseFormLobby;
        }
    }

    // ----------------------------------------------------------------------
    // ## Canvas Switching and Visibility
    // ----------------------------------------------------------------------

    /// <summary>
    /// 현재 활성화할 메인 UI 화면(캔버스 그룹)을 전환하고, Command 캔버스의 활성화 상태를 제어합니다.
    /// </summary>
    /// <param name="type">활성화할 새로운 메인 UI 타입 (main 또는 inGame)</param>
    /// <param name="useCommand">Command 캔버스를 함께 활성화할지 여부</param>
    public void SetUIType(UIType type, bool useCommand = true)
    {
        m_currentUIType = type;

        // Command 캔버스 활성화 상태 토글 (명령 캔버스가 항상 켜져 있어야 하는지 여부 제어)
        m_isCommandActive = useCommand;

        // 캔버스 페이드 및 상호작용 설정
        CanvasSetting(m_commandCanvas, m_isCommandActive);
        CanvasSetting(m_inGameCanvas, m_currentUIType == UIType.inGame);
        CanvasSetting(m_mainUICanvas, m_currentUIType == UIType.main);

        UIAction();
        // 로컬 함수: 캔버스 그룹을 페이드 인/아웃하고 상호작용 가능 여부를 설정
        void CanvasSetting(CanvasGroup canvas, bool active)
        {
            // DOTween을 사용하여 알파 값을 m_fadeTime 동안 전환 (페이드 애니메이션)
            canvas.alpha = active ? 1f : 0f;
            //canvas.DOFade(active ? 1f : 0f, m_fadeTime);

            // 상호작용 및 레이캐스트 차단 여부 설정
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

    // ----------------------------------------------------------------------
    // ## UI Retrieval and Reparenting
    // ----------------------------------------------------------------------

    /// <summary>
    /// 지정된 타입의 부모 캔버스 내에서 특정 컴포넌트 T를 찾아서 반환합니다.
    /// </summary>
    /// <typeparam name="T">찾으려는 컴포넌트 타입 (UIBaseFormMaker 상속)</typeparam>
    /// <param name="type">UIBaseData.UIType에 해당하는 부모 캔버스 타입</param>
    /// <returns>찾은 컴포넌트 T 또는 null</returns>
    public T GetCompoent<T>(UIBaseData.UIType type) where T : UIBaseFormMaker
    {
        Transform parent = GetParent(type);

        if (parent == null) return null;

        // 비활성화된 자식까지 포함하여 검색 (true)
        return parent.GetComponentInChildren<T>(true);
    }

    /// <summary>
    /// UI 객체를 지정된 캔버스 그룹으로 이동시켜 활성화합니다. (Push: 화면에 표시)
    /// </summary>
    /// <param name="type">이동시킬 대상 부모 캔버스의 타입</param>
    /// <param name="targetUI">이동시킬 UI 객체의 Transform</param>
    public void PushUI(UIBaseData.UIType type, Transform targetUI)
    {
        // 대상 캔버스 그룹의 Transform 아래로 이동
        targetUI.SetParent(GetParent(type));
        // Note: 부모 캔버스 그룹의 alpha가 1이면 UI가 표시됩니다.
    }

    /// <summary>
    /// UI 객체를 임시 보관소(Closet)로 이동시켜 비활성화 상태로 보관합니다. (Pop: 화면에서 숨김/풀링)
    /// </summary>
    /// <param name="targetUI">이동시킬 UI 객체의 Transform</param>
    public void PopUI(Transform targetUI)
    {
        // 임시 보관소 캔버스 아래로 이동 (화면 밖/비활성 영역으로 이동)
        targetUI.SetParent(CloseCanvas.transform);
    }

    /// <summary>
    /// UIBaseData.UIType에 해당하는 실제 부모 Transform (CanvasGroup의 Transform)을 반환합니다.
    /// </summary>
    /// <param name="type">UIBaseData.UIType</param>
    /// <returns>해당 캔버스 그룹의 Transform</returns>
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
                Logger.LogError($"[AutoUIManager] Undefined UIType received: {type}");
                return null;
        }
    }
}