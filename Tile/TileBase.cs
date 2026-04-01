using UnityEngine;
using static MapData;

/// <summary>
/// 타일 클릭 시 발생하는 상호작용(Interaction) 규격을 정의한 인터페이스입니다.
/// 콜백(Callback) 함수를 무분별하게 넘기는 대신, 이 인터페이스를 UI 매니저에 주입하여
/// 타일과 UI 시스템 간의 결합도를 낮추고 유지보수성을 확보합니다.
/// </summary>
public interface TileClickEvent
{
    /// <summary> 타일(또는 유닛)이 유저에 의해 선택되었을 때 호출됩니다. </summary>
    public void OnSelect();

    /// <summary> 타일(또는 유닛) 선택이 해제되었을 때 호출됩니다. </summary>
    public void OnDeselect();

    /// <summary> 타일 위의 유닛을 업그레이드할 때 호출됩니다. 사용 코스트 반환 </summary>
    public void OnUpgrade();

    /// <summary> 타일 위의 유닛을 스킬을 호출합니다. </summary>
    public void OnSkill();

    public int GetUpgradeCost();

    public float GetSkillLastTime();

    public float GetSkillTime();
}

/// <summary>
/// 인게임의 모든 타일 객체의 최상위 베이스 클래스입니다.
/// 타일의 위치 설정, 스프라이트 변경, 배치 가능 구역 판정을 담당하며,
/// TileClickEvent를 구현하여 하위 타일들이 클릭 이벤트를 재정의(Override)할 수 있도록 지원합니다.
/// </summary>
public class TileBase : CachObject, TileClickEvent
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
    // ## Public Methods (Core Logic)
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

    // ----------------------------------------------------------------------
    // ## Interface Implementation (TileClickEvent)
    // ----------------------------------------------------------------------

    /// <summary> [TileClickEvent] 타일 선택 시의 기본 동작입니다. 하위 클래스에서 재정의(Override)하여 사용합니다. </summary>
    public virtual void OnSelect()
    {
    }

    /// <summary> [TileClickEvent] 타일 선택 해제 시의 기본 동작입니다. 하위 클래스에서 재정의(Override)하여 사용합니다. </summary>
    public virtual void OnDeselect()
    {
    }

    /// <summary> [TileClickEvent] 타일 업그레이드 시의 코스트 값을 반환합니다. 하위 클래스에서 재정의(Override)하여 사용합니다. </summary>
    public virtual int GetUpgradeCost()
    {
        return 0;
    }

    /// <summary> [TileClickEvent] 타일 업그레이드 시의 기본 동작입니다. 하위 클래스에서 재정의(Override)하여 사용합니다. </summary>
    public virtual void OnUpgrade()
    {
    }

    /// <summary> [TileClickEvent] 스킬 관련 기본 동작입니다. 하위 클래스에서 재정의(Override)하여 사용합니다. </summary>
    public virtual void OnSkill()
    {
    }

    /// <summary> [TileClickEvent] 스킬 쿨타임 관련 동작입니다. 하위 클래스에서 재정의(Override)하여 사용합니다. </summary>
    public virtual float GetSkillLastTime()
    { return 0; }

    public virtual float GetSkillTime() { return 0; }
}