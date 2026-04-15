#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;


[InitializeOnLoad]
public class ButtonSoundAutoAdder
{
    static ButtonSoundAutoAdder()
    {
        EditorApplication.hierarchyChanged += OnHierarchyChanged;
    }

    [MenuItem("Tools/Fix Button Sounds")]
    public static void ManualFix()
    {
        int addedCount = 0;

        addedCount += CheckAndAddInActiveScene();
        var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
        if (prefabStage != null)
        {
            addedCount += CheckAndAddInPrefab(prefabStage);
        }

        EditorUtility.DisplayDialog("Button Sound Fixer",
            $"{addedCount}���� ��ư�� ClickSound�� �߰��߽��ϴ�.", "Ȯ��");
    }

    private static void OnHierarchyChanged()
    {
        CheckAndAddInActiveScene();

        var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
        if (prefabStage != null)
        {
            CheckAndAddInPrefab(prefabStage);
        }
    }

    private static int CheckAndAddInActiveScene()
    {
        int count = 0;
        Button[] allButtons = GameObject.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (var btn in allButtons)
        {
            if (!EditorUtility.IsPersistent(btn.gameObject))
            {
                if (AddScriptIfMissing(btn)) count++;
            }
        }
        return count;
    }

    private static int CheckAndAddInPrefab(PrefabStage stage)
    {
        int count = 0;
        Button[] buttonsInPrefab = stage.prefabContentsRoot.GetComponentsInChildren<Button>(true);

        foreach (var btn in buttonsInPrefab)
        {
            if (AddScriptIfMissing(btn)) count++;
        }
        return count;
    }

    private static bool AddScriptIfMissing(Button btn)
    {
        if (btn.gameObject.GetComponent<ClickSound>() == null)
        {
            var clickSound = Undo.AddComponent<ClickSound>(btn.gameObject);
            clickSound.SetButton();

            if (!Application.isPlaying)
            {
                EditorSceneManager.MarkSceneDirty(btn.gameObject.scene);
            }

            Debug.Log($"<color=cyan>[AutoSound]</color> {btn.name} ������Ʈ�� ClickSound�� �߰��߽��ϴ�.");
            return true;
        }
        return false;
    }
}
#endif