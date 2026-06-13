using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class StagePanel : UIBase
{
    [Inject] private readonly AddressableManager addressableManager;
    [Inject] private readonly UserDataManager dataManager;
    [Inject] private readonly SceneLoadManager sceneLoadManager;

    enum RectTransforms
    {
        Content
    }

    protected override void Awake()
    {
        Init();
    }

    public void Init()
    {
        Bind<RectTransform>(typeof(RectTransforms));

        var rect = Get<RectTransform>((int)RectTransforms.Content);

        var baseprefab = GetComponentInChildren<StageMoveButton>(true);

        TaskHelp().Forget();

        async UniTask TaskHelp()
        {
            var textAsset = await addressableManager.LoadAssetAndCacheAsync<TextAsset>("StageList");

            StageData stage = JsonConvert.DeserializeObject<StageData>(textAsset.text);

            for (int mainStage = 0; mainStage < stage.MainStageCount; mainStage++)
            {
                for (int subStage = 0; subStage < stage.SubStages[mainStage].SubStageCount; subStage++)
                {
                    var obje = Instantiate(baseprefab, rect);
                    obje.Init(mainStage, subStage + 1, sceneLoadManager, dataManager, uiManager); // 메인 스테이지 번호와 서브 번호 전달
                    obje.gameObject.SetActive(true);    // 버튼 활성화
                }
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
        }
    }


#if UNITY_EDITOR
    [ContextMenu("Make Json")]
    private void MakeJson()
    {
        StageData stagedata = new();
        stagedata.SubStages = new();

        stagedata.MainStageCount = 1;
        stagedata.SubStages.Add(new() { MainStage = 0, SubStageCount = 1 });

        // 결과물을 JSON 문자열로 변환하여 출력
        Logger.Log(JsonConvert.SerializeObject(stagedata));
    }
#endif
}

[Serializable]
public class StageData
{
    public int MainStageCount = 0; 
    public List<SubStageData> SubStages = new(); 

    public void AddStageData(int mainStage)
    {
        if (SubStages.Any(x => x.MainStage == mainStage))
            SubStages.Find(x => x.MainStage == mainStage).SubStageCount++;
        else
        {
            MainStageCount++;
            SubStages.Add(new() { MainStage = mainStage, SubStageCount = 1 });
        }
    }
}

[Serializable]
public class SubStageData
{
    public int MainStage;      // 부모 메인 스테이지 번호
    public int SubStageCount;  // 해당 메인 스테이지가 포함한 서브 스테이지 개수
}