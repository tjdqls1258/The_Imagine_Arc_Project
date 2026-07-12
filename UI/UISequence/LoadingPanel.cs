using System;
using UnityEngine;
using static LoadingPanel;

public partial class LoadingPanel : CacheObject
{
    public enum CanvasGroups
    {
        DownloadPaenl,
        StartPanel,
        CheckDownload
    }

    private bool m_bindDone = false;
    private CanvasGroups canvasGroups;

    private void CheckBind()
    {
        if (m_bindDone)
            return;

        m_bindDone = true;
        Bind<CanvasGroup>(typeof(CanvasGroups));
    }

    public void ShowPanel(CanvasGroups group)
    {
        CheckBind();

        canvasGroups = group;

        foreach (CanvasGroups grp in Enum.GetValues(typeof(CanvasGroups)))
        {
            Get<CanvasGroup>((int)grp).alpha = group == grp ? 1 : 0;
            Get<CanvasGroup>((int)grp).interactable = group == grp;
            Get<CanvasGroup>((int)grp).blocksRaycasts = group == grp;
        }
    }
}