using UnityEngine;
using UnityEngine.EventSystems;

public class SpawnCoolTimeState : IButtonState
{
    private UnitButton m_btn;
    private float m_currentCoolTime;
    private float m_maxCoolTime;

    public SpawnCoolTimeState(UnitButton button)
    {
        m_btn = button;
    }

    public void Enter()
    {
        m_btn.PreviewCharacter.SetSpawn(false);
        m_btn.ToggleSkillUI(false);
        m_btn.SetBlockState(true);
        // 쿨타임 시간 설정
        // m_maxCoolTime = m_btn.PreviewCharacter.CharacterData.characterData.spawnCoolTime;
        m_maxCoolTime = 15f; // 임시로 15초 설정
        m_currentCoolTime = m_maxCoolTime;

        // m_btn.SetCoolTimeUI(true);
    }

    public void Update()
    {
        if (m_currentCoolTime > 0)
        {
            m_currentCoolTime -= Time.deltaTime;

            m_btn.UpdateCoolTimeText(m_currentCoolTime / m_maxCoolTime);

            // 쿨타임이 0 이하가 되면 다시 스폰 가능한 상태로 자동 전이
            if (m_currentCoolTime <= 0)
            {
                m_btn.ChangeState(m_btn.SpawnReady);
            }
        }
    }

    public void Exit()
    {

    }


    public void OnPointerDown(PointerEventData e) { }
    public void OnPointerUp(PointerEventData e) { }
    public void OnBeginDrag(PointerEventData e) { }
    public void OnDrag(PointerEventData e) { }
    public void OnEndDrag(PointerEventData e) { }
}
