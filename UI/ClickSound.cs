using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class ClickSound : MonoBehaviour
{
    public SoundPath clickSound;
    public Button m_btn;
    [SerializeField] private bool useClickSound = true;

    public void Awake()
    {
        if (useClickSound == false) return;
        if (m_btn == null)
            m_btn = GetComponent<Button>();

        m_btn.OnClickAsObservable().Subscribe(_ =>
        {
            MessageBroker.Default.Publish(new SoundManager.PlaySoundEvent(clickSound));
        }).AddTo(this);
    }

#if UNITY_EDITOR
    public void SetButton()
    {
        m_btn = GetComponent<Button>();
        clickSound = SoundPath.ClickSound;
    }
#endif
}
