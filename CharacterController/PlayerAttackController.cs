using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class PlayerAttackController : MonoBehaviour, IGamePlayCharacter
{
    private readonly string effectName = "EffectPrefabs/Hit_FX01.prefab";

    [Header("Attack Settings")]
    [SerializeField] private float m_attackDistance = 5f;
    [SerializeField] private float m_attackDelay = 1f;

    public float AttackDelay => m_attackDelay;

    private List<EnemyController> m_enemyList = new();
    private EnemyController m_target;
    private float lastSkillTime;

    [Header("Component & Object References")]
    [SerializeField] private GameObject m_atkRangeObject;
    private InGameCharacterData m_characterData;
    private CharacterAnimationController m_characterAnimationController;
    private Transform m_modelTransform;

    [SerializeField] private HPController m_pHpController;
    
    private void SetAtkDistance()
    {
        CircleCollider2D collider = GetComponent<CircleCollider2D>();
        if (collider != null)
        {
            collider.isTrigger = true;
            collider.radius = m_characterData.characterData.characterState.atkRang;
            if (m_atkRangeObject != null)
                m_atkRangeObject.transform.localScale = Vector3.one * m_characterData.characterData.characterState.atkRang * 2;
        }
    }

    public void InitCharacterData(InGameCharacterData characterData, CharacterAnimationController animator)
    {
        lastSkillTime = 0;
        m_characterData = characterData;
        SetEffect().Forget();
        m_pHpController.InitController(characterData.characterData.characterState, DieAction_PlayerAction);

        if (animator != null)
            m_characterAnimationController = animator;

        m_characterAnimationController.SetAction(AtkAction, null, DieAction, null);
        m_modelTransform = m_characterAnimationController.transform;

        SetAtkDistance();
    }

    private async UniTask SetEffect()
    {
        if (GameMaster.Instance.objectPoolManager.CheckAddKey(effectName)) return;
        GameMaster.Instance.objectPoolManager.AddKey(effectName);
        var effectObject = await GameMaster.Instance.addressableManager.InstantiateObjectAsync(effectName);
        GameMaster.Instance.objectPoolManager.SetPoolObject(effectName, effectObject);
    }

    public void UpdateTargeting()
    {
        if (!GetCurrentTarget())
        {
            m_enemyList.RemoveAll(e => e.isDie);
            m_target = m_enemyList.Count > 0
                ? m_enemyList.OrderBy(e => Vector2.Distance(e.transform.position, transform.position)).FirstOrDefault()
                : null;
        }
    }

    public void LookAtTarget()
    {
        if (!GetCurrentTarget()) return;
        m_modelTransform.localScale = m_target.transform.position.x > transform.position.x ? Util.REVERSE_2D : Vector3.one;
    }

    protected void OnTriggerEnter2D(Collider2D collision)
    {
        EnemyController enemy = collision.gameObject.GetComponent<EnemyController>();
        if (enemy != null && !m_enemyList.Contains(enemy)) m_enemyList.Add(enemy);
    }

    protected void OnTriggerExit2D(Collider2D collision)
    {
        EnemyController enemy = collision.gameObject.GetComponent<EnemyController>();
        if (enemy != null)
        {
            m_enemyList.Remove(enemy);
            if (enemy == m_target) m_target = null;
        }
    }

    private void AtkAction()
    {
        if (!GetCurrentTarget()) return;
        m_target.Hit(m_characterData.GetAtk());
        var effect = GameMaster.Instance.objectPoolManager.AddPoolObject(effectName);
        if (effect != null) effect.transform.position = m_target.transform.position;
    }

    public virtual void Hit(int atk) => m_pHpController.UpdateHp(-atk);
    public GameObject GetAtkRangeObject() => m_atkRangeObject;

    private void OnDestroy()
    {
        if (GameMaster.Instance.addressableManager != null)
            GameMaster.Instance.objectPoolManager.RemovePoolObject(effectName);
    }

    protected virtual void DieAction_PlayerAction() { }
    public virtual void DieAction() => DieAction_PlayerAction();
    public void Upgrade() { m_characterData.UpgradeCharacter(1); m_pHpController.UpgradeCharacter(1); }

    public bool UseSkill(SkillContext skillContext) => m_characterData.activeSkill.ExecuteActive(skillContext);
    public float SkillLastTime() => lastSkillTime;
    public IGamePlayCharacter GetSelf() => this;
    public bool GetCurrentTarget() => m_target != null && !m_target.isDie;

#if UNITY_EDITOR
    [ContextMenu("Setting HP Component")]
    public void SettingHP()
    {
        m_pHpController = GetComponent<HPController>();
        if (m_pHpController == null) return;
        UnityEditor.EditorUtility.SetDirty(this);
    }
#endif
}