using UnityEngine;
using static MapData;

/// <summary>
/// 인게임의 모든 타일 객체의 최상위 베이스 클래스입니다.
/// 타일의 위치 설정, 스프라이트 변경 및 배치 가능 구역 판정을 담당합니다.
/// </summary>
public class TileBase : CachObject
{
    // ====== Protected Fields ======

    /// <summary> 타일의 이미지를 렌더링하는 컴포넌트입니다. </summary>
    protected SpriteRenderer tileImage;

    // ====== Properties ======

    /// <summary> 
    /// 이 타일이 가지고 있는 고유 데이터(좌표, 타입, 스프라이트 이름 등)입니다. 
    /// 외부에서는 읽기만 가능하며 초기화 시 설정됩니다.
    /// </summary>
    public TileData m_tileData
    {
        private set;
        get;
    }

    // ----------------------------------------------------------------------
    // ## Lifecycle
    // ----------------------------------------------------------------------

    private void Awake()
    {
        // 렌더러 컴포넌트를 캐싱합니다.
        tileImage = GetComponent<SpriteRenderer>();
    }

    // ----------------------------------------------------------------------
    // ## Public Methods
    // ----------------------------------------------------------------------

    /// <summary>
    /// 전달받은 타일 데이터를 기반으로 타일을 초기화하고 월드 위치를 설정합니다.
    /// </summary>
    /// <param name="tileData">MapData로부터 로드된 타일 상세 정보</param>
    public virtual void Init(TileData tileData)
    {
        m_tileData = tileData;

        // 타일 데이터의 x, y 좌표를 기반으로 로컬 위치를 결정합니다. (Z는 보통 0)
        transform.localPosition = new Vector3(tileData.x, tileData.y, 0);
    }

    /// <summary>
    /// 타일의 스프라이트 이미지를 동적으로 변경합니다.
    /// </summary>
    /// <param name="sprite">적용할 스프라이트 에셋</param>
    public virtual void SetTileSprite(Sprite sprite)
    {
        if (tileImage != null)
        {
            tileImage.sprite = sprite;
        }
    }

    /// <summary>
    /// 해당 타일에 유닛을 배치(Spawn)할 수 있는지 여부를 체크합니다.
    /// </summary>
    /// <param name="spawnPathCharacter">
    /// True: 경로(Path) 위에 배치하는 유닛인지 여부 (예: 트랩 등)
    /// False: 일반 배치 구역(Spawn)에 배치하는 유닛인지 여부 (예: 타워 등)
    /// </param>
    /// <returns>배치 가능하면 True, 불가능하면 False를 반환합니다.</returns>
    public virtual bool CheckSpawnPoint(bool spawnPathCharacter = false)
    {
        // 1. 일반 지상/타워 유닛 배치 체크: 타일 타입이 Spawn 구역이고 경로 유닛이 아닐 때
        // 2. 경로 전용 유닛(트랩 등) 배치 체크: 타일 타입이 Path이고 경로 유닛일 때
        if ((m_tileData.type == MapObject.Spawn && spawnPathCharacter == false) ||
            (m_tileData.type == MapObject.Path && spawnPathCharacter))
        {
            return true;
        }

        return false;
    }
}