using Newtonsoft.Json;
using System;
using VContainer;

public class UserSettingData : IUserData
{
    [Serializable]
    public struct UserSettingOption
    {
        public bool muteMasterSound;       // 마스터 사운드 활성화 여부
        public float masterSoundValue; // 마스터 사운드 볼륨 크기

        public bool muteEffectSound;       // 효과음 활성화 여부
        public float effectSoundValue; // 효과음 볼륨 크기

        public bool muteBgmSound;          // 배경음 활성화 여부
        public float bgmSoundValue;    // 배경음 볼륨 크기

        /// <summary>
        /// 데이터가 없거나 초기화가 필요할 때 사용할 기본값을 설정합니다.
        /// </summary>
        public void SetDefault()
        {
            muteMasterSound = true;
            muteEffectSound = true;
            muteBgmSound = true;

            masterSoundValue = 0f;
            effectSoundValue = 0f;
            bgmSoundValue = 0f;
        }
    }

    public UserSettingOption userSettingOption;

    public void InitData()
    {
        Logger.Log($"{GetType()}::Set Init");

        userSettingOption = new();
    }

    public bool LoadData()
    {
        Logger.Log($"{GetType()}::Load Data");
        try
        {
            var getDataJson = PlayerPrefsHelper.GetString(PlayerPrefsHelper.PrefabsKey.UserSettingOption, string.Empty);

            if (string.Empty == getDataJson)
            {
                userSettingOption = new();
                userSettingOption.SetDefault();
                SaveData();
                return true;
            }

            userSettingOption = JsonConvert.DeserializeObject<UserSettingOption>(getDataJson);

            return true;
        }
        catch (Exception e)
        {
            Logger.LogError($"Load Error : {e.ToString()}");
            return false;
        }
    }

    public bool SaveData()
    {
        Logger.Log($"{GetType()}::Save Data");
        try
        {
            var data = JsonConvert.SerializeObject(userSettingOption);

            PlayerPrefsHelper.SetString(PlayerPrefsHelper.PrefabsKey.UserSettingOption, data);
            PlayerPrefsHelper.SetInt(PlayerPrefsHelper.PrefabsKey.HasSettingData, 1);
            return true;
        }
        catch (Exception e)
        {
            Logger.LogError($"Save Error : {e.ToString()}");
            return false;
        }
    }

    public void SetSoundData(SoundManager soundManager)
    {
        soundManager.MasterValue(userSettingOption.masterSoundValue, userSettingOption.muteMasterSound);
        soundManager.BGMValue(userSettingOption.bgmSoundValue, userSettingOption.muteBgmSound);
        soundManager.EffectValue(userSettingOption.effectSoundValue, userSettingOption.muteEffectSound);
    }
}

public class UserGameSettingData : IUserData
{
    [Serializable]
    public struct UserGameOption
    {
        public int GameSpeedIndex;

        public void SetDefault()
        {
            GameSpeedIndex = 0;
        }
    }

    public UserGameOption userGameSettingOption;
    public void InitData()
    {
        userGameSettingOption = new();
    }

    public bool LoadData()
    {
        Logger.Log($"{GetType()}::Load Data");
        try
        {
            var getDataJson = PlayerPrefsHelper.GetString(PlayerPrefsHelper.PrefabsKey.UserGameOption, string.Empty);

            if (string.Empty == getDataJson)
            {
                userGameSettingOption = new();
                userGameSettingOption.SetDefault();
                SaveData();
                return true;
            }

            userGameSettingOption = JsonConvert.DeserializeObject<UserGameOption>(getDataJson);

            return true;
        }
        catch (Exception e)
        {
            Logger.LogError($"Load Error : {e.ToString()}");
            return false;
        }
    }

    public bool SaveData()
    {
        Logger.Log($"{GetType()}::Save Data");
        try
        {
            var data = JsonConvert.SerializeObject(userGameSettingOption);

            PlayerPrefsHelper.SetString(PlayerPrefsHelper.PrefabsKey.UserGameOption, data);
            PlayerPrefsHelper.SetInt(PlayerPrefsHelper.PrefabsKey.HasSettingData, 1);
            return true;
        }
        catch (Exception e)
        {
            Logger.LogError($"Save Error : {e.ToString()}");
            return false;
        }
    }
}