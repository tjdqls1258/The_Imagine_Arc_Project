using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

// Note: IUserData는 동기적 LoadData()/SaveData()를, 
// IAsyncUserData는 비동기 Task LoadData()/SaveData()를 정의한다고 가정합니다.

/// <summary>
/// 사용자 데이터 관리자 (UserDataManager)
/// 게임의 모든 사용자 데이터를 관리하는 Singleton 클래스입니다.
/// 데이터를 동기(Non-Async)와 비동기(Async) 타입으로 분리하여 관리하며,
/// 데이터 로드, 저장, 초기화 로직을 통합하여 제공합니다.
/// </summary>
public class UserDataManager : Singleton<UserDataManager>
{
    /// <summary>
    /// 저장된 데이터가 존재하는지 여부를 나타냅니다. (주로 PlayerPrefs로 확인)
    /// </summary>
    public bool hasSaveData { get; private set; }

    /// <summary>
    /// 동기적 로드/저장이 가능한 IUserData 인터페이스를 구현한 데이터 목록입니다.
    /// (예: UserSettings, 작은 인벤토리 데이터 등)
    /// </summary>
    public Dictionary<Type, IUserData> userDatas { get; private set; } = new();

    /// <summary>
    /// 비동기적 로드/저장이 필요한 IAsyncUserData 인터페이스를 구현한 데이터 목록입니다.
    /// (예: 큰 인벤토리, 서버 통신이 필요한 재화 데이터 등)
    /// </summary>
    public Dictionary<Type, IAsyncUserData> asyncUserDatas { get; private set; } = new();

    // ----------------------------------------------------------------------
    // ## Initialization
    // ----------------------------------------------------------------------

    /// <summary>
    /// UserDataManager를 초기화하고 관리할 데이터 클래스들을 등록합니다.
    /// </summary>
    public override void Init()
    {
        userDatas.Clear();
        asyncUserDatas.Clear(); // 비동기 목록 초기화

        // 동기 데이터 등록
        userDatas.Add(typeof(UserSettingData) ,new UserSettingData());

        // 비동기 데이터 등록
        asyncUserDatas.Add(typeof(UserData), new UserData());

        // 저장 데이터 존재 여부 확인 (PlayerPrefs를 이용한 간단한 확인)
        hasSaveData = PlayerPrefasHelper.GetInt(PlayerPrefasHelper.PrefabsKey.HasSettingData, 0) != 0;
    }

    // ----------------------------------------------------------------------
    // ## Non-Async UserData Management (동기 데이터)
    // ----------------------------------------------------------------------

    /// <summary>
    /// 모든 동기 데이터 클래스를 기본값으로 초기화합니다.
    /// (저장된 데이터가 없을 때, 새로운 유저에게 호출)
    /// </summary>
    public void InitDefaultData()
    {
        foreach (var item in userDatas.Values)
        {
            item.InitData();
        }
    }

    /// <summary>
    /// 저장된 동기 사용자 데이터를 로드합니다. (PlayerPrefs 등 빠른 로컬 저장소)
    /// </summary>
    public void LoadUserData()
    {
        // 최신 저장 여부 상태를 다시 확인합니다.
        hasSaveData = PlayerPrefasHelper.GetInt(PlayerPrefasHelper.PrefabsKey.HasSettingData, 0) != 0;

        if (hasSaveData)
        {
            foreach (var item in userDatas.Values)
            {
                // LoadData()의 반환 값(bool)을 통해 로드 성공/실패 여부를 확인합니다.
                if (item.LoadData() == false) // LoadData()가 true를 반환하면 성공, false를 반환하면 실패라고 가정
                {
                    // 로드 실패 시 로그 기록
                    Logger.LogError($"[UserDataManager] {item.GetType()} is Load Fail");
                }
            }
        }
    }

    /// <summary>
    /// 모든 동기 사용자 데이터를 저장합니다.
    /// </summary>
    public void SaveUserData()
    {
        bool isSaveFailed = false;

        foreach (var item in userDatas.Values)
        {
            // SaveData()의 반환 값(bool)을 통해 저장 성공/실패 여부를 확인합니다.
            if (item.SaveData() == false)
            {
                Logger.LogError($"[UserDataManager] {item.GetType()} is Save Fail");
                isSaveFailed = true;
            }
        }

        // 모든 동기 데이터 저장이 성공했을 경우에만 '저장 데이터 존재' 플래그를 업데이트합니다.
        if (isSaveFailed == false)
        {
            hasSaveData = true;
            PlayerPrefasHelper.SetInt(PlayerPrefasHelper.PrefabsKey.HasSettingData, 1);
        }

        // Note: PlayerPrefs.Save()를 명시적으로 호출해야 한다면 여기에 추가해야 합니다.
        // PlayerPrefs.Save(); 
    }

    // ----------------------------------------------------------------------
    // ## Async UserData Management (비동기 데이터)
    // ----------------------------------------------------------------------

    /// <summary>
    /// 모든 비동기 데이터 클래스에 대해 비동기 로드를 병렬로 실행합니다.
    /// (예: 서버 통신, 대용량 파일 로드 등)
    /// </summary>
    /// <returns>모든 비동기 로드가 완료될 때까지 대기하는 Task</returns>
    public async UniTask AsyncLoadUserData()
    {
        List<UniTask> tasks = new();
        foreach (var item in asyncUserDatas.Values)
        {
            // 각 항목의 비동기 로드 Task를 리스트에 추가
            tasks.Add(item.LoadData());
        }

        // 모든 로드 Task가 완료될 때까지 비동기적으로 대기
        await UniTask.WhenAll(tasks);
    }

    /// <summary>
    /// 모든 비동기 데이터 클래스에 대해 비동기 저장을 병렬로 실행합니다.
    /// </summary>
    /// <returns>모든 비동기 저장이 완료될 때까지 대기하는 Task</returns>
    public async UniTask AsyncSaveUserData()
    {
        List<UniTask> tasks = new();

        foreach (var item in asyncUserDatas.Values)
        {
            // 각 항목의 비동기 저장 Task를 리스트에 추가
            tasks.Add(item.SaveData());
        }

        // 모든 저장 Task가 완료될 때까지 비동기적으로 대기
        await UniTask.WhenAll(tasks);
    }

    public IUserDataBase GetUserData<T>() where T : IUserDataBase
    {
        var type = typeof(T);

        if (userDatas.ContainsKey(type))
            return userDatas[type];

        else if(asyncUserDatas.ContainsKey(type))
            return asyncUserDatas[type];

        return null;
    }
}