using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 스테이지 선택 화면을 관리하는 클래스입니다.
/// 외부 JSON 데이터를 로드하여 스테이지 버튼들을 동적으로 생성합니다.
/// </summary>
public class StagePanel : UIBase
{
    // ====== UI Binding Enums (CachObject 시스템 활용) ======
    enum RectTransforms
    {
        Content // 스테이지 버튼들이 생성되어 배치될 부모 컨텐츠 영역
    }

    // ----------------------------------------------------------------------
    // ## Initialization (Lifecycle)
    // ----------------------------------------------------------------------

    protected override void Awake()
    {
        // 초기화 로직 실행
        Init();
    }

    /// <summary>
    /// 패널을 초기화하고 어드레서블 리소스(JSON)로부터 스테이지 리스트를 구성합니다.
    /// </summary>
    public void Init()
    {
        // UI 컴포넌트 바인딩
        Bind<RectTransform>(typeof(RectTransforms));

        var rect = Get<RectTransform>((int)RectTransforms.Content);

        // 자식 객체 중 비활성화되어 숨겨져 있는 원본 버튼 프리팹을 참조합니다.
        var baseprefab = GetComponentInChildren<StageMoveButton>(true);

        // 비동기 태스크 실행 (Fire and Forget)
        TaskHelp().Forget();

        /// <summary> 비동기로 스테이지 데이터를 로드하고 버튼을 생성하는 내부 함수 </summary>
        async UniTask TaskHelp()
        {
            // 1. 어드레서블 매니저를 통해 "StageList" 키를 가진 텍스트 자산 로드
            var textAsset = await GameMaster.Instance.addressableManager.LoadAssetAndCacheAsync<TextAsset>("StageList");

            // 2. 로드된 JSON 텍스트를 StageData 객체로 역직렬화(Deserialize)
            StageData stage = JsonConvert.DeserializeObject<StageData>(textAsset.text);

            // 3. 2중 루프를 통해 메인 스테이지 및 서브 스테이지 버튼 동적 생성
            for (int mainStage = 0; mainStage < stage.MainStageCount; mainStage++)
            {
                for (int subStage = 0; subStage < stage.SubStages[mainStage].SubStageCount; subStage++)
                {
                    // 버튼 프리팹 복제 및 데이터 설정
                    var obje = Instantiate(baseprefab, rect);
                    obje.Init(mainStage, subStage + 1); // 메인 스테이지 번호와 서브 번호 전달
                    obje.gameObject.SetActive(true);    // 버튼 활성화
                }
            }

            // 4. 생성된 버튼들에 맞춰 레이아웃을 즉시 갱신 (스크롤 뷰 영역 등 계산)
            LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
        }
    }

    // ----------------------------------------------------------------------
    // ## Editor Tools
    // ----------------------------------------------------------------------

#if UNITY_EDITOR
    /// <summary>
    /// 에디터 인스펙터 메뉴를 통해 기본 스테이지 JSON 형식을 로그로 출력하는 툴입니다.
    /// </summary>
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

// ----------------------------------------------------------------------
// ## Data Models (JSON Mapping)
// ----------------------------------------------------------------------

/// <summary>
/// 전체 스테이지 구성을 담는 데이터 클래스입니다.
/// </summary>
[Serializable]
public class StageData
{
    public int MainStageCount = 0; // 총 메인 스테이지 개수

    public List<SubStageData> SubStages = new(); // 각 메인 스테이지별 서브 스테이지 상세 정보

    /// <summary>
    /// 새로운 스테이지 데이터를 추가하거나 기존 스테이지의 서브 개수를 늘립니다. (데이터 생성용)
    /// </summary>
    public void AddStageData(int mainStage)
    {
        // 해당 메인 스테이지가 이미 존재하면 서브 스테이지 카운트만 증가
        if (SubStages.Any(x => x.MainStage == mainStage))
            SubStages.Find(x => x.MainStage == mainStage).SubStageCount++;
        else
        {
            // 존재하지 않으면 리스트에 추가하고 전체 카운트 증가
            MainStageCount++;
            SubStages.Add(new() { MainStage = mainStage, SubStageCount = 1 });
        }
    }
}

/// <summary>
/// 개별 메인 스테이지에 속한 서브 스테이지 정보를 담는 클래스입니다.
/// </summary>
[Serializable]
public class SubStageData
{
    public int MainStage;      // 부모 메인 스테이지 번호
    public int SubStageCount;  // 해당 메인 스테이지가 포함한 서브 스테이지 개수
}