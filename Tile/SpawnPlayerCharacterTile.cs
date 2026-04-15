using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class SpawnPlayerCharacterTile : TileBase, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
    private bool m_spawnUnitTile = false;
    private PlayerCharacterContrroller m_character;

    public bool CheckSpawn() => m_spawnUnitTile;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (m_spawnUnitTile == false) return;

        GameMaster.Instance.uiManager.GetAutoUIManager()
            .GetCompoent<InGameUIManager>(UIBaseData.UIType.InGameUI)
            .OnClickCharacter(m_character.GetCharacterData(), this);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (m_spawnUnitTile == false) return;
        m_character.OnPointerDownAction();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (m_spawnUnitTile == false) return;
        m_character.OnPointerUpAction();
    }

    public override void OnDeselect()
    {
        m_character.OnPointerUpAction();
        GameUtil.mainCamera.transform.position = GameData.Instance.DefaulteCameraPos;
    }

    public override void OnSelect()
    {
        m_character.OnPointerDownAction();
        GameUtil.mainCamera.transform.position = new Vector3(transform.position.x, transform.position.y, -10);
    }

    public override int GetUpgradeCost()
    {
        return m_character.GetCharacterData().characterData.cost;
    }

    public override void OnUpgrade()
    {
        m_character.UpgradeCharacter();
    }

    public void SpawnUnit(PlayerCharacterContrroller character)
    {
        m_spawnUnitTile = true;
        m_character = character;
    }

    public void UnitDie()
    {
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