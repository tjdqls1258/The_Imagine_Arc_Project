using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

public enum PRIORITY_TIME : int
{
    SetBaseTime = 0,
    SkillDrage,
    SkillCutScene,
    Optiuon,
}

public struct TimeScaleRequestEvent
{
    public string RequesterID;
    public float TargetScale;
    public PRIORITY_TIME Priority;

    public TimeScaleRequestEvent(string id, float scale, PRIORITY_TIME priority)
    {
        RequesterID = id;
        TargetScale = scale;
        Priority = priority;
    }
}

public struct TimeScaleReleaseEvent
{
    public string RequesterID;

    public TimeScaleReleaseEvent(string id)
    {
        RequesterID = id;
    }
}

public class InGameTimeScaleManager : System.IDisposable
{
    private class TimeRequest
    {
        public string CallID;
        public float Scale;
        public int Priority;
    }

    private CompositeDisposable m_disposables = new();
    private readonly List<TimeRequest> m_requests = new();

    public void Init(float defaultSpeed)
    {
        m_requests.Add(new TimeRequest
        {
            CallID = "BaseSpeed",
            Scale = defaultSpeed,
            Priority = (int)PRIORITY_TIME.SetBaseTime
        }); 
        
        MessageBroker.Default.Receive<TimeScaleRequestEvent>()
        .Subscribe(e => {
            m_requests.RemoveAll(r => r.CallID == e.RequesterID);
            m_requests.Add(new TimeRequest
            {
                CallID = e.RequesterID,
                Scale = e.TargetScale,
                Priority = (int)e.Priority
            });
            ApplyHighestPriorityTimeScale();
        }).AddTo(m_disposables);

        MessageBroker.Default.Receive<TimeScaleReleaseEvent>()
            .Subscribe(e => {
                m_requests.RemoveAll(r => r.CallID == e.RequesterID);
                ApplyHighestPriorityTimeScale();
            }).AddTo(m_disposables);

        ApplyHighestPriorityTimeScale();
    }

    private void ApplyHighestPriorityTimeScale()
    {
        var topRequest = m_requests.OrderByDescending(r => r.Priority).First();
        Time.timeScale = topRequest.Scale;
        Time.fixedDeltaTime = GameUtil.TimeConstants.DEFAULT_FIXEDDELTA * topRequest.Scale;
    }

    public void Dispose()
    {
        Time.timeScale = GameUtil.TimeConstants.DEFAULT_TIMESCALE;
        Time.fixedDeltaTime = GameUtil.TimeConstants.DEFAULT_FIXEDDELTA;

        m_disposables.Dispose();
        m_requests.Clear();
    }
}
