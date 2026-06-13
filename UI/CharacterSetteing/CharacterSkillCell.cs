using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using static PopupManager;

public class CharacterSkillCell : MonoBehaviour
{
    private Button m_button;
    private Image m_skillIcon;
    private SkillBase m_skillBase;

    public async UniTask SetSkill(int skillID, AddressableManager addressableManager)
    {
        if (m_button == null)
        {
            m_button = GetComponent<Button>();

            m_button.OnClickAsObservable().Subscribe(_ =>
            {
                MessageBroker.Default.Publish(new ToolTipBoxEvent(m_skillBase));
            }).AddTo(this);
        }

        if (m_skillIcon == null)
            m_skillIcon = GetComponent<Image>();

        m_skillBase = await addressableManager.LoadAssetAndCacheAsync<SkillBase>(string.Format(Util.CHARACTER_SKILL_PATH, skillID));
        m_skillIcon.sprite = m_skillBase.SkillIcon;
    }
}
