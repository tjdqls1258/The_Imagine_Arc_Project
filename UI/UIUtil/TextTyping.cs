using Cysharp.Threading.Tasks;
using System.Threading;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class TextTyping : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI m_textMesh;
    private CancellationTokenSource m_cancelToken;
    private float delayTime = 0.05f;

    public string text
    {
        set
        {
            if (m_cancelToken != null)
                m_cancelToken.Cancel();

            TypingAnimation(value).Forget();
        }
    }

    private void Awake()
    {
        if(m_textMesh == null)
            m_textMesh = GetComponent<TextMeshProUGUI>();
        m_cancelToken = new();
    }

    private async UniTask TypingAnimation(string text)
    {
        if (m_textMesh == null) 
            return;

        if (m_cancelToken != null && m_cancelToken.IsCancellationRequested)
        {
            m_cancelToken = new();
        }

        m_textMesh.text = string.Empty;

        foreach (var item in text)
        {
            m_textMesh.text = m_textMesh.text + item;
            await UniTask.WaitForSeconds(delayTime, cancellationToken: destroyCancellationToken);

            if (m_cancelToken.IsCancellationRequested || destroyCancellationToken.IsCancellationRequested)
                return;
        }
    }

    private void OnDisable()
    {
        if (m_cancelToken != null)
        {
            m_cancelToken.Cancel();
            m_cancelToken.Dispose();
        }
    }
}
