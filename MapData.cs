using Newtonsoft.Json;
using System;
using UnityEngine;
using UnityEngine.U2D;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 게임의 스테이지별 타일 데이터 및 경로 정보를 저장하는 ScriptableObject입니다.
/// JSON 직렬화를 통해 데이터 시트(StageList) 업데이트 기능도 포함하고 있습니다.
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObject/MapData")]
[Serializable]
public class MapData : ScriptableObject
{
    // ====== Inspector Settings (ScriptableObject 전용) ======

    [Header("MainStageData"), JsonIgnore] // JSON 저장 시 실제 에셋 참조는 무시합니다.
    [SerializeField] private SpriteAtlas m_atlas;

    [Tooltip("에셋 로드 시 참조할 아틀라스의 이름입니다.")]
    [SerializeField] private string m_atlasName;

    [Space, Header("Map Info")]
    public int m_mainStage = 1;  // 메인 챕터 번호
    public int m_subStage = 1;   // 세부 스테이지 번호
    public int m_width = 10;     // 맵의 가로 타일 개수
    public int m_height = 10;    // 맵의 세로 타일 개수

    // ====== Data Lists ======

    [Space, Tooltip("맵을 구성하는 개별 타일들의 위치와 타입 정보입니다.")]
    public TileData[] tileDatas;

    [Space, Tooltip("몬스터 이동 경로 등 맵 내의 모든 경로 데이터 목록입니다.")]
    public PathData[] pathDatas;

    // ====== Internal Structures ======

    /// <summary> 타일이나 오브젝트의 역할을 정의하는 열거형입니다. </summary>
    [Serializable]
    public enum MapObject
    {
        None = -1,
        Wall,               // 벽/장애물
        Spawn,              // 플레이어 캐릭터 생성 지점
        Path,               // 이동 가능 경로
        EnemySpawnPoint,    // 적 생성 지점
        PlayerEndPoint,     // 방어 목표 지점
        Delete = 999,       // 에디터에서 제거용
    }

    /// <summary> 개별 타일의 좌표 및 속성 데이터 클래스입니다. </summary>
    [Serializable]
    public class TileData
    {
        public int x, y;            // 맵 좌표
        public MapObject type;      // 타일 종류
        public string spriteName;   // 아틀라스 내 스프라이트 이름
    }

    /// <summary> 특정 인덱스를 가진 연속된 좌표 리스트(경로) 데이터 클래스입니다. </summary>
    [Serializable]
    public class PathData
    {
        public int index; // 경로 식별 번호 (예: 1번 라인, 2번 라인)
        public List<SerializeableVector2Int> path = new(); // 좌표 리스트
    }

    /// <summary> 
    /// Unity의 Vector2Int는 JSON 직렬화 시 문제가 발생할 수 있어 정의한 직렬화용 구조체입니다. 
    /// </summary>
    [Serializable]
    public struct SerializeableVector2Int
    {
        public int x, y;

        /// <summary> 실제 연산에 필요한 Unity Vector2Int 타입으로 변환합니다. </summary>
        public Vector2Int GetVector2Int()
        {
            return new Vector2Int(x, y);
        }
    }

    // ====== Data Setting Methods ======

    /// <summary>
    /// 에디터에서 아틀라스 참조 및 이름을 일괄 설정합니다.
    /// </summary>
    public void SetImageSetting(SpriteAtlas atlas)
    {
        m_atlas = atlas;
        m_atlasName = atlas.name;
    }

    // ====== Editor Methods (JSON Serialization) ======

#if UNITY_EDITOR
    /// <summary> 맵 데이터 에셋들이 저장된 폴더 경로입니다. </summary>
    private readonly string MAPDATA_PATH = $"Assets/ScriptableObjectData/MapData";

    /// <summary>
    /// 현재 프로젝트 내의 모든 MapData를 검색하여 유효한 스테이지 리스트를 JSON 파일로 추출합니다.
    /// </summary>
    public void SaveToJson()
    {
        // JSON 직렬화 옵션 설정 (들여쓰기 적용, Null 무시)
        var settings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore
        };

        // 전체 스테이지 정보를 담을 객체 생성 (StageData는 별도 정의 필요)
        StageData stageData = new();

        // 1. 지정된 경로에서 모든 ScriptableObject 형식의 에셋 GUID를 찾습니다.
        var sAssetGuid = AssetDatabase.FindAssets("MapData- t:MapData", new[] { MAPDATA_PATH });
        // 2. GUID를 실제 에셋 경로(String)로 변환합니다.
        var sAssetPathList = Array.ConvertAll<string, string>(sAssetGuid, AssetDatabase.GUIDToAssetPath);

        // 3. 각 경로의 에셋을 로드하여 스테이지 정보에 추가합니다.
        foreach (var sAssetPath in sAssetPathList)
        {
            MapData mapData = AssetDatabase.LoadAssetAtPath(sAssetPath, typeof(MapData)) as MapData;

            if (mapData != null)
            {
                // 로드된 데이터의 메인 스테이지 번호를 스테이지 리스트에 등록
                stageData.AddStageData(mapData.m_mainStage);
            }
        }

        // 4. 추출된 데이터를 JSON 문자열로 변환하여 파일로 저장합니다.
        string jsonOutput = JsonConvert.SerializeObject(stageData, settings);
        string savePath = $"{Application.dataPath}/TextAsset/StageList.json";

        System.IO.File.WriteAllText(savePath, jsonOutput);

        Logger.Log($"[MapData] StageList JSON saved. Content: {jsonOutput}, Path: {savePath}");
    }
#endif
}