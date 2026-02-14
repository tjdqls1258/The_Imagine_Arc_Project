using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// LoadingPanel의 partial 클래스이며, 실제 리소스 다운로드 및 로딩 수치를 
/// 시각적으로 업데이트하는 로직을 포함합니다.
/// </summary>
public partial class LoadingPanel : CachObject
{
    [Header("Loading Panel UI")]
    [Tooltip("로딩 진행률을 표시할 이미지 (Fill Amount 방식)")]
    [SerializeField] private Image m_loadingBar;

    [Tooltip("다운로드 상태 및 퍼센트 수치를 표시할 텍스트")]
    [SerializeField] private TextMeshProUGUI m_loadingText;

    // ----------------------------------------------------------------------
    // ## Progress Update Logic
    // ----------------------------------------------------------------------

    /// <summary>
    /// 다운로드 진행 상황을 계산하여 로딩바와 텍스트를 갱신합니다.
    /// </summary>
    /// <param name="label">현재 다운로드 중인 항목의 이름 (예: Patch, Assets)</param>
    /// <param name="current">현재까지 다운로드된 용량/수량</param>
    /// <param name="max">전체 다운로드해야 할 총 용량/수량</param>
    public void LoadingText(string label, long current, long max)
    {
        // 1. 0으로 나누기 방지 (최대치가 0인 경우 처리)
        if (max <= 0) return;

        // 2. 백분율(%) 계산: (현재치 / 최대치) * 100 
        // Math.Truncate를 사용하여 소수점 이하는 버리고 정수 부분만 취함
        double progressRatio = (double)current / max;
        string percent = Math.Truncate(progressRatio * 100).ToString("N0");

        // 3. 로그 출력 및 텍스트 갱신 (N0: 세 자리마다 콤마 표시)
        Logger.Log($"Download {label} : Loading {percent}%");
        m_loadingText.text = $"Download {label} : {percent}%";

        // 4. 로딩바 이미지 갱신 (Fill Amount는 0.0 ~ 1.0 사이의 값)
        m_loadingBar.fillAmount = (float)progressRatio;
    }
}