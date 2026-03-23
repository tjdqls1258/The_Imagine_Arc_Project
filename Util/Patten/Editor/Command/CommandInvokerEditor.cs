using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;

namespace Util_Patten.Editor
{
    [CustomEditor(typeof(CommandInvoker))]
    public class CommandInvokerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (Application.isPlaying)
            {
                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField("Command History", EditorStyles.boldLabel);

                // Reflection으로 private stack 가져오기
                var field = target.GetType().GetField("m_commandHistory", BindingFlags.NonPublic | BindingFlags.Instance);
                var history = field?.GetValue(target) as IEnumerable<ICommand>;

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                if (history != null)
                {
                    foreach (var cmd in history)
                    {
                        EditorGUILayout.LabelField($"→ {cmd.GetType().Name}");
                    }
                }
                EditorGUILayout.EndVertical();

                if (GUILayout.Button("Undo Last Command", GUILayout.Height(25)))
                {
                    (target as CommandInvoker).UndoCommand();
                }
            }
        }
    }
}