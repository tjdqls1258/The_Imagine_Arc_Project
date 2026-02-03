using DG.Tweening;
using UnityEngine;
using UnityEngine.Playables;

public class TimeLineController : CachObject
{
    [SerializeField] protected PlayableDirector m_targetTimeLine;

    public virtual void Init() { }
    public void StartTimeLine()
    {
        m_targetTimeLine.Play();
    }

    public virtual void StopTimeLine()
    {
        m_targetTimeLine.Stop();
    }

    public virtual void PauseTimeLine()
    {
        m_targetTimeLine.Pause();
    }

    protected void SetTimeLineTween(Tween tween)
    {
        PauseTimeLine();
        tween.OnComplete(StartTimeLine);
    }
}
