using UnityEngine.EventSystems;

public class SpawnReadyState : IButtonState
{
    private UnitButton m_btn;

    public SpawnReadyState(UnitButton button)
    {
        m_btn = button;
    }

    public void Enter()
    {
        //TO COOLTIME
        m_btn.PreviewCharacter.SetSpawn(false);
        m_btn.ToggleSkillUI(false);
    }

    public void OnBeginDrag(PointerEventData e)
    {
        if (!m_btn.HasEnoughCost || m_btn.PreviewCharacter == null)
            return;

        m_btn.ChangeState(m_btn.SpawnDrag);
    }

    public void Exit() { }
    public void OnPointerDown(PointerEventData e) { }
    public void OnPointerUp(PointerEventData e) { }
    public void OnDrag(PointerEventData e) { }
    public void OnEndDrag(PointerEventData e) { }
    public void Update() { }
}