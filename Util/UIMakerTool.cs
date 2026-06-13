#if UNITY_EDITOR
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VContainer;

public class UIMakerTool : MonoBehaviour
{
    [Inject] private readonly AddressableManager addressableManager;
    [SerializeField] CanvasGroup m_commandCanvas; // 공용 UI 루트
    [SerializeField] CanvasGroup m_mainCanvas;    // 메인 UI 루트
    [SerializeField] CanvasGroup m_inGameCanvas;  // 인게임 UI 루트

    [SerializeField] UIScriptableData m_uiData;   // UI 데이터가 저장된 ScriptableObject

    [ContextMenu("UI 생성")]
    public void UpdateUI()
    {
        m_uiData.MakeUIList(m_commandCanvas.transform, m_mainCanvas.transform, m_inGameCanvas.transform, addressableManager).Forget();
    }

    public void MakeUI()
    {
        m_uiData.MakeJson();
    }

    public void LoadData()
    {
        m_uiData.LoadJson(addressableManager);
    }

    [ContextMenu("현재 상황 업데이트")]
    public void MakeJson()
    {
        List<UIBaseFormMaker> commandUIlist = m_commandCanvas.gameObject.GetComponentsInChildren<UIBaseFormMaker>(true).ToList();
        List<UIBaseFormMaker> mainUIList = m_mainCanvas.gameObject.GetComponentsInChildren<UIBaseFormMaker>(true).ToList();
        List<UIBaseFormMaker> inGameList = m_inGameCanvas.gameObject.GetComponentsInChildren<UIBaseFormMaker>(true).ToList();

        List<UIBaseData> uiBaseDataList = new();

        uiBaseDataList.AddRange(DataSetting(UIBaseData.UIType.Command, commandUIlist));
        uiBaseDataList.AddRange(DataSetting(UIBaseData.UIType.MainUI, mainUIList));
        uiBaseDataList.AddRange(DataSetting(UIBaseData.UIType.InGameUI, inGameList));

        if (uiBaseDataList.Count <= 0)
        {
            Debug.LogError("UI가 설정되지 않았습니다.");
            return;
        }

        JsonSerializerSettings settingJson = new();
        settingJson.Formatting = Formatting.Indented;
        settingJson.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;

        Debug.Log(JsonConvert.SerializeObject(uiBaseDataList, settingJson));
        m_uiData.SettingData(uiBaseDataList);

        EditorUtility.SetDirty(m_uiData);
        AssetDatabase.SaveAssets();

        List<UIBaseData> DataSetting(UIBaseData.UIType uiType, List<UIBaseFormMaker> uiBase)
        {
            List<UIBaseData> dataList = new();
            foreach (UIBaseFormMaker commandUI in uiBase)
            {
                UIBaseData data = new();

                data.dataName = commandUI.name.Split("_")[0];
                data.uiType = uiType;

                data.SettingAnchorPos(commandUI.MyRT.anchoredPosition);
                data.SettingSizeDetail(commandUI.MyRT.sizeDelta);
                data.SettingAnchorMinMax(commandUI.MyRT.anchorMin, commandUI.MyRT.anchorMax);
                data.SettingPivot(commandUI.MyRT.pivot);

                dataList.Add(data);
            }
            return dataList;
        }

        MakeUI();
    }

    [ContextMenu("Clear All")]
    public void ClearAll()
    {
        DeleteAll(m_commandCanvas.transform);
        DeleteAll(m_mainCanvas.transform);
        DeleteAll(m_inGameCanvas.transform);

        void DeleteAll(Transform tr)
        {
            for (int i = tr.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(tr.GetChild(i).gameObject);
            }
        }
    }

    public void ShowUI(UIBaseData.UIType type)
    {
        m_commandCanvas.alpha = (type == UIBaseData.UIType.Command) ? 1 : 0;
        m_mainCanvas.alpha = (type == UIBaseData.UIType.MainUI) ? 1 : 0;
        m_inGameCanvas.alpha = (type == UIBaseData.UIType.InGameUI) ? 1 : 0;
    }
}

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

        if (GUILayout.Button("공용 UI(Command) 보기"))
            targetObject.ShowUI(UIBaseData.UIType.Command);

        if (GUILayout.Button("메인 UI(Main) 보기"))
            targetObject.ShowUI(UIBaseData.UIType.MainUI);

        if (GUILayout.Button("인게임 UI(InGame) 보기"))
            targetObject.ShowUI(UIBaseData.UIType.InGameUI);
    }
}
#endif