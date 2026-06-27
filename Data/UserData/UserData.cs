using Cysharp.Threading.Tasks;
using NetExcute;
using System;
using System.Collections.Generic;
using VContainer;


public class UserData : IAsyncUserData
{
    public const int MAX_CHARACTER_SETTING = 16;
    public const int MAX_DECKCOUNT = 3;

    public UserInfo myUserInfo { get; private set; }
    public Dictionary<long, UserCharacterData> oderCharacter = new();
    public List<int> userSkillList = new();

    public Dictionary<long, UserCharacterData[]> characterDeckList
    {
        get;
        set;
    } = new();

    
    public UniTask InitData()
    {
        InitCharacterDeck();

        Logger.Log($"{GetType()}::Set Init");

        // TODO: 웹 소켓 연결 혹은 API 서버 초기 인증 로직 구현 필요
        return UniTask.CompletedTask;
    }

    public async UniTask LoadData()
    {
        Logger.Log($"{GetType()}::Load Data");

        try
        {
            //테스트 로그인
            await NetExcute.NetExcute.Instance.Requset<SetUserInfoResponse>(new SetUserInfoRequest() { nickName = "amanTest" }, (res) =>
            {
                GameUtil.UUID = res.uuid;

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
            Logger.LogError($"Load Error : {e.ToString()}");
        }
    }

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
            Logger.LogError($"Save Error : {e.ToString()}");
        }

        return UniTask.CompletedTask;
    }

    void InitCharacterDeck()
    {
        for (int i = 0; i < MAX_DECKCOUNT; i++)
        {
            characterDeckList.Add(i, new UserCharacterData[MAX_CHARACTER_SETTING]);
        }
    }

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

        userSkillList.Add(1);
        userSkillList.Add(2);
    }
}