using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;

/// <summary>
/// 사용자의 계정 정보 및 인게임 데이터를 관리하는 클래스입니다.
/// IAsyncUserData 인터페이스를 상속받아 비동기 방식으로 로드 및 저장을 처리합니다.
/// </summary>
public class UserData : IAsyncUserData
{
    /// <summary> 서버로부터 받아온 사용자 상세 정보(ID, 닉네임, 재화 등)입니다. </summary>
    public NetExcute.UserInfo myUserInfo { get; private set; }

    // ----------------------------------------------------------------------
    // ## Initialization & Data Lifecycle
    // ----------------------------------------------------------------------

    /// <summary>
    /// 사용자 데이터 시스템을 초기화합니다. 
    /// 최초 로그인 시 기본 설정이나 웹 통신 준비 단계에서 호출됩니다.
    /// </summary>
    public UniTask InitData()
    {
        Logger.Log($"{GetType()}::Set Init");

        // TODO: 웹 소켓 연결 혹은 API 서버 초기 인증 로직 구현 필요

        return UniTask.CompletedTask;
    }

    /// <summary>
    /// 서버로부터 사용자 데이터를 비동기로 불러옵니다.
    /// </summary>
    public UniTask LoadData()
    {
        Logger.Log($"{GetType()}::Load Data");

        try
        {
            // TODO: API 요청을 통해 서버 DB에 저장된 유저 정보를 가져와 myUserInfo에 할당
            // 예: myUserInfo = await WebRequest.GetUserInfo();
        }
        catch (Exception e)
        {
            // 데이터 로드 중 발생하는 네트워크 오류나 파싱 에러를 처리
            Logger.LogError($"Load Error : {e.ToString()}");
        }

        return UniTask.CompletedTask;
    }

    /// <summary>
    /// 현재 클라이언트의 변경된 유저 데이터를 서버 DB에 동기화(저장)합니다.
    /// </summary>
    public UniTask SaveData()
    {
        Logger.Log($"{GetType()}::Save Data");

        try
        {
            // TODO: 서버 API에 Post 요청을 보내 현재 myUserInfo 상태를 저장
            // 예: await WebRequest.PostUserData(myUserInfo);
        }
        catch (Exception e)
        {
            // 서버 점검이나 타임아웃 등 저장 실패 시 예외 처리
            Logger.LogError($"Save Error : {e.ToString()}");
        }

        return UniTask.CompletedTask;
    }
}