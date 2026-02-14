using System;
using UnityEngine;
using static LoadingPanel;

/// <summary>
/// 게임의 초기 진입 시 로딩 과정(다운로드 체크, 패치 진행, 시작 화면)을 관리하는 클래스입니다.
/// partial 클래스를 통해 UI 구성 요소와 제어 로직을 분리하여 관리할 수 있습니다.
/// </summary>
public partial class LoadingPanel : CachObject
{
    // ====== UI Binding Enums (CachObject 시스템 활용) ======

    /// <summary> 로딩 단계별로 화면에 표시될 각 캔버스 그룹 인덱스입니다. </summary>
    public enum CanvasGroups
    {
        DownloadPaenl, // 어드레서블 리소스 다운로드 진행률 화면
        StartPaenl,    // 다운로드 완료 후 "터치하여 시작" 화면
        CheckDownload  // 다운로드 용량 확인 및 승인 팝업 화면
    }

    // ====== Runtime State ======

    /// <summary> UI 컴포넌트 바인딩(Bind)이 완료되었는지 확인하는 플래그입니다. </summary>
    private bool m_bindDone = false;

    /// <summary> 현재 활성화된 화면 그룹 상태를 저장합니다. </summary>
    private CanvasGroups canvasGroups;

    // ----------------------------------------------------------------------
    // ## Initialization (Lazy Binding)
    // ----------------------------------------------------------------------

    /// <summary>
    /// UI 컴포넌트들을 최초 1회 자동으로 연결(Bind)합니다.
    /// 호출 시점에 바인딩을 수행하는 지연 초기화(Lazy Initialization) 방식을 사용합니다.
    /// </summary>
    private void CheckBind()
    {
        if (m_bindDone)
            return;

        m_bindDone = true;
        // Enum에 정의된 CanvasGroup 컴포넌트들을 찾아 배열로 캐싱
        Bind<CanvasGroup>(typeof(CanvasGroups));
    }

    // ----------------------------------------------------------------------
    // ## Panel Control
    // ----------------------------------------------------------------------

    /// <summary>
    /// 특정 로딩 단계를 화면에 표시하고 나머지 단계는 숨깁니다.
    /// </summary>
    /// <param name="group">활성화할 대상 CanvasGroups 단계</param>
    public void ShowPanel(CanvasGroups group)
    {
        // 1. 컴포넌트 바인딩 상태 확인 및 초기화
        CheckBind();

        canvasGroups = group;

        // 2. 모든 캔버스 그룹을 순회하며 상태 설정
        foreach (CanvasGroups grp in Enum.GetValues(typeof(CanvasGroups)))
        {
            // 대상 그룹이면 알파를 1(표시), 아니면 0(숨김)으로 설정
            Get<CanvasGroup>((int)grp).alpha = group == grp ? 1 : 0;

            // 상호작용 및 레이캐스트 차단 여부를 활성 상태에 맞춰 설정
            // 활성 상태일 때만 버튼 클릭 등이 가능해집니다.
            Get<CanvasGroup>((int)grp).interactable = group == grp;
            Get<CanvasGroup>((int)grp).blocksRaycasts = group == grp;
        }
    }
}