using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Diagnostics;

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

        m_btn.onClick.AddListener(PlaySound);
    }

    private void PlaySound()
    {
        if (useClickSound == false) return;

        SoundManager.Instance.Play(clickSound, SoundType.EFFECT).Forget();
    }

#if UNITY_EDITOR
    public void SetButton()
    {
        m_btn = GetComponent<Button>();
        clickSound = SoundPath.ClickSound;
    }
#endif
}
