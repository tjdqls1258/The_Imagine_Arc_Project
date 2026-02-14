using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 플레이어 캐릭터의 전체적인 상태와 컴포넌트(공격, 애니메이션, 스킬)를 총괄하는 메인 컨트롤러입니다.
/// 캐릭터 스폰 상태 관리 및 사용자 인터랙션(클릭 등)에 따른 시각적 피드백을 제어합니다.
/// </summary>
public class PlayerCharacterContrroller : MonoBehaviour
{
    // ====== Runtime Components & Data ======

    /// <summary> 캐릭터가 보유한 스킬 리스트 </summary>
    private List<SkillBase> m_currentSkill;

    /// <summary> 캐릭터의 인게임 스탯 및 설정 데이터 </summary>
    private InGameCharacterData m_characterData;

    /// <summary> 전투 및 타겟팅 로직을 담당하는 컴포넌트 </summary>
    private PlayerAttackController m_atkController;

    /// <summary> 애니메이션 재생 및 이벤트를 관리하는 컴포넌트 </summary>
    private CharacterAnimationController m_characterAniumationController;

    // ====== State & Events ======

    /// <summary> 유닛이 사망했을 때 호출될 유니티 이벤트 액션 </summary>
    private UnityAction m_unitDieAction;

    /// <summary> 마우스/터치로 캐릭터를 클릭 중인지 여부 </summary>
    private bool m_onClick = false;

    /// <summary> 현재 맵에 정식으로 배치(스폰)되어 작동 중인지 여부 </summary>
    private bool m_isSpawn = false;

    // ----------------------------------------------------------------------
    // ## Initialization
    // ----------------------------------------------------------------------

    private void Awake()
    {
        // 내부 컴포넌트 캐싱
        m_atkController = GetComponent<PlayerAttackController>();
        m_characterAniumationController = GetComponent<CharacterAnimationController>();
    }

    /// <summary>
    /// 캐릭터 데이터를 주입하고 하위 컴포넌트들을 초기화합니다.
    /// </summary>
    /// <param name="characterData">주입할 인게임 캐릭터 데이터</param>
    public void SetCharacter(InGameCharacterData characterData)
    {
        m_characterData = characterData;

        // 공격 컨트롤러 초기화 (데이터 및 애니메이터 전달)
        m_atkController.InitCharacterData(m_characterData, m_characterAniumationController);

        // TODO: 캐릭터 데이터에 정의된 스킬들을 로드하고 세팅하는 로직 추가 예정
    }

    /// <summary>
    /// 캐릭터의 스킬 시스템을 초기화합니다.
    /// </summary>
    public void SetSkill()
    {
        // 스킬 생성 및 리스트 관리 로직 구현부
    }

    // ----------------------------------------------------------------------
    // ## Public Interface
    // ----------------------------------------------------------------------

    /// <summary> 현재 캐릭터의 데이터를 반환합니다. </summary>
    public InGameCharacterData GetCharacterData() => m_characterData;

    /// <summary> 사망 시 호출될 외부 액션을 구독합니다. </summary>
    public void AddDieAction(UnityAction action)
    {
        m_unitDieAction += action;
    }

    /// <summary> 현재 배치(스폰) 상태를 반환합니다. </summary>
    public bool CheckSpawn() => m_isSpawn;

    // ----------------------------------------------------------------------
    // ## Interaction & Feedback (UI/UX)
    // ----------------------------------------------------------------------

    /// <summary>
    /// 마우스 클릭/터치가 시작되었을 때 호출되어 공격 사거리를 표시합니다.
    /// </summary>
    public void OnPointerDownAction()
    {
        m_onClick = true;
        AtkAreaActive(m_onClick);
    }

    /// <summary>
    /// 마우스 클릭/터치가 떼어졌을 때 호출되어 공격 사거리를 숨깁니다.
    /// </summary>
    public void OnPointerUpAction()
    {
        m_onClick = false;
        AtkAreaActive(m_onClick);
    }

    /// <summary>
    /// 공격 사거리 시각화 오브젝트를 활성화 또는 비활성화합니다.
    /// </summary>
    /// <param name="Active">활성화 여부</param>
    public void AtkAreaActive(bool Active)
    {
        // 공격 컨트롤러에 저장된 Range Object를 제어
        m_atkController.GetAtkRangeObject().SetActive(Active);
    }

    // ----------------------------------------------------------------------
    // ## Lifecycle Control
    // ----------------------------------------------------------------------

    /// <summary>
    /// 캐릭터를 활성화하여 실제 전투 로직(공격 등)이 작동하도록 설정합니다.
    /// </summary>
    /// <param name="isSpawn">true 시 전투 시작, false 시 대기 상태</param>
    public void SetSpawn(bool isSpawn)
    {
        // 드래그 중에는 로직이 돌지 않도록 공격 컴포넌트 자체를 On/Off 함
        m_atkController.enabled = isSpawn;
        m_isSpawn = isSpawn;
    }
}