using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.U2D;
using static MapData;

/// <summary>
/// 맵 에디터 관리자 (MapEditorManager)
/// Unity 에디터 환경에서 타일 기반 맵의 생성, 로드, 저장 및 시각적 편집을 관리하는 MonoBehaviour입니다.
/// 맵 데이터는 ScriptableObject (MapData)와 JSON 파일로 관리됩니다.
/// </summary>
public class MapEditorManager : MonoBehaviour
{
#if UNITY_EDITOR
    // ====== Constants ======

    /// <summary> MapData ScriptableObject 파일 저장 경로 포맷입니다. </summary>
    private readonly string AssetPathFormat = "Assets/ScriptableObjectData/MapData/{0}.asset";

    // ====== Inspector Settings & References ======

    [Header("Data Settings")]
    [Tooltip("현재 편집 중인 MapData ScriptableObject 인스턴스입니다.")]
    [SerializeField] public MapData m_currentMapData;

    [Tooltip("맵 파일 이름에 사용될 메인 스테이지 번호입니다.")]
    [SerializeField] private int m_mainStage;

    [Tooltip("맵 파일 이름에 사용될 서브 스테이지 번호입니다.")]
    [SerializeField] private int m_subStage;

    [Header("Map Dimensions")]
    [Tooltip("맵의 가로 크기입니다.")]
    [SerializeField] private int m_width;

    [Tooltip("맵의 세로 크기입니다.")]
    [SerializeField] private int m_height;

    [Header("Editor References")]
    [Tooltip("맵 프리뷰를 위해 사용되는 카메라입니다.")]
    [SerializeField] private Camera cam;

    [Tooltip("맵 타일 편집을 위해 인스턴스화되는 기본 프리팹입니다.")]
    [SerializeField] private TileEdtiorBase m_baseEditorTile;

    [Tooltip("경로 포인트 시각화를 위한 오브젝트 프리팹입니다.")]
    [SerializeField] private PathDataObejctMono m_basePathDataObject;

    [Tooltip("타일 스프라이트를 담고 있는 SpriteAtlas입니다.")]
    [Header("Image Set")]
    public SpriteAtlas m_atlas;

    // ====== Internal State & Caches ======

    /// <summary> UI 조작 값을 참조하기 위한 에디터 UI 객체입니다. </summary>
    private MapEditorUI m_ui;

    /// <summary> 인스턴스화된 타일 객체들을 관리하는 리스트입니다. </summary>
    private List<GameObject> m_tileObjects = new();

    /// <summary> 화면에 배치된 타일 오브젝트(TileEdtiorBase)를 좌표(Vector2Int)별로 저장하는 딕셔너리입니다. </summary>
    private Dictionary<Vector2Int, TileEdtiorBase> m_tileBase = new();

    /// <summary> 편집 중인 실제 타일 데이터(TileData)를 좌표(Vector2Int)별로 저장하는 딕셔너리입니다. </summary>
    private Dictionary<Vector2Int, TileData> m_tileData;

    /// <summary> 인덱스 번호별 경로(Path) 데이터를 저장하는 딕셔너리입니다. </summary>
    private Dictionary<int, PathData> m_pathList = new();

    /// <summary> 화면에 생성된 경로 시각화 오브젝트들을 관리하는 리스트입니다. </summary>
    private List<PathDataObejctMono> m_pathDataObjectList = new();

    /// <summary> 경로 간의 연결선을 그리기 위한 컴포넌트입니다. </summary>
    [SerializeField] LineRenderer lineRender;

    // ----------------------------------------------------------------------
    // ## UI Integration
    // ----------------------------------------------------------------------

    /// <summary>
    /// 컴포넌트 실행 시 LineRenderer의 기본 속성(색상, 두께)을 설정합니다.
    /// </summary>
    private void Awake()
    {
        if (lineRender == null)
            lineRender = gameObject.GetComponent<LineRenderer>();
        lineRender.startColor = lineRender.endColor = Color.blue;
        lineRender.widthMultiplier = 0.2f;
    }

    /// <summary>
    /// 맵 에디터 UI 인스턴스를 설정합니다.
    /// </summary>
    public void SetUI(MapEditorUI ui)
    { m_ui = ui; }

    // ----------------------------------------------------------------------
    // ## Data Loading & Creation
    // ----------------------------------------------------------------------

    /// <summary>
    /// 편집 중인 데이터가 없을 경우 새 데이터를 생성합니다.
    /// </summary>
    public void UpdateMapData()
    {
        if (m_currentMapData == null)
        {
            CreateMapData();
        }
    }

    /// <summary>
    /// 설정된 스테이지 번호에 맞는 에셋 파일을 로드하거나 새로 생성한 뒤, 화면에 맵을 배치합니다.
    /// </summary>
    public void LoadMapData()
    {
        var filename = $"MapData-{m_mainStage}-{m_subStage}";
        var path = string.Format(AssetPathFormat, filename);

        // 프로젝트 폴더 내 에셋 로드 시도
        var load = AssetDatabase.LoadAssetAtPath(path, typeof(MapData));

        if (load == null)
        {
            Debug.Log($"[MapEditor] MapData not found at {path}. Creating new data.");
            CreateMapData();
        }
        else
        {
            m_currentMapData = load as MapData;
            Debug.Log($"[MapEditor] Successfully loaded MapData: {filename}");
        }

        // 로드된 데이터 기반으로 시각적 맵 생성
        CreateMap();
    }

    /// <summary>
    /// 새로운 MapData ScriptableObject 에셋 파일을 생성하고 경로 데이터를 직렬화합니다.
    /// </summary>
    public void CreateMapData()
    {
        // 타일 데이터 추출
        List<TileData> ti = new();
        if (m_tileData != null)
        {
            foreach (var item in m_tileData.Values)
                ti.Add(item);
        }

        // ScriptableObject 인스턴스화 및 필드 설정
        MapData data = ScriptableObject.CreateInstance<MapData>();
        data.m_width = m_width;
        data.m_height = m_height;
        data.m_mainStage = m_mainStage;
        data.m_subStage = m_subStage;
        data.tileDatas = ti.ToArray();
        data.SetImageSetting(m_atlas);

        // 경로 데이터 추출 및 저장
        List<PathData> pathDatas = new();
        for (int i = 0; i < m_pathList.Keys.Count; i++)
        {
            pathDatas.Add(m_pathList[i]);
        }
        data.pathDatas = pathDatas.ToArray();
        m_currentMapData = data;

        // 에셋 파일 실체화
        var filename = $"MapData-{m_mainStage}-{m_subStage}";
        var path = string.Format(AssetPathFormat, filename);
        AssetDatabase.CreateAsset(m_currentMapData, path);

        Debug.Log($"[MapEditor] New MapData asset created at: {path}");
    }

    // ----------------------------------------------------------------------
    // ## Data Saving
    // ----------------------------------------------------------------------

    /// <summary>
    /// 현재 편집 내용을 SO 에셋에 업데이트하고, JSON 파일로 변환하여 저장합니다.
    /// </summary>
    public void SaveMapData()
    {
        // 타일 데이터 업데이트
        List<TileData> ti = new();
        if (m_tileData != null)
        {
            foreach (var item in m_tileData.Values)
                ti.Add(item);
        }

        m_currentMapData.tileDatas = ti.ToArray();
        m_currentMapData.SetImageSetting(m_atlas);

        // 경로 데이터 업데이트
        List<PathData> pathDatas = new();
        for (int i = 0; i < m_pathList.Keys.Count; i++)
        {
            pathDatas.Add(m_pathList[i]);
        }
        m_currentMapData.pathDatas = pathDatas.ToArray();

        // 데이터 직렬화 및 에셋 리프레시
        m_currentMapData.SaveToJson();
        EditorUtility.SetDirty(m_currentMapData);
        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();

        Debug.Log($"[MapEditor] MapData saved and assets refreshed.");
    }

    // ----------------------------------------------------------------------
    // ## Map Visualization & Editor Control
    // ----------------------------------------------------------------------

    /// <summary>
    /// 씬에 생성된 모든 타일 및 경로 오브젝트를 즉시 삭제합니다.
    /// </summary>
    [ContextMenu("Delete")]
    public void DeleteAll()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }

        // 경로 모드 시각화 해제
        PathModeOff();

        // 메모리 데이터 초기화
        if (m_tileData != null)
            m_tileData.Clear();
        if (m_tileBase != null)
            m_tileBase.Clear();

        m_tileObjects.Clear();
    }

    /// <summary>
    /// 맵 크기에 맞게 타일을 초기화하고 카메라 위치를 설정합니다.
    /// </summary>
    [ContextMenu("InitMap")]
    public void InitMap()
    {
        InitMapSync();

        // 카메라를 전체 맵의 중앙에 위치시킴
        cam.gameObject.transform.position = new Vector3((m_width * 0.5f) - 0.5f, (m_height * 0.5f) - 0.5f, -10);

        Debug.Log($"[MapEditor] Map initialized: {m_width}x{m_height}");
    }

    /// <summary>
    /// 맵 초기화 및 가로x세로 크기만큼 타일 오브젝트를 인스턴스화합니다.
    /// </summary>
    public void InitMapSync()
    {
        DeleteAll();

        m_tileData = new Dictionary<Vector2Int, TileData>();

        for (int x = 0; x < m_width; x++)
        {
            for (int y = 0; y < m_height; y++)
            {
                var obj = Instantiate(m_baseEditorTile, transform);
                Setting(obj.gameObject, x, y);
            }
        }

        PathModeOff();

        // 내부 로컬 함수: 타일 초기 속성 및 델리게이트 연결
        void Setting(GameObject obj, int x, int y)
        {
            var tileEditor = obj.GetComponent<TileEdtiorBase>();
            Vector2Int postition = new Vector2Int(x, y);

            tileEditor.currentPos = postition;
            tileEditor.onclickEnter = GetTileData; // 타일 클릭 시 실행될 메서드 연결
            m_tileBase.Add(postition, tileEditor);

            obj.transform.localPosition = new Vector3(x, y, 0);
            obj.SetActive(true);

            TileData initialData = new TileData() { x = x, y = y };
            m_tileData.Add(postition, initialData);
        }
    }

    /// <summary>
    /// 로드된 SO 데이터를 화면의 시각적 타일과 경로 데이터로 복원합니다.
    /// </summary>
    public void CreateMap()
    {
        InitMap();

        // 타일 비주얼 복구
        foreach (var item in m_currentMapData.tileDatas)
        {
            Setting(item);
        }

        // 경로 데이터 캐시 복구
        int currentIndex = 0;
        foreach (var path in m_currentMapData.pathDatas)
        {
            if (m_pathList.ContainsKey(currentIndex) == false)
                m_pathList.Add(currentIndex, path);
            else
                m_pathList[currentIndex] = path;

            currentIndex++;
        }

        // 내부 로컬 함수: 개별 타일 스프라이트 및 데이터 설정
        void Setting(TileData tileData)
        {
            Vector2Int key = new Vector2Int(tileData.x, tileData.y);

            if (!m_tileBase.ContainsKey(key))
            {
                Debug.LogWarning($"[MapEditor] Loaded TileData ({key}) is outside the current map bounds. Skipping.");
                return;
            }

            var sp = m_tileBase[key].gameObject.GetComponent<SpriteRenderer>();
            Sprite sprite = m_atlas.GetSprite(tileData.spriteName);

            if (sprite != null)
                sp.sprite = sprite;

            m_tileBase[key].InitTileEdtiorBase(tileData);

            if (m_tileData.ContainsKey(key))
                m_tileData[key] = tileData;
            else
                m_tileData.Add(key, tileData);
        }

        Debug.Log($"[MapEditor] Map preview created from loaded data. Total tiles: {m_tileData.Count}");
    }

    // ----------------------------------------------------------------------
    // ## Editor Interaction
    // ----------------------------------------------------------------------

    /// <summary>
    /// 타일 클릭 시 현재 에디터 모드(타일/경로)에 따라 데이터를 갱신합니다.
    /// </summary>
    /// <param name="key">클릭된 타일 좌표</param>
    public void GetTileData(Vector2Int key)
    {
        // 경로 편집 모드 처리
        if (m_ui.pathMode)
        {
            // 경로 삭제 로직
            if (m_ui.pathRemoveMode && m_pathList[m_ui.pathIndex].path.Any(x => x.GetVector2Int() == key))
            {
                m_pathList[m_ui.pathIndex].path.RemoveAll(x => x.GetVector2Int() == key);
                if (m_pathDataObjectList.Any(x => x.PathPos == key))
                {
                    var data = m_pathDataObjectList.Find(x => x.PathPos == key);
                    data.gameObject.SetActive(false);
                }
                // 삭제 후 시각적 인덱스 번호 재정렬
                for (int i = 0; i < m_pathDataObjectList.Count; i++)
                {
                    if (m_pathDataObjectList.Count <= i) break;
                    m_pathDataObjectList[i].SetIndex(i);
                }
                return;
            }
            // 경로 추가 로직
            else if (m_ui.pathRemoveMode == false)
            {
                // 오브젝트 풀링 활용
                if (m_pathDataObjectList.Count > m_pathList[m_ui.pathIndex].path.Count)
                {
                    m_pathDataObjectList[m_pathList[m_ui.pathIndex].path.Count].gameObject.SetActive(true);
                    m_pathDataObjectList[m_pathList[m_ui.pathIndex].path.Count].SetPathData(m_pathList[m_ui.pathIndex].path.Count, key);
                }
                else
                {
                    var pathObject = Instantiate(m_basePathDataObject, position: new(key.x, key.y, 0), Quaternion.identity);
                    pathObject.SetPathData(m_pathDataObjectList.Count, key);
                    m_pathDataObjectList.Add(pathObject);
                }

                m_pathList[m_ui.pathIndex].path.Add(new() { x = key.x, y = key.y });
            }
            return;
        }

        // 일반 타일 편집 처리
        if (m_tileData.ContainsKey(key) == false)
        {
            var tile = new TileData() { x = key.x, y = key.y };
            m_tileData.Add(key, tile);
        }

        m_tileData[key].spriteName = m_ui.GetCurrentSpriteName();
        m_tileData[key].type = m_ui.GetCurrentType();
    }

    /// <summary>
    /// 특정 인덱스의 경로 전체를 삭제합니다.
    /// </summary>
    public void RemovePathData(int pathData)
    {
        if (m_pathList.ContainsKey(pathData) == false) return;

        m_pathList.Remove(pathData);
        PathModeOn(System.Math.Max(pathData - 1, 0));
    }

    /// <summary>
    /// 모든 경로 시각화 요소를 비활성화합니다.
    /// </summary>
    public void PathModeOff()
    {
        lineRender.positionCount = 0;
        foreach (var pathData in m_pathDataObjectList)
        {
            pathData.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 특정 인덱스의 경로를 활성화하고 연결선(LineRenderer)과 포인트를 화면에 그립니다.
    /// </summary>
    public void PathModeOn(int pathIndex)
    {
        PathModeOff();
        if (m_pathList.ContainsKey(pathIndex) == false)
        {
            m_pathList.Add(pathIndex, new() { index = pathIndex });
        }

        List<Vector3> pos = new();
        lineRender.positionCount = m_pathList[pathIndex].path.Count;

        for (int i = 0; i < m_pathList[pathIndex].path.Count; i++)
        {
            Vector3 position = new() { x = m_pathList[pathIndex].path[i].x, y = m_pathList[pathIndex].path[i].y, z = 0 };

            // 시각적 포인트 오브젝트 설정
            if (m_pathDataObjectList.Count > i)
            {
                m_pathDataObjectList[i].SetPathData(i, new() { x = m_pathList[pathIndex].path[i].x, y = m_pathList[pathIndex].path[i].y });
            }
            else
            {
                var obj = Instantiate(m_basePathDataObject);
                obj.SetPathData(i, new() { x = m_pathList[pathIndex].path[i].x, y = m_pathList[pathIndex].path[i].y });
                m_pathDataObjectList.Add(obj);
            }

            pos.Add(position);
        }

        // LineRenderer 포지션 일괄 설정
        lineRender.SetPositions(pos.ToArray());
    }

#endif
}