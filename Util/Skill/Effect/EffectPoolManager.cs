using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class EffectPoolManager : MonoBehaviour
{
    private static EffectPoolManager m_instance;
    public static EffectPoolManager Instance 
    {
        get
        {
            if (m_instance == null)
            {
                var obj = new GameObject();
                m_instance = obj.AddComponent<EffectPoolManager>();
                obj.name = m_instance.GetType().Name;
            }
            return m_instance;
        }
    }

    private Dictionary<SkillEffectObject, IObjectPool<SkillEffectObject>> m_pools = new();

    private void Awake()
    {
        if (m_instance == null) m_instance = this;
        else Destroy(gameObject);
    }

    public SkillEffectObject SpawnEffect(SkillEffectObject prefab, SkillContext context)
    {
        if (prefab == null) return null;

        if (!m_pools.TryGetValue(prefab, out var pool))
        {
            pool = CreateNewPool(prefab);
            m_pools.Add(prefab, pool);
        }

        SkillEffectObject effect = pool.Get();
        effect.PlayEffect(context);

        return effect;
    }

    private IObjectPool<SkillEffectObject> CreateNewPool(SkillEffectObject prefab)
    {
        IObjectPool<SkillEffectObject> newPool = null;

        newPool = new ObjectPool<SkillEffectObject>(
            createFunc: () =>
            {
                GameObject obj = Instantiate(prefab.gameObject, transform);

                SkillEffectObject effectObj = obj.GetComponent<SkillEffectObject>();
                if (effectObj == null)
                {
                    Debug.LogError($"{prefab.gameObject.name}에 ISkillEffectObject를 구현한 컴포넌트가 없습니다!");
                }

                effectObj.SetPool(newPool);
                return effectObj;
            },
            actionOnGet: (effect) => ((MonoBehaviour)effect).gameObject.SetActive(true),
            actionOnRelease: (effect) => ((MonoBehaviour)effect).gameObject.SetActive(false),
            actionOnDestroy: (effect) => Destroy(((MonoBehaviour)effect).gameObject),
            collectionCheck: false,
            defaultCapacity: 10,
            maxSize: 50
        );

        return newPool;
    }
}
