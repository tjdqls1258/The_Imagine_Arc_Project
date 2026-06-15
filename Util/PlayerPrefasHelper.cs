using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// 유니티 PlayerPrefs를 안전하게 사용하기 위한 헬퍼 클래스입니다.
/// Enum을 이용한 키(Key) 관리로 오타를 방지하고, 데이터 저장/로드 시의 일관성을 제공합니다.
/// </summary>
public static class PlayerPrefasHelper
{
    /// <summary> 사용되는 저장 데이터 키 목록입니다. </summary>
    public enum PrefabsKey
    {
        HasSettingData,  
        UserSettingOption, 
        UserGameOption,
    }

    #region Use PrefabsKey (강력한 형식의 접근 방식)

    /// <summary> Enum 키에 대응하는 실제 문자열 키 값을 관리하는 딕셔너리입니다. </summary>
    private static readonly Dictionary<PrefabsKey, string> keyValueDic =
        new Dictionary<PrefabsKey, string>
        {
            { PrefabsKey.HasSettingData, "HAS_SAVEDATA" },
            { PrefabsKey.UserSettingOption, "USERSETTING_OPTION" },
            { PrefabsKey.UserGameOption, "USERSETTING_GAMEOPTION" },
        };

    /// <summary>
    /// [String] 데이터를 저장합니다.
    /// </summary>
    /// <param name="key">PrefabsKey 열거형 키</param>
    /// <param name="value">저장할 문자열 값</param>
    /// <returns>성공 여부</returns>
    public static bool SetString(PrefabsKey key, string value = "")
    {
        if (keyValueDic.ContainsKey(key) == false)
        {
            Logger.LogError($"정의되지 않은 키입니다: {key}");
            return false;
        }

        PlayerPrefs.SetString(keyValueDic[key], value);
        PlayerPrefs.Save(); // 즉시 저장
        return true;
    }

    /// <summary>
    /// [String] 데이터를 불러옵니다.
    /// </summary>
    public static string GetString(PrefabsKey key, string defaultValue = "")
    {
        if (keyValueDic.ContainsKey(key) == false)
        {
            Logger.LogError($"정의되지 않은 키입니다: {key}");
            return string.Empty;
        }

        return PlayerPrefs.GetString(keyValueDic[key], defaultValue);
    }

    /// <summary>
    /// [Float] 데이터를 저장합니다.
    /// </summary>
    public static bool SetFloat(PrefabsKey key, float value)
    {
        if (keyValueDic.ContainsKey(key) == false)
        {
            Logger.LogError($"정의되지 않은 키입니다: {key}");
            return false;
        }

        PlayerPrefs.SetFloat(keyValueDic[key], value);
        PlayerPrefs.Save();
        return true;
    }

    /// <summary>
    /// [Float] 데이터를 불러옵니다.
    /// </summary>
    public static float GetFloat(PrefabsKey key, float defaultValue = 0f)
    {
        if (keyValueDic.ContainsKey(key) == false)
        {
            Logger.LogError($"정의되지 않은 키입니다: {key}");
            return 0;
        }

        return PlayerPrefs.GetFloat(keyValueDic[key], defaultValue);
    }

    /// <summary>
    /// [Int] 데이터를 저장합니다.
    /// </summary>
    public static bool SetInt(PrefabsKey key, int value)
    {
        if (keyValueDic.ContainsKey(key) == false)
        {
            Logger.LogError($"정의되지 않은 키입니다: {key}");
            return false;
        }
        PlayerPrefs.SetInt(keyValueDic[key], value);
        PlayerPrefs.Save();
        return true;
    }

    /// <summary>
    /// [Int] 데이터를 불러옵니다.
    /// </summary>
    public static int GetInt(PrefabsKey key, int defaultValue = 0)
    {
        if (keyValueDic.ContainsKey(key) == false)
        {
            Logger.LogError($"정의되지 않은 키입니다: {key}");
            return 0;
        }

        return PlayerPrefs.GetInt(keyValueDic[key], defaultValue);
    }
    #endregion

    #region Default (문자열 키를 직접 사용하는 레거시 방식)

    // Enum에 등록되지 않은 동적인 키가 필요한 경우를 위해 오버로딩되어 있습니다.

    public static void SetString(string key, string value = "")
    {
        PlayerPrefs.SetString(key, value);
        PlayerPrefs.Save();
    }

    public static string GetString(string key, string defaultValue = "")
    {
        return PlayerPrefs.GetString(key, defaultValue);
    }

    public static void SetFloat(string key, float value)
    {
        PlayerPrefs.SetFloat(key, value);
        PlayerPrefs.Save();
    }

    public static float GetFloat(string key, float defaultValue = 0f)
    {
        return PlayerPrefs.GetFloat(key, defaultValue);
    }

    public static void SetInt(string key, int value)
    {
        PlayerPrefs.SetInt(key, value);
        PlayerPrefs.Save();
    }

    public static int GetInt(string key, int defaultValue = 0)
    {
        return PlayerPrefs.GetInt(key, defaultValue);
    }
    #endregion
}