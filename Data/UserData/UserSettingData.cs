using Newtonsoft.Json;
using System;

/// <summary>
/// 사용자의 게임 환경 설정(사운드 온/오프, 볼륨 등)을 관리하는 싱글톤 클래스입니다.
/// 로컬 저장소에 JSON 형식으로 데이터를 저장하고 불러오는 기능을 담당합니다.
/// </summary>
public class UserSettingData : Singleton<UserSettingData>, IUserData
{
    // ====== Data Structures ======

    /// <summary>
    /// 실제 저장될 사운드 관련 옵션 데이터를 담는 구조체입니다.
    /// </summary>
    [Serializable]
    public struct UserSettingOption
    {
        public bool masterSound;       // 마스터 사운드 활성화 여부
        public float masterSoundValue; // 마스터 사운드 볼륨 크기

        public bool effectSound;       // 효과음 활성화 여부
        public float effectSoundValue; // 효과음 볼륨 크기

        public bool bgmSound;          // 배경음 활성화 여부
        public float bgmSoundValue;    // 배경음 볼륨 크기

        /// <summary>
        /// 데이터가 없거나 초기화가 필요할 때 사용할 기본값을 설정합니다.
        /// </summary>
        public void SetDefault()
        {
            masterSound = true;
            effectSound = true;
            bgmSound = true;

            masterSoundValue = 0.5f;
            effectSoundValue = 0.5f;
            bgmSoundValue = 0.5f;
        }
    }

    // ====== Members ======

    /// <summary> 현재 메모리에 로드된 유저 설정 인스턴스입니다. </summary>
    public UserSettingOption userSettingOption;

    // ----------------------------------------------------------------------
    // ## Initialization & Data Lifecycle
    // ----------------------------------------------------------------------

    /// <summary>
    /// 데이터를 초기화합니다. 싱글톤 생성 시점에 호출됩니다.
    /// </summary>
    public void InitData()
    {
        Logger.Log($"{GetType()}::Set Init");

        userSettingOption = new();
    }

    /// <summary>
    /// 로컬 저장소(PlayerPrefs)로부터 저장된 설정을 불러옵니다.
    /// 저장된 데이터가 없다면 기본값으로 생성하여 저장합니다.
    /// </summary>
    /// <returns>로드 성공 여부</returns>
    public bool LoadData()
    {
        Logger.Log($"{GetType()}::Load Data");
        try
        {
            // 1. PlayerPrefs에서 JSON 문자열을 가져옴
            var getDataJson = PlayerPrefasHelper.GetString(PlayerPrefasHelper.PrefabsKey.UserSettingOption, string.Empty);

            // 2. 데이터가 비어있다면(첫 실행 등) 기본값 세팅 후 즉시 저장
            if (string.Empty == getDataJson)
            {
                userSettingOption = new();
                userSettingOption.SetDefault();
                SaveData();
                return true;
            }

            // 3. JSON 문자열을 구조체 객체로 역직렬화(Deserialize)
            userSettingOption = JsonConvert.DeserializeObject<UserSettingOption>(getDataJson);
            return true;
        }
        catch (Exception e)
        {
            // 데이터 로드 중 파싱 에러 등이 발생할 경우 로그 출력
            Logger.LogError($"Load Error : {e.ToString()}");
            return false;
        }
    }

    /// <summary>
    /// 현재 유저 설정을 JSON 문자열로 변환하여 로컬 저장소에 저장합니다.
    /// </summary>
    /// <returns>저장 성공 여부</returns>
    public bool SaveData()
    {
        Logger.Log($"{GetType()}::Save Data");
        try
        {
            // 1. 객체를 JSON 문자열로 직렬화(Serialize)
            var data = JsonConvert.SerializeObject(userSettingOption);

            // 2. 헬퍼 클래스를 통해 로컬 시스템에 문자열 저장
            PlayerPrefasHelper.SetString(PlayerPrefasHelper.PrefabsKey.UserSettingOption, data);
            return true;
        }
        catch (Exception e)
        {
            // 권한 문제나 직렬화 오류 시 예외 처리
            Logger.LogError($"Save Error : {e.ToString()}");
            return false;
        }
    }
}