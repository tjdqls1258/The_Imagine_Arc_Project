using UnityEditor;
using UnityEngine;
using System;
using System.Linq;

namespace Util_Patten.Editor
{
    // StateManager 및 그 자식 클래스들을 위한 에디터
    [CustomEditor(typeof(StateManager<,>), true)]
    public class StateManagerEditor : UnityEditor.Editor
    {
        private SerializedProperty m_statesProp;
        private SerializedProperty m_currentStateProp;

        private void OnEnable()
        {
            // StateManager 클래스 내부의 변수 이름과 일치해야 합니다.
            m_statesProp = serializedObject.FindProperty("m_states");
            m_currentStateProp = serializedObject.FindProperty("m_currentState");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // 1. 현재 설정된 상태 표시 (Enum)
            EditorGUILayout.LabelField("Current Configuration", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(m_currentStateProp);

            EditorGUILayout.Space(10);

            // 2. 런타임 디버깅 정보 (게임 실행 중에만 표시)
            if (Application.isPlaying)
            {
                DrawRuntimeStateInfo();
            }

            // 3. 상태 리스트 관리
            EditorGUILayout.LabelField("State List (Logic)", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(m_statesProp, true);

            EditorGUILayout.Space(5);

            // 4. 새로운 상태 클래스 추가 버튼
            if (GUILayout.Button("Add New State Logic", GUILayout.Height(30)))
            {
                ShowStateSelectionMenu();
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawRuntimeStateInfo()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                GUI.color = Color.yellow;
                string stateName = m_currentStateProp.enumDisplayNames[m_currentStateProp.enumValueIndex];
                EditorGUILayout.LabelField($"ACTIVE STATE: {stateName}", EditorStyles.boldLabel);
                GUI.color = Color.white;
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(5);
        }

        private void ShowStateSelectionMenu()
        {
            GenericMenu menu = new GenericMenu();

            // 현재 에디터가 가리키는 대상의 제네릭 타입을 분석하여 적절한 State 타입을 찾음
            Type targetType = target.GetType();
            while (targetType != null && !targetType.IsGenericType && targetType.BaseType != null)
            {
                targetType = targetType.BaseType;
            }

            if (targetType == null || !targetType.IsGenericType) return;

            // State<T, TOwner> 형태의 베이스 타입을 구성
            Type[] genericArgs = targetType.GetGenericArguments();
            Type stateBaseType = typeof(State<,>).MakeGenericType(genericArgs);

            // 프로젝트 내에서 해당 베이스 타입을 상속받은 모든 클래스 검색
            var stateTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => stateBaseType.IsAssignableFrom(p) && !p.IsAbstract && !p.IsInterface);

            foreach (var type in stateTypes)
            {
                menu.AddItem(new GUIContent(type.Name), false, () => AddStateInstance(type));
            }

            if (menu.GetItemCount() == 0)
                menu.AddDisabledItem(new GUIContent("No matching State classes found"));

            menu.ShowAsContext();
        }

        private void AddStateInstance(Type type)
        {
            serializedObject.Update();

            int index = m_statesProp.arraySize;
            m_statesProp.InsertArrayElementAtIndex(index);

            // SerializeReference를 사용하여 인스턴스 할당
            var element = m_statesProp.GetArrayElementAtIndex(index);
            element.managedReferenceValue = Activator.CreateInstance(type);

            serializedObject.ApplyModifiedProperties();
        }
    }
}