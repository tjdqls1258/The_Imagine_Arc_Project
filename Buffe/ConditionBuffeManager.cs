using System.Collections.Generic;
using UnityEngine;
using Util_Patten.FSM;

public class ConditionBuffeManager : MonoBehaviour
{
    private BaseCharacterStat m_baseStat;

    // 현재 걸려있는 모든 버프/디버프 리스트
    private List<ActiveCondition> m_activeConditions = new List<ActiveCondition>();

    public void SetCharacterStat(BaseCharacterStat stat)
    {
        m_baseStat = stat;
    }

    public void ApplyCondition(ConditionBuffeSO conditionSO, float value)
    {
        ActiveCondition existing = m_activeConditions.Find(c => c.Data.ConditionID == conditionSO.ConditionID);
        
        if (existing != null)
        {
            existing.AddStack();
        }
        else
        {
            m_activeConditions.Add(new ActiveCondition(conditionSO, value));
        }
    }

    private void Update()
    {
        if (m_activeConditions.Count == 0) return;

        ITargetable self = GetComponent<ITargetable>();
        float dt = Time.deltaTime;

        for (int i = m_activeConditions.Count - 1; i >= 0; i--)
        {
            var cond = m_activeConditions[i];

            cond.UpdateCondition(self, dt);

            if (cond.Duration <= 0)
            {
                m_activeConditions.RemoveAt(i);
            }
        }
    }

    public float GetStat(StatType statType)
    {
        float finalValue = m_baseStat.GetStat(statType);
        float flatBonus = 0f;
        float percentBonus = 0f;

        foreach (var cond in m_activeConditions)
        {
            flatBonus += cond.GetFlatModifier(statType);
            percentBonus += cond.GetPercentModifier(statType);
        }

        finalValue = (finalValue + flatBonus) * (1f + percentBonus);

        return finalValue;
    }
}
