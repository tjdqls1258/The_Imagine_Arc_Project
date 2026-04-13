using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSkillCell : MonoBehaviour
{
    private Button m_button;
    private Image m_skillIcon;
    private SkillBase m_skillBase;

    public async UniTask SetSkill(int skillID)
    {
        if (m_button == null)
        {
            m_button = GetComponent<Button>();
            m_button.onClick.AddListener(OnClickSkill);
        }

        if (m_skillIcon == null)
            m_skillIcon = GetComponent<Image>();

        m_skillBase = await GameMaster.Instance.addressableManager.LoadAssetAndCacheAsync<SkillBase>(string.Format(Util.CHARACTER_SKILL_PATH, skillID));
        m_skillIcon.sprite = m_skillBase.SkillIcon;
    }

    private void OnClickSkill()
    {
        GameMaster.Instance.popupManager.ShowToolTipPopup(m_skillBase);
        Logger.Log($"{m_skillBase.SkillDescription}");
    }
}
