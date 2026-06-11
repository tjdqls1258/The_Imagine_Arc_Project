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

    public int GetUpgradeCost();

    public float GetSkillLastTime();

    public float GetSkillCoolTime();
}

/// <summary>
/// 인게임의 모든 타일 객체의 최상위 베이스 클래스입니다.
/// 타일의 위치 설정, 스프라이트 변경, 배치 가능 구역 판정을 담당하며,
/// TileClickEvent를 구현하여 하위 타일들이 클릭 이벤트를 재정의(Override)할 수 있도록 지원합니다.
/// </summary>
public class TileBase : CachObject, TileClickEvent
{
    protected SpriteRenderer tileImage;

    public TileData tileData
    {
        private set;
        get;
    }

    private void Awake()
    {
        tileImage = GetComponent<SpriteRenderer>();
    }
    public virtual void Init(TileData tileData)
    {
        this.tileData = tileData;

        transform.localPosition = new Vector3(tileData.x, tileData.y, 0);
    }

    public virtual void SetTileSprite(Sprite sprite)
    {
        if (tileImage != null)
        {
            tileImage.sprite = sprite;
        }
    }

    public virtual bool CheckSpawnPoint(bool spawnPathCharacter = false)
    {
        if ((tileData.type == MapObject.Spawn && spawnPathCharacter == false) ||
            (tileData.type == MapObject.Path && spawnPathCharacter))
        {
            return true;
        }

        return false;
    }
   
    public virtual void OnSelect()
    {
    }

    public virtual void OnDeselect()
    {
    }

    public virtual int GetUpgradeCost()
    {
        return 0;
    }

    public virtual void OnUpgrade()
    {
    }

    public virtual float GetSkillLastTime()
    { return 0; }

    public virtual float GetSkillCoolTime() { return 0; }

    public void SpawnableCharacter(bool spawnPathCharacter, bool draging)
    {
        if(draging == false)
        {
            tileImage.color = Color.white;
            return;
        }

        if(tileData.type == MapObject.Spawn && spawnPathCharacter == false)
            tileImage.color = Color.yellow;
        else if (tileData.type == MapObject.Path && spawnPathCharacter)
            tileImage.color = Color.yellow;
        else
            tileImage.color = Color.red;
    }
}