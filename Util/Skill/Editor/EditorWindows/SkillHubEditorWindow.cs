using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class SkillHubEditorWindow : EditorWindow
{
    private List<SkillBase> skillList = new List<SkillBase>();
    private SkillBase selectedSkill;

    private Dictionary<SkillBase, string> skillTypeCache = new Dictionary<SkillBase, string>();
    private List<string> categoryTabs = new List<string>();
    private int selectedCategoryIndex = 0;

    private Vector2 listScrollPos;
    private Vector2 detailScrollPos;
    private Editor cachedEditor;
    private string searchKeyword = "";
    private int newSkillId = 0;

    [MenuItem("Tools/Data/1. Skill Hub")]
    public static void ShowWindow()
    {
        var window = GetWindow<SkillHubEditorWindow>("Skill Hub");
        window.minSize = new Vector2(850, 500);
    }

    private void OnEnable()
    {
        RefreshSkillAssets();
    }

    private void OnDisable()
    {
        if (cachedEditor != null) DestroyImmediate(cachedEditor);
    }

    private void RefreshSkillAssets()
    {
        skillList.Clear();
        skillTypeCache.Clear();
        categoryTabs.Clear();

        categoryTabs.Add("All"); // 전체 보기 탭 기본 추가

        string[] guids = AssetDatabase.FindAssets("t:SkillBase");

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            SkillBase skillAsset = AssetDatabase.LoadAssetAtPath<SkillBase>(path);

            if (skillAsset != null && !skillList.Contains(skillAsset))
            {
                skillList.Add(skillAsset);

                // SerializedObject를 통해 Enum 'Type'의 값을 안전하게 문자열로 추출
                string tabName = "Unknown";
                if (skillAsset is UserSkillBase)
                {
                    tabName = "User Skill";
                }
                else
                {
                    SerializedObject so = new SerializedObject(skillAsset);
                    SerializedProperty typeProp = so.FindProperty("Type");

                    if (typeProp != null && typeProp.propertyType == SerializedPropertyType.Enum)
                    {
                        tabName = typeProp.enumNames[typeProp.enumValueIndex];
                    }
                }

                skillTypeCache[skillAsset] = tabName;
                if (!categoryTabs.Contains(tabName))
                {
                    categoryTabs.Add(tabName);
                }
            }
        }

        skillList = skillList.OrderBy(s => s.name).ToList();

        if (selectedCategoryIndex >= categoryTabs.Count) selectedCategoryIndex = 0;

        if (selectedSkill != null && !skillList.Contains(selectedSkill))
            selectedSkill = null;
    }

    private void OnGUI()
    {
        DrawTopBar();

        EditorGUILayout.BeginHorizontal();
        DrawLeftSkillList();
        DrawRightDetailArea();
        EditorGUILayout.EndHorizontal();
    }

    private void DrawTopBar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

        if (GUILayout.Button("새로고침", EditorStyles.toolbarButton, GUILayout.Width(80)))
        {
            RefreshSkillAssets();
        }

        GUILayout.Space(20);
        EditorGUILayout.LabelField("스킬 검색:", GUILayout.Width(60));
        searchKeyword = EditorGUILayout.TextField(searchKeyword, EditorStyles.toolbarSearchField, GUILayout.Width(200));

        GUILayout.FlexibleSpace();

        EditorGUILayout.LabelField("신규 스킬 ID:", GUILayout.Width(80));
        newSkillId = EditorGUILayout.IntField(newSkillId, GUILayout.Width(50));

        if (GUILayout.Button("+ 스킬 생성", EditorStyles.toolbarButton, GUILayout.Width(80)))
        {
            CreateNewSkill(newSkillId);
        }

        EditorGUILayout.EndHorizontal();
    }

    private void DrawLeftSkillList()
    {
        EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(260), GUILayout.ExpandHeight(true));

        EditorGUILayout.LabelField($"등록된 스킬 목록 ({skillList.Count}개)", EditorStyles.boldLabel);
        GUILayout.Space(5);

        // --- 추가된 부분: 카테고리 탭 (All / Active / Passive 등) ---
        selectedCategoryIndex = GUILayout.Toolbar(selectedCategoryIndex, categoryTabs.ToArray());
        GUILayout.Space(5);

        listScrollPos = EditorGUILayout.BeginScrollView(listScrollPos);

        foreach (var skill in skillList)
        {
            if (!string.IsNullOrEmpty(searchKeyword) && !skill.name.ToLower().Contains(searchKeyword.ToLower()))
                continue;

            string skillType = skillTypeCache.ContainsKey(skill) ? skillTypeCache[skill] : "Unknown";
            if (selectedCategoryIndex != 0 && categoryTabs[selectedCategoryIndex] != skillType)
                continue;

            GUI.backgroundColor = (selectedSkill == skill) ? Color.cyan : Color.white;

            string buttonText = selectedCategoryIndex == 0 ? $"[{skillType}] {skill.name}" : skill.name;

            if (GUILayout.Button(buttonText, GUILayout.Height(30)))
            {
                selectedSkill = skill;
                GUI.FocusControl(null);
            }
        }

        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    private void DrawRightDetailArea()
    {
        EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

        if (selectedSkill == null)
        {
            EditorGUILayout.LabelField("좌측 목록에서 편집할 스킬을 선택해주세요.", EditorStyles.centeredGreyMiniLabel);
            EditorGUILayout.EndVertical();
            return;
        }

        EditorGUILayout.BeginHorizontal();

        // 상단에 선택된 스킬 이름과 타입 표시
        string typeStr = skillTypeCache.ContainsKey(selectedSkill) ? skillTypeCache[selectedSkill] : "";
        EditorGUILayout.LabelField($"선택된 스킬: {selectedSkill.name} ({typeStr})", EditorStyles.boldLabel);

        if (GUILayout.Button("에셋 위치 찾기", GUILayout.Width(130)))
        {
            Selection.activeObject = selectedSkill;
            EditorGUIUtility.PingObject(selectedSkill);
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(10);

        detailScrollPos = EditorGUILayout.BeginScrollView(detailScrollPos);

        // 인스펙터 렌더링
        if (cachedEditor == null || cachedEditor.target != selectedSkill)
        {
            if (cachedEditor != null) DestroyImmediate(cachedEditor);
            cachedEditor = Editor.CreateEditor(selectedSkill);
        }

        if (cachedEditor != null)
        {
            EditorGUILayout.BeginVertical(GUI.skin.window);
            cachedEditor.OnInspectorGUI();
            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    private void CreateNewSkill(int id)
    {
        string defaultPath = "Assets/Util/Skill/";
        if (!Directory.Exists(defaultPath)) Directory.CreateDirectory(defaultPath);

        string assetPath = $"{defaultPath}Skill_{id}.asset";

        if (File.Exists(assetPath))
        {
            EditorUtility.DisplayDialog("경고", $"이미 해당 ID({id})를 가진 스킬 에셋이 존재합니다.\n경로: {assetPath}", "확인");
            return;
        }

        SkillBase newAsset = ScriptableObject.CreateInstance<SkillBase>();

        AssetDatabase.CreateAsset(newAsset, assetPath);
        AssetDatabase.SaveAssets();

        RefreshSkillAssets(); // 리스트 및 탭 갱신
        selectedSkill = newAsset;
        newSkillId++;

        Debug.Log($"[Skill Hub] 새로운 스킬 에셋 생성: {assetPath}");
    }
}