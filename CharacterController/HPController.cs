using System;
using UnityEngine;
using UnityEngine.UI;

public class HPController : MonoBehaviour
{
    [Header("Hp Setting")]
    [Tooltip("체력 수치를 시각적으로 표시할 UI 이미지 (Fill Amount 방식)")]
    [SerializeField] protected Image m_hpBar;

    [Tooltip("최대 체력 수치")]
    [SerializeField] protected float m_maxHP;
    protected float m_currentHp = 0;

    protected Action m_dieAction;

    public float currentHp
    {
        protected set
        {
            m_currentHp = Mathf.Clamp(value, 0, m_maxHP);

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

    public virtual void InitController(CharacterState characterStateData, Action dieAction)
    {
        m_maxHP = characterStateData.maxHp;

        currentHp = characterStateData.maxHp;

        m_dieAction = dieAction;
    }

    public void UpdateHp(float addHp)
    {
        Logger.Log($"AddHp {currentHp} + {addHp}");

        currentHp = currentHp + addHp;

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

#if UNITY_EDITOR
    [ContextMenu("Set HpBar Image (name : CurrentHPImage)")]
    public void SettingImage()
    {
        foreach(var obj in GetComponentsInChildren<Image>())
        {
            if(obj.name == "CurrentHPImage")
                m_hpBar = obj;
        }

        UnityEditor.EditorUtility.SetDirty(this);
    }

    [ContextMenu("Remove MpBar Image (name : MpBar)")]
    public void RemoveMpBar()
    {
        GameObject getTarget = null;
        foreach(var obj in GetComponentsInChildren<RectTransform>())
        {
            if (obj.name == "MPBar")
            {
                getTarget = obj.gameObject;
                break;
            }
        }

        if (getTarget != null)
        {
            UnityEditor.Undo.DestroyObjectImmediate(getTarget);

            UnityEditor.EditorUtility.SetDirty(this);
        }
    }
#endif
}