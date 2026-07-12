using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class GrowthHubEditorWindow : EditorWindow
{
    private List<GrowthData> growthList = new List<GrowthData>();
    private GrowthData selectedGrowth;
    private Editor cachedEditor;
    private GrowthLibrary library;

    private Vector2 listScrollPos;
    private Vector2 detailScrollPos;
    private string searchKeyword = "";
    private bool isMonsterData = false;

    // 시뮬레이션 변수
    private int simLevel = 1;
    private int simEnforce = 0;
    private int simRank = 1;
    private int simUpgrade = 0;
    private float baseStatInput = 100f;

    private int currentTab = 0;
    private readonly string[] tabNames = { "Level", "Enforce", "Rank", "Upgrade" };
    private readonly string[] propertyNames = { "levelRules", "enforceRules", "rankRules", "upgradeRules" };

    [MenuItem("Tools/Data/2. Growth Hub")]
    public static void ShowWindow() => GetWindow<GrowthHubEditorWindow>("Growth Hub").minSize = new Vector2(850, 500);

    private void OnEnable() => RefreshAssets();
    private void OnDisable() { if (cachedEditor != null) DestroyImmediate(cachedEditor); }

    private void RefreshAssets()
    {
        growthList.Clear();
        string[] guids = AssetDatabase.FindAssets("t:GrowthData");
        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var asset = AssetDatabase.LoadAssetAtPath<GrowthData>(path);
            if (asset != null) growthList.Add(asset);
        }
        growthList = growthList.OrderBy(x => x.name).ToList();
    }

    private void OnGUI()
    {
        library = (GrowthLibrary)EditorGUILayout.ObjectField("Growth Library", library, typeof(GrowthLibrary), false);

        if (library == null)
        {
            EditorGUILayout.HelpBox("GrowthLibrary 에셋을 할당해주세요.", MessageType.Warning);
            return;
        }

        // 2. 새로운 성장 데이터 생성
        if (GUILayout.Button("Create New Growth Data"))
        {
            var newData = ScriptableObject.CreateInstance<GrowthData>();
            string path = AssetDatabase.GetAssetPath(library);
            path = path.Replace(System.IO.Path.GetFileName(path), $"Growth_{library.growthDataList.Count}.asset");

            AssetDatabase.CreateAsset(newData, path);
            library.growthDataList.Add(newData);
            EditorUtility.SetDirty(library);
            AssetDatabase.SaveAssets();
        }

        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        if (GUILayout.Button("새로고침", EditorStyles.toolbarButton, GUILayout.Width(80))) RefreshAssets();
        searchKeyword = EditorGUILayout.TextField(searchKeyword, EditorStyles.toolbarSearchField, GUILayout.Width(200));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        DrawLeftList();
        DrawRightDetail();
        EditorGUILayout.EndHorizontal();
    }

    private void DrawLeftList()
    {
        EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(250), GUILayout.ExpandHeight(true));
        listScrollPos = EditorGUILayout.BeginScrollView(listScrollPos);
        foreach (var item in growthList)
        {
            if (!string.IsNullOrEmpty(searchKeyword) && !item.name.ToLower().Contains(searchKeyword.ToLower())) continue;

            GUI.backgroundColor = (selectedGrowth == item) ? Color.cyan : Color.white;
            if (GUILayout.Button(item.name, GUILayout.Height(30))) selectedGrowth = item;
        }
        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    private void DrawRightDetail()
    {
        EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

        try
        {
            if (selectedGrowth == null)
            {
                EditorGUILayout.HelpBox("성장 데이터를 선택해주세요.", MessageType.Info);
                return;
            }

            EditorGUILayout.LabelField($"Editing: {selectedGrowth.name}", EditorStyles.boldLabel);
            SerializedObject so = new SerializedObject(selectedGrowth);
            EditorGUILayout.PropertyField(so.FindProperty("growthID"));

            GUILayout.Space(10);
            currentTab = GUILayout.Toolbar(currentTab, tabNames);
            GUILayout.Space(10);

            detailScrollPos = EditorGUILayout.BeginScrollView(detailScrollPos);
            string propName = propertyNames[currentTab];
            SerializedProperty listProp = so.FindProperty(propName);
            EditorGUILayout.LabelField($"{tabNames[currentTab]} Rules", EditorStyles.boldLabel);
            DrawStatAdder($"Add {tabNames[currentTab]} Stat", listProp);

            EditorGUILayout.PropertyField(listProp, true);
            so.ApplyModifiedProperties(); 

            GUILayout.Space(20);

            DrawStatSimulator();
            EditorGUILayout.EndScrollView();
        }
        finally
        {
            EditorGUILayout.EndVertical();
        }
    }

    private void DrawStatSimulator()
    {
        EditorGUILayout.LabelField("Stat Simulator (Total Growth)", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical("helpbox");

        baseStatInput = EditorGUILayout.FloatField("Base Stat Value", baseStatInput);

        EditorGUILayout.BeginHorizontal();
        simLevel = EditorGUILayout.IntSlider("Level", simLevel, 1, 101);
        simEnforce = EditorGUILayout.IntSlider("Enforce", simEnforce, 0, 10);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        simRank = EditorGUILayout.IntSlider("Rank", simRank, 1, 6);
        simUpgrade = EditorGUILayout.IntSlider("Upgrade", simUpgrade, 0, 10);
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(10);
        SerializedObject so = new SerializedObject(selectedGrowth);

        EditorGUILayout.PropertyField(so.FindProperty("levelRules"), true);
        EditorGUILayout.PropertyField(so.FindProperty("enforceRules"), true);
        so.ApplyModifiedProperties();

        GUILayout.Space(10);

        // 스탯별 상세 계산
        foreach (StatType type in Enum.GetValues(typeof(StatType)))
        {
            if (type == StatType.MoveSpeed && !isMonsterData) continue;

            // 타입별 보너스 합산
            float levelBonus = selectedGrowth.GetValue(GrowthType.Level, type) * (simLevel - 1);
            float enforceBonus = selectedGrowth.GetValue(GrowthType.Enforce, type) * simEnforce;
            float rankBonus = selectedGrowth.GetValue(GrowthType.Rank, type) * simRank;
            float upgradeBonus = selectedGrowth.GetValue(GrowthType.Upgrade, type) * simUpgrade;

            float totalBonus = baseStatInput * (levelBonus + enforceBonus + rankBonus + upgradeBonus);
            float finalStat = baseStatInput + totalBonus;

            EditorGUILayout.LabelField($"{type}:", $"{finalStat:F2} (Bonus: {totalBonus:F2})", EditorStyles.boldLabel);
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawStatAdder(string label, SerializedProperty listProperty)
    {
        EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();

        foreach (StatType type in Enum.GetValues(typeof(StatType)))
        {
            bool exists = false;
            for (int i = 0; i < listProperty.arraySize; i++)
            {
                var element = listProperty.GetArrayElementAtIndex(i);
                if (element.FindPropertyRelative("statType").enumValueIndex == (int)type)
                {
                    exists = true;
                    break;
                }
            }

            if (!exists)
            {
                if (GUILayout.Button($"+ {type}", GUILayout.Width(70)))
                {
                    listProperty.arraySize++;
                    var newElement = listProperty.GetArrayElementAtIndex(listProperty.arraySize - 1);
                    newElement.FindPropertyRelative("statType").enumValueIndex = (int)type;
                    newElement.FindPropertyRelative("value").floatValue = 0f;
                }
            }
        }
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(5);
    }

}