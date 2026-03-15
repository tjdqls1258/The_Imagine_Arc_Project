using System;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 유닛이 배치되는 전장의 타일 컴포넌트입니다.
/// 유닛의 배치 상태를 관리하며, 클릭 이벤트를 UI 시스템으로 중개합니다.
/// TileBase를 상속받아 TileClickEvent 인터페이스 규격을 따르므로, UI와의 결합도가 매우 낮습니다.
/// </summary>
public class SpawnPlayerCharacterTile : TileBase, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
    // ====== 상태 및 참조 변수 ======

    /// <summary> 타일에 유닛이 배치되어 있는지 여부 </summary>
    private bool m_spawnUnitTile = false;

    /// <summary> 현재 이 타일에 배치된 캐릭터의 컨트롤러 참조 </summary>
    private PlayerCharacterContrroller m_character;

    // ----------------------------------------------------------------------
    // ## State Check
    // ----------------------------------------------------------------------

    /// <summary> 타일에 유닛 배치가 가능한지(비어있는지) 확인합니다. </summary>
    public bool CheckSpawn() => m_spawnUnitTile;

    // ----------------------------------------------------------------------
    // ## Input Handling (EventSystem Interfaces)
    // ----------------------------------------------------------------------

    /// <summary>
    /// 타일을 클릭했을 때 호출됩니다. 유닛이 있다면 상세 정보 UI를 엽니다.
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        // 유닛이 없는 타일이면 무시
        if (m_spawnUnitTile == false) return;

        // [리팩토링 핵심] 콜백 지옥 탈출
        // 복잡한 람다식 대신, TileClickEvent 인터페이스를 구현한 자기 자신(this)을 넘깁니다.
        // UI 매니저는 전달받은 객체의 OnSelect, OnDeselect 등을 적절한 시점에 알아서 호출합니다.
        GameMaster.Instance.uiManager.AutoUIManager
            .GetCompoent<InGameUIManager>(UIBaseData.UIType.InGameUI)
            .OnClickCharacter(m_character.GetCharacterData(), this);
    }

    /// <summary> 타일을 누르는 순간 유닛에게 피드백(눌림 연출 등)을 전달합니다. </summary>
    public void OnPointerDown(PointerEventData eventData)
    {
        if (m_spawnUnitTile == false) return;
        m_character.OnPointerDownAction();
    }

    /// <summary> 타일에서 손을 떼는 순간 유닛의 피드백을 해제합니다. </summary>
    public void OnPointerUp(PointerEventData eventData)
    {
        if (m_spawnUnitTile == false) return;
        m_character.OnPointerUpAction();
    }

    // ----------------------------------------------------------------------
    // ## TileClickEvent Interface (UI 매니저에 의해 수동 호출되는 콜백 영역)
    // ----------------------------------------------------------------------

    /// <summary>
    /// [TileClickEvent] UI에서 캐릭터 선택이 해제되거나 창이 닫힐 때 호출됩니다.
    /// 유닛의 강조 연출을 끄고, 카메라를 기본 뷰로 복귀시킵니다.
    /// </summary>
    public override void OnDeselect()
    {
        m_character.OnPointerUpAction();
        GameUtil.mainCamera.transform.position = GameData.Instance.DefaulteCameraPos;
    }

    /// <summary>
    /// [TileClickEvent] UI에서 캐릭터가 선택되어 정보창이 뜰 때 호출됩니다.
    /// 유닛을 강조하고, 카메라를 현재 타일 위치로 포커싱합니다.
    /// </summary>
    public override void OnSelect()
    {
        m_character.OnPointerDownAction();
        // 카메라를 타일 위치로 이동 (Z축은 유지하여 2D 뷰포트 확보)
        GameUtil.mainCamera.transform.position = new Vector3(transform.position.x, transform.position.y, -10);
    }


    public override int GetUpgradeCost()
    {
        return m_character.GetCharacterData().characterData.cost;
    }
    /// <summary>
    /// [TileClickEvent] UI의 '업그레이드' 버튼이 눌렸을 때 호출됩니다.
    /// 실제 인게임 캐릭터 객체의 업그레이드 로직을 실행합니다.
    /// </summary>
    public override void OnUpgrade()
    {
        m_character.UpgradeCharacter();
    }

    // ----------------------------------------------------------------------
    // ## Unit Lifecycle (배치 및 해제)
    // ----------------------------------------------------------------------

    /// <summary>
    /// 타일에 새로운 유닛을 배치하고 상태를 업데이트합니다.
    /// </summary>
    /// <param name="character">배치될 캐릭터 컨트롤러</param>
    public void SpawnUnit(PlayerCharacterContrroller character)
    {
        m_spawnUnitTile = true;
        m_character = character;
    }

    /// <summary>
    /// 배치된 유닛이 파괴되거나 회수되었을 때 타일을 빈 상태로 전환합니다.
    /// </summary>
    public void UnitDie()
    {
        m_spawnUnitTile = false;
        m_character = null;
    }
}