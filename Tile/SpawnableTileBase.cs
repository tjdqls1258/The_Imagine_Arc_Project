using UnityEngine;
using UnityEngine.EventSystems;
using VContainer;

public class SpawnableTileBase : TileBase, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
    public bool isUnitSpawnTile = true;
    protected bool m_spawnUnitTile = false;
    protected PlayerCharacterController m_character;

    public bool CheckSpawn() => m_spawnUnitTile;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isUnitSpawnTile == false) return;
        if (m_spawnUnitTile == false) return;

        uiManager.GetAutoUIManager()
            .GetCompoent<InGameUIManager>(UIBaseData.UIType.InGameUI)
            .OnClickCharacter(m_character.CharacterData, this);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (isUnitSpawnTile == false) return;
        if (m_spawnUnitTile == false) return;
        m_character.OnPointerDownAction();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (isUnitSpawnTile == false) return;
        if (m_spawnUnitTile == false) return;
        m_character.OnPointerUpAction();
    }

    public override void OnDeselect()
    {
        if (isUnitSpawnTile == false) return;
        m_character.OnPointerUpAction();
        GameUtil.mainCamera.transform.position = GameData.Instance.DefaulteCameraPos;
    }

    public override void OnSelect()
    {
        if (isUnitSpawnTile == false) return;
        m_character.OnPointerDownAction();
        GameUtil.mainCamera.transform.position = new Vector3(transform.position.x, transform.position.y, -10);
    }

    public override int GetUpgradeCost()
    {
        return m_character.CharacterData.characterData.cost;
    }

    public override void OnUpgrade()
    {

        m_character.UpgradeCharacter();
    }

    public void SpawnUnit(PlayerCharacterController character)
    {
        m_spawnUnitTile = true;
        m_character = character;
        m_character.AddDieAction(UnitDie);
    }

    public void UnitDie()
    {
        m_character.RemoveDieAction(UnitDie);
        m_spawnUnitTile = false;
        m_character = null;
    }

    public override float GetSkillLastTime()
    {
        return m_character.GetLastSkillTime();
    }

    public override float GetSkillCoolTime()
    {
        return m_character.GetSkillCoolTime();
    }
}
