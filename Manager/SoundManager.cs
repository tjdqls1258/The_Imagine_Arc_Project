using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

#region Enums
public enum SoundType
{
    BGM,
    EFFECT,
    MaxCount,
}

public enum SoundPath
{
    None = -1,
    BGM_Title,
    ClickSound,
}
#endregion


public class SoundManager : MonoBehaviour
{
    [Header("Audio Mixer Settings")]
    [SerializeField] private AudioMixerGroup audioMixerGroup;

    UserSettingData SettingData => GameMaster.Instance.dataManager.GetUserData<UserSettingData>() as UserSettingData;

    private Dictionary<string, AudioClip> m_clipDic = new();
    private AudioSource[] m_audioSources = new AudioSource[(int)SoundType.MaxCount];

    public void Init()
    {
        string[] soundNames = System.Enum.GetNames(typeof(SoundType));

        for (int i = 0; i < (int)SoundType.MaxCount; i++)
        {
            GameObject go = new GameObject { name = soundNames[i] };
            go.transform.parent = transform;

            AudioSource source = go.AddComponent<AudioSource>();
            m_audioSources[i] = source;

            AudioMixerGroup[] matchingGroups = audioMixerGroup.audioMixer.FindMatchingGroups(soundNames[i]);

            if (matchingGroups == null || matchingGroups.Length == 0)
            {
                Debug.LogError($"[SoundManager] AudioMixer Group '{soundNames[i]}' not found.");
                continue;
            }

            source.outputAudioMixerGroup = matchingGroups[0];
        }

        m_audioSources[(int)SoundType.BGM].loop = true;
    }

    public async UniTask Play(SoundPath path, SoundType type = SoundType.EFFECT, float pitch = 1f)
    {
        if (path == SoundPath.None) return;

        AudioClip audioClip = await GetOrAddAudioClip(path.ToString(), type);

        if (audioClip != null)
        {
            Play(audioClip, type, pitch);
        }
    }

    private async UniTask<AudioClip> GetOrAddAudioClip(string path, SoundType type = SoundType.EFFECT)
    {
        AudioClip audioClip = null;

        if (type == SoundType.BGM)
        {
            audioClip = await GameMaster.Instance.addressableManager.LoadAssetAndCacheAsync<AudioClip>(path);
        }
        else
        {
            if (m_clipDic.TryGetValue(path, out audioClip) == false)
            {
                audioClip = await GameMaster.Instance.addressableManager.LoadAssetAndCacheAsync<AudioClip>(path);
                if (audioClip != null)
                {
                    m_clipDic.Add(path, audioClip);
                }
            }
        }

        if (audioClip == null)
            Debug.LogWarning($"[SoundManager] AudioClip Missing! Path: {path}");

        return audioClip;
    }

    private void Play(AudioClip audioClip, SoundType soundType, float pitch = 1f)
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
            audioSource.PlayOneShot(audioClip);
        }
    }

    public void MasterValue(float value, bool mute)
    {
        SettingData.userSettingOption.muteMasterSound = mute;
        SettingData.userSettingOption.masterSoundValue = value;

        if (SettingData.userSettingOption.muteMasterSound)
            audioMixerGroup.audioMixer.SetFloat("Master", -80f);
        else
            audioMixerGroup.audioMixer.SetFloat("Master", SettingData.userSettingOption.masterSoundValue);
    }

    public void BGMValue(float bgmValue, bool mute)
    {
        SettingData.userSettingOption.muteBgmSound = mute;
        SettingData.userSettingOption.bgmSoundValue = bgmValue;

        if (SettingData.userSettingOption.muteBgmSound)
            audioMixerGroup.audioMixer.SetFloat(SoundType.BGM.ToString(), -80f);
        else
            audioMixerGroup.audioMixer.SetFloat(SoundType.BGM.ToString(), SettingData.userSettingOption.bgmSoundValue);
    }

    public void EffectValue(float effectValue, bool mute)
    {
        SettingData.userSettingOption.muteEffectSound = mute;
        SettingData.userSettingOption.effectSoundValue = effectValue;

        if (SettingData.userSettingOption.muteEffectSound)
            audioMixerGroup.audioMixer.SetFloat(SoundType.EFFECT.ToString(), -80f);
        else
            audioMixerGroup.audioMixer.SetFloat(SoundType.EFFECT.ToString(), SettingData.userSettingOption.effectSoundValue);
    }

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

    public void ReleseVoice()
    {
        List<string> keysToUnload = new List<string>(m_clipDic.Keys);

        foreach (var key in keysToUnload)
        {
            GameMaster.Instance.addressableManager.UnloadAsset(key);
            m_clipDic.Remove(key);
        }

        keysToUnload.Clear();
    }
}