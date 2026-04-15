#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Linq;
using UnityEngine;

public class SkillBase : ScriptableObject, IUpdateDataFormSheet, IToolTip
{
    public enum SkillType
    {
        Passive,
        Active,
        Buff
    }
    public int ID;
    public string SkillName;
    public string SkillDescription;
    public float SkillCoolTime;
    public float Duration;
    public string SkillIconName;
    public SkillType Type;

    public Sprite SkillIcon;
    [SerializeReference] public SkillActionBase SkillClass;

    public virtual void SkillUse(InGameCharacterData data, IGamePlayCharacter targetControll = null)
    {
        SkillClass.ActiveSkill(data, targetControll);
    }

    public string GetTitle()
    {
        return SkillName;
    }

    public string GetDescription()
    {
        return $"{SkillDescription}";
    }

    public string GetCoolTime()
    {
        if (SkillCoolTime <= 0.0001f)
            return string.Empty;

        return $"스킬 쿨타임 : {SkillCoolTime:N0}";
    }

#if UNITY_EDITOR
    [ContextMenu("Set Skill Icon")]
    public void SetSkillIcon()
    {
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(string.Format(Util.SKILL_SPRITE_PATH, SkillIconName));
        SkillIcon = sprite;
        AssetDatabase.SaveAssetIfDirty(this);
        AssetDatabase.SaveAssets();
    }
#endif
}

[Serializable] // SerializeReference를 위해 필요할 수 있습니다.
public abstract class SkillActionBase
{
    public abstract bool ActiveSkill(InGameCharacterData data, IGamePlayCharacter targetControll = null);
}

#if UNITY_EDITOR

[CustomEditor(typeof(SkillBase))]
public class SkillBaseEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        SerializedProperty iterator = serializedObject.GetIterator();
        bool enterChildren = true;
        while (iterator.NextVisible(enterChildren))
        {
            enterChildren = false;

            if (iterator.name == "SkillClass") continue;
            // m_Script 필드를 숨기고 싶지 않다면 그대로 두시고, 숨기고 싶다면 여기에 추가하세요.
            EditorGUILayout.PropertyField(iterator, true);
        }

        EditorGUILayout.Space(15);
        EditorGUILayout.LabelField("스킬 액션 설정", EditorStyles.boldLabel);

        // 2. SkillClass 프로퍼티 가져오기
        SerializedProperty skillClassProp = serializedObject.FindProperty("SkillClass");

        // 3. 현재 할당된 클래스 이름 확인
        string currentTypeName = "없음 (None)";
        if (!string.IsNullOrEmpty(skillClassProp.managedReferenceFullTypename))
        {
            currentTypeName = skillClassProp.managedReferenceFullTypename.Split(' ').Last();
        }

        // 4. 드롭다운 UI 그리기
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Skill Class", GUILayout.Width(EditorGUIUtility.labelWidth));

        if (GUILayout.Button(currentTypeName, EditorStyles.popup))
        {
            ShowTypeMenu(skillClassProp); // 버튼 클릭 시 메뉴 표시
        }
        GUILayout.EndHorizontal();

        // 5. 클래스가 할당되었다면, 해당 클래스의 내부 멤버들을 인스펙터에 표시
        if (skillClassProp.managedReferenceValue != null)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(skillClassProp, new GUIContent("액션 세부 설정"), true);
            EditorGUI.indentLevel--;
        }

        serializedObject.ApplyModifiedProperties();
    }

    // 드롭다운 메뉴 생성 (TypeCache 활용)
    private void ShowTypeMenu(SerializedProperty property)
    {
        GenericMenu menu = new GenericMenu();

        // None (비우기)
        menu.AddItem(new GUIContent("None"), false, () =>
        {
            serializedObject.Update();
            property.managedReferenceValue = null;
            serializedObject.ApplyModifiedProperties();
        });

        menu.AddSeparator("");

        // SkillActionBase를 상속받은 모든 클래스를 찾아 메뉴에 추가
        var types = TypeCache.GetTypesDerivedFrom<SkillActionBase>();
        foreach (var type in types)
        {
            if (type.IsAbstract || type.IsInterface) continue;

            Type targetType = type;
            menu.AddItem(new GUIContent(targetType.Name), false, () =>
            {
                serializedObject.Update();
                // 해당 클래스의 인스턴스를 생성하여 할당
                property.managedReferenceValue = Activator.CreateInstance(targetType);
                serializedObject.ApplyModifiedProperties();
            });
        }

        menu.ShowAsContext();
    }
}

#endif