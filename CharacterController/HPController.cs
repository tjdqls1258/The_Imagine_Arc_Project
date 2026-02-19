using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 캐릭터의 체력(HP)을 관리하고 관련 UI(HP Bar)를 제어하는 컨트롤러입니다.
/// </summary>
public class HPController : MonoBehaviour
{
    // ====== Inspector Settings ======

    [Header("Hp Setting")]
    [Tooltip("체력 수치를 시각적으로 표시할 UI 이미지 (Fill Amount 방식)")]
    [SerializeField] protected Image m_hpBar;

    [Tooltip("최대 체력 수치")]
    [SerializeField] protected float m_maxHP;

    /// <summary> 현재 체력 내부 변수 </summary>
    protected float m_currentHp = 0;

    /// <summary> 체력이 0이 되었을 때 호출될 콜백 액션 </summary>
    protected Action m_dieAction;

    // ====== Properties ======

    /// <summary>
    /// 현재 체력에 접근하거나 설정하는 프로퍼티입니다.
    /// 설정 시 자동으로 HP Bar의 fillAmount를 갱신합니다.
    /// </summary>
    public float currentHp
    {
        protected set
        {
            // 체력 값 업데이트
            m_currentHp = Mathf.Clamp(value, 0, m_maxHP);

            // UI 갱신: 현재체력/최대체력 비율을 계산하여 이미지를 채움 (최대 1로 제한)
            if (m_hpBar != null && m_maxHP > 0)
            {
                m_hpBar.fillAmount = Math.Min(m_currentHp / m_maxHP, 1);
            }
        }
        get
        {
            return m_currentHp;
        }
    }

    // ----------------------------------------------------------------------
    // ## Public Methods
    // ----------------------------------------------------------------------

    /// <summary>
    /// 캐릭터의 상태 데이터를 기반으로 컨트롤러를 초기화합니다.
    /// </summary>
    /// <param name="characterStateData">최대 HP 정보가 담긴 데이터 객체</param>
    /// <param name="dieAction">사망 시 실행할 함수</param>
    public virtual void InitController(CharacterState characterStateData, Action dieAction)
    {
        m_maxHP = characterStateData.maxHp;

        // 프로퍼티를 통해 현재 체력을 최대치로 설정하고 UI를 갱신
        currentHp = characterStateData.maxHp;

        m_dieAction = dieAction;
    }

    /// <summary>
    /// 체력을 변화시키고 사망 여부를 체크합니다.
    /// </summary>
    /// <param name="addHp">변화량 (데미지일 경우 음수, 회복일 경우 양수)</param>
    public void UpdateHp(float addHp)
    {
        Logger.Log($"AddHp {currentHp} + {addHp}");

        // 체력 합산 (프로퍼티 세터를 통해 UI 자동 업데이트)
        currentHp = currentHp + addHp;

        // 사망 판정: 체력이 0 이하로 떨어지면 등록된 사망 액션 실행
        if (currentHp <= 0 && m_dieAction != null)
        {
            m_dieAction.Invoke();
        }
    }

    public virtual void UpgradeCharacter(int count)
    {
        float addValue = m_maxHP * (0.1f * count);
        m_maxHP = m_maxHP + addValue;
        currentHp = currentHp + addValue;
    }
}