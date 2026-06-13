using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "UIScriptableData", menuName = "Scriptable Objects/UIScriptableData")]
public class UIScriptableData : ScriptableObject
{
    public List<UIBaseData> m_UIDataList;
 
    public void SettingData(List<UIBaseData> data)
    {
        m_UIDataList = data;

#if UNITY_EDITOR
        AssetDatabase.SaveAssetIfDirty(this);
        AssetDatabase.SaveAssets();
#endif
    }

    public async UniTask MakeUIList(Transform command, Transform mainUI, Transform InGame, AddressableManager addressableManager)
    {
        var uiData = m_UIDataList;
        if (uiData == null) return;

        List<UniTask> tasks = new();

        foreach (var d in uiData)
        {
            switch (d.uiType)
            {
                case UIBaseData.UIType.Command:
                    tasks.Add(InstantiateObjectSetting(d, command, addressableManager));
                    break;
                case UIBaseData.UIType.MainUI:
                    tasks.Add(InstantiateObjectSetting(d, mainUI, addressableManager));
                    break;
                case UIBaseData.UIType.InGameUI:
                    tasks.Add(InstantiateObjectSetting(d, InGame, addressableManager));
                    break;
            }
        }

        await UniTask.WhenAll(tasks);
    }

    private async UniTask InstantiateObjectSetting(UIBaseData data, Transform parent, AddressableManager addressableManager)
    {
        var obj = await addressableManager.InstantiateObjectAsync(data.dataName, parent);

        if (obj == null) return;

        RectTransform rect = (RectTransform)obj.transform;

        rect.gameObject.name = data.dataName;
        rect.anchorMin = data.GetAchorMinMax().min;
        rect.anchorMax = data.GetAchorMinMax().max;
        rect.pivot = data.GetPivot();
        rect.anchoredPosition = data.GetAnchorPos();
        rect.sizeDelta = data.GetSizeDetail();
    }

#if UNITY_EDITOR
    [ContextMenu("Make Json")]
    public void MakeJson()
    {
        var settings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore
        };

        string data = JsonConvert.SerializeObject(m_UIDataList, settings);

        string path = Path.Combine($"{Application.dataPath}/TextAsset/", "UIData.json");

        File.WriteAllText(path, data);
        Logger.Log($"[UIScriptableData] JSON 파일이 다음 경로에 저장되었습니다: {path}");

        AssetDatabase.Refresh();
    }

    [ContextMenu("Load Json")]
    public void LoadJson(AddressableManager addressable)
    {
        LoadJsonUI(addressable).Forget();
    }

    public async UniTask LoadJsonUI(AddressableManager addressable)
    {
        var data = await addressable.LoadAssetAndCacheAsync<TextAsset>("UIData");

        if (data == null)
        {
            Logger.LogError("[UIScriptableData] Addressables에서 UIData를 로드하는 데 실패했습니다.");
            return;
        }

        m_UIDataList = JsonConvert.DeserializeObject<List<UIBaseData>>(data.text);

        Logger.Log($"[UIScriptableData] JSON 로드 성공. 로드된 항목 수: {m_UIDataList?.Count}");
    }
#endif
}