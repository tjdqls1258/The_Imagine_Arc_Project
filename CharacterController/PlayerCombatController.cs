using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;


#if UNITY_EDITOR
using Unity.VisualScripting;
#endif

[RequireComponent(typeof(CircleCollider2D))]
public class PlayerCombatController : MonoBehaviour,  ISkillCaster
{
    private readonly string effectName = "EffectPrefabs/Hit_FX01.prefab";

    [Header("Attack Settings")]
    [SerializeField] private float m_attackDelay = 1f;
    public float AttackDelay => m_attackDelay;

    [Header("Component References")]
    [SerializeField] private GameObject m_atkRangeObject;
    [SerializeField] private HPController m_pHpController;
    public ConditionBuffeManager ConditionManager { get; private set; }

    private List<EnemyController> m_enemyList = new();
    private CharacterAnimationController m_animController;
    private InGameCharacterData m_inGameData;
    private SkillContext m_nomalAtkContext;
    private UnityAction m_dieCallback;

    private bool isDie = false;

    private void Awake()
    {
        ConditionManager = GetComponent<ConditionBuffeManager>();
    }

    public void InitCombat(InGameCharacterData data, CharacterAnimationController anim, UnityAction dieCallback)
    {
        isDie = false;
        m_inGameData = data;
        m_animController = anim;
        m_dieCallback = dieCallback;

        ConditionManager.SetCharacterStat(m_inGameData);
        m_pHpController.InitController(m_inGameData.characterData.characterState, DieAction, ConditionManager);

        m_animController?.SetAction(AtkAction, null, DieAction, null);

        m_nomalAtkContext = new SkillContext { Caster = this, SkillRange = ConditionManager.GetStat(StatType.AttackRange), Condition = ConditionManager };

        SetAtkDistance();
        SetEffect().Forget();
    }

    private void SetAtkDistance()
    {
        if (TryGetComponent(out CircleCollider2D collider))
        {
            collider.isTrigger = true;
            collider.radius = ConditionManager.GetStat(StatType.AttackRange);
            if (m_atkRangeObject != null)
                m_atkRangeObject.transform.localScale = Vector3.one * collider.radius * 2;
        }
    }

    private async UniTask SetEffect()
    {
        if (GameMaster.Instance.objectPoolManager.CheckAddKey(effectName)) return;
        GameMaster.Instance.objectPoolManager.AddKey(effectName);
        var effect = await GameMaster.Instance.addressableManager.InstantiateObjectAsync(effectName);
        GameMaster.Instance.objectPoolManager.SetPoolObject(effectName, effect);
    }

    public void UpdateTargeting()
    {
        if (!GetCurrentTarget())
        {
            m_enemyList.RemoveAll(e => e.StateManager.IsDie());
            m_nomalAtkContext.PrimaryTarget = m_enemyList.Count > 0
                ? m_enemyList.OrderBy(e => Vector2.Distance(e.transform.position, transform.position)).FirstOrDefault()?.GetTarget()
                : null;
        }
    }

    public void LookAtTarget()
    {
        if (!GetCurrentTarget()) return;
        m_animController.transform.localScale = m_nomalAtkContext.PrimaryTarget.GetTransform().position.x > transform.position.x
            ? Util.REVERSE_2D : Vector3.one;
    }

    private void AtkAction()
    {
        if (!GetCurrentTarget()) return;
        m_nomalAtkContext.Damage = ConditionManager.GetStat(StatType.AttackDamage);
        m_inGameData.nomalAtkSkill.ExecuteActive(m_nomalAtkContext);

        var effect = GameMaster.Instance.objectPoolManager.AddPoolObject(effectName);
        if (effect != null) effect.transform.position = m_nomalAtkContext.PrimaryTarget.GetTransform().position;
    }

    protected void OnTriggerEnter2D(Collider2D col)
    {
        if (col.TryGetComponent(out EnemyController enemy) && !m_enemyList.Contains(enemy))
            m_enemyList.Add(enemy);
    }

    protected void OnTriggerExit2D(Collider2D col)
    {
        if (col.TryGetComponent(out EnemyController enemy))
        {
            m_enemyList.Remove(enemy);
            if (enemy.GetTarget() == m_nomalAtkContext.PrimaryTarget) m_nomalAtkContext.PrimaryTarget = null;
        }
    }

    public void Hit(float atk) => m_pHpController.UpdateHp(-atk);
    public void ApplyEffect(EffectPayload payload) => Hit(payload.Value);

    public void Upgrade()
    {
        m_inGameData.UpgradeCharacter();
        m_pHpController.UpgradeCharacter(ConditionManager.GetStat(StatType.MaxHp));
    }

    public void DieAction()
    {
        isDie = true;
        m_dieCallback?.Invoke();
    }

    // --- Interfaces & Getters ---
    public GameObject GetAtkRangeObject() => m_atkRangeObject;
    public float SkillLastTime() => 0f; // 기존 로직 반영
    public Transform GetTransform() => transform;
    public int GetCasterID() => GetInstanceID();
    public bool GetCurrentTarget() => m_nomalAtkContext.PrimaryTarget != null && !m_nomalAtkContext.PrimaryTarget.IsDie();


    private void OnDestroy()
    {
        if (GameMaster.Instance.addressableManager != null)
            GameMaster.Instance.objectPoolManager.RemovePoolObject(effectName);
    }
#if UNITY_EDITOR
    [ContextMenu("Setting HP Component")]
    public void SettingHP()
    {
        m_pHpController = GetComponent<HPController>(); 
        if (m_pHpController == null) return;
        UnityEditor.EditorUtility.SetDirty(this);
    }

    [ContextMenu("Setting ConditionManager Component")]
    public void SettingCondition()
    {
        ConditionManager = GetComponent<ConditionBuffeManager>();
        if(ConditionManager == null)
            ConditionManager = this.AddComponent<ConditionBuffeManager>();

        if (ConditionManager == null) return;
        UnityEditor.EditorUtility.SetDirty(this);
    }

    [ContextMenu("Setting atkRang")]
    public void SettingAtkRangObject()
    {
        for(int i =0; i< transform.childCount; i++)
        {
            if(transform.GetChild(i).name.Contains("AtkRange"))
            {
                m_atkRangeObject = transform.GetChild(i).gameObject;
                break;
            }
        }
        if (m_atkRangeObject == null) return;
        UnityEditor.EditorUtility.SetDirty(this);
    }



#endif
}