using Cysharp.Threading.Tasks;
using NetExcute;
using System;
using System.Collections.Generic;


/// <summary>
/// 사용자의 계정 정보(UserInfo) 및 캐릭터 인벤토리, 덱 정보를 총괄 관리하는 클래스입니다.
/// IAsyncUserData 인터페이스를 통해 서버와의 비동기 데이터 동기화 로직을 규격화합니다.
/// </summary>
public class UserData : IAsyncUserData
{
    // ====== 시스템 상수 (제약 조건) ======
    /// <summary> 하나의 덱에 배치할 수 있는 최대 캐릭터 수 </summary>
    public const int MAX_CHARACTER_SETTING = 16;
    /// <summary> 사용자가 생성 및 관리할 수 있는 총 덱 페이지 수 </summary>
    public const int MAX_DECKCOUNT = 3;

    // ====== 핵심 유저 데이터 ======
    /// <summary> 서버로부터 받아온 사용자 기본 정보(닉네임, UID, 재화 등)입니다. </summary>
    public UserInfo myUserInfo { get; private set; }

    /// <summary> 
    /// 사용자가 소지 중인 전체 캐릭터 인벤토리입니다.
    /// Key: 캐릭터 고유 ID, Value: 성장 데이터(레벨, 강화 등)
    /// </summary>
    public Dictionary<long, UserCharacterData> oderCharacter = new();

    /// <summary> 
    /// 각 페이지별로 설정된 덱 리스트입니다.
    /// Key: 덱 인덱스(0~MAX_DECKCOUNT), Value: 배치된 캐릭터 배열
    /// </summary>
    public Dictionary<long, UserCharacterData[]> characterDeckList
    {
        get;
        set;
    } = new();

    // ----------------------------------------------------------------------
    // ## Initialization & Lifecycle (데이터 생명주기)
    // ----------------------------------------------------------------------

    /// <summary>
    /// 사용자 데이터 시스템을 초기화합니다. 
    /// 딕셔너리 구조를 생성하고 서버 통신 전 필요한 기본 설정을 수행합니다.
    /// </summary>
    public UniTask InitData()
    {
        // 1. 덱 관리를 위한 기본 딕셔너리 구조 생성
        InitCharacterDeck();

        Logger.Log($"{GetType()}::Set Init");

        // TODO: 웹 소켓 연결 혹은 API 서버 초기 인증 로직 구현 필요
        return UniTask.CompletedTask;
    }

    /// <summary>
    /// 서버(또는 로컬 스토리지)로부터 유저 데이터를 비동기로 불러옵니다.
    /// </summary>
    public async UniTask LoadData()
    {
        Logger.Log($"{GetType()}::Load Data");

        try
        {
            //테스트 로그인
            await NetExcute.NetExcute.Instance.Requset<SetUserInfoResponse>(new SetUserInfoRequest() { nickName = "amanTest" }, (res) =>
            {
                GameMaster.Instance.UUID = res.uuid;

                NetExcute.NetExcute.Instance.Requset<UserDeckCharacterListResponse>(new UserDeckCharacterListRequset(), (res) =>
                {
                    int deckNumber = 0;

                    foreach (var character in res.champions)
                        oderCharacter.Add(character.championId, new() { ID = character.championId, Enforce = 1, passiveSkillID = character.passiveSkillId, activeSkillID = character.activeSkillId });

                    foreach (var item in res.decks)
                    {
                        int count = 0;
                        foreach (var id in item)
                        {
                            if (characterDeckList.ContainsKey(id) == false)
                                characterDeckList.Add(deckNumber, new UserCharacterData[MAX_CHARACTER_SETTING]);

                            if (oderCharacter.ContainsKey(id))
                                characterDeckList[deckNumber][count] = oderCharacter[id];
                            count++;
                        }
                        deckNumber++;
                    }
                }, null).Forget();
            },  ()=> SetTestCharacterData());

           

            // TODO: API 요청을 통해 실제 서버 DB의 데이터를 myUserInfo에 할당
            // 예: myUserInfo = await WebRequest.GetUserInfo();
        }
        catch (Exception e)
        {
            // 네트워크 단절, 파싱 오류 등 로드 실패 시 예외 처리
            Logger.LogError($"Load Error : {e.ToString()}");
        }
    }

    /// <summary>
    /// 현재 클라이언트에서 변경된 유저 데이터(덱 수정, 재화 변동 등)를 서버 DB에 저장합니다.
    /// </summary>
    public UniTask SaveData()
    {
        Logger.Log($"{GetType()}::Save Data");

        try
        {
            // TODO: 서버 API에 POST 요청을 보내 현재 상태를 동기화
            // 예: await WebRequest.PostUserData(myUserInfo);
        }
        catch (Exception e)
        {
            // 서버 점검이나 타임아웃 등으로 인한 저장 실패 시 복구 로직 필요
            Logger.LogError($"Save Error : {e.ToString()}");
        }

        return UniTask.CompletedTask;
    }

    /// <summary>
    /// 덱 페이지 개수만큼 딕셔너리를 순회하며 캐릭터 슬롯 배열을 미리 할당합니다.
    /// </summary>
    void InitCharacterDeck()
    {
        for (int i = 0; i < MAX_DECKCOUNT; i++)
        {
            // 각 페이지마다 고정된 슬롯 수(12개)만큼의 배열 생성
            characterDeckList.Add(i, new UserCharacterData[MAX_CHARACTER_SETTING]);
        }
    }

    // ----------------------------------------------------------------------
    // ## Editor Debugging (테스트 데이터)
    // ----------------------------------------------------------------------

//#if UNITY_EDITOR
    public void SetTestCharacterData()
    {
        // 0번 덱에 테스트용 캐릭터 4종 배치
        characterDeckList[0][0] = new() { ID = 1, Enforce = 0, level = 1, Rank = 1 };
        characterDeckList[0][2] = new() { ID = 2, Enforce = 0, level = 1, Rank = 1 };
        characterDeckList[0][3] = new() { ID = 3, Enforce = 0, level = 1, Rank = 1 };
        characterDeckList[0][5] = new() { ID = 4, Enforce = 0, level = 1, Rank = 1 };

        //// 소지품(인벤토리)에 테스트 캐릭터 11종 추가
        oderCharacter.Add(1, new() { ID = 1, Enforce = 0, level = 1, Rank = 1 });
        oderCharacter.Add(2, new() { ID = 2, Enforce = 0, level = 1, Rank = 1 });
        oderCharacter.Add(3, new() { ID = 3, Enforce = 0, level = 1, Rank = 1 });
        oderCharacter.Add(4, new() { ID = 4, Enforce = 0, level = 1, Rank = 1 });
        oderCharacter.Add(5, new() { ID = 5, Enforce = 0, level = 1, Rank = 1 });
        oderCharacter.Add(6, new() { ID = 6, Enforce = 0, level = 1, Rank = 1 });
        oderCharacter.Add(7, new() { ID = 7, Enforce = 0, level = 1, Rank = 1 });
        oderCharacter.Add(8, new() { ID = 8, Enforce = 5, level = 10, Rank = 1 });
        oderCharacter.Add(11, new() { ID = 11, Enforce = 0, level = 1, Rank = 1 });
        oderCharacter.Add(14, new() { ID = 14, Enforce = 0, level = 1, Rank = 1 });
        oderCharacter.Add(16, new() { ID = 16, Enforce = 0, level = 1, Rank = 1 });
    }
}