using FancyScrollView;
using System;
using UnityEngine;

/// <summary>
/// FancyGridView 내에서 셀(Cell)들과 스크롤 뷰 간의 통신을 담당하는 컨텍스트 클래스입니다.
/// 클릭 이벤트 델리게이트와 선택된 인덱스 정보를 공유합니다.
/// </summary>
public class CharacterPanelContext : FancyGridViewContext
{
    /// <summary> 현재 선택된 아이템의 인덱스입니다. (초기값 -1: 선택 없음) </summary>
    public int SelectedIndex = -1;

    /// <summary> 특정 캐릭터 셀이 클릭되었을 때 실행될 콜백 액션입니다. </summary>
    public Action<CharacterData> OnCellClicked;
}

/// <summary>
/// 캐릭터 데이터를 그리드 형태로 화면에 표시하는 메인 스크롤 컨트롤러입니다.
/// FancyGridView를 상속받아 데이터 관리 및 셀 템플릿 설정을 수행합니다.
/// </summary>
public class CharacterPanelScroll : FancyGridView<CharacterData, CharacterPanelContext>
{
    /// <summary> 그리드 내에서 셀들을 그룹화하여 관리하는 내부 클래스입니다. </summary>
    class CellGroup : DefaultCellGroup { }

    [Header("Cell Settings")]
    [SerializeField] CharacterCell cellPrefab = default; // 스크롤 뷰에서 사용할 개별 캐릭터 셀 프리팹

    // ----------------------------------------------------------------------
    // ## FancyScrollView Setup
    // ----------------------------------------------------------------------

    /// <summary>
    /// FancyScrollView의 초기 설정 시 셀 템플릿과 그룹 방식을 등록합니다.
    /// </summary>
    protected override void SetupCellTemplate() => Setup<CellGroup>(cellPrefab);

    // ----------------------------------------------------------------------
    // ## Event Handling
    // ----------------------------------------------------------------------

    /// <summary>
    /// 외부(예: 캐릭터 패널 매니저)에서 셀 클릭 이벤트를 수신할 수 있도록 콜백을 등록합니다.
    /// </summary>
    /// <param name="callback">클릭된 캐릭터 데이터를 전달받는 함수</param>
    public void OnCellClicked(Action<CharacterData> callback)
    {
        // Context에 콜백을 저장하여 모든 하위 셀들이 접근할 수 있도록 합니다.
        Context.OnCellClicked = callback;
    }
}