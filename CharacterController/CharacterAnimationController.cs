using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CharacterAnimationController : MonoBehaviour
{
    // ====== Animation Parameter Enums ======

    /// <summary> Animator에서 사용하는 Trigger 타입의 파라미터 목록입니다. </summary>
    public enum AnimationTrigger
    {
        None,
        ATK,    // 공격 애니메이션 실행
        HIT,    // 피격 애니메이션 실행
        SKILL   // 스킬 애니메이션 실행
    }

    /// <summary> Animator에서 사용하는 Bool 타입의 파라미터 목록입니다. </summary>
    public enum AnimationBool
    {
        None,
        DIE     // 사망 상태 유지 (True일 때 사망 애니메이션 고정)
    }

    // ====== Members & Callbacks ======

    private Animator m_animator;

    /// <summary> 공격 애니메이션 중 실제 타격 시점에 실행될 액션입니다. </summary>
    private UnityAction m_atteckAction;

    /// <summary> 피격/사망 관련 애니메이션 중 특정 시점에 실행될 액션입니다. </summary>
    private UnityAction m_hitAction;

    private UnityAction m_dieAction;

    private UnityAction m_skillAction;

    // ----------------------------------------------------------------------
    // ## Initialization
    // ----------------------------------------------------------------------

    private void Awake()
    {
        // 동일한 오브젝트에 부착된 Animator 컴포넌트를 캐싱합니다.
        m_animator = GetComponentInChildren<Animator>();
    }

    // ----------------------------------------------------------------------
    // ## Animation Control Methods
    // ----------------------------------------------------------------------

    /// <summary>
    /// Enum에 정의된 트리거를 사용하여 단발성 애니메이션을 재생합니다.
    /// </summary>
    /// <param name="triggerName">재생할 애니메이션 트리거 이름</param>
    public void PlayAnimation_Trigger(AnimationTrigger triggerName)
    {
        if (triggerName == AnimationTrigger.None) return;
        m_animator.SetTrigger(triggerName.ToString());
    }

    /// <summary>
    /// Enum에 정의된 불 값을 사용하여 애니메이션 상태를 전환합니다.
    /// </summary>
    /// <param name="aniName">전환할 애니메이션 상태 이름</param>
    /// <param name="value">상태 값 (True/False)</param>
    public void PlayAnimation_Bool(AnimationBool aniName, bool value)
    {
        if (aniName == AnimationBool.None) return;
        m_animator.SetBool(aniName.ToString(), value);
    }

    // ----------------------------------------------------------------------
    // ## Animation Event Handlers (Called by Animator)
    // ----------------------------------------------------------------------

    /// <summary>
    /// [Animation Event] 공격 애니메이션의 타격 프레임에서 호출되어 등록된 액션을 실행합니다.
    /// </summary>
    public void EventAtteckAnimation()
    {
        if (m_atteckAction != null)
            m_atteckAction.Invoke();
    }

    /// <summary>
    /// [Animation Event] 피격 관련 애니메이션 중 특정 프레임에서 호출되어 등록된 액션을 실행합니다.
    /// </summary>
    public void EventHitAnimation()
    {
        if (m_hitAction != null)
            m_hitAction.Invoke();
    }

    public void EventDieAnimation()
    {
        if(m_dieAction != null)
            m_dieAction.Invoke();
    }

    public void EventSkillAnimation()
    {
        if(m_skillAction != null)
            m_skillAction.Invoke();
    }

    // ----------------------------------------------------------------------
    // ## Action Management (Delegate)
    // ----------------------------------------------------------------------

    /// <summary>
    /// 애니메이션 이벤트 액션을 초기화(덮어쓰기)합니다.
    /// </summary>
    public void SetAction(UnityAction atteckAction, UnityAction hitAction, UnityAction dieAction, UnityAction skillAction)
    {
        m_atteckAction = atteckAction;
        m_hitAction = hitAction;
        m_dieAction = dieAction;
        m_skillAction = skillAction;
    }

    /// <summary>
    /// 기존 애니메이션 이벤트 액션에 새로운 기능을 추가합니다.
    /// </summary>
    public void AddAction(UnityAction atteckAction, UnityAction hitAction, UnityAction dieAction, UnityAction skillAction)
    {
        if (atteckAction != null)
            m_atteckAction += atteckAction;
        if (hitAction != null)
            m_hitAction += hitAction;
        if(dieAction != null)
            m_dieAction += dieAction;
        if(skillAction != null)
            m_skillAction += skillAction;
    }

    /// <summary>
    /// 등록된 애니메이션 이벤트 액션을 제거합니다.
    /// </summary>
    public void RemoveAction(UnityAction atteckAction, UnityAction hitAction, UnityAction dieAction, UnityAction skillAction)
    {
        if (atteckAction != null)
            m_atteckAction -= atteckAction;
        if (hitAction != null)
            m_hitAction -= hitAction;
        if(dieAction != null)
            m_dieAction -= dieAction;
        if(skillAction != null)
            m_skillAction -= skillAction;
    }
}