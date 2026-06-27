#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(UserSkillBase))]
public class UserSkillBaseEditor : Editor
{
    private SerializedProperty m_skillIDProp;
    private SerializedProperty m_skillNameProp;
    private SerializedProperty m_skillDesc;
    private SerializedProperty m_skillIconProp;
    private SerializedProperty m_cooldownProp;
    private SerializedProperty m_costProp;
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
        m_costProp = serializedObject.FindProperty("Cost");

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
        EditorGUILayout.PropertyField(m_costProp);
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
            EditorGUILayout.PropertyField(m_targetingModeProp, new GUIContent("Current Targeting"), true);
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

        if (GUILayout.Button("Add Effect Module", GUILayout.Height(25)))
        {
            ShowTypeMenu(typeof(EffectModule), (selectedType) =>
            {
                m_effectModesProp.arraySize++;
                SerializedProperty newElement = m_effectModesProp.GetArrayElementAtIndex(m_effectModesProp.arraySize - 1);
                newElement.managedReferenceValue = Activator.CreateInstance(selectedType);
                serializedObject.ApplyModifiedProperties();
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
}
#endif