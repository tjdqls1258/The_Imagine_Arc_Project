using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.Collections.Generic;

public class EditorSceneUtilWindow : EditorWindow
{
    private const string SAVE_KEY = "UTIL_SCENELIST";

    private List<SceneAsset> scenes = new List<SceneAsset>();


    [MenuItem("Tools/Editor Scene Util")]
    public static void Open()
    {
        GetWindow<EditorSceneUtilWindow>("Editor Scene Util");
    }

    private void OnEnable()
    {
        LoadScenes();
    }

    private void OnDisable()
    {
        SaveScenes();
    }

    private void OnGUI()
    {
        GUILayout.Label("Scene List", EditorStyles.boldLabel);

        int removeIndex = -1;

        for (int i = 0; i < scenes.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();

            scenes[i] = (SceneAsset)EditorGUILayout.ObjectField(scenes[i], typeof(SceneAsset), false);

            if (GUILayout.Button("Open", GUILayout.Width(60)))
            {
                string path = AssetDatabase.GetAssetPath(scenes[i]);
                EditorSceneManager.OpenScene(path);
            }

            if (GUILayout.Button("X", GUILayout.Width(20)))
            {
                removeIndex = i;
            }

            EditorGUILayout.EndHorizontal();
        }

        if (removeIndex >= 0)
        {
            scenes.RemoveAt(removeIndex);
            SaveScenes();
        }

        if (GUILayout.Button("Add Scene"))
        {
            scenes.Add(null);
        }
    }

    void SaveScenes()
    {
        List<string> paths = new List<string>();

        foreach (var scene in scenes)
        {
            if (scene != null)
                paths.Add(AssetDatabase.GetAssetPath(scene));
        }

        string data = string.Join("|", paths);
        EditorPrefs.SetString(SAVE_KEY, data);
    }

    void LoadScenes()
    {
        scenes.Clear();

        string data = EditorPrefs.GetString(SAVE_KEY, "");

        if (string.IsNullOrEmpty(data))
            return;

        string[] paths = data.Split('|');

        foreach (var path in paths)
        {
            SceneAsset scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
            if (scene != null)
                scenes.Add(scene);
        }
    }
}
