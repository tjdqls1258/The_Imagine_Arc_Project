using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class SpawnPlayerCharacterTile : TileBase, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
    private Action<Vector2Int> spawnAction;
    private bool m_spawnUnitTile = false;
    private PlayerCharacterContrroller m_character;

    public bool CheckSpawn() => m_spawnUnitTile;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (m_spawnUnitTile == false) return;
        GameMaster.Instance.uiManager.AutoUIManager.GetCompoent<InGameUIManager>(UIBaseData.UIType.InGameUI).OnClickCharacter(m_character.GetCharacterData(),
            () => 
            {
                m_character.OnPointerDownAction();
                GameUtil.mainCamera.transform.position = new Vector3(transform.position.x, transform.position.y, -10);
            },
            () => 
            { 
                m_character.OnPointerUpAction();
                GameUtil.mainCamera.transform.position = GameData.Instance.DefaulteCameraPos;
            },
            m_character.UpgradeCharacter);
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

    public void SpawnUnit(PlayerCharacterContrroller character)
    {
        m_spawnUnitTile = true;
        m_character = character;
    }

    public void UnitDie()
    {
        m_spawnUnitTile = false;
    }
}
