using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public partial class LoadingPanel : CachObject
{
    [Header("Loading Panel UI")]
    [SerializeField] private Image m_loadingBar;
    [SerializeField] private TextMeshProUGUI m_loadingText;

    public void LoadingText(string label, long current, long max)
    {
        if (max <= 0) return;

        double progressRatio = (double)current / max;
        string percent = Math.Truncate(progressRatio * 100).ToString("N0");

        m_loadingText.text = $"Download {label} : {percent}%";
        m_loadingBar.fillAmount = (float)progressRatio;
    }
}