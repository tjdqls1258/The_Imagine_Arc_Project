using SettingSpawnEnemyWindows;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapData))]
public class MapDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Open Path Setting Window"))
        {
            EnemySpawnSettingWindow.ShowWindow((MapData)target);
        }
    }
}