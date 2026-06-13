using NetExcute;
using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

namespace NetExcute
{
    [Serializable]
    public class UserCharacterData : IComparable, IEquatable<UserCharacterData>
    {
        public long ID;      // 캐릭터 고유 식별 번호 (CharacterData 시트 데이터와 매칭)
        public int level;   // 현재 레벨
        public int Enforce; // 강화 수치
        public int Rank;    // 희귀도/진화 등급

        public long nomalAtk;
        public long activeSkillID = 1;
        public long passiveSkillID = 0;

        public int CompareTo(object obj)
        {
            UserCharacterData data = obj as UserCharacterData;
            if (data == null)
                throw new NotImplementedException();

            return ID.CompareTo(data.ID);
        }

        public bool Equals(UserCharacterData other)
        {
            if (other == null) return false;
            return ID == other.ID;
        }

        // ====== Client Only ======>

        private BaseCharacterStat baseCharacterStat;
        private bool m_isDirty = true;
        public int Level
        {
            get => level;
            set
            {
                if (level != value)
                {
                    level = value;
                    m_isDirty = true;
                }
            }
        }

        public CharacterData GetCharacterData(CSVHelper csvHelper)
        {
            return csvHelper.GetScripteData<CharacterDataList>().GetData(ID);
        }

        public BaseCharacterStat GetInGameBaseStat(CSVHelper csvHelper)
        {
            CharacterData baseData = GetCharacterData(csvHelper);

            if (baseCharacterStat != null && m_isDirty == false)
                return baseCharacterStat;

            baseCharacterStat = new BaseCharacterStat(baseData.characterState);

            baseCharacterStat.SetStat(StatType.MaxHp, baseData.characterState.maxHp);
            baseCharacterStat.SetStat(StatType.AttackDamage, baseData.characterState.atkPower);
            baseCharacterStat.SetStat(StatType.Defense, baseData.characterState.defPower);
            baseCharacterStat.SetStat(StatType.AttackSpeed, baseData.characterState.atkSpeed);
            baseCharacterStat.SetStat(StatType.AttackRange, baseData.characterState.atkRang);

            int levelUps = level - 1;

            if (levelUps > 0)
            {
                // TODO: 해당 캐릭터의 성장 데이터 ID를 가져오는 방식에 맞게 수정
                //string growthID = "Attacker_Level"; // 테스트용 임시 ID
                //GrowthData growth = DataManager.Instance.GetGrowthData(growthID);

                //if (growth != null)
                //{
                //    finalStat.AddStat(StatType.MaxHp, growth.MaxHpAdd * levelUps);
                //    finalStat.AddStat(StatType.AttackDamage, growth.AtkPowerAdd * levelUps);
                //    finalStat.AddStat(StatType.Defense, growth.DefPowerAdd * levelUps);
                //    finalStat.AddStat(StatType.AttackSpeed, growth. * levelUps);
                //}
            }

            m_isDirty = false;
            return baseCharacterStat;
        }
    }

    [Serializable]
    public class UserInfo
    {
        [Header("User Basic Info")]
        public string NickName; // 유저 닉네임
        public string UID;      // 유저 고유 ID

        [Header("Currency")]
        public UserGoods userGoods; // 보유 재화 정보

        [Header("Characters")]
        public UserCollection userCollection; // 캐릭터 인벤토리 및 덱 정보
    }

    [Serializable]
    public class UserGoods
    {
        public long Goods;     // 일반 재화 (예: 인게임 골드)
        public long RealGoods; // 유료 재화 (예: 유료 다이아몬드)
    }

    [Serializable]
    public class UserCollection
    {
        [Header("Collection")]
        public List<UserCharacterData> OwnCharacterList;

        public List<UserCharacterDataDeckInfo> OwnDeckList;

        public int mainCharacterID;
    }

    [Serializable]
    public class UserCharacterDataDeckInfo
    {
        public int deckIndex = 0; // 덱 번호 (예: 1번 덱, 2번 덱)
        public List<UserCharacterData> userCharacterDatas;
    }


    // --- 전체 정보 요청 프로토콜 ---
    public class UserInfoRequset : RequsetHeader
    {
        public override string GetRutor() => "Info/GetUserInfo";
    }

    public class UserInfoResponse : Response
    {
        public UserInfo userInfo;
    }

    // --- 캐릭터 컬렉션 요청 프로토콜 ---
    public class UserCharacterDataRequest : RequsetHeader
    {
        public override string GetRutor() => "Info/UserCharacterData";
    }

    public class UserCharacterDataResponse : Response
    {
        public UserCollection userCollection;
    }

    // --- 재화 정보 요청 프로토콜 ---
    public class UserGoodsDataRequest : RequsetHeader
    {
        public override string GetRutor() => "Info/UserCharacterGoods";
    }

    public class UserGoodsDataResponse : Response
    {
        public UserGoods userCollection;
    }

    //캐릭터 뽑기
    public class DrawCharacterRequest : RequsetHeader
    {
        public string uuid;
        public int count;
        public override string GetMethod()
        {
            return "get";
        }

        public override string GetRutor()
        {
            return "drawing/";
        }
    }

    public class DrawCharacterResponse : Response
    {
        public List<int> data;
    }

    //팀 덱
    public class GetUserChampionsListRequest : RequsetHeader
    {
        public string uuid;

        public override string GetRutor()
        {
            return "champions/lists";
        }
    }

    public class GetUserChampionsListResponse : Response
    {
        public List<List<long>> data;
    }

    public class SetUserInfoRequest : RequsetHeader
    {
        public string nickName;
        public override string GetRutor()
        {
            return "members";
        }
    }

    public class SetUserInfoResponse : Response
    {
        public long id;
        public string uuid;
        public string nickName;
    }

    //유저 챔피언 & 덱 조회
    public class UserDeckCharacterListRequset : RequsetHeader
    {
        public override string GetRutor()
        {
            return string.Format("champions/{0}/player", GameUtil.UUID);
        }

        public override string GetMethod()
        {
            return "GET";
        }
    }

    public class UserDeckCharacterListResponse : Response
    {
        [Serializable]
        public class ChampionClass
        {
            public long championId;
            public long enforce;
            public long activeSkillId;
            public long passiveSkillId;
            private long totalExperience;
        }
        public string uuid;
        public List<ChampionClass> champions;
        public List<List<long>> decks;
    }

    public class UserInfoRequest : RequsetHeader
    {
        public override string GetRutor()
        {
            return string.Format("members/{0}", GameUtil.UUID);
        }
        public override string GetMethod()
        {
            return "GET";
        }
    }

    public class UserInfoResopnse : Response
    {

    }
}
