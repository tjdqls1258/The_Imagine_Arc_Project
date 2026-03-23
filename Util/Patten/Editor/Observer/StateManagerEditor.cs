using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Util_Patten.Editor
{
    // 1. MonoBehaviour 형태인 ObserverStation용 에디터
    [CustomEditor(typeof(ObserverStation<>), true)]
    public class ObserverStationEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            // 기본 리스트(m_observers)와 값(m_value) 표시
            DrawDefaultInspector();

            if (Application.isPlaying)
            {
                DrawObserverDebugger(target);
            }
        }

        private void DrawObserverDebugger(object targetObject)
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.LabelField("Observer Live Monitor", EditorStyles.boldLabel);

                // Reflection을 통해 제네릭 Value 프로퍼티 가져오기
                PropertyInfo valueProp = targetObject.GetType().GetProperty("Value");
                object currentValue = valueProp?.GetValue(targetObject);

                GUI.enabled = false;
                EditorGUILayout.LabelField("Current Value:", currentValue?.ToString() ?? "Null");
                GUI.enabled = true;

                EditorGUILayout.Space(5);

                if (GUILayout.Button("Test Notify (Trigger Event)", GUILayout.Height(25)))
                {
                    // SetValue를 호출하여 강제로 알림 발생 테스트
                    MethodInfo setMethod = targetObject.GetType().GetMethod("SetValue");
                    setMethod?.Invoke(targetObject, new object[] { currentValue });

                    Debug.Log($"<color=cyan>[Observer System]</color> Test notification sent with value: {currentValue}");
                }
            }
            EditorGUILayout.EndVertical();
        }
    }

    // 2. 일반 클래스 형태인 ObserverEventStation용 프로퍼티 드로어
    // (이 클래스가 다른 MonoBehaviour의 필드로 있을 때 인스펙터에 디버그 버튼 노출)
    [CustomPropertyDrawer(typeof(ObserverEventStation<>), true)]
    public class ObserverEventDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // 기본 필드 표시 (foldout)
            Rect foldoutRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label);

            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;

                // m_value 필드 표시
                SerializedProperty valueProp = property.FindPropertyRelative("m_value");
                Rect valueRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + 2, position.width, EditorGUIUtility.singleLineHeight);
                EditorGUI.PropertyField(valueRect, valueProp);

                // 실행 중일 때만 테스트 버튼 표시
                if (Application.isPlaying)
                {
                    Rect buttonRect = new Rect(position.x, valueRect.y + EditorGUIUtility.singleLineHeight + 5, position.width, 20);
                    if (GUI.Button(buttonRect, "Manual Invoke (Event Test)"))
                    {
                        // 리플렉션으로 NotifyObserver 실행
                        object targetObj = GetTargetObjectOfProperty(property);
                        MethodInfo method = targetObj.GetType().GetMethod("NotifyObserver", BindingFlags.Instance | BindingFlags.NonPublic);
                        method?.Invoke(targetObj, null);
                    }
                }

                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!property.isExpanded) return EditorGUIUtility.singleLineHeight;
            return EditorGUIUtility.singleLineHeight * (Application.isPlaying ? 3.5f : 2.5f);
        }

        // SerializedProperty에서 실제 객체 인스턴스를 찾아오는 유틸리티
        private object GetTargetObjectOfProperty(SerializedProperty prop)
        {
            var path = prop.propertyPath.Replace(".Array.data[", "[");
            object obj = prop.serializedObject.targetObject;
            var elements = path.Split('.');
            foreach (var element in elements)
            {
                if (element.Contains("["))
                {
                    var elementName = element.Substring(0, element.IndexOf("["));
                    var index = Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                    obj = GetValue_Imp(obj, elementName, index);
                }
                else
                {
                    obj = GetValue_Imp(obj, element);
                }
            }
            return obj;
        }

        private object GetValue_Imp(object source, string name) => source?.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)?.GetValue(source);
        private object GetValue_Imp(object source, string name, int index) => (GetValue_Imp(source, name) as System.Collections.IEnumerable)?.Cast<object>().ElementAt(index);
    }
}