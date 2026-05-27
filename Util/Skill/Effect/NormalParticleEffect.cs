using UnityEngine;
using UnityEngine.Pool;

[RequireComponent(typeof(ParticleSystem))]
public class NormalParticleEffect : SkillEffectObject
{
    private ParticleSystem m_particleSystem;
    private IObjectPool<SkillEffectObject> m_managedPool;

    private void Awake()
    {
        m_particleSystem = GetComponent<ParticleSystem>();

        var mainModule = m_particleSystem.main;
        if (mainModule.stopAction != ParticleSystemStopAction.Callback)
        {
            Debug.LogWarning($"[NormalParticleEffect] '{gameObject.name}'의 파티클 Stop Action이 Callback으로 설정되지 않았습니다! 메모리 누수가 발생할 수 있습니다.");
        }
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
        {
            transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
        }

        SettingSize(context);
        m_particleSystem.Play(true);
    }

    public override void ReleaseToPool()
    {
        if (m_managedPool != null)
        {
            m_managedPool.Release(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnParticleSystemStopped()
    {
        ReleaseToPool();
    }

    public GameObject GetPrefab() => this.gameObject;
}