using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UniRx;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.UI;
using VContainer;

public class SkillCutscenePresenter : MonoBehaviour
{
    [Inject] private AddressableManager m_addressableManager;

    [SerializeField] private Image m_cutsceneTargetImage;
    [SerializeField] private PlayableDirector m_director;
    [SerializeField] private GameObject m_cutscenePanel;

    private void Start()
    {
        MessageBroker.Default.Receive<SkillFiredEvent>()
        .Subscribe(e => {
            MessageBroker.Default.Publish(new TimeScaleRequestEvent("SkillCutscene", 0f, PRIORITY_TIME.SkillCutScene));
            
            m_cutscenePanel.SetActive(true);
            OnSkillFiredAsync(e).Forget();
        })
        .AddTo(this);
    }

    private async UniTaskVoid OnSkillFiredAsync(SkillFiredEvent e)
    {
        if (string.IsNullOrEmpty(e.timeLineKey)) return;

        try
        {
            var timeline = await m_addressableManager.LoadAssetAndCacheAsync<TimelineAsset>(e.timeLineKey);

            Sprite cutsceneSprite = await e.caster.GetCutsceneSpriteAsync(m_addressableManager);

            if (cutsceneSprite != null)
            {
                BindSpriteToTimeline(timeline, cutsceneSprite);
            }

            m_director.playableAsset = timeline;
            m_director.Play();

            await UniTask.WaitUntil(() => m_director.state != PlayState.Playing);
        }
        catch (OperationCanceledException) 
        {
            MessageBroker.Default.Publish(new TimeScaleReleaseEvent("SkillCutscene"));
            m_cutscenePanel.SetActive(false);
        }
    }

    private void BindSpriteToTimeline(TimelineAsset timeline, Sprite loadedSprite)
    {
        if (m_cutsceneTargetImage != null && loadedSprite != null)
        {
            m_cutsceneTargetImage.sprite = loadedSprite;
        }
    }
}
