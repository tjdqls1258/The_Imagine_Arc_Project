using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class UserDataManager 
{
    public bool hasSaveData { get; private set; }

    private Dictionary<Type, IUserData> userDatas = new();
    private Dictionary<Type, IAsyncUserData> asyncUserDatas = new();

    public bool CheckHasSaveData() => hasSaveData;

    public void Init()
    {
        userDatas.Clear();
        asyncUserDatas.Clear();

        userDatas.Add(typeof(UserSettingData), new UserSettingData());
        userDatas.Add(typeof(UserGameSettingData), new UserGameSettingData());

        asyncUserDatas.Add(typeof(UserData), new UserData());

        hasSaveData = PlayerPrefsHelper.GetInt(PlayerPrefsHelper.PrefabsKey.HasSettingData, 0) != 0;
    }

    public void InitDefaultData()
    {
        foreach (var item in userDatas.Values)
        {
            item.InitData();
        }
    }

    public void LoadUserData()
    {
        hasSaveData = PlayerPrefsHelper.GetInt(PlayerPrefsHelper.PrefabsKey.HasSettingData, 0) != 0;

        if (hasSaveData)
        {
            foreach (var item in userDatas.Values)
            {
                if (item.LoadData() == false)
                {
                    Debug.LogError($"[UserDataManager] {item.GetType()} 로드 실패");
                }
            }
        }
    }

    public void SaveUserData()
    {
        bool isSaveFailed = false;

        foreach (var item in userDatas.Values)
        {
            if (item.SaveData() == false)
            {
                Debug.LogError($"[UserDataManager] {item.GetType()} 저장 실패");
                isSaveFailed = true;
            }
        }

        if (isSaveFailed == false)
        {
            hasSaveData = true;
        }
    }

    public async UniTask AsyncLoadUserData()
    {
        List<UniTask> tasks = new();
        List<UniTask> inittasks = new();

        foreach (var item in asyncUserDatas.Values)
        {
            inittasks.Add(item.InitData());
            tasks.Add(item.LoadData());
        }

        await UniTask.WhenAll(tasks);
    }

    public async UniTask AsyncSaveUserData()
    {
        List<UniTask> tasks = new();

        foreach (var item in asyncUserDatas.Values)
        {
            tasks.Add(item.SaveData());
        }

        await UniTask.WhenAll(tasks);
    }

    public IUserDataBase GetUserData<T>() where T : IUserDataBase
    {
        var type = typeof(T);

        if (userDatas.ContainsKey(type))
            return userDatas[type];

        else if (asyncUserDatas.ContainsKey(type))
            return asyncUserDatas[type];

        return null;
    }
}