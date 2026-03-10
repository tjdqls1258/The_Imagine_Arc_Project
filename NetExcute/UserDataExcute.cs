using System;
using System.Collections.Generic;
using UnityEngine;

namespace NetExcute
{
    /// <summary>
    /// [Instance Data] 유저가 보유한 개별 캐릭터의 유동적인 성장 상태를 관리합니다.
    /// 마스터 데이터(CharacterData)의 ID를 참조하여 조인(Join)하는 구조입니다.
    /// </summary>
    [Serializable]
    public class UserCharacterData
    {
        public int ID;      // 캐릭터 식별 번호 (Static Data와 연결)
        public int level;   // 레벨
        public int Enforce; // 강화 수치
        public int Rank;    // 초월 등급
    }

    /// <summary>
    /// [User Profile] 서버-클라이언트 간 동기화되는 유저 데이터의 루트 모델입니다.
    /// </summary>
    [Serializable]
    public class UserInfo
    {
        [Header("User Basic Info")]
        public string NickName;
        public string UID;

        [Header("Currency")]
        public UserGoods userGoods;

        [Header("Characters")]
        public UserCollection userCollection;
    }

    /// <summary>
    /// [Economy] 재화 데이터. 64비트 정수(long)를 사용하여 재화 인플레이션 및 오버플로우를 방지합니다.
    /// </summary>
    [Serializable]
    public class UserGoods
    {
        public long Goods;     // 무료 재화 (Gold 등)
        public long RealGoods; // 유료 재화 (Diamond 등)
    }

    /// <summary>
    /// [Inventory] 보유 캐릭터 및 덱 구성 정보를 포함한 컬렉션 관리 클래스입니다.
    /// </summary>
    [Serializable]
    public class UserCollection
    {
        [Header("Collection")]
        /// <summary> 유저가 보유한 전체 캐릭터 목록 </summary>
        public List<UserCharacterData> OwnCharacterList;
        /// <summary> 저장된 프리셋 덱 목록 </summary>
        public List<UserCharacterDataDeckInfo> OwnDeckList;
        /// <summary> 대표 캐릭터 설정값 </summary>
        public int mainCharacterID;
    }

    /// <summary>
    /// [Deck Preset] 유저가 설정한 특정 덱의 인덱스와 포함된 캐릭터 정보를 담습니다.
    /// </summary>
    [Serializable]
    public class UserCharacterDataDeckInfo
    {
        public int deckIndex = 0;
        /// <summary> 덱에 포함된 유닛들의 성장 데이터 리스트 </summary>
        public List<UserCharacterData> userCharacterDatas;
    }

    // ----------------------------------------------------------------------
    // ## Network Protocol (Request / Response)
    // ----------------------------------------------------------------------

    /// <summary> 유저 정보 요청을 위한 API 헤더 </summary>
    public class UserInfoRequset : RequsetHeader
    {
        public override string GetRutor() => "Info/GetUserInfo";
    }

    /// <summary> 서버로부터 전달받은 유저 데이터 패킷 </summary>
    public class UserInfoResponse : Response
    {
        public UserInfo userInfo;
    }

    public class UserCharacterDataRequest : RequsetHeader
    {
        public override string GetRutor()
        {
            return "Info/UserCharacterData";
        }
    }

    public class UserCharacterDataResponse : Response
    {
        public UserCollection userCollection;
    }

    public class UserGoodsDataRequest : RequsetHeader
    {
        public override string GetRutor()
        {
            return "Info/UserCharacterGoods";
        }
    }

    public class UserGoodsDataResponse : Response
    {
        public UserGoods userCollection;
    }

}