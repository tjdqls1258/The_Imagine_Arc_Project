using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;

public class SkillDragState : IButtonState
{
    private UnitButton m_btn;
    private SkillContext m_aimContext => m_btn.PreviewCharacter.stateManager.skillContext;

    public SkillDragState(UnitButton button) { m_btn = button; }

    public void Enter()
    {
        MessageBroker.Default.Publish(new TimeScaleRequestEvent("SkillDrag", 0.1f, PRIORITY_TIME.SkillDrag));

        if (m_aimContext != null)
        {
            m_btn.CharacterData.activeSkill.BeginAiming(m_aimContext);
        }
    }

    public void Exit()
    {
        MessageBroker.Default.Publish(new TimeScaleReleaseEvent("SkillDrag"));

        if (m_aimContext != null)
        {
            m_btn.CharacterData.activeSkill.CancelAiming(m_aimContext);
        }
    }

    public void OnDrag(PointerEventData e)
    {
        if (m_aimContext == null) return;

        m_btn.UpdateSkillContextPosition(m_aimContext, e.position);
        m_btn.CharacterData.activeSkill.UpdateAiming(m_aimContext);
    }

    public void OnEndDrag(PointerEventData e)
    {
        if (m_aimContext == null)
        {
            m_btn.ChangeState(m_btn.SkillIdle);
            return;
        }

        m_aimContext.Damage = m_btn.PreviewCharacter.stateManager.GetDamage();

        m_btn.UpdateSkillContextPosition(m_aimContext, e.position);
        if(m_btn.CharacterData.activeSkill.EndAimingAndExecute(m_aimContext, true))
            m_btn.SkillReadyTime = Time.time + m_btn.CharacterData.activeSkill.Cooldown;

        m_btn.ChangeState(m_btn.SkillIdle);
    }

    public void OnPointerDown(PointerEventData e) { }
    public void OnPointerUp(PointerEventData e) { }
    public void OnBeginDrag(PointerEventData e) { }
    public void Update() { }
}