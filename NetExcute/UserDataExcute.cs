using System;
using System.Collections.Generic;
using UnityEngine;

namespace NetExcute
{
    /// <summary>
    /// 서버와 클라이언트 간에 공유되는 사용자의 전체 정보를 담는 데이터 모델입니다.
    /// </summary>
    [Serializable]
    public class UserInfo
    {
        /// <summary>
        /// 사용자가 보유한 개별 캐릭터의 성장 상태를 관리하는 클래스입니다.
        /// </summary>
        [Serializable]
        public class UserCharacterData
        {
            public int ID;      // 캐릭터 고유 식별 번호 (CharacterData와 매칭)
            public int level;   // 현재 캐릭터 레벨
            public int Enforce; // 강화(능력치 상승) 수치
            public int Rank;    // 동일 캐릭터 중복 획득 등을 통한 초월(Rank Up) 등급
        }

        [Header("User Basic Info")]
        public string NickName;     // 사용자 닉네임
        public string UID;          // 사용자 고유 계정 ID

        [Header("Currency")]
        public long Goods;          // 인게임 획득 재화 (골드 등)
        public long RealGoods;      // 유료 구매 재화 (다이아 등)

        [Header("Collection")]
        public List<UserCharacterData> OwnCharacterList; // 사용자가 보유 중인 캐릭터 리스트
        public int mainCharacterID; // 로비/메인 화면에 노출될 대표 캐릭터 ID
    }

    // ----------------------------------------------------------------------
    // ## API Request / Response Protocols
    // ----------------------------------------------------------------------

    /// <summary>
    /// 서버에 유저 정보를 요청하기 위한 데이터 헤더입니다.
    /// </summary>
    public class UserInfoRequset : RequsetHeader
    {
        /// <summary> API 서버의 유저 정보 획득 엔드포인트 경로를 반환합니다. </summary>
        public override string GetRutor()
        {
            return "Info/GetUserInfo";
        }
    }

    /// <summary>
    /// 서버로부터 받은 유저 정보 응답 결과를 담는 클래스입니다.
    /// </summary>
    public class UserInfoResponse : Response
    {
        /// <summary> 성공 시 서버에서 전달된 실제 유저 데이터 객체입니다. </summary>
        public UserInfo userInfo;
    }
}