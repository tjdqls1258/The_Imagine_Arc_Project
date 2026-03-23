using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

/// <summary>
/// 게임 내 팝업 UI를 스택 기반으로 관리하는 MonoSingleton 클래스입니다.
/// 팝업 데이터를 로드하고, 팝업을 비동기로 생성/표시하며, 닫는 로직을 처리합니다.
/// </summary>
public class PopupManager : MonoBehaviour 
{
    // ====== Data Structures ======

    /// <summary>
    /// 관리할 팝업의 종류를 정의합니다. (확장성을 위해 Enum으로 관리)
    /// </summary>
    [Serializable]
    public enum PopupType
    {
        None = 0, // 기본값 또는 오류 처리용
        PopupMsg, // 예시: 간단한 메시지 팝업
        PopupQ, // OK/Cancel
        // Todo: 다른 팝업 타입 추가 예정 (Settings, Inventory, Shop 등)
    }

    /// <summary>
    /// Addressables를 통해 로드할 팝업의 메타데이터 구조입니다. (JSON 파일 구조와 일치)
    /// </summary>
    [Serializable]
    public class PopupData
    {
        public PopupType popupType; // 팝업의 Enum 타입
        public string path;        // 팝업 프리팹의 Addressable 키 (경로)
    }

    // ====== Runtime State & Caching ======

    // Addressables 키(path)를 팝업 타입(PopupType)에 매핑하여 저장하는 딕셔너리
    private readonly Dictionary<PopupType, string> _popupDataMap = new();

    // 현재 활성화된 팝업들을 순서대로 저장하는 스택 (후입선출)
    private readonly Stack<PopupBase> _stackPopup = new();

    // 스택의 가장 위에 있는 (현재 사용자에게 보이는) 팝업 참조
    private PopupBase _currentPopup = null;

    // 팝업 데이터 테이블의 Addressable 키 상수
    private const string POPUP_DATA_TABLE_KEY = "PopupDataTable";
    [SerializeField] private TextAsset m_localPopupDataText;

    // ----------------------------------------------------------------------
    // ## Initialization and Data Loading
    // ----------------------------------------------------------------------
    public void Init()
    {

    }
    /// <summary>
    /// Addressables에서 JSON 형태의 팝업 데이터 테이블을 로드하고, 팝업 타입과 경로를 매핑합니다.
    /// </summary>
    public async UniTask SettingPopupDataAsync()
    {
        // 1. 데이터 로드 (TextAsset 형태)
        TextAsset popupDataTable =
            await GameMaster.Instance.addressableManager.LoadAssetAndCacheAsync<TextAsset>(POPUP_DATA_TABLE_KEY);

        if (popupDataTable == null)
        {
            Logger.LogError($"[PopupManager] Failed to load popup data table: {POPUP_DATA_TABLE_KEY}");
            return;
        }

        // 2. JSON 역직렬화
        try
        {
            List<PopupData> data = JsonConvert.DeserializeObject<List<PopupData>>(popupDataTable.text);

            // 3. 딕셔너리에 매핑 저장
            foreach (PopupData dataItem in data)
                _popupDataMap.Add(dataItem.popupType, dataItem.path);
        }
        catch (Exception e)
        {
            Logger.LogError($"[PopupManager] Failed to deserialize popup data table: {e.Message}");
        }
        finally
        {
            // 4. 로드된 TextAsset은 더 이상 필요 없으므로 해제
            // LoadAddressableAssetAsync를 사용했기 때문에 명시적으로 Release가 필요합니다.
            GameMaster.Instance.addressableManager.UnloadAsset(POPUP_DATA_TABLE_KEY);
        }
    }

    public void SettingPopupData()
    {
        // 1. JSON 역직렬화
        try
        {
            List<PopupData> data = JsonConvert.DeserializeObject<List<PopupData>>(m_localPopupDataText.text);

            // 2. 딕셔너리에 매핑 저장
            foreach (PopupData dataItem in data)
            {
                if(_popupDataMap.ContainsKey(dataItem.popupType) == false)
                    _popupDataMap.Add(dataItem.popupType, dataItem.path);
            }
        }
        catch (Exception e)
        {
            Logger.LogError($"[PopupManager] Failed to deserialize popup data table: {e.Message}");
        }
    }

    // ----------------------------------------------------------------------
    // ## Popup Control
    // ----------------------------------------------------------------------

    /// <summary>
    /// 지정된 타입의 팝업을 Addressables에서 로드하여 화면에 표시하고 스택에 추가합니다.
    /// </summary>
    /// <param name="popupType">표시할 팝업의 타입</param>
    /// <param name="par">팝업에 전달할 초기화 매개변수</param>
    /// <returns>생성된 팝업의 PopupBase 인스턴스</returns>
    public async UniTask<PopupBase> ShowPopup(PopupType popupType, params object[] par)
    {
        if (!_popupDataMap.TryGetValue(popupType, out string path))
        {
            Logger.LogError($"[PopupManager] PopupType {popupType} not found in map.");
            return null;
        }

        // 1. 팝업 프리팹을 Addressables에서 로드하여 컴포넌트(PopupBase)를 인스턴스화
        PopupBase popup = await GameMaster.Instance.addressableManager.InstantiateComponentAsync<PopupBase>(path, transform);

        if (popup == null)
        {
            Logger.LogError($"[PopupManager] Failed to instantiate popup at path: {path}");
            return null;
        }

        // 2. 스택에 추가 및 현재 팝업 업데이트
        _stackPopup.Push(popup);
        _currentPopup = popup;

        // 3. 팝업 초기화
        popup.Init(par);

        return popup;
    }

    /// <summary>
    /// 현재 스택의 가장 위에 있는 (가장 최근에 열린) 팝업을 닫습니다.
    /// </summary>
    public void CloseCurrentPopup()
    {
        if (_stackPopup.Count <= 0 || _currentPopup == null)
        {
            Logger.LogError($"{GetType().Name}::No active Popup to close.");
            return;
        }

        // 1. 현재 팝업을 스택에서 제거
        PopupBase closedPopup = _stackPopup.Pop();

        // 2. 팝업 객체를 파괴 (AutoRelease 컴포넌트가 Addressables ReleaseInstance를 처리할 것으로 예상)
        Destroy(closedPopup.gameObject);

        // 3. 현재 팝업 상태 업데이트
        if (_stackPopup.Count <= 0)
        {
            _currentPopup = null; // 스택이 비면 null
        }
        else
        {
            _currentPopup = _stackPopup.Peek(); // 다음 팝업을 현재 팝업으로 지정
            // Todo: 새 currentPopup에 포커스/활성화 로직 추가 (예: SetActive(true))
        }
    }

    /// <summary>
    /// 스택에 남아 있는 모든 팝업을 닫고 상태를 초기화합니다.
    /// </summary>
    public void ClosePopupAll()
    {
        while (_currentPopup != null)
        {
            CloseCurrentPopup();
        }
        // 스택이 완전히 비었음을 보장
        _stackPopup.Clear();
    }
}