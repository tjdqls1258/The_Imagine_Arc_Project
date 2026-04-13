using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ToolTipBox : MonoBehaviour
{
    [SerializeField] private GameObject parent;
    [SerializeField] private TextMeshProUGUI m_title;
    [SerializeField] private TextMeshProUGUI m_desc;
    [SerializeField] private TextMeshProUGUI m_cooltime;

    [SerializeField] private Vector2 m_cursorPadding = new Vector2(15f, 15f);

    private RectTransform m_transform;
    private CanvasGroup m_canvasGroup;

    private void Awake()
    {
        parent.GetComponent<Button>().onClick.AddListener(CloseToolTip);
        m_canvasGroup = GetComponent<CanvasGroup>();
    }

    public void ShowToolTip(IToolTip tip)
    {
        if(m_transform == null)
            m_transform = GetComponent<RectTransform>();

        parent.SetActive(true);
        m_title.text = tip.GetTitle();
        m_desc.text = tip.GetDescription();
        m_cooltime.text = tip.GetCoolTime();

        UpdatePosition().Forget();
        parent.transform.SetAsLastSibling();
    }

    private void CloseToolTip()
    {
        parent.SetActive(false);
    }

    async UniTask UpdatePosition()
    {
        m_canvasGroup.alpha = 0.0f;
        LayoutRebuilder.ForceRebuildLayoutImmediate(m_transform);

        await UniTask.WaitForEndOfFrame(this);

        UpdatePos();
        m_canvasGroup.alpha = 1;
    }

    private void UpdatePos()
    {
        Vector2 mousePosition = Input.mousePosition;
        m_transform.position = mousePosition;

        Vector2 currentLeftBottom = m_transform.TransformPoint(m_transform.rect.min);
        Vector2 shiftToMouse = mousePosition - currentLeftBottom;
        m_transform.position += (Vector3)(shiftToMouse + m_cursorPadding);

        //화면 밖 나가지 않게 조정
        Rect rect = m_transform.rect;

        Vector2 leftBottom = m_transform.TransformPoint(rect.min);
        Vector2 rightTop = m_transform.TransformPoint(rect.max);
        var size = rightTop - leftBottom;

        rightTop = new Vector2(Screen.width, Screen.height) - size;

        float x = Mathf.Clamp(leftBottom.x, 0, rightTop.x);
        float y = Mathf.Clamp(leftBottom.y, 0, rightTop.y);

        Vector2 pivotOffset = (Vector2)m_transform.position - leftBottom;

        m_transform.position = new Vector2(x, y) + pivotOffset;
    }
}
