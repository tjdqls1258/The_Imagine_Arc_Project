using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// UI 패널 중앙 관리 및 풀링 시스템
/// 게임의 개별 UI 패널들을 로드, 표시 및 풀링을 통해 관리하는 중앙 관리자 Singleton 클래스입니다.
/// AutoUIManager와 연동하여 캔버스 계층 구조를 제어하며, UI를 활성(Open)과 비활성(Close) 풀로 나누어 재활용합니다.
/// </summary>
public class UIManager : Singleton<UIManager>
{
    // ====== Constants and Enums ======

    /// <summary> Addressable 에셋 경로 포맷 </summary>
    private const string UIPATH_FORMAT = "UIPanel/{0}.prefab";

    /// <summary>
    /// UIManager가 관리하는 개별 UI 패널의 타입 목록입니다.
    /// </summary>
    public enum UISequence
    {
        None = -1,
        StageSeletePanel,  // 스테이지 선택 패널
        CharacterListPanel, // 캐릭터 목록 패널
        ShopPanel,          // 상점 패널
        CharacterSelectPanel,//캐릭터 덱 설정 패널
        // Todo: 필요한 UI 패널 타입 상시 추가
    }

    // ====== Cached Managers and References ======

    /// <summary> Master Canvas의 Transform (모든 UI의 최상위 부모) </summary>
    public Transform MasterCanvas { private set; get; }

    /// <summary> 실질적인 UI 계층 이동 및 애니메이션을 담당하는 컴포넌트 </summary>
    public AutoUIManager AutoUIManager { private set; get; }

    // ====== UI Object Pooling Dictionaries ======

    /// <summary> 현재 화면에 표시 중이거나 활성화된 UI 패널 (Open Pool) </summary>
    public readonly Dictionary<UISequence, GameObject> m_openUIPool = new();

    /// <summary> 현재 비활성화되어 보관소(Closet)에 대기 중인 UI 패널 (Close Pool) </summary>
    public readonly Dictionary<UISequence, GameObject> m_closeUIPool = new();

    // ----------------------------------------------------------------------
    // ## Initialization
    // ----------------------------------------------------------------------

    public override void Init()
    {
        base.Init();
    }

    /// <summary>
    /// 시스템의 뼈대가 되는 MasterCanvas를 로드하고 AutoUIManager를 초기화합니다.
    /// </summary>
    /// <param name="parent">MasterCanvas가 생성될 부모 Transform</param>
    public async UniTask LoadMasterCanvasAsync(Transform parent)
    {
        // 1. "MasterCanvas" 프리팹 인스턴스화
        GameObject masterCanvasObj = await AddressableManager.Instance.InstantiateObjectAsync("MasterCanvas", parent);

        if (masterCanvasObj == null)
        {
            Logger.LogError("[UIManager] Failed to load MasterCanvas.");
            return;
        }

        // 2. 참조 캐싱
        MasterCanvas = masterCanvasObj.transform;
        AutoUIManager = MasterCanvas.GetComponent<AutoUIManager>();

        if (AutoUIManager == null)
        {
            Logger.LogError("[UIManager] MasterCanvas is missing AutoUIManager component.");
        }
    }

    // ----------------------------------------------------------------------
    // ## UI Control (Show / Close)
    // ----------------------------------------------------------------------

    /// <summary>
    /// UI 패널을 화면에 표시합니다. 풀에 있으면 재사용하고, 없으면 새로 로드합니다.
    /// </summary>
    /// <param name="type">표시할 UI 패널 종류</param>
    /// <param name="uiType">배치될 타겟 캔버스 레이어 타입</param>
    public async UniTask ShowUI(UISequence type, UIBaseData.UIType uiType = UIBaseData.UIType.MainUI)
    {
        // 1. Close Pool(비활성 보관소)에 기존 객체가 있는지 확인
        if (m_closeUIPool.ContainsKey(type))
        {
            // [재사용] 보관소에서 꺼내어 활성 캔버스로 이동
            GameObject ui = m_closeUIPool[type];
            AutoUIManager.PushUI(uiType, ui.transform);

            // 풀 상태 변경 (Close -> Open)
            ChangeItem(m_closeUIPool, m_openUIPool, type);
        }
        else
        {
            // [신규 로드] 풀에 없으므로 Addressables를 통해 새로 생성
            Logger.Log($"Loading UI panel: {type.ToString()}");

            string path = string.Format(UIPATH_FORMAT, type.ToString());
            GameObject ui = await AddressableManager.Instance.InstantiateObjectAsync(path, AutoUIManager.GetParent(uiType));

            if (ui == null)
            {
                Logger.LogError($"[UIManager] Failed to instantiate UI panel at: {path}");
                return;
            }

            // Open Pool에 등록
            m_openUIPool.Add(type, ui);

            ui.GetComponent<UIBase>().SetSequence(type);
        }

        // 2. 해당 UI의 내부 활성화 로직(애니메이션 등) 호출
        m_openUIPool[type].GetComponent<UIBase>()?.ShowUI();
    }

    /// <summary>
    /// 활성화된 UI 패널을 닫고 비활성 보관소(Close Pool)로 이동시킵니다.
    /// </summary>
    /// <param name="type">닫을 UI 패널 종류</param>
    public void CloseUI(UISequence type)
    {
        if (m_openUIPool.ContainsKey(type))
        {
            GameObject ui = m_openUIPool[type];

            // 1. 보관소(Closet) 캔버스로 계층 이동
            AutoUIManager.PopUI(ui.transform);

            // 2. 풀 상태 변경 (Open -> Close)
            ChangeItem(m_openUIPool, m_closeUIPool, type);
        }
        else
        {
            Logger.LogError($"{GetType().Name}::Cannot find UI panel in Open Pool: {type}");
        }
    }

    // ----------------------------------------------------------------------
    // ## Helper Methods
    // ----------------------------------------------------------------------

    /// <summary>
    /// 활성/비활성 Dictionary 간에 항목을 안전하게 이동시킵니다.
    /// </summary>
    private void ChangeItem(Dictionary<UISequence, GameObject> where, Dictionary<UISequence, GameObject> from, UISequence Item)
    {
        if (where.ContainsKey(Item))
        {
            from.Add(Item, where[Item]);
            where.Remove(Item);
        }
    }
}