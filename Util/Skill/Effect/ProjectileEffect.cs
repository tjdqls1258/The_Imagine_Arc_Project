using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class ProjectileEffect : SkillEffectObject
{
    private IObjectPool<SkillEffectObject> m_managedPool;

    [Header("Flight Settings")]
    public float MoveSpeed = 20f;
    public float MaxLifeTime = 5f; // 허공에 날아갔을 때의 최대 생존 시간

    [Header("Hit Effects")]
    [Tooltip("적에게 맞았을 때 터질 피격 이펙트 (폭발 등)")]
    public SkillEffectObject HitVfxPrefab;

    [Tooltip("관통 여부 (true면 적을 맞춰도 파괴되지 않고 계속 날아감)")]
    public bool IsPiercing = false;

    private bool m_isFlying = false;
    private Vector3 m_moveDirection;

    private SkillContext m_currentContext;
    private List<EffectModule> m_effectsToApply;

    public override void SetPool(IObjectPool<SkillEffectObject> pool)
    {
        m_managedPool = pool;
    }

    public void SetupPayload(SkillContext context, List<EffectModule> effects)
    {
        m_currentContext = context;
        m_effectsToApply = effects;
    }

    public override void PlayEffect(SkillContext context)
    {
        transform.position = context.Caster.GetTransform().position;

        m_moveDirection = (context.TargetPosition - transform.position).normalized;
        m_moveDirection.y = 0;

        if (m_moveDirection.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(m_moveDirection, Vector3.up);
        }

        SettingSize(context);

        m_isFlying = true;
        Invoke(nameof(ReleaseToPool), MaxLifeTime);
    }

    private void Update()
    {
        if (!m_isFlying) return;

        transform.position += m_moveDirection * (MoveSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!m_isFlying) return;

        if (other.TryGetComponent(out ITargetable target))
        {
            if (target == m_currentContext.Caster as ITargetable) return;

            if (m_effectsToApply != null)
            {
                foreach (var effect in m_effectsToApply)
                {
                    effect.Apply(m_currentContext, target);
                }
            }

            if (HitVfxPrefab != null && EffectPoolManager.Instance != null)
            {
                m_currentContext.TargetPosition = transform.position;
                EffectPoolManager.Instance.SpawnEffect(HitVfxPrefab, m_currentContext);
            }

            if (!IsPiercing)
            {
                ReleaseToPool();
            }
        }
    }

    public override void ReleaseToPool()
    {
        m_isFlying = false;
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
}