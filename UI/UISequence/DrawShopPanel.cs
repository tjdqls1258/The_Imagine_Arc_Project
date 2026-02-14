using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Random = UnityEngine.Random;

/// <summary>
/// 캐릭터 뽑기(상점) 화면을 관리하는 클래스입니다.
/// 가챠 로직 실행, 결과 연출(Timeline) 실행, 뽑기 결과 카드 표시 및 메모리 해제를 담당합니다.
/// </summary>
public class DrawShopPanel : UIBase
{
    // ====== UI Binding Enums (CachObject 시스템 활용) ======

    /// <summary> 뽑기 버튼 종류 </summary>
    enum DrawButton
    {
        Draw1,  // 1회 뽑기
        Draw10, // 10회 뽑기
    }

    /// <summary> 뽑기 결과를 시각적으로 보여줄 카드 이미지 리스트 (최대 10개) </summary>
    enum characterCards
    {
        Character, Character_1, Character_2, Character_3, Character_4,
        Character_5, Character_6, Character_7, Character_8, Character_9
    }

    /// <summary> 뽑기 연출을 담당하는 타임라인 컨트롤러 컴포넌트 </summary>
    enum DrawTineLineCom
    {
        DrawAction,
    }

    /// <summary> 현재 뽑기 결과로 생성된 캐릭터 데이터 리스트 (메모리 해제 관리용) </summary>
    private List<CharacterData> currentData;

    // ----------------------------------------------------------------------
    // ## Initialization (Lifecycle)
    // ----------------------------------------------------------------------

    protected override void Awake()
    {
        // 1. UI 시퀀스 설정 및 컴포넌트 자동 바인딩
        m_UISequence = UIManager.UISequence.ShopPanel;
        Bind<Button>(typeof(DrawButton));
        Bind<DrawTimeLine>(typeof(DrawTineLineCom));
        Bind<Image>(typeof(characterCards));

        Init();

        // 2. 버튼 클릭 이벤트 리스너 등록
        Get<Button>((int)DrawButton.Draw1).onClick.AddListener(() =>
        {
            DrawCharacter(1);
        });

        Get<Button>((int)DrawButton.Draw10).onClick.AddListener(() =>
        {
            DrawCharacter(10);
        });
    }

    /// <summary>
    /// UI가 화면에 나타날 때 호출됩니다. 기존에 표시되던 카드들을 초기화합니다.
    /// </summary>
    public override void ShowUI()
    {
        base.ShowUI();

        // 결과 카드 이미지들을 모두 비활성화 처리
        foreach (var objName in Enum.GetValues(typeof(characterCards)))
        {
            Get<Image>((int)objName).gameObject.SetActive(false);
        }
    }

    // ----------------------------------------------------------------------
    // ## Gacha Logic
    // ----------------------------------------------------------------------

    /// <summary>
    /// 캐릭터 뽑기 로직을 실행합니다.
    /// </summary>
    /// <param name="drawCount">뽑기 횟수 (1 또는 10)</param>
    private void DrawCharacter(int drawCount)
    {
        // 1. 이전 뽑기 결과 리소스 메모리 해제
        UnLoadCurrentData();

        // 2. 랜덤 ID 생성 (테스트용: ID 1~30 사이의 랜덤 캐릭터)
        List<int> ids = new();
        for (int i = 0; i < drawCount; i++)
        {
            ids.Add(Random.Range(1, 30));
        }

        int count = 0;
        currentData = new List<CharacterData>();

        // 3. 연출용 타임라인 오브젝트 활성화
        Get<DrawTimeLine>(0).gameObject.SetActive(true);

        // 4. 생성된 ID를 기반으로 데이터 로드 및 UI 카드 셋팅
        foreach (int id in ids)
        {
            // CSV 헬퍼를 통해 캐릭터 원본 데이터 참조
            var characterDtat = GameMaster.Instance.csvHelper.GetScripteData<CharacterDataList>().GetData(id);
            currentData.Add(characterDtat);

            int currentCount = count;

            // 결과 카드 오브젝트 활성화 및 이미지 비동기 로드
            Get<Image>(currentCount).gameObject.SetActive(true);
            characterDtat.GetCharacterSprite(targetImage: Get<Image>(currentCount)).Forget();

            count += 1;
        }

        // 5. 타임라인 컨트롤러에 데이터를 넘겨 연출 실행
        Get<DrawTimeLine>(0).DrawCharacter(currentData.ToArray());
    }

    // ----------------------------------------------------------------------
    // ## Cleanup & Memory Management
    // ----------------------------------------------------------------------

    /// <summary>
    /// UI가 닫힐 때 호출됩니다. 현재 표시된 리소스들을 정리합니다.
    /// </summary>
    public override void CloseUI(bool isClosetAll = false)
    {
        UnLoadCurrentData();
        base.CloseUI(isClosetAll);
    }

    /// <summary>
    /// 현재 로드된 캐릭터 스프라이트(Atlas) 리소스들을 어드레서블 메모리에서 해제합니다.
    /// </summary>
    private void UnLoadCurrentData()
    {
        if (currentData == null) return;

        foreach (var item in currentData)
        {
            // 각 캐릭터 데이터가 들고 있는 아틀라스 참조 해제
            item.UnloadAtlas();
        }
        currentData.Clear();
    }
}