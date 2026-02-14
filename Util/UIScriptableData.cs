using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// UI 레이아웃 정보를 저장하고 관리하는 ScriptableObject입니다.
/// 에디터 환경에서는 JSON 데이터를 로드/저장하는 데이터 허브 역할을 하며,
/// 런타임에서는 저장된 정보를 기반으로 실제 UI 객체들을 비동기 생성 및 배치합니다.
/// </summary>
[CreateAssetMenu(fileName = "UIScriptableData", menuName = "Scriptable Objects/UIScriptableData")]
public class UIScriptableData : ScriptableObject
{
    /// <summary>
    /// 로드되었거나 에디터 툴(UIMakerTool)에 의해 추출된 UI 레이아웃 데이터 목록입니다.
    /// </summary>
    public List<UIBaseData> m_UIDataList;

    // ----------------------------------------------------------------------
    // ## Editor/Data Setting (Unity Editor Only)
    // ----------------------------------------------------------------------

    /// <summary>
    /// [에디터 전용] 추출된 UI 데이터를 리스트에 설정하고 에디터 에셋으로 즉시 저장합니다.
    /// </summary>
    /// <param name="data">UIMakerTool 등에서 전달된 UI 속성 데이터 리스트</param>
    public void SettingData(List<UIBaseData> data)
    {
        m_UIDataList = data;

#if UNITY_EDITOR
        // ScriptableObject의 변경 사항을 에디터에 알리고 파일로 저장
        AssetDatabase.SaveAssetIfDirty(this);
        AssetDatabase.SaveAssets();
#endif
    }

    // ----------------------------------------------------------------------
    // ## Runtime UI Instantiation (Creation)
    // ----------------------------------------------------------------------

    /// <summary>
    /// 저장된 데이터를 기반으로 모든 UI 프리팹을 비동기 생성하여 지정된 부모 아래에 배치합니다.
    /// </summary>
    /// <param name="command">Command 타입 UI가 배치될 루트 Transform</param>
    /// <param name="mainUI">MainUI 타입 UI가 배치될 루트 Transform</param>
    /// <param name="InGame">InGameUI 타입 UI가 배치될 루트 Transform</param>
    public async UniTask MakeUIList(Transform command, Transform mainUI, Transform InGame)
    {
        var uiData = m_UIDataList;
        if (uiData == null) return;

        // 대량의 생성 작업을 동시에 처리하기 위한 Task 리스트
        List<UniTask> tasks = new();

        foreach (var d in uiData)
        {
            // 각 데이터의 UIType에 따라 적절한 부모 트랜스폼을 할당하여 생성 태스크 추가
            switch (d.uiType)
            {
                case UIBaseData.UIType.Command:
                    tasks.Add(InstantiateObjectSetting(d, command));
                    break;
                case UIBaseData.UIType.MainUI:
                    tasks.Add(InstantiateObjectSetting(d, mainUI));
                    break;
                case UIBaseData.UIType.InGameUI:
                    tasks.Add(InstantiateObjectSetting(d, InGame));
                    break;
            }
        }

        // 모든 UI 요소의 로드 및 배치가 완료될 때까지 비동기로 대기 (성능 최적화)
        await UniTask.WhenAll(tasks);
    }

    /// <summary>
    /// [비동기] Addressables를 통해 프리팹을 생성하고 RectTransform 수치를 적용합니다.
    /// </summary>
    /// <param name="data">배치될 UI의 수치 정보</param>
    /// <param name="parent">배치될 부모 Transform</param>
    public async UniTask InstantiateObjectSetting(UIBaseData data, Transform parent)
    {
        // 1. Addressables 매니저를 호출하여 프리팹 인스턴스화
        var obj = await AddressableManager.Instance.InstantiateObjectAsync(data.dataName, parent);

        if (obj == null) return;

        // 2. RectTransform 컴포넌트 참조 (UI 전용 트랜스폼)
        RectTransform rect = (RectTransform)obj.transform;

        // 3. 데이터에 기반한 UI 레이아웃 복원
        rect.gameObject.name = data.dataName;
        rect.anchorMin = data.GetAchorMinMax().min;
        rect.anchorMax = data.GetAchorMinMax().max;
        rect.pivot = data.GetPivot();
        rect.anchoredPosition = data.GetAnchorPos();
        rect.sizeDelta = data.GetSizeDetail();
    }

    // ----------------------------------------------------------------------
    // ## JSON Data Utilities (Editor-Side Persistence)
    // ----------------------------------------------------------------------

#if UNITY_EDITOR

    /// <summary>
    /// [ContextMenu] 현재 ScriptableObject에 담긴 데이터를 외부 JSON 파일로 저장합니다.
    /// </summary>
    [ContextMenu("Make Json")]
    public void MakeJson()
    {
        // JSON 직렬화 설정 (들여쓰기 적용으로 가독성 확보)
        var settings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore
        };

        string data = JsonConvert.SerializeObject(m_UIDataList, settings);

        // 프로젝트 내 특정 경로(Assets/TextAsset/UIData.json)에 파일 작성
        string path = Path.Combine($"{Application.dataPath}/TextAsset/", "UIData.json");

        File.WriteAllText(path, data);
        Logger.Log($"[UIScriptableData] JSON 파일이 다음 경로에 저장되었습니다: {path}");

        // 에셋 데이터베이스 갱신하여 유니티 에디터에 즉시 반영
        AssetDatabase.Refresh();
    }

    /// <summary>
    /// [ContextMenu] Addressables 시스템 혹은 외부에서 관리되는 JSON 파일을 읽어 데이터 리스트를 복구합니다.
    /// </summary>
    [ContextMenu("Load Json")]
    public void LoadJson()
    {
        LoadJsonUI().Forget();
    }

    /// <summary>
    /// [비동기] Addressables 키워드 "UIData"를 사용하여 텍스트 파일을 로드하고 리스트를 역직렬화합니다.
    /// </summary>
    public async UniTask LoadJsonUI()
    {
        // Addressables를 통해 TextAsset 자원 로드
        var data = await AddressableManager.Instance.LoadAssetAndCacheAsync<TextAsset>("UIData");

        if (data == null)
        {
            Logger.LogError("[UIScriptableData] Addressables에서 UIData를 로드하는 데 실패했습니다.");
            return;
        }

        // JSON 문자열을 객체 리스트로 복구
        m_UIDataList = JsonConvert.DeserializeObject<List<UIBaseData>>(data.text);

        Logger.Log($"[UIScriptableData] JSON 로드 성공. 로드된 항목 수: {m_UIDataList?.Count}");
    }
#endif
}