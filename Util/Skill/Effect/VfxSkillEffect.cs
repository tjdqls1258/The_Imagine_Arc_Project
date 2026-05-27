using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.VFX;

public class VfxSkillEffect : SkillEffectObject
{
    private VisualEffect m_vfxGraph;
    private IObjectPool<SkillEffectObject> m_managedPool;

    [Header("VFX Lifecycle")]
    [Tooltip("VFX가 재생된 후 풀로 반납될 때까지의 시간")]
    public float LifeTime = 2f;

    private void Awake()
    {
        m_vfxGraph = GetComponent<VisualEffect>();
    }

    public override void SetPool(IObjectPool<SkillEffectObject> pool)
    {
        m_managedPool = pool;
    }

    public override void PlayEffect(SkillContext context)
    {
        transform.position = context.TargetPosition;

        Vector3 dir = context.TargetPosition;
        dir.y = 0;
        if (dir.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(dir, Vector3.up);

        SettingSize(context);

        m_vfxGraph.Play();

        if (LifeTime > 0f)
        {
            Invoke(nameof(ReleaseToPool), LifeTime);
        }
    }

    public override void SettingSize(SkillContext context)
    {
        base.SettingSize(context);

        if (m_vfxGraph != null && m_vfxGraph.HasFloat("Radius"))
        {
            var calculatedSize = context.SkillRange * SizeRatio;
            m_vfxGraph.SetFloat("Radius", calculatedSize);
        }
    }

    public override void ReleaseToPool()
    {
        CancelInvoke(nameof(ReleaseToPool));

        if (m_managedPool != null)
        {
            m_managedPool.Release(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDisable()
    {
        CancelInvoke(nameof(ReleaseToPool));
    }
}
