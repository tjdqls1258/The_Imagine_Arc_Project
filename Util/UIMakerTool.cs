#if UNITY_EDITOR
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 유니티 에디터 상에서 UI를 자동으로 생성하고 현재 배치를 데이터화(JSON)하는 헬퍼 클래스입니다.
/// </summary>
public class UIMakerTool : MonoBehaviour
{
    // ====== Inspector 필드: 각 UI의 루트가 될 캔버스 그룹들 ======
    [SerializeField] CanvasGroup m_commandCanvas; // 공용 UI 루트
    [SerializeField] CanvasGroup m_mainCanvas;    // 메인 UI 루트
    [SerializeField] CanvasGroup m_inGameCanvas;  // 인게임 UI 루트

    [SerializeField] UIScriptableData m_uiData;   // UI 데이터가 저장된 ScriptableObject

    /// <summary> [ContextMenu] 저장된 데이터를 바탕으로 에디터 상에 UI를 실제로 생성합니다. </summary>
    [ContextMenu("UI 생성")]
    public void UpdateUI()
    {
        // ScriptableObject에 구현된 생성 로직을 비동기로 호출
        m_uiData.MakeUIList(m_commandCanvas.transform, m_mainCanvas.transform, m_inGameCanvas.transform).Forget();
    }

    /// <summary> UI 리스트를 JSON 형태로 변환하여 저장 명령을 내립니다. </summary>
    public void MakeUI()
    {
        m_uiData.MakeJson();
    }

    /// <summary> 저장된 JSON 파일을 다시 로드합니다. </summary>
    public void LoadData()
    {
        m_uiData.LoadJson();
    }

    /// <summary> [ContextMenu] 현재 하이어라키의 UI 배치 상태를 읽어와 데이터화(JSON 저장)합니다. </summary>
    [ContextMenu("현재 상황 업데이트")]
    public void MakeJson()
    {
        // 1. 각 캔버스 루트 하위의 UIBaseFormMaker(UI 개별 요소)들을 검색하여 리스트화
        List<UIBaseFormMaker> commandUIlist = m_commandCanvas.gameObject.GetComponentsInChildren<UIBaseFormMaker>(true).ToList();
        List<UIBaseFormMaker> mainUIList = m_mainCanvas.gameObject.GetComponentsInChildren<UIBaseFormMaker>(true).ToList();
        List<UIBaseFormMaker> inGameList = m_inGameCanvas.gameObject.GetComponentsInChildren<UIBaseFormMaker>(true).ToList();

        List<UIBaseData> uiBaseDataList = new();

        // 2. 검색된 UI들의 RectTransform 정보를 UIBaseData 형식으로 변환하여 통합 리스트에 추가
        uiBaseDataList.AddRange(DataSetting(UIBaseData.UIType.Command, commandUIlist));
        uiBaseDataList.AddRange(DataSetting(UIBaseData.UIType.MainUI, mainUIList));
        uiBaseDataList.AddRange(DataSetting(UIBaseData.UIType.InGameUI, inGameList));

        if (uiBaseDataList.Count <= 0)
        {
            Debug.LogError("UI가 설정되지 않았습니다.");
            return;
        }

        // 3. JSON 직렬화 설정 (보기 편하게 들여쓰기 추가 및 순환 참조 방지)
        JsonSerializerSettings settingJson = new();
        settingJson.Formatting = Formatting.Indented;
        settingJson.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;

        // 4. 데이터를 JSON으로 변환하여 로그로 출력하고 ScriptableObject에 세팅
        Debug.Log(JsonConvert.SerializeObject(uiBaseDataList, settingJson));
        m_uiData.SettingData(uiBaseDataList);

        // 5. 에디터 상에서 변경된 ScriptableObject 저장
        EditorUtility.SetDirty(m_uiData);
        AssetDatabase.SaveAssets();

        // [로컬 함수] UI 컴포넌트에서 RectTransform의 핵심 수치들을 추출하는 로직
        List<UIBaseData> DataSetting(UIBaseData.UIType uiType, List<UIBaseFormMaker> uiBase)
        {
            List<UIBaseData> dataList = new();
            foreach (UIBaseFormMaker commandUI in uiBase)
            {
                UIBaseData data = new();

                // 이름 추출 (언더바 뒤의 접미사 제거)
                data.dataName = commandUI.name.Split("_")[0];
                data.uiType = uiType;

                // RectTransform의 위치, 크기, 앵커, 피벗 정보를 데이터 객체에 저장
                data.SettingAnchorPos(commandUI.MyRT.anchoredPosition);
                data.SettingSizeDetail(commandUI.MyRT.sizeDelta);
                data.SettingAnchorMinMax(commandUI.MyRT.anchorMin, commandUI.MyRT.anchorMax);
                data.SettingPivot(commandUI.MyRT.pivot);

                dataList.Add(data);
            }
            return dataList;
        }

        // 실제 파일(JSON)로 저장 수행
        MakeUI();
    }

    /// <summary> [ContextMenu] 모든 캔버스 그룹 내의 UI 객체들을 즉시 삭제합니다. (에디터 전용) </summary>
    [ContextMenu("Clear All")]
    public void ClearAll()
    {
        DeleteAll(m_commandCanvas.transform);
        DeleteAll(m_mainCanvas.transform);
        DeleteAll(m_inGameCanvas.transform);

        void DeleteAll(Transform tr)
        {
            // 역순으로 순회하며 자식 객체들을 즉시 파괴
            for (int i = tr.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(tr.GetChild(i).gameObject);
            }
        }
    }

    /// <summary> 특정 타입의 UI만 활성화하고 나머지는 투명하게 처리하여 작업 편의성을 제공합니다. </summary>
    public void ShowUI(UIBaseData.UIType type)
    {
        m_commandCanvas.alpha = (type == UIBaseData.UIType.Command) ? 1 : 0;
        m_mainCanvas.alpha = (type == UIBaseData.UIType.MainUI) ? 1 : 0;
        m_inGameCanvas.alpha = (type == UIBaseData.UIType.InGameUI) ? 1 : 0;
    }
}

/// <summary>
/// UIMakerTool을 인스펙터 상에서 버튼 형태로 조작할 수 있게 해주는 커스텀 에디터 클래스입니다.
/// </summary>
[CustomEditor(typeof(UIMakerTool))]
public class UIMakerToolEditor : Editor
{
    UIMakerTool targetObject;

    private void Awake()
    {
        targetObject = (UIMakerTool)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI(); // 기본 인스펙터 필드 표시

        GUILayout.Space(10);
        GUILayout.Label("UI Data Management", EditorStyles.boldLabel);

        // 인스펙터 버튼들을 통해 스크립트의 메서드들을 실행
        if (GUILayout.Button("현재 배치 데이터를 JSON으로 업데이트"))
        {
            targetObject.MakeJson();
        }

        if (GUILayout.Button("데이터 기반으로 UI 자동 생성"))
        {
            targetObject.LoadData();
            targetObject.UpdateUI();
        }

        if (GUILayout.Button("모든 UI 제거 (Clear)"))
        {
            if (EditorUtility.DisplayDialog("경고", "모든 UI 객체를 삭제하시겠습니까?", "예", "아니오"))
            {
                targetObject.ClearAll();
            }
        }

        GUILayout.Space(10);
        GUILayout.Label("View Mode Control", EditorStyles.boldLabel);

        // 특정 모드의 UI만 빠르게 확인하기 위한 뷰어 버튼들
        if (GUILayout.Button("공용 UI(Command) 보기"))
            targetObject.ShowUI(UIBaseData.UIType.Command);

        if (GUILayout.Button("메인 UI(Main) 보기"))
            targetObject.ShowUI(UIBaseData.UIType.MainUI);

        if (GUILayout.Button("인게임 UI(InGame) 보기"))
            targetObject.ShowUI(UIBaseData.UIType.InGameUI);
    }
}
#endif