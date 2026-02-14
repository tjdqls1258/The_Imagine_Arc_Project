#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 유니티 에디터 상에서 Button 컴포넌트가 감지되면 
/// 자동으로 ClickSound 스크립트를 추가해주는 자동화 툴입니다.
/// </summary>
[InitializeOnLoad]
public class ButtonSoundAutoAdder
{
    // ----------------------------------------------------------------------
    // ## Editor Events Registration
    // ----------------------------------------------------------------------

    static ButtonSoundAutoAdder()
    {
        // 하이어라키 구조가 변경될 때마다(오브젝트 생성, 이름 변경 등) 실행 환경 등록
        EditorApplication.hierarchyChanged += OnHierarchyChanged;
    }

    // ----------------------------------------------------------------------
    // ## Menu Items
    // ----------------------------------------------------------------------

    /// <summary>
    /// [수동 실행 메뉴] 상단 Tools 메뉴를 통해 현재 씬 및 프리팹 모드의 모든 버튼을 검사합니다.
    /// </summary>
    [MenuItem("Tools/Fix Button Sounds")]
    public static void ManualFix()
    {
        int addedCount = 0;

        // 1. 현재 열려있는 씬(Active Scene) 검사
        addedCount += CheckAndAddInActiveScene();

        // 2. 프리팹 편집 모드(Prefab Stage)인 경우 해당 프리팹 검사
        var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
        if (prefabStage != null)
        {
            addedCount += CheckAndAddInPrefab(prefabStage);
        }

        EditorUtility.DisplayDialog("Button Sound Fixer",
            $"{addedCount}개의 버튼에 ClickSound를 추가했습니다.", "확인");
    }

    // ----------------------------------------------------------------------
    // ## Hierarchy Change Logic
    // ----------------------------------------------------------------------

    /// <summary>
    /// 에디터 하이어라키 변경 시 자동 실행되어 버튼 상태를 체크합니다.
    /// </summary>
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
        // 씬 내의 모든 버튼 탐색 (비활성화된 오브젝트 포함)
        Button[] allButtons = GameObject.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (var btn in allButtons)
        {
            // 실제 프로젝트 에셋이 아닌, '씬'에 배치된 인스턴스만 대상으로 함
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
        // 프리팹 편집창 루트부터 모든 자식 버튼 탐색
        Button[] buttonsInPrefab = stage.prefabContentsRoot.GetComponentsInChildren<Button>(true);

        foreach (var btn in buttonsInPrefab)
        {
            if (AddScriptIfMissing(btn)) count++;
        }
        return count;
    }

    // ----------------------------------------------------------------------
    // ## Core Logic
    // ----------------------------------------------------------------------

    /// <summary>
    /// 버튼에 ClickSound 스크립트가 없다면 Undo 시스템을 통해 안전하게 추가합니다.
    /// </summary>
    private static bool AddScriptIfMissing(Button btn)
    {
        // 이미 ClickSound가 있는 경우 중복 추가 방지
        if (btn.gameObject.GetComponent<ClickSound>() == null)
        {
            // 1. Undo 시스템에 등록: Ctrl+Z가 작동하며, 에디터 변경사항이 안전하게 기록됨
            var clickSound = Undo.AddComponent<ClickSound>(btn.gameObject);

            // 2. 사운드 스크립트의 초기 설정 실행
            clickSound.SetButton();

            // 3. 변경 사항이 있음을 에디터에 알림 (저장 별표 표시)
            if (!Application.isPlaying)
            {
                EditorSceneManager.MarkSceneDirty(btn.gameObject.scene);
            }

            Debug.Log($"<color=cyan>[AutoSound]</color> {btn.name} 오브젝트에 ClickSound를 추가했습니다.");
            return true;
        }
        return false;
    }
}
#endif