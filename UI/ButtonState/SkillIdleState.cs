using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

public class SkillIdleState : IButtonState
{
    private UnitButton m_btn;
    private float m_pointerDownTime;
    private bool m_isHolding;

    public SkillIdleState(UnitButton button) { m_btn = button; }

    public void Enter() { m_btn.ToggleSkillUI(true); }
    public void Exit() { /* 사거리 표시 UI 끄기 */ }

    public void OnPointerDown(PointerEventData e)
    {
        m_pointerDownTime = Time.time;
        m_isHolding = true;
    }

    public void OnPointerUp(PointerEventData e)
    {
        m_isHolding = false;
    }

    public void OnBeginDrag(PointerEventData e)
    {
        if (m_btn.IsSkillReady)
        {
            m_btn.ChangeState(m_btn.SkillDrag);
        }
    }

    public void Update()
    {
        if (m_btn.PreviewCharacter == null) return;
        if (m_btn.PreviewCharacter.IsSpwan() == false) return; // 맵에서 사라지면 루프 종료

        if (m_btn.SkillReadyTime <= Time.time)
        {
            m_btn.SetBlockState(false);
        }
        else
        {
            m_btn.SetBlockState(true);
            m_btn.UpdateSkillCoolTimeText();
        }
    }
    public void OnDrag(PointerEventData e) { }
    public void OnEndDrag(PointerEventData e) { }
}