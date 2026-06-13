using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UniRx;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using VContainer;

namespace NetExcute
{
    [Serializable]
    public class DownloadClass<T> where T : Response
    {
        public T data;
    }

    public class NetExcute : Singleton<NetExcute>
    {
        public CancellationTokenSource cancellation = new();

        public async UniTask Requset<T>(RequsetHeader header, Action<T> requsetAction, Action fail) where T : Response
        {
            string url = $"{Config.WebURL}{header.GetRutor()}";
            string method = header.GetMethod().ToUpper();
            Logger.Log($"[Tag RequsetData] Requset {url}, {JsonConvert.SerializeObject(header)}");

            if (method == "GET")
            {
                string quertString = header.GetQueryString();
                if (!string.IsNullOrEmpty(quertString))
                {
                    url += $"?{quertString}";
                }
            }

            MessageBroker.Default.Publish(new UIManager.LoadingEvent(true));
            using (UnityWebRequest unityWeb = new UnityWebRequest(url, method))
            {
                if (method != "GET")
                {
                    byte[] data = header.GetData();
                    if (data != null && data.Length > 0)
                    {
                        unityWeb.uploadHandler = new UploadHandlerRaw(header.GetData());
                        unityWeb.SetRequestHeader("Content-Type", RequsetHeader.REQUSET_CONTENT_TYPE);
                    }
                }
                unityWeb.downloadHandler = new DownloadHandlerBuffer();

                try
                {
                    await unityWeb.SendWebRequest().ToUniTask(cancellationToken: cancellation.Token);

                    if (unityWeb.result == UnityWebRequest.Result.Success)
                    {
                        string downLoadValue = unityWeb.downloadHandler.text;
                        Logger.Log($"Web Request Success : {downLoadValue}");
                        DownloadClass<T> downloadClass = JsonConvert.DeserializeObject<DownloadClass<T>>(downLoadValue);

                        if (downLoadValue != string.Empty)
                        {
                            T res = downloadClass.data;

                            if (requsetAction != null)
                                requsetAction.Invoke(res);
                        }
                    }
                    else
                    {
                        Logger.LogError($"[NetExcute] Request Fail : {unityWeb.error}");
                        if (fail != null)
                            fail.Invoke();
                    }
                }
                catch (OperationCanceledException)
                {
                    Logger.Log("[NetExcute] Request was canceled.");
                    if (fail != null)
                        fail.Invoke();
                }
                catch(Exception ex)
                {
                    Logger.LogError(ex.ToString());
                    if (fail != null)
                        fail.Invoke();
                }

                MessageBroker.Default.Publish(new UIManager.LoadingEvent(false));
            }
        }
    }

    [Serializable]
    public class Response
    {
        public int error = -1;

        public virtual bool IsSuccess()
        {
            return error == 0;
        }
    }

    [Serializable]
    public abstract class RequsetHeader
    {
        public const string REQUSET_CONTENT_TYPE = "application/json";

        public abstract string GetRutor();

        public virtual string GetMethod()
        {
            return "post";
        }

        public virtual byte[] GetData()
        {
            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(this));
        }

        public virtual string RequsetStaringData()
        {
            return JsonConvert.SerializeObject(this);
        }

        public string GetQueryString()
        {
            string json = JsonConvert.SerializeObject(this);
            var jObj = JObject.Parse(json);

            var properties = jObj.Properties()
                .Where(p => p.Value.Type != JTokenType.Null) // null 값 제외
                .Select(p => $"{p.Name}={Uri.EscapeDataString(p.Value.ToString())}");

            return string.Join("&", properties);
        }
    }
}