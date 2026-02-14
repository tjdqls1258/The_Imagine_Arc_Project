using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace NetExcute
{
    /// <summary>
    /// 게임의 웹 통신을 총괄하는 중앙 실행 클래스입니다.
    /// 유니티의 UnityWebRequest를 UniTask와 결합하여 비동기 HTTP 통신을 수행합니다.
    /// </summary>
    public class NetExcute : Singleton<NetExcute>
    {
        /// <summary> 어플리케이션 종료 또는 필요 시 통신을 일괄 취소하기 위한 토큰 소스입니다. </summary>
        public CancellationTokenSource cancellation = new();

        /// <summary>
        /// [비동기] 서버에 API 요청을 보내고 응답을 처리합니다.
        /// </summary>
        /// <typeparam name="T">Response를 상속받은 응답 데이터 타입</typeparam>
        /// <param name="header">요청 경로 및 본문 데이터를 포함한 헤더 객체</param>
        /// <param name="requsetAction">통신 성공 시 실행될 콜백 (응답 객체 전달)</param>
        /// <param name="fail">통신 실패(네트워크 오류 등) 시 실행될 콜백</param>
        public async UniTask Requset<T>(RequsetHeader header, Action<T> requsetAction, Action fail) where T : Response
        {
            // 1. 서버 베이스 URL과 API 상세 경로를 결합하여 전체 URL 생성
            string url = Path.Combine(Config.WebURL, header.GetRutor());
            Logger.Log($"[Tag RequsetData] Requset {url}");

            // 2. UnityWebRequest 설정 (using 사용으로 통신 완료 후 자동 자원 해제)
            using (UnityWebRequest unityWeb = new UnityWebRequest(url, header.GetMethod()))
            {
                // 요청 본문(JSON) 데이터 설정
                unityWeb.uploadHandler = new UploadHandlerRaw(header.GetData());
                // 응답 데이터를 받기 위한 버퍼 설정
                unityWeb.downloadHandler = new DownloadHandlerBuffer();
                // HTTP 헤더 설정 (JSON 통신 명시)
                unityWeb.SetRequestHeader("Content-Type", RequsetHeader.REQUSET_CONTENT_TYPE);

                try
                {
                    // 3. 서버에 요청을 보내고 비동기 대기 (cancellation 토큰 연결)
                    await unityWeb.SendWebRequest().ToUniTask(cancellationToken: cancellation.Token);

                    // 4. 결과 처리
                    if (unityWeb.result == UnityWebRequest.Result.Success)
                    {
                        string downLoadValue = unityWeb.downloadHandler.text;

                        if (downLoadValue != string.Empty)
                        {
                            // JSON 데이터를 지정된 응답 객체 T로 역직렬화
                            T res = JsonConvert.DeserializeObject<T>(downLoadValue);

                            // 성공 콜백 호출
                            if (requsetAction != null)
                                requsetAction.Invoke(res);
                        }
                    }
                    else
                    {
                        // 서버 오류, 타임아웃 등 네트워크 실패 처리
                        Logger.LogError($"[NetExcute] Request Fail : {unityWeb.error}");
                        if (fail != null)
                            fail.Invoke();
                    }
                }
                catch (OperationCanceledException)
                {
                    Logger.Log("[NetExcute] Request was canceled.");
                }
            }
        }
    }

    /// <summary>
    /// 서버로부터 받는 모든 응답 데이터의 베이스 클래스입니다.
    /// 에러 코드 확인 및 성공 여부를 판단합니다.
    /// </summary>
    [Serializable]
    public class Response
    {
        /// <summary> 에러 코드 (보통 0이면 성공, 나머지는 에러를 의미함) </summary>
        public int error = -1;

        /// <summary> 서버 로직상의 성공 여부를 반환합니다. </summary>
        public virtual bool IsSuccess()
        {
            return error == 0;
        }
    }

    /// <summary>
    /// 서버에 보낼 요청 데이터를 정의하는 추상 클래스입니다.
    /// API 경로, 메서드, 데이터 직렬화 로직을 포함합니다.
    /// </summary>
    [Serializable]
    public abstract class RequsetHeader
    {
        public const string REQUSET_CONTENT_TYPE = "application/json";

        /// <summary> API의 세부 경로(Endpoint)를 반환해야 합니다. </summary>
        public abstract string GetRutor();

        /// <summary> HTTP 메서드(GET, POST, PUT 등)를 정의합니다. 기본은 POST입니다. </summary>
        public virtual string GetMethod()
        {
            return "post";
        }

        /// <summary> 객체의 필드 데이터를 UTF8 인코딩된 바이트 배열(JSON)로 변환합니다. </summary>
        public virtual byte[] GetData()
        {
            return Encoding.UTF8.GetBytes(JsonUtility.ToJson(this));
        }

        /// <summary> 디버깅 또는 로그 확인을 위한 직렬화 문자열을 반환합니다. </summary>
        public virtual string RequsetStaringData()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}