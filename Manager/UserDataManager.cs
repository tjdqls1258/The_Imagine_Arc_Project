using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 사용자 데이터 관리자 (UserDataManager)
/// 싱글톤 패턴을 통해 게임 전역에서 유저 데이터의 로드, 저장, 초기화를 총괄합니다.
/// 동기(Sync) 데이터와 비동기(Async) 데이터를 구분하여 최적의 입출력 퍼포먼스를 보장합니다.
/// </summary>
public class UserDataManager
{
    /// <summary>
    /// 로컬 저장소에 기존 유저의 저장 데이터가 존재하는지 여부를 나타냅니다.
    /// </summary>
    public bool hasSaveData { get; private set; }

    /// <summary>
    /// [Sync Data] PlayerPrefs나 소규모 로컬 파일에 즉각적으로 로드/저장되는 데이터 목록입니다.
    /// (예: 환경 설정, 튜토리얼 완료 여부 등)
    /// </summary>
    public Dictionary<Type, IUserData> userDatas { get; private set; } = new();

    /// <summary>
    /// [Async Data] 서버 통신이나 대용량 파일 IO가 수반되어 비동기 처리가 필요한 데이터 목록입니다.
    /// (예: 유저 인벤토리, 덱 정보, 재화 데이터 등)
    /// </summary>
    public Dictionary<Type, IAsyncUserData> asyncUserDatas { get; private set; } = new();

    // ----------------------------------------------------------------------
    // ## Initialization (초기화 및 등록)
    // ----------------------------------------------------------------------

    /// <summary>
    /// 관리할 데이터 클래스들을 인스턴스화하고 시스템에 등록합니다.
    /// </summary>
    public void Init()
    {
        userDatas.Clear();
        asyncUserDatas.Clear();

        // 1. 동기 방식 데이터 객체 등록
        userDatas.Add(typeof(UserSettingData), new UserSettingData());

        // 2. 비동기 방식 데이터 객체 등록 (앞서 만든 UserData 클래스 포함)
        asyncUserDatas.Add(typeof(UserData), new UserData());

        // 3. 기존 저장 데이터가 있는지 PlayerPrefs를 통해 간단히 체크
        hasSaveData = PlayerPrefasHelper.GetInt(PlayerPrefasHelper.PrefabsKey.HasSettingData, 0) != 0;
    }

    // ----------------------------------------------------------------------
    // ## Non-Async UserData Management (로컬 동기 데이터 제어)
    // ----------------------------------------------------------------------

    /// <summary>
    /// 신규 유저 또는 데이터 초기화 시, 모든 동기 데이터를 기본값(Default)으로 설정합니다.
    /// </summary>
    public void InitDefaultData()
    {
        foreach (var item in userDatas.Values)
        {
            item.InitData();
        }
    }

    /// <summary>
    /// 로컬 저장소(PlayerPrefs 등)로부터 모든 동기 데이터를 불러옵니다.
    /// </summary>
    public void LoadUserData()
    {
        hasSaveData = PlayerPrefasHelper.GetInt(PlayerPrefasHelper.PrefabsKey.HasSettingData, 0) != 0;

        if (hasSaveData)
        {
            foreach (var item in userDatas.Values)
            {
                // 각 데이터 클래스의 로드 성공 여부를 확인하여 예외 처리
                if (item.LoadData() == false)
                {
                    Logger.LogError($"[UserDataManager] {item.GetType()} 로드 실패");
                }
            }
        }
    }

    /// <summary>
    /// 현재 메모리 상의 동기 데이터를 로컬 저장소에 즉시 기록합니다.
    /// 모든 데이터가 성공적으로 저장되었을 때만 세이브 플래그를 업데이트합니다.
    /// </summary>
    public void SaveUserData()
    {
        bool isSaveFailed = false;

        foreach (var item in userDatas.Values)
        {
            if (item.SaveData() == false)
            {
                Logger.LogError($"[UserDataManager] {item.GetType()} 저장 실패");
                isSaveFailed = true;
            }
        }

        // 전체 저장이 성공한 경우에만 '저장 데이터 있음' 상태로 기록
        if (isSaveFailed == false)
        {
            hasSaveData = true;
        }
    }

    // ----------------------------------------------------------------------
    // ## Async UserData Management (서버/대용량 비동기 데이터 제어)
    // ----------------------------------------------------------------------

    /// <summary>
    /// [비동기] 모든 비동기 데이터를 병렬로 로드합니다. (로딩 화면에서 주로 호출)
    /// UniTask.WhenAll을 사용하여 모든 데이터가 로드될 때까지 효율적으로 대기합니다.
    /// </summary>
    public async UniTask AsyncLoadUserData()
    {
        List<UniTask> tasks = new();
        List<UniTask> inittasks = new();

        foreach (var item in asyncUserDatas.Values)
        {
            // 1. 초기화 작업 예약
            inittasks.Add(item.InitData());
            // 2. 실제 데이터 로드(서버 요청 등) 작업 예약
            tasks.Add(item.LoadData());
        }

        // 모든 초기화 및 로드 작업이 병렬로 완료될 때까지 비동기 대기 (최적화)
        await UniTask.WhenAll(tasks);
    }

    /// <summary>
    /// [비동기] 모든 비동기 데이터를 서버 또는 파일 시스템에 병렬로 저장합니다.
    /// </summary>
    public async UniTask AsyncSaveUserData()
    {
        List<UniTask> tasks = new();

        foreach (var item in asyncUserDatas.Values)
        {
            tasks.Add(item.SaveData());
        }

        await UniTask.WhenAll(tasks);
    }

    // ----------------------------------------------------------------------
    // ## Data Access (데이터 접근 인터페이스)
    // ----------------------------------------------------------------------

    /// <summary>
    /// 특정 타입의 유저 데이터 객체를 가져옵니다. 
    /// 동기/비동기 목록을 모두 검색하여 해당 인스턴스를 반환합니다.
    /// </summary>
    /// <typeparam name="T">찾으려는 데이터 클래스 타입 (IUserDataBase 상속 필수)</typeparam>
    public IUserDataBase GetUserData<T>() where T : IUserDataBase
    {
        var type = typeof(T);

        // 1. 동기 데이터 목록 검색
        if (userDatas.ContainsKey(type))
            return userDatas[type];

        // 2. 비동기 데이터 목록 검색
        else if (asyncUserDatas.ContainsKey(type))
            return asyncUserDatas[type];

        return null;
    }
}