using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SettingSpawnEnemyWindows
{
    public class EnemySpawnSettingWindow : EditorWindow
    {
        private MapData m_mapData;
        private SerializedObject m_serializedMapData;

        // UI 요소들
        private ObjectField m_mapDataField;
        private ListView m_spawnListView;
        private ScrollView m_rightPane;

        [MenuItem("Tools/Window/Enemy SpawnData Setting (Window)")]
        public static void ShowWindow(MapData mapData = null)
        {
            var window = GetWindow<EnemySpawnSettingWindow>("Enemy Spawn Data");
            window.minSize = new Vector2(600, 400); // 분할 창을 위해 최소 사이즈 지정
            if (mapData != null)
                window.m_mapDataField.value = (mapData);
        }

        public void CreateGUI()
        {
            // 1. 최상단 루트 요소 설정 (가로 방향 분할을 위해 Flex-direction 설정)
            VisualElement root = rootVisualElement;
            root.style.flexDirection = FlexDirection.Column;
            root.style.paddingTop = 10;
            root.style.paddingBottom = 10;
            root.style.paddingLeft = 10;
            root.style.paddingRight = 10;

            // 2. MapData 할당용 상단 필드
            m_mapDataField = new ObjectField("Target Map Data")
            {
                objectType = typeof(MapData),
                allowSceneObjects = false
            };

            Button saveButton = new Button(() =>
            {
                SortByTime();
                AssetDatabase.SaveAssets();
                Debug.Log("MapData Saved Successfully!");
            }) { text = "Force Save All" };

            m_mapDataField.RegisterValueChangedCallback(evt => OnMapDataSelected(evt.newValue as MapData));
            root.Add(m_mapDataField);
            root.Add(saveButton);

            // 3. 메인 콘텐츠 영역 (좌/우 분할)
            VisualElement splitContainer = new VisualElement();
            splitContainer.style.flexDirection = FlexDirection.Row;
            splitContainer.style.flexGrow = 1;
            splitContainer.style.marginTop = 10;
            root.Add(splitContainer);

            // --- 좌측 패널 (ListView 영역) ---
            VisualElement leftPane = new VisualElement();
            leftPane.style.width = Length.Percent(40);
            leftPane.style.borderRightWidth = 1;
            leftPane.style.borderRightColor = Color.gray;
            leftPane.style.paddingRight = 10;

            Label listTitle = new Label("Spawn Timeline (Waves)");
            listTitle.style.unityFontStyleAndWeight = FontStyle.Bold;
            listTitle.style.marginBottom = 5;
            leftPane.Add(listTitle);

            // UI Toolkit의 핵심: ListView를 사용하면 드래그 앤 드롭 정렬, 추가/삭제가 자동 지원됩니다.
            m_spawnListView = new ListView
            {
                showAlternatingRowBackgrounds = AlternatingRowBackground.All,
                showBorder = true,
                showAddRemoveFooter = true, // 하단에 +/- 버튼 자동 생성
                reorderable = true, // 드래그로 순서 변경 가능
                virtualizationMethod = CollectionVirtualizationMethod.FixedHeight,
                fixedItemHeight = 25
            };

            // 리스트에서 항목을 선택했을 때 우측 패널에 띄우는 이벤트
            m_spawnListView.selectionChanged += OnSpawnItemSelected;
            leftPane.Add(m_spawnListView);
            splitContainer.Add(leftPane);

            // --- 우측 패널 (디테일/인스펙터 영역) ---
            m_rightPane = new ScrollView();
            m_rightPane.style.width = Length.Percent(60);
            m_rightPane.style.paddingLeft = 10;
            splitContainer.Add(m_rightPane);

            // 초기 로드 시 선택된 데이터가 있다면 바인딩
            if (m_mapData != null)
            {
                m_mapDataField.value = m_mapData;
            }
            LoadEnemyLibrary();
        }

        private void OnMapDataSelected(MapData newData)
        {
            // 1. 이전 데이터와의 연결을 완전히 끊음 (에러 방지 핵심)
            m_spawnListView.SetSelection(-1);
            m_spawnListView.Unbind();

            m_rightPane.Clear();
            m_mapData = newData;

            if (m_mapData == null) return;

            m_serializedMapData = new SerializedObject(m_mapData);
            m_serializedMapData.Update();

            SerializedProperty spawnDatasProp = m_serializedMapData.FindProperty("enemySpawnDatas");

            // 3. ListView에 소스 연결 및 바인딩
            // 유니티 6에서는 itemsSource를 명시적으로 지정해주는 것이 안전합니다.
            m_spawnListView.BindProperty(spawnDatasProp);

            m_spawnListView.makeItem = () => new Label();
            m_spawnListView.bindItem = (element, i) =>
            {
                // [방어 코드] 인덱스가 현재 프로퍼티 범위 내에 있는지 확인
                if (spawnDatasProp == null || i >= spawnDatasProp.arraySize) return;

                var label = element as Label;
                var prop = spawnDatasProp.GetArrayElementAtIndex(i);

                // 프로퍼티가 유효한지 다시 한번 체크
                if (prop == null) return;

                var time = prop.FindPropertyRelative("spawnTime").floatValue;
                var id = prop.FindPropertyRelative("enemyDataID").intValue;
                var level = prop.FindPropertyRelative("enemyLevel").intValue;
                var path = prop.FindPropertyRelative("pathIndex").intValue;

                var enemyInfo = m_enemyBaseLibrary.Find(x => x.id == id);
                string enemyName = enemyInfo != null ? enemyInfo.enemyName : "Unknown";

                label.text = $"[P:{path}] {time:F1}s | Lv.{level} {enemyName}";
            };

            // 4. 강제 리프레시
            m_spawnListView.Rebuild();
        }

        private void OnSpawnItemSelected(System.Collections.Generic.IEnumerable<object> selection)
        {
            m_rightPane.Clear();

            if (m_mapData == null || m_serializedMapData == null) return;

            int selectedIndex = m_spawnListView.selectedIndex;

            m_serializedMapData.Update();
            SerializedProperty spawnDatasProp = m_serializedMapData.FindProperty("enemySpawnDatas");
            if (selectedIndex < 0 || spawnDatasProp == null || selectedIndex >= spawnDatasProp.arraySize)
            {
                return;
            }

            SerializedProperty selectedProp = spawnDatasProp.GetArrayElementAtIndex(m_spawnListView.selectedIndex);
            if (selectedProp == null) return;

            // --- 1. Path 선택 드롭다운 (pathIndex 저장) ---
            SerializedProperty pathIndexProp = selectedProp.FindPropertyRelative("pathIndex");

            // MapData 내의 pathDatas 리스트를 기반으로 선택지 생성
            List<string> pathChoices = new List<string>();
            int pathCount = m_mapData.pathDatas != null ? m_mapData.pathDatas.Length : 0;

            if (pathCount > 0)
            {
                for (int i = 0; i < pathCount; i++)
                {
                    // 경로에 별도의 이름 필드가 있다면 그걸 사용하고, 없으면 Index를 표시합니다.
                    pathChoices.Add($"Path {i}");
                }
            }
            else
            {
                pathChoices.Add("No Paths Defined");
            }

            // 현재 저장된 index가 범위를 벗어나지 않도록 보정
            int currentPathIdx = Mathf.Clamp(pathIndexProp.intValue, 0, Mathf.Max(0, pathCount - 1));

            PopupField<string> pathPopup = new PopupField<string>("Spawn Path", pathChoices, currentPathIdx);
            pathPopup.RegisterValueChangedCallback(evt => {
                pathIndexProp.intValue = pathPopup.index;
                m_serializedMapData.ApplyModifiedProperties();
                m_spawnListView.RefreshItem(m_spawnListView.selectedIndex);
                EditorUtility.SetDirty(m_mapData);
            });
            m_rightPane.Add(pathPopup);


            // --- 2. 소환 타이밍 (spawnTime) ---
            PropertyField spawnTimeField = new PropertyField(selectedProp.FindPropertyRelative("spawnTime"), "Spawn Time (Sec)");
            spawnTimeField.Bind(m_serializedMapData);
            m_rightPane.Add(spawnTimeField);


            // --- 3. 몬스터 종류 선택 (enemyDataID) ---
            SerializedProperty idProp = selectedProp.FindPropertyRelative("enemyDataID");
            int enemyIdx = m_enemyBaseLibrary.FindIndex(x => x.id == idProp.intValue);
            if (enemyIdx < 0) enemyIdx = 0;

            PopupField<string> enemyPopup = new PopupField<string>("Enemy Type", m_enemyNames, enemyIdx);
            enemyPopup.RegisterValueChangedCallback(evt => {
                idProp.intValue = m_enemyBaseLibrary[enemyPopup.index].id;
                m_serializedMapData.ApplyModifiedProperties();
                m_spawnListView.RefreshItem(m_spawnListView.selectedIndex);
                EditorUtility.SetDirty(m_mapData);
            });
            m_rightPane.Add(enemyPopup);


            // --- 4. 몬스터 레벨 (enemyLevel) ---
            PropertyField levelField = new PropertyField(selectedProp.FindPropertyRelative("enemyLevel"), "Enemy Level");
            levelField.Bind(m_serializedMapData);
            levelField.RegisterValueChangeCallback(evt => {
                m_spawnListView.RefreshItem(m_spawnListView.selectedIndex);
            });
            m_rightPane.Add(levelField);
        }

        private void SortByTime()
        {
            if (m_mapData == null) return;

            // 데이터 정렬 로직 (Undo 기록 포함)
            Undo.RecordObject(m_mapData, "Sort Spawn Data");

            var listSpawnData = System.Linq.Enumerable.ToList(m_mapData.enemySpawnDatas);
            m_mapData.enemySpawnDatas = System.Linq.Enumerable.ToArray(System.Linq.Enumerable.OrderBy(listSpawnData, x => x.spawnTime)); // spawnTime 필드 기준 정렬

            EditorUtility.SetDirty(m_mapData);
            m_serializedMapData.Update(); // 뷰 갱신
        }

        private List<EnemyData> m_enemyBaseLibrary = new();
        private List<string> m_enemyNames = new();

        private void LoadEnemyLibrary()
        {
            // 실제 경로는 프로젝트 구조에 맞게 수정하세요.
            TextAsset csvAsset = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Util/GoogleSheet/CSVData/EnemyData.csv");
            if (csvAsset == null) return;

            var lines = csvAsset.text.Split('\n');
            m_enemyBaseLibrary.Clear();
            m_enemyNames.Clear();

            for (int i = 0; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i])) continue;
                var cols = lines[i].Split(',');
                var characterState = new CharacterState()
                {
                    maxHp = float.Parse(cols[3]),
                    atkPower = int.Parse(cols[4]),
                    defPower = int.Parse(cols[5]),
                    atkSpeed = int.Parse(cols[6])
                };
                // CSV 데이터 객체화
                var data = new EnemyData
                {
                    id = int.Parse(cols[0]),
                    controllObjectKey = cols[1],
                    enemyLevel = int.Parse(cols[2]),
                    characterState = characterState,
                    enemyName = cols[7]
                };
                m_enemyBaseLibrary.Add(data);
                m_enemyNames.Add($"{data.id} : {data.enemyName}"); // 드롭다운에 표시할 이름
            }
        }

        //private void OnSpawnItemSelected(System.Collections.Generic.IEnumerable<object> selection)
        //{
            

        //    // 나머지 일반 필드들 (수정하고 싶은 것만 추가로 노출 가능)
        //    // PropertyField field = new PropertyField(enemyDataProp);
        //    // field.SetEnabled(false); // 드롭다운으로만 수정하게 하려면 비활성화
        //    // m_rightPane.Add(field);
        //}
    }
}