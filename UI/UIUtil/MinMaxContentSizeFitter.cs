using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
[ExecuteAlways]
public class MinMaxContentSizeFitter : UIBehaviour, ILayoutSelfController
{
    public enum FitMode
    {
        Unconstrained,
        MinMaxFit
    }

    [Header("Horizontal Settings")]
    public FitMode horizontalFit = FitMode.Unconstrained;
    public float minWidth = 0f;
    public float maxWidth = 1000f;

    [Header("Vertical Settings")]
    public FitMode verticalFit = FitMode.Unconstrained;
    public float minHeight = 0f;
    public float maxHeight = 1000f;

    private RectTransform m_rectTransform;
    private RectTransform rectTransform
    {
        get
        {
            if (m_rectTransform == null) m_rectTransform = GetComponent<RectTransform>();
            return m_rectTransform;
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        SetDirty();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
    }

    protected override void OnRectTransformDimensionsChange()
    {
        base.OnRectTransformDimensionsChange();
        SetDirty();
    }

    public void SetLayoutHorizontal()
    {
        HandleSelfFittingAlongAxis(true);
    }

    public void SetLayoutVertical()
    {
        HandleSelfFittingAlongAxis(false);
    }

    private void HandleSelfFittingAlongAxis(bool isHorizeontal)
    {
        FitMode fitting = (isHorizeontal ? horizontalFit : verticalFit);
        if (fitting == FitMode.Unconstrained)
            return;

        float min = (isHorizeontal ? minWidth : minHeight);
        float max = (isHorizeontal ? maxWidth : maxHeight);

        float preferredSize = LayoutUtility.GetPreferredSize(rectTransform, isHorizeontal ? 0 : 1);

        float clampedSize = Mathf.Clamp(preferredSize, min, max);

        rectTransform.SetSizeWithCurrentAnchors((RectTransform.Axis)(isHorizeontal ? 0 : 1), clampedSize);
    }

    protected void SetDirty()
    {
        if (!IsActive()) return;
        LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        SetDirty();
    }
#endif
}
