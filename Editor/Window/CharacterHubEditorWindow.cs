using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.U2D;

public class CharacterHubEditorWindow : EditorWindow
{
    private List<CharacterData> characterList = new List<CharacterData>();
    private GrowthLibrary growthLibrary; // 연결된 성장 데이터 라이브러리 SO
    private CharacterData selectedCharacter;
    private Vector2 listScrollPos;
    private Vector2 detailScrollPos;

    [MenuItem("Tools/Data/3. Character Hub")]
    public static void ShowWindow() => GetWindow<CharacterHubEditorWindow>("Character Hub").minSize = new Vector2(800, 600);

    private void OnEnable() => RefreshData();

    private void RefreshData()
    {
        // 1. CSV 데이터 로드
        string filePath = $"{Application.dataPath}/Util/GoogleSheet/CSVData/CharacterData.csv";
        if (!System.IO.File.Exists(filePath)) return;

        string csvText = System.IO.File.ReadAllText(filePath);
        var dataDict = ScriptDataLoader<CharacterData>.ReadFile((csvText, typeof(CharacterData)), new CharacterDataList());
        characterList = dataDict.Values.ToList();
    }

    private void OnGUI()
    {
        // 상단 툴바
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        if (GUILayout.Button("CSV 새로고침", EditorStyles.toolbarButton)) RefreshData();
        growthLibrary = (GrowthLibrary)EditorGUILayout.ObjectField("Growth Library (SO)", growthLibrary, typeof(GrowthLibrary), false);
        EditorGUILayout.EndHorizontal();

        // 메인 영역
        EditorGUILayout.BeginHorizontal();
        DrawLeftList();
        DrawRightDetail();
        EditorGUILayout.EndHorizontal();
    }

    private void DrawLeftList()
    {
        EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(250), GUILayout.ExpandHeight(true));
        listScrollPos = EditorGUILayout.BeginScrollView(listScrollPos);

        foreach (var charData in characterList)
        {
            if (GUILayout.Button(charData.characterName, GUILayout.Height(30))) selectedCharacter = charData;
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    private void DrawRightDetail()
    {
        EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

        try
        {
            if (selectedCharacter == null)
            {
                EditorGUILayout.HelpBox("캐릭터를 선택해주세요.", MessageType.Info);
                return;
            }

            detailScrollPos = EditorGUILayout.BeginScrollView(detailScrollPos);

            EditorGUILayout.LabelField($"캐릭터: {selectedCharacter.characterName}", EditorStyles.boldLabel);

            DrawMainSpritePreview(selectedCharacter.modelSpriteName, Util.CHARACTER_IMAGE_NAME);
            GUILayout.Space(10);

            EditorGUILayout.LabelField("Base Stats (CSV)", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("helpbox");
            {
                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField($"HP: {selectedCharacter.characterState.maxHp}", EditorStyles.miniBoldLabel);
                EditorGUILayout.LabelField($"ATK: {selectedCharacter.characterState.atkPower}", EditorStyles.miniBoldLabel);
                EditorGUILayout.LabelField($"DEF: {selectedCharacter.characterState.defPower}", EditorStyles.miniBoldLabel);
                EditorGUILayout.LabelField($"ATK Speed: {selectedCharacter.characterState.atkSpeed}", EditorStyles.miniBoldLabel);
                EditorGUILayout.LabelField($"ATK Rang: {selectedCharacter.characterState.atkRang}", EditorStyles.miniBoldLabel);
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndVertical();

            GUILayout.Space(20);

            EditorGUILayout.LabelField("Growth Data (SO)", EditorStyles.boldLabel);
            if (growthLibrary != null)
            {
                var linkedGrowth = growthLibrary.growthDataList.FirstOrDefault(x => x.growthID == selectedCharacter.GrowthDataID);
                if (linkedGrowth != null)
                {
                    DrawGrowthSection("Level Up Rules", linkedGrowth.levelRules);
                    DrawGrowthSection("Enforce Rules", linkedGrowth.enforceRules);
                    DrawGrowthSection("Rank Up Rules", linkedGrowth.rankRules);
                    DrawGrowthSection("Upgrade Rules", linkedGrowth.upgradeRules);
                }
                else
                {
                    EditorGUILayout.HelpBox($"GrowthID '{selectedCharacter.GrowthDataID}'를 찾을 수 없습니다.", MessageType.Error);
                }
            }

            EditorGUILayout.EndScrollView();
        }
        finally
        {
            EditorGUILayout.EndVertical();
        }
    }

    private void DrawMainSpritePreview(string atlasName, string spriteName)
    {
        string fullPath = string.Format(Util.CHARACTER_SPRITE_PATH, atlasName);
        SpriteAtlas atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(fullPath);

        if (atlas != null)
        {
            Sprite sprite = atlas.GetSprite(spriteName);
            if (sprite != null) GUILayout.Box(sprite.texture, GUILayout.Width(600), GUILayout.Height(600));
            else EditorGUILayout.HelpBox("Sprite 없음", MessageType.Warning);
        }
        else EditorGUILayout.HelpBox($"Atlas 파일 없음{fullPath}", MessageType.Error);
    }

    private void DrawGrowthSection(string label, List<StatGrowthRule> rules)
    {
        EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
        foreach (var rule in rules)
        {
            EditorGUILayout.BeginHorizontal("helpbox");
            EditorGUILayout.LabelField(rule.statType.ToString(), GUILayout.Width(100));
            EditorGUILayout.LabelField($"+{rule.value}", GUILayout.Width(100));
            EditorGUILayout.EndHorizontal();
        }
        GUILayout.Space(10);
    }
}