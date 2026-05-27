using UnityEngine;
using UnityEngine.Pool;

public abstract class SkillEffectObject : MonoBehaviour
{
    public float SizeRatio = 1f;
    public abstract void SetPool(IObjectPool<SkillEffectObject> pool);

    public abstract void PlayEffect(SkillContext context);

    public abstract void ReleaseToPool();

    public virtual void SettingSize(SkillContext context)
    {
        var size = context.SkillRange * SizeRatio;
        gameObject.transform.localScale = new Vector3(size, size, size);
    }
}
