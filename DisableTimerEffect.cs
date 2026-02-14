using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 파티클 이펙트의 수명 주기를 관리합니다.
/// 재생이 완료되면 자동으로 오브젝트를 비활성화하여 오브젝트 풀링 효율을 높입니다.
/// </summary>
public class DisableTimerEffect : MonoBehaviour
{
    [Tooltip("수동 비활성화 모드 사용 시 대기 시간 (초)")]
    [SerializeField] private float m_disableTime = 2f;

    [Tooltip("유니티 자체 Stop Action(Disable) 기능을 사용할지 여부")]
    [SerializeField] private bool m_useUnityStopAction = true;

    private ParticleSystem[] particleSystems;
    private void Awake()
    {
        particleSystems = transform.GetComponentsInChildren<ParticleSystem>();

        if(particleSystems != null )
        {
            foreach(ParticleSystem p in particleSystems)
            {
                var main = p.main;
                // 파티클 재생 종료 시 해당 오브젝트를 자동으로 Disable 하도록 설정
                main.stopAction = ParticleSystemStopAction.Disable;
            }
        }
    }

    private void OnEnable()
    {
        // 오브젝트가 활성화될 때마다 실행 (오브젝트 풀링 대응)
        if (!m_useUnityStopAction)
        {
            StartDisableTimer().Forget();
        }
    }

    /// <summary>
    /// 지정된 시간이 지나면 오브젝트를 비활성화하는 수동 타이머입니다.
    /// (파티클이 Looping이거나 Stop Action이 작동하지 않는 특수 상황용)
    /// </summary>
    private async UniTaskVoid StartDisableTimer()
    {
        await UniTask.Delay((int)(m_disableTime * 1000));

        // 아직 오브젝트가 살아있는지 확인 후 비활성화
        if (this != null && gameObject.activeSelf)
        {
            gameObject.SetActive(false);
        }
    }
}
