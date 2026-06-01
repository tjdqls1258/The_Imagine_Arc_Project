using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// HPController를 상속받아 체력(HP)과 마나(MP)를 동시에 관리하는 컨트롤러입니다.
/// 마나 게이지 UI 업데이트 및 수치 제어 기능이 추가되었습니다.
/// </summary>
public class MpHpController : HPController
{
    // ====== Inspector Settings ======

    [Header("Mp Setting")]
    [Tooltip("마나 수치를 시각적으로 표시할 UI 이미지 (Fill Amount 방식)")]
    [SerializeField] protected Image m_mpBar;

    [Tooltip("최대 마나 수치")]
    [SerializeField] protected float m_maxMP;

    /// <summary> 현재 마나 내부 변수 </summary>
    protected float m_currentMp;

    // ====== Properties ======

    /// <summary>
    /// 현재 마나에 접근하거나 설정하는 프로퍼티입니다.
    /// 설정 시 자동으로 MP Bar의 fillAmount를 갱신합니다.
    /// </summary>
    public float currentMp
    {
        private set
        {
            // 마나 값 업데이트
            m_currentMp = value;

            // UI 갱신: 0으로 나누기 방지 및 게이지 반영
            if (m_mpBar != null)
            {
                if (m_maxMP == 0)
                    m_mpBar.fillAmount = 0;
                else
                    // 현재마나/최대마나 비율을 계산하여 이미지를 채움 (최대 1로 제한)
                    m_mpBar.fillAmount = Math.Min(m_currentMp / m_maxMP, 1);
            }
        }
        get
        {
            return m_currentMp;
        }
    }

    // ----------------------------------------------------------------------
    // ## Public Methods (Overrides & Extensions)
    // ----------------------------------------------------------------------

    /// <summary>
    /// 캐릭터의 상태 데이터를 기반으로 HP와 MP 컨트롤러를 초기화합니다.
    /// </summary>
    /// <param name="characterState">캐릭터 상태 데이터 (MP 정보가 포함된 MpCharacterState여야 함)</param>
    /// <param name="dieAction">사망 시 실행할 함수</param>
    public override void InitController(CharacterState characterState, Action dieAction, ConditionBuffeManager manager)
    {
        base.InitController(characterState, dieAction, manager);

        var mpCharacter = characterState as CharacterState;
    }

    /// <summary>
    /// 마나 수치를 변화시킵니다.
    /// </summary>
    /// <param name="addMp">변화량 (사용 시 음수, 회복 시 양수)</param>
    public void UpDateMp(float addMp)
    {
        // 마나 합산 (프로퍼티 세터를 통해 UI 자동 업데이트)
        currentMp = currentMp + addMp;
    }
}