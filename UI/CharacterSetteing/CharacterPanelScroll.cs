using FancyScrollView;
using NetExcute;
using System;
using UnityEngine;

/// <summary>
/// FancyGridView 내에서 개별 셀(Cell)들과 메인 스크롤 뷰 간의 데이터 및 상태를 공유하는 매개체 클래스입니다.
/// 클릭 이벤트, 소지 중인 덱 데이터, 현재 선택된 상태 등을 관리합니다.
/// </summary>
public class CharacterPanelContext : FancyGridViewContext
{
    /// <summary> 현재 덱에 배치되어 있는 유저 캐릭터 데이터 배열 (중복 배치 체크 등 용도) </summary>
    public UserCharacterData[] userCharacterDatas;

    /// <summary> 현재 선택되어 있는 타겟 캐릭터 데이터 </summary>
    public UserCharacterData selecteCharacterData;

    /// <summary> 현재 선택된 아이템의 인덱스입니다. (초기값 -1: 선택 없음) </summary>
    public int SelectedIndex = -1;

    /// <summary> 
    /// 특정 캐릭터 셀이 클릭되었을 때 실행될 콜백 액션입니다.
    /// 하위 셀에서 이 액션을 호출하면 상위 컨트롤러(CharacterSelectPanel 등)로 데이터가 전달됩니다.
    /// </summary>
    public Action<UserCharacterData> OnCellClicked;
}

/// <summary>
/// 캐릭터 데이터를 그리드(Grid) 형태로 화면에 표시하는 최적화 스크롤 컨트롤러입니다.
/// FancyGridView를 상속받아 유연한 레이아웃 구성과 셀 재사용 로직을 수행합니다.
/// </summary>
public class CharacterPanelScroll : FancyGridView<UserCharacterData, CharacterPanelContext>
{
    /// <summary> 그리드 레이아웃 내에서 셀들을 행/열 단위로 그룹화하여 관리하는 내부 클래스입니다. </summary>
    class CellGroup : DefaultCellGroup { }

    [Header("Cell Settings")]
    /// <summary> 스크롤 뷰에서 템플릿으로 사용할 개별 캐릭터 셀 프리팹입니다. </summary>
    [SerializeField] CharacterCell cellPrefab = default;

    // ----------------------------------------------------------------------
    // ## FancyScrollView Setup
    // ----------------------------------------------------------------------

    /// <summary>
    /// FancyScrollView 시스템 초기화 시 호출됩니다.
    /// 사용할 셀 프리팹과 그룹 관리 방식을 시스템에 등록합니다.
    /// </summary>
    protected override void SetupCellTemplate() => Setup<CellGroup>(cellPrefab);

    // ----------------------------------------------------------------------
    // ## Event & Data Handling
    // ----------------------------------------------------------------------

    /// <summary>
    /// 외부 매니저로부터 클릭 콜백과 참조 데이터를 전달받아 Context를 설정합니다.
    /// </summary>
    /// <param name="callback">셀 클릭 시 실행될 함수</param>
    /// <param name="characterDatas">현재 덱에 설정된 캐릭터 목록</param>
    /// <param name="selecteCharacterData">현재 선택 타겟 데이터</param>
    public void OnCellClicked(Action<UserCharacterData> callback,
        UserCharacterData[] characterDatas = null, UserCharacterData selecteCharacterData = null)
    {
        // 모든 하위 셀이 공유하는 Context에 데이터를 저장하여 참조 효율성을 높입니다.
        Context.OnCellClicked = callback;
        Context.userCharacterDatas = characterDatas;
        Context.selecteCharacterData = selecteCharacterData;
    }
}