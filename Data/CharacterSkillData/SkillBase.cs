#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
#endif
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

#if UNITY_EDITOR
    [ContextMenu("Set Skill Icon")]
    public void SetSkillIcon()
    {
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(string.Format(Util.SKILL_SPRITE_PATH, SkillIconName));
        SkillIcon = sprite;
        AssetDatabase.SaveAssetIfDirty(this);
        AssetDatabase.SaveAssets();
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
#endif
}

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

        // 1. 기본 변수들 먼저 그리기 (SkillClass는 제외)
        SerializedProperty iterator = serializedObject.GetIterator();
        bool enterChildren = true;
        while (iterator.NextVisible(enterChildren))
        {
            enterChildren = false;

            // "SkillClass"는 기본 UI(빈 공간)로 그리지 않고 아래에서 커스텀으로 그립니다.
            if (iterator.name == "SkillClass") continue;

            EditorGUILayout.PropertyField(iterator, true);
        }

        EditorGUILayout.Space(15);
        EditorGUILayout.LabelField("스킬 핵심 로직 설정", EditorStyles.boldLabel);

        // 2. SkillClass 프로퍼티 가져오기
        SerializedProperty skillClassProp = serializedObject.FindProperty("SkillClass");

        // 3. 현재 할당된 클래스 이름 확인
        string currentTypeName = "비어있음 (None)";
        if (!string.IsNullOrEmpty(skillClassProp.managedReferenceFullTypename))
        {
            currentTypeName = skillClassProp.managedReferenceFullTypename.Split(' ').Last();
        }

        // 4. 드롭다운 UI 그리기
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Skill Class", GUILayout.Width(EditorGUIUtility.labelWidth));

        if (GUILayout.Button(currentTypeName, EditorStyles.popup))
        {
            ShowTypeMenu(skillClassProp); // 버튼 누르면 메뉴 표시
        }
        GUILayout.EndHorizontal();

        // 5. 클래스가 할당되었다면, 그 클래스 내부의 변수들을 펼쳐서 보여주기
        if (currentTypeName != "비어있음 (None)")
        {
            EditorGUI.indentLevel++; // 살짝 들여쓰기해서 보기 좋게
            EditorGUILayout.PropertyField(skillClassProp, new GUIContent("세부 수치 입력"), true);
            EditorGUI.indentLevel--;
        }

        serializedObject.ApplyModifiedProperties();
    }

    // 드롭다운 메뉴 띄우기 (TypeCache 활용)
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
                // 해당 클래스를 생성하여 꽂아넣음
                property.managedReferenceValue = Activator.CreateInstance(targetType);
                serializedObject.ApplyModifiedProperties();
            });
        }

        menu.ShowAsContext();
    }
}

#endif