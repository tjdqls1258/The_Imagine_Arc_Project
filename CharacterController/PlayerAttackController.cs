using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 플레이어 캐릭터의 공격 로직 및 전투 상태를 제어하는 컨트롤러입니다.
/// 사거리 내 적 감지, 타겟팅 최적화, 애니메이션 기반 공격 실행 및 HP 관리를 담당합니다.
/// </summary>
[RequireComponent(typeof(CircleCollider2D))]
public class PlayerAttackController : MonoBehaviour, IGamePlayCharacter
{
    // ====== Constants & Settings ======
    private readonly string effectName = "EffectPrefabs/Hit_FX01.prefab"; // 임시 공격 시 생성될 타격 이펙트 경로

    [Header("Attack Settings")]
    [Tooltip("공격 사거리 (CircleCollider2D의 반지름으로 사용됨).")]
    [SerializeField] private float m_attackDistance = 5f;

    [Tooltip("공격 사이의 지연 시간 (초).")]
    [SerializeField] private float m_attackDelay = 1f;

    // ====== Runtime State & Caches ======

    /// <summary> 사거리 내에 감지된 적 리스트 </summary>
    private List<EnemyController> m_enemyList = new();

    /// <summary> 현재 공격 대상으로 선정된 적 </summary>
    private EnemyController m_target;

    /// <summary> 공격 쿨타임 계산용 타이머 </summary>
    private float m_currentDelay = 0;

    private float lastSkillTime;

    [Header("Component & Object References")]
    [SerializeField] private GameObject m_atkRangeObject; // 사거리를 시각적으로 표시할 오브젝트 (범위 표시용)
    private InGameCharacterData m_characterData;         // 캐릭터 고유 데이터 데이터
    private CharacterAnimationController m_characterAnimationController; // 애니메이션 제어기
    private Transform m_modelTransform;

    [SerializeField] private MpHpController m_pHpController; // 체력 및 마나 컨트롤러

    // ----------------------------------------------------------------------
    // ## Initialization
    // ----------------------------------------------------------------------

    private void Awake()
    {
        // 1. 트리거 콜라이더를 통한 사거리 설정
        CircleCollider2D collider = GetComponent<CircleCollider2D>();
        if (collider != null)
        {
            collider.isTrigger = true;
            collider.radius = m_attackDistance;

            // 사거리 가이드 오브젝트 크기를 반지름에 맞춰 조절 (직경이므로 *2)
            if (m_atkRangeObject != null)
                m_atkRangeObject.transform.localScale = Vector3.one * m_attackDistance * 2;
        }
    }

    /// <summary>
    /// 캐릭터 데이터를 주입하고 전투에 필요한 컴포넌트들을 초기화합니다.
    /// </summary>
    public void InitCharacterData(InGameCharacterData characterData, CharacterAnimationController animator)
    {
        lastSkillTime = 0;
        m_characterData = characterData;

        // 이펙트 프리팹을 오브젝트 풀에 미리 등록 (비동기)
        SetEffect().Forget();

        // 캐릭터 상태 수치(Stat) 설정 및 HP 컨트롤러 초기화
        m_pHpController.InitController(characterData.characterData.characterState, DieAction_PlayerAction);

        if (animator != null)
            m_characterAnimationController = animator;

        // 애니메이션 컨트롤러에 실제 공격 로직(AtkAction)을 콜백으로 등록
        // 애니메이션의 '공격 시점' 이벤트 발생 시 AtkAction이 실행됩니다.
        m_characterAnimationController.SetAction(AtkAction, null, DieAction, null);

        m_modelTransform = m_characterAnimationController.transform;
    }

    /// <summary>
    /// 타격 이펙트를 어드레서블에서 로드하여 오브젝트 풀에 준비시킵니다.
    /// </summary>
    private async UniTask SetEffect()
    {
        if (GameMaster.Instance.objectPoolManager.CheckAddKey(effectName))
            return;

        GameMaster.Instance.objectPoolManager.AddKey(effectName);
        var effectObject = await GameMaster.Instance.addressableManager.InstantiateObjectAsync(effectName);
        GameMaster.Instance.objectPoolManager.SetPoolObject(effectName, effectObject);
    }

    // ----------------------------------------------------------------------
    // ## Update Logic (Targeting & FSM)
    // ----------------------------------------------------------------------

    private void Update()
    {
        // 1. 감지된 적이 없으면 대기 상태로 전환
        if (m_enemyList.Count <= 0)
        {
            if (m_currentDelay > 0.001f)
                m_currentDelay = 0;
            m_target = null;
            return;
        }

        // 2. 타겟팅 로직 수행
        SetTarget();

        // 3. 타겟이 존재하면 공격 프로세스(딜레이 체크) 실행
        if (m_target != null)
            CharacterAction(m_target);

        SeeTarget();
    }

    /// <summary>
    /// 현재 타겟의 유효성을 검사하고, 필요 시 가장 가까운 적을 새 타겟으로 선정합니다.
    /// </summary>
    protected virtual void SetTarget()
    {
        if (m_target == null || m_target.isDie)
        {
            // 죽은 적은 리스트에서 제거
            m_enemyList.RemoveAll(e => e.isDie);

            if (m_enemyList.Count > 0)
            {
                // LINQ를 사용하여 물리적 거리가 가장 가까운 적을 타겟으로 설정
                m_target = m_enemyList.OrderBy(e => Vector2.Distance(e.transform.position, transform.position)).FirstOrDefault();
            }
            else
            {
                m_target = null;
            }
        }
    }

    // ----------------------------------------------------------------------
    // ## Detection (Trigger Events)
    // ----------------------------------------------------------------------

    protected void OnTriggerEnter2D(Collider2D collision)
    {
        EnemyController enemy = collision.gameObject.GetComponent<EnemyController>();
        if (enemy != null && !m_enemyList.Contains(enemy))
        {
            m_enemyList.Add(enemy);
        }
    }

    protected void OnTriggerExit2D(Collider2D collision)
    {
        EnemyController enemy = collision.gameObject.GetComponent<EnemyController>();
        if (enemy != null)
        {
            m_enemyList.Remove(enemy);

            if (enemy == m_target)
                m_target = null;
        }
    }

    // ----------------------------------------------------------------------
    // ## Attack Execution (Animation Callback)
    // ----------------------------------------------------------------------

    /// <summary>
    /// [핵심] 애니메이션 이벤트에 의해 호출되는 실제 공격 함수입니다.
    /// 데미지 적용 및 이펙트 생성을 담당합니다.
    /// </summary>
    private void AtkAction()
    {
        if (m_target == null)
            return;

        // 1. 타겟에게 데미지 전달
        m_target.Hit(m_characterData.GetAtk());

        Logger.Log($"Action {m_target.gameObject.name}: ATTACK!");

        // 2. 타격 지점에 풀링된 이펙트 생성 및 위치 조정
        var effect = GameMaster.Instance.objectPoolManager.AddPoolObject(effectName);
        if (effect != null)
            effect.transform.position = m_target.transform.position;
    }

    /// <summary>
    /// 공격 애니메이션 재생을 위한 쿨타임 로직입니다.
    /// </summary>
    private void CharacterAction(EnemyController target)
    {
        if (m_currentDelay <= m_attackDelay)
        {
            m_currentDelay += Time.deltaTime;
        }
        else
        {
            // 쿨타임 완료: 공격 애니메이션 트리거 실행
            m_currentDelay = 0;
            m_characterAnimationController.PlayAnimation_Trigger(CharacterAnimationController.AnimationTrigger.ATK);
        }
    }

    // ----------------------------------------------------------------------
    // ## Combat Interaction & Lifecycle
    // ----------------------------------------------------------------------

    /// <summary>
    /// 적에게 피격되었을 때 호출됩니다.
    /// </summary>
    public virtual void Hit(int atk)
    {
        m_pHpController.UpdateHp(-atk);
    }

    public GameObject GetAtkRangeObject() => m_atkRangeObject;

    private void OnDestroy()
    {
        // 객체 파괴 시 등록된 이펙트 풀 정보 제거
        if (GameMaster.Instance.addressableManager != null)
            GameMaster.Instance.objectPoolManager.RemovePoolObject(effectName);
    }

    /// <summary> 플레이어 전용 사망 로직 </summary>
    protected virtual void DieAction_PlayerAction() { }

    /// <summary> IGamePlayCharacter 인터페이스 구현: 사망 시 처리 </summary>
    public virtual void DieAction()
    {
        DieAction_PlayerAction();
    }

    public void Upgrade()
    {
        m_characterData.UpgradeCharacter(1);
        m_pHpController.UpgradeCharacter(1);
    }

    private void SeeTarget()
    {
        if (m_target == null) return;

        if (m_target.transform.position.x > transform.position.x)
            m_modelTransform.localScale = Util.REVERSE_2D;
        else
            m_modelTransform.localScale = Vector3.one;
    }

    public bool UseSkill()
    {
        return m_characterData.activeSkill.SkillClass.ActiveSkill(m_characterData, m_target);
    }

    public float SkillLastTime()
    {
        return lastSkillTime;
    }

    public IGamePlayCharacter GetSelf() => this;
}