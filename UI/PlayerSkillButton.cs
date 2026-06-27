using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerSkillButton : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private AddressableManager addressableManager;
    private UserSkillBase m_userSkill;
    private SkillContext m_userSkillContext;

    [Header("UI References")]
    [SerializeField] private Image m_skillImage;
    [SerializeField] private TextMeshProUGUI m_costText;
    [SerializeField] private TextMeshProUGUI m_coolTimeText;
    [SerializeField] private GameObject m_blockImage;

    private float m_beforeTimeScale = 1f;
    private float m_beforeFixedDeltaTime = 0.02f;
    private float SkillReadyTime;

    private bool HasEnoughCost = false;

    public Camera MainCamera => GameUtil.mainCamera;

    public LayerMask TileMask => 1 << 8;

    public void SetSkill(UserSkillBase skillData, SkillContext userSkillContext,AddressableManager addressableManager = null)
    {
        if (skillData == null)
        {
            gameObject.SetActive(false);
            return;
        }
        else
            gameObject.SetActive(true);

        this.addressableManager = addressableManager;
        m_userSkill = skillData;
        m_userSkillContext = userSkillContext;
        m_skillImage.sprite = skillData.SkillIcon;
        m_costText.text = skillData.GetCost().ToString();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (CanUseSkill() == false) return;
        
        SkillDragEnter(true);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (CanUseSkill() == false) return;
        
        UpdateSkillContextPosition(m_userSkillContext, eventData.position);
        m_userSkill.UpdateAiming(m_userSkillContext);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (CanUseSkill() == false) return;

        UpdateSkillContextPosition(m_userSkillContext, eventData.position);
        if (m_userSkill.EndAimingAndExecute(m_userSkillContext))
            SkillReadyTime = Time.time + m_userSkill.Cooldown;

        SkillDragEnter(false);
    }

    private void SkillDragEnter(bool isEnter)
    {
        if (isEnter)
        {
            m_beforeTimeScale = Time.timeScale;
            m_beforeFixedDeltaTime = Time.fixedDeltaTime;

            Time.timeScale = 0.1f;
            Time.fixedDeltaTime = m_beforeFixedDeltaTime * Time.timeScale;

            m_userSkill.BeginAiming(m_userSkillContext);
        }
        else
        {
            Time.timeScale = m_beforeTimeScale;
            Time.fixedDeltaTime = m_beforeFixedDeltaTime;

            m_userSkill.CancelAiming(m_userSkillContext);
        }
    }

    private void UpdateSkillContextPosition(SkillContext context, Vector2 screenPosition)
    {
        if (context == null) return;

        Vector3 worldPos = MainCamera.ScreenToWorldPoint(screenPosition);
        worldPos.z = 0;

        context.TargetPosition = worldPos;
    }

    private void Update()
    {
        if (m_userSkillContext == null) return;
        if (CanUseSkill() == false)
        {
            SetBlockState(true);
            UpdateSkillCoolTimeText();
        }
        else
        {
            SetBlockState(false);
        }
    }

    private void SetBlockState(bool isBlocked)
    {
        if(m_blockImage.activeSelf != isBlocked)
            m_blockImage.SetActive(isBlocked);
    }

    private void UpdateSkillCoolTimeText()
    {
        if(SkillReadyTime - Time.time <= 0)
        {
            m_coolTimeText.text = string.Empty;
            return;
        }

        m_coolTimeText.text = (SkillReadyTime - Time.time).ToString("N1");
    }

    public void SubscribeCost(ReactiveProperty<int> reactiveProperty)
    {
        reactiveProperty.Subscribe(UpdateCostAction).AddTo(this);
    }

    private void UpdateCostAction(int currentCost)
    {
        if (m_userSkill == null) return;

        HasEnoughCost = currentCost >= m_userSkill.GetCost();
    }

    private bool CanUseSkill()
    {
        if (m_userSkill == null) return false;
        if (m_userSkillContext == null) return false;
        return HasEnoughCost && SkillReadyTime <= Time.time;
    }
}