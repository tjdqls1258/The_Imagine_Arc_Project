using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

#region Enums
/// <summary>
/// 관리할 사운드의 카테고리를 정의합니다.
/// </summary>
public enum SoundType
{
    BGM,    // 배경 음악 (Loop)
    EFFECT, // 효과음 (OneShot)
    MaxCount,
}

/// <summary>
/// Addressables에 등록된 사운드 에셋의 키(Key) 이름과 일치해야 하는 Enum입니다.
/// </summary>
public enum SoundPath
{
    None = -1,
    // BGM 목록
    BGM_Title,
    // 효과음 목록
    ClickSound,
}
#endregion

/// <summary>
/// 게임의 모든 사운드 재생, 볼륨 조절, 리소스 로딩을 담당하는 중앙 관리자입니다.
/// </summary>
public class SoundManager : MonoSingleton<SoundManager>
{
    // ====== Inspector References ======
    [Header("Audio Mixer Settings")]
    [Tooltip("Master, BGM, EFFECT 그룹이 포함된 메인 오디오 믹서를 연결합니다.")]
    [SerializeField] private AudioMixerGroup audioMixerGroup;

    // ====== Runtime State & Caches ======
    private Dictionary<string, AudioClip> m_clipDic = new(); // 효과음 캐시 (Key: 에셋이름)
    private AudioSource[] m_audioSources = new AudioSource[(int)SoundType.MaxCount];

    // ----------------------------------------------------------------------
    // ## Initialization
    // ----------------------------------------------------------------------

    /// <summary>
    /// SoundManager 초기화 및 각 사운드 타입별 전용 AudioSource를 생성합니다.
    /// </summary>
    public override void Init()
    {
        base.Init();

        string[] soundNames = System.Enum.GetNames(typeof(SoundType));

        for (int i = 0; i < (int)SoundType.MaxCount; i++)
        {
            // 1. 타입별 전용 게임오브젝트 및 AudioSource 생성
            GameObject go = new GameObject { name = soundNames[i] };
            go.transform.parent = transform;

            AudioSource source = go.AddComponent<AudioSource>();
            m_audioSources[i] = source;

            // 2. 오디오 믹서에서 해당하는 그룹(BGM/EFFECT)을 찾아 연결
            AudioMixerGroup[] matchingGroups = audioMixerGroup.audioMixer.FindMatchingGroups(soundNames[i]);

            if (matchingGroups == null || matchingGroups.Length == 0)
            {
                Logger.LogError($"[SoundManager] AudioMixer Group '{soundNames[i]}' not found.");
                continue;
            }

            source.outputAudioMixerGroup = matchingGroups[0];
        }

        // BGM 채널은 기본적으로 반복 재생 설정
        m_audioSources[(int)SoundType.BGM].loop = true;
    }

    // ----------------------------------------------------------------------
    // ## Core Playback & Resource Management
    // ----------------------------------------------------------------------

    /// <summary>
    /// [비동기] 지정된 경로의 사운드를 로드/캐시하여 재생합니다.
    /// </summary>
    /// <param name="path">SoundPath Enum 값 (ToString으로 에셋 키 사용)</param>
    /// <param name="type">BGM 또는 EFFECT 여부</param>
    /// <param name="pitch">재생 속도/피치 (기본 1.0f)</param>
    public async UniTask Play(SoundPath path, SoundType type = SoundType.EFFECT, float pitch = 1f)
    {
        if (path == SoundPath.None) return;

        AudioClip audioClip = await GetOrAddAudioClip(path.ToString(), type);

        if (audioClip != null)
        {
            Play(audioClip, type, pitch);
        }
    }

    /// <summary>
    /// 캐시된 클립을 반환하거나, 없을 경우 Addressables에서 새로 로드합니다.
    /// </summary>
    private async UniTask<AudioClip> GetOrAddAudioClip(string path, SoundType type = SoundType.EFFECT)
    {
        AudioClip audioClip = null;

        // 1. BGM 처리 (AddressableManager에서 자체 캐싱)
        if (type == SoundType.BGM)
        {
            audioClip = await AddressableManager.Instance.LoadAssetAndCacheAsync<AudioClip>(path);
        }
        // 2. EFFECT 처리 (Dictionary에 명시적으로 캐싱)
        else
        {
            if (m_clipDic.TryGetValue(path, out audioClip) == false)
            {
                audioClip = await AddressableManager.Instance.LoadAssetAndCacheAsync<AudioClip>(path);
                if (audioClip != null)
                {
                    m_clipDic.Add(path, audioClip);
                }
            }
        }

        if (audioClip == null)
            Logger.LogWarning($"[SoundManager] AudioClip Missing! Path: {path}");

        return audioClip;
    }

    /// <summary>
    /// 준비된 AudioClip을 사운드 타입에 맞는 채널에서 재생합니다.
    /// </summary>
    public void Play(AudioClip audioClip, SoundType soundType, float pitch = 1f)
    {
        if (audioClip == null) return;

        AudioSource audioSource = m_audioSources[(int)soundType];
        audioSource.pitch = pitch;

        if (soundType == SoundType.BGM)
        {
            if (audioSource.isPlaying) audioSource.Stop();
            audioSource.clip = audioClip;
            audioSource.Play();
        }
        else
        {
            // 효과음은 겹쳐서 재생될 수 있도록 PlayOneShot 사용
            audioSource.PlayOneShot(audioClip);
        }
    }

    // ----------------------------------------------------------------------
    // ## Volume Control & Settings
    // ----------------------------------------------------------------------

    /// <summary> 전체 마스터 볼륨 조절 </summary>
    public void MasterValue(float value, bool mute = false)
    {
        audioMixerGroup.audioMixer.SetFloat("Master", value);

        if (!mute)
        {
            UserSettingData.Instance.userSettingOption.masterSoundValue = value;
            UserSettingData.Instance.SaveData();
        }
    }

    /// <summary> BGM 믹서 파라미터 조절 </summary>
    public void BGMValue(float bgmValue)
    {
        audioMixerGroup.audioMixer.SetFloat(SoundType.BGM.ToString(), bgmValue);
        UserSettingData.Instance.userSettingOption.bgmSoundValue = bgmValue;
        UserSettingData.Instance.SaveData();
    }

    /// <summary> 효과음 믹서 파라미터 조절 </summary>
    public void EffectValue(float effectValue)
    {
        audioMixerGroup.audioMixer.SetFloat(SoundType.EFFECT.ToString(), effectValue);
        UserSettingData.Instance.userSettingOption.effectSoundValue = effectValue;
        UserSettingData.Instance.SaveData();
    }

    public void Mute() => MasterValue(0.0f, true);
    public void UnMute() => MasterValue(UserSettingData.Instance.userSettingOption.masterSoundValue, true);

    // ----------------------------------------------------------------------
    // ## Cleanup & Resource Release
    // ----------------------------------------------------------------------

    /// <summary>
    /// 모든 재생 중인 사운드를 중지하고 캐시 목록을 비웁니다.
    /// </summary>
    public void Clear()
    {
        foreach (var audio in m_audioSources)
        {
            if (audio != null)
            {
                audio.clip = null;
                audio.Stop();
            }
        }
        m_clipDic.Clear();
    }

    /// <summary>
    /// 캐시된 효과음 에셋들을 Addressables 메모리에서 해제합니다.
    /// </summary>
    public void ReleseVoice()
    {
        // 딕셔너리 순회 중 삭제를 위해 키 리스트 복사 후 처리
        List<string> keysToUnload = new List<string>(m_clipDic.Keys);

        foreach (var key in keysToUnload)
        {
            AddressableManager.Instance.UnloadAsset(key);
            m_clipDic.Remove(key);
        }

        keysToUnload.Clear();
    }
}