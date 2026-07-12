#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SkillBase))]
public class SkillBaseEditor : Editor
{
    private SerializedProperty m_skillIDProp;
    private SerializedProperty m_skillNameProp;
    private SerializedProperty m_skillDesc;
    private SerializedProperty m_skillIconProp;
    private SerializedProperty m_cooldownProp;
    private SerializedProperty m_skillType;
    private SerializedProperty m_ActivationTrigger;
    private SerializedProperty m_ProcChance;

    private SerializedProperty m_targetingModeProp;
    private SerializedProperty m_effectModesProp;

    private void OnEnable()
    {
        m_skillIDProp = serializedObject.FindProperty("SkillID");
        m_skillNameProp = serializedObject.FindProperty("SkillName");
        m_skillDesc = serializedObject.FindProperty("SkillDesc");
        m_skillIconProp = serializedObject.FindProperty("SkillIcon");
        m_ActivationTrigger = serializedObject.FindProperty("ActivationTrigger");
        m_skillType = serializedObject.FindProperty("Type");
        m_ProcChance = serializedObject.FindProperty("ProcChance");
        m_cooldownProp = serializedObject.FindProperty("Cooldown");

        m_targetingModeProp = serializedObject.FindProperty("TargetingMode");
        m_effectModesProp = serializedObject.FindProperty("EffectModes");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("Base Information", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.PropertyField(m_skillIDProp);
        EditorGUILayout.PropertyField(m_skillNameProp);
        EditorGUILayout.PropertyField(m_skillDesc);
        EditorGUILayout.PropertyField(m_skillIconProp);
        EditorGUILayout.PropertyField(m_ActivationTrigger);
        EditorGUILayout.PropertyField(m_skillType);
        EditorGUILayout.PropertyField(m_cooldownProp);
        EditorGUILayout.PropertyField(m_ProcChance);
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(10);

        DrawTargetingModuleUI();

        EditorGUILayout.Space(10);

        DrawEffectModulesUI();

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawTargetingModuleUI()
    {
        EditorGUILayout.LabelField("Targeting System", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical("helpbox");

        if (m_targetingModeProp.managedReferenceValue != null)
        {
            
            DrawIndicatorDropdown(m_targetingModeProp, "MaxRangeIndicatorPrefab", "Max Range Prefab");
            DrawIndicatorDropdown(m_targetingModeProp, "ShapeIndicatorPrefab", "Shape Prefab");

            GUILayout.Space(2);
            if (GUILayout.Button("Remove Targeting Module", GUILayout.Height(20)))
            {
                m_targetingModeProp.managedReferenceValue = null;
                serializedObject.ApplyModifiedProperties();
            }
        }
        else
        {
            EditorGUILayout.HelpBox("타겟팅 모듈이 설정되지 않았습니다.", MessageType.Warning);
        }

        if (GUILayout.Button("Set / Change Targeting Module", GUILayout.Height(25)))
        {
            ShowTypeMenu(typeof(TargetingModule), (selectedType) =>
            {
                m_targetingModeProp.managedReferenceValue = Activator.CreateInstance(selectedType);
                serializedObject.ApplyModifiedProperties();
            });
        }
        EditorGUILayout.EndVertical();
    }

    private void DrawEffectModulesUI()
    {
        EditorGUILayout.LabelField("Effect Modules", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical("helpbox");

        EditorGUILayout.PropertyField(m_effectModesProp, new GUIContent("Effects List"), true);

        for (int i = 0; i < m_effectModesProp.arraySize; i++)
        {
            var element = m_effectModesProp.GetArrayElementAtIndex(i);
            DrawPrefabDropdown(element, "EffectPrefab", $"Effect {i} Prefab", typeof(SkillEffectObject));
        }

        if (GUILayout.Button("Add Effect Module", GUILayout.Height(25)))
        {
            ShowTypeMenu(typeof(EffectModule), (selectedType) => {
                m_effectModesProp.arraySize++;
                m_effectModesProp.GetArrayElementAtIndex(m_effectModesProp.arraySize - 1).managedReferenceValue = Activator.CreateInstance(selectedType);
            });
        }
        EditorGUILayout.EndVertical();
    }

    // 특정 기반 클래스(추상 클래스)를 상속받은 모든 구체 클래스를 찾아 드롭다운으로 보여줍니다.
    private void ShowTypeMenu(Type baseType, Action<Type> onSelect)
    {
        GenericMenu menu = new GenericMenu();

        var derivedTypes = TypeCache.GetTypesDerivedFrom(baseType)
                                    .Where(t => !t.IsAbstract && !t.IsInterface);

        foreach (var type in derivedTypes)
        {
            menu.AddItem(new GUIContent(type.Name), false, () => onSelect(type));
        }

        menu.ShowAsContext();
    }

    private void DrawIndicatorDropdown(SerializedProperty moduleProp, string fieldName, string label)
    {
        SerializedProperty prop = moduleProp.FindPropertyRelative(fieldName);

        string[] guids = AssetDatabase.FindAssets("t:Prefab");
        List<string> names = new List<string> { "None" };
        List<IndicatorObject> objects = new List<IndicatorObject> { null };

        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            IndicatorObject comp = go.GetComponent<IndicatorObject>();

            if (comp != null)
            {
                names.Add(go.name);
                objects.Add(comp);
            }
        }

        int currentIndex = objects.IndexOf((IndicatorObject)prop.objectReferenceValue);
        if (currentIndex == -1) currentIndex = 0;

        int newIndex = EditorGUILayout.Popup(label, currentIndex, names.ToArray());
        if (newIndex != currentIndex)
        {
            prop.objectReferenceValue = objects[newIndex];
            serializedObject.ApplyModifiedProperties();
        }
    }

    private void DrawPrefabDropdown(SerializedProperty parentProp, string fieldName, string label, Type componentType)
    {
        if (parentProp == null) return;

        SerializedProperty prop = parentProp.FindPropertyRelative(fieldName);

        if (prop == null)
        {
            return;
        }

        string[] guids = AssetDatabase.FindAssets("t:Prefab");
        List<string> names = new List<string> { "None" };
        List<UnityEngine.Object> objects = new List<UnityEngine.Object> { null };

        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            var comp = go.GetComponent(componentType);

            if (comp != null)
            {
                names.Add(go.name);
                objects.Add(comp);
            }
        }

        int currentIndex = objects.IndexOf(prop.objectReferenceValue);
        if (currentIndex == -1) currentIndex = 0;

        int newIndex = EditorGUILayout.Popup(label, currentIndex, names.ToArray());

        if (newIndex != currentIndex)
        {
            prop.objectReferenceValue = objects[newIndex];
            serializedObject.ApplyModifiedProperties(); // 변경사항 적용
        }
    }
}
#endif