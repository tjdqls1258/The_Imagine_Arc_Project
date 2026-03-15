using System;
using System.Collections.Generic;
using UnityEngine;

namespace NetExcute
{
    /// <summary>
    /// [Instance Data] 사용자가 보유한 개별 캐릭터의 유동적인 성장 상태(레벨, 강화 등)를 관리합니다.
    /// 데이터 정렬 및 비교를 위해 IComparable, IEquatable 인터페이스를 구현합니다.
    /// </summary>
    [Serializable]
    public class UserCharacterData : IComparable, IEquatable<UserCharacterData>
    {
        public int ID;      // 캐릭터 식별 번호 (CharacterData 마스터 데이터와 매칭)
        public int level;   // 현재 레벨
        public int Enforce; // 강화 수치
        public int Rank;    // 초월/진화 등급

        /// <summary> 캐릭터 리스트 정렬 시 사용됩니다. (기본적으로 ID 순 정렬) </summary>
        public int CompareTo(object obj)
        {
            UserCharacterData data = obj as UserCharacterData;
            if (data == null)
                throw new NotImplementedException();

            return ID.CompareTo(data.ID);
        }

        /// <summary> 리스트 검색(Contains, IndexOf) 시 ID 비교를 통해 동일 객체 여부를 판단합니다. </summary>
        public bool Equals(UserCharacterData other)
        {
            if (other == null) return false;
            return ID == other.ID;
        }

        // ====== Client Only ======
        /// <summary> 
        /// [Helper] 클라이언트 환경에서 현재 데이터의 마스터 정보(이름, 이미지 등 원본 데이터)를 가져옵니다. 
        /// </summary>
        public CharacterData GetCharacterData()
        {
            // GameMaster 내 csvHelper를 통해 캐싱된 마스터 데이터 리스트에서 정보를 참조
            return GameMaster.Instance.csvHelper.GetScripteData<CharacterDataList>().GetData(ID);
        }
    }

    /// <summary>
    /// [User Profile] 서버-클라이언트 간 동기화되는 유저 데이터의 최상위 루트 모델입니다.
    /// </summary>
    [Serializable]
    public class UserInfo
    {
        [Header("User Basic Info")]
        public string NickName; // 유저 닉네임
        public string UID;      // 고유 계정 ID

        [Header("Currency")]
        public UserGoods userGoods; // 소지 재화 정보

        [Header("Characters")]
        public UserCollection userCollection; // 캐릭터 인벤토리 및 덱 정보
    }

    /// <summary>
    /// [Economy] 재화 데이터 모델입니다. 
    /// 64비트 정수(long)를 사용하여 인플레이션 발생 시의 데이터 오버플로우를 방지합니다.
    /// </summary>
    [Serializable]
    public class UserGoods
    {
        public long Goods;     // 무료 재화 (예: 인게임 골드)
        public long RealGoods; // 유료 재화 (예: 유료 다이아몬드)
    }

    /// <summary>
    /// [Inventory] 사용자가 보유한 전체 캐릭터 목록과 설정된 덱 리스트를 포함하는 컬렉션 관리 클래스입니다.
    /// </summary>
    [Serializable]
    public class UserCollection
    {
        [Header("Collection")]
        /// <summary> 유저가 보유한 전체 캐릭터(인스턴스 데이터) 목록 </summary>
        public List<UserCharacterData> OwnCharacterList;

        /// <summary> 서버에 저장된 프리셋 덱(팀 구성) 리스트 </summary>
        public List<UserCharacterDataDeckInfo> OwnDeckList;

        /// <summary> 로비 화면 등에 노출될 대표 캐릭터의 ID </summary>
        public int mainCharacterID;
    }

    /// <summary>
    /// [Deck Preset] 유저가 설정한 특정 번호의 덱 정보(인덱스 및 포함 캐릭터)를 담습니다.
    /// </summary>
    [Serializable]
    public class UserCharacterDataDeckInfo
    {
        public int deckIndex = 0; // 덱 번호 (예: 1번 덱, 2번 덱)

        /// <summary> 해당 덱에 포함된 유닛들의 성장 정보 리스트 </summary>
        public List<UserCharacterData> userCharacterDatas;
    }

    // ----------------------------------------------------------------------
    // ## Network Protocol (Request / Response)
    // ----------------------------------------------------------------------

    // --- 전체 정보 통신 프로토콜 ---
    /// <summary> 전체 유저 정보 조회를 위한 API 요청 헤더 </summary>
    public class UserInfoRequset : RequsetHeader
    {
        public override string GetRutor() => "Info/GetUserInfo";
    }

    /// <summary> 서버로부터 전달받은 전체 유저 데이터 패킷 </summary>
    public class UserInfoResponse : Response
    {
        public UserInfo userInfo;
    }

    // --- 캐릭터 컬렉션 통신 프로토콜 ---
    /// <summary> 유저의 캐릭터 인벤토리 및 덱 정보만 요청하는 헤더 </summary>
    public class UserCharacterDataRequest : RequsetHeader
    {
        public override string GetRutor() => "Info/UserCharacterData";
    }

    /// <summary> 서버로부터 전달받은 캐릭터 컬렉션 데이터 패킷 </summary>
    public class UserCharacterDataResponse : Response
    {
        public UserCollection userCollection;
    }

    // --- 재화 통신 프로토콜 ---
    /// <summary> 유저의 현재 재화 상태만 요청하는 헤더 </summary>
    public class UserGoodsDataRequest : RequsetHeader
    {
        public override string GetRutor() => "Info/UserCharacterGoods";
    }

    /// <summary> 서버로부터 전달받은 재화 정보 패킷 </summary>
    public class UserGoodsDataResponse : Response
    {
        // 필드명이 userCollection으로 되어 있으나 실제로는 UserGoods 데이터를 담습니다.
        public UserGoods userCollection;
    }
}