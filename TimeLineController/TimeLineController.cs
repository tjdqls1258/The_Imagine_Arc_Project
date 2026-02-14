using DG.Tweening;
using UnityEngine;
using UnityEngine.Playables;

/// <summary>
/// 유니티 타임라인(PlayableDirector)의 실행을 제어하고, 
/// DOTween 애니메이션과 타임라인의 흐름을 동기화하는 베이스 클래스입니다.
/// </summary>
public class TimeLineController : CachObject
{
    // ====== Inspector Settings ======

    /// <summary> 제어 대상이 되는 유니티 타임라인 컴포넌트입니다. </summary>
    [SerializeField] protected PlayableDirector m_targetTimeLine;

    // ----------------------------------------------------------------------
    // ## Core Control Methods
    // ----------------------------------------------------------------------

    /// <summary> 초기화 로직을 수행합니다. (상속받아 재정의 가능) </summary>
    public virtual void Init() { }

    /// <summary> 타임라인 재생을 시작합니다. </summary>
    public void StartTimeLine()
    {
        m_targetTimeLine.Play();
    }

    /// <summary> 타임라인 재생을 완전히 정지합니다. </summary>
    public virtual void StopTimeLine()
    {
        m_targetTimeLine.Stop();
    }

    /// <summary> 타임라인을 현재 프레임에서 일시 정지합니다. </summary>
    public virtual void PauseTimeLine()
    {
        m_targetTimeLine.Pause();
    }

    // ----------------------------------------------------------------------
    // ## DOTween Integration
    // ----------------------------------------------------------------------

    /// <summary>
    /// 외부 트윈(Tween) 애니메이션이 실행되는 동안 타임라인을 일시 정지시키고,
    /// 애니메이션이 완료되면 다시 타임라인을 재생하도록 설정합니다.
    /// </summary>
    /// <param name="tween">실행할 DOTween 애니메이션 객체</param>
    protected void SetTimeLineTween(Tween tween)
    {
        // 1. 타임라인을 멈춰서 연출 대기 상태로 만듦
        PauseTimeLine();

        // 2. 전달받은 트윈이 종료(OnComplete)되는 시점에 다시 타임라인을 재생(StartTimeLine)
        tween.OnComplete(StartTimeLine);
    }
}