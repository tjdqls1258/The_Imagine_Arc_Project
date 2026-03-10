using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// 스테이지 선택 화면에서 개별 스테이지 항목을 담당하는 버튼 클래스입니다.
/// 클릭 시 해당 스테이지 정보를 설정하고 게임 씬으로의 전환을 관리합니다.
/// </summary>
public class StageMoveButton : CachObject
{
    // ====== UI Binding Enums ======
    enum Texts
    {
        StageText // 스테이지 번호(예: 1-1)를 표시할 텍스트
    }

    // ====== Runtime Data ======
    private int main;         // 메인 스테이지 인덱스
    private int sub;          // 서브 스테이지 인덱스
    private Button m_myButton; // 버튼 컴포넌트 레퍼런스

    // ----------------------------------------------------------------------
    // ## Initialization
    // ----------------------------------------------------------------------

    /// <summary>
    /// 버튼의 스테이지 정보를 초기화하고 텍스트 및 이벤트를 설정합니다.
    /// </summary>
    /// <param name="mainStage">메인 스테이지 번호</param>
    /// <param name="subStage">서브 스테이지 번호</param>
    public void Init(int mainStage, int subStage)
    {
        main = mainStage;
        sub = subStage;

        // 1. 컴포넌트 참조 및 바인딩
        m_myButton = GetComponent<Button>();
        Bind<TextMeshProUGUI>(typeof(Texts));

        // 2. 버튼 클릭 리스너 등록
        m_myButton.onClick.AddListener(SceneMove);

        // 3. UI 텍스트 업데이트 (출력 형식: "0-1")
        Get<TextMeshProUGUI>((int)Texts.StageText).text = $"{main}-{sub}";
    }

    // ----------------------------------------------------------------------
    // ## Scene Transition Logic
    // ----------------------------------------------------------------------

    /// <summary>
    /// 버튼 클릭 시 호출되며, 선택한 스테이지 데이터를 저장하고 게임 씬을 로드합니다.
    /// </summary>
    private void SceneMove()
    {
        // 1. 싱글톤 데이터 매니저에 현재 선택한 스테이지 정보 저장
        GameData.Instance.MainStage = main;
        GameData.Instance.SubStage = sub;

        // 2. 씬 로드 매니저를 통해 인게임 씬(GameScene)으로 전환
        // Forget()을 사용하여 비동기 연산의 완료를 기다리지 않고 흐름을 이어갑니다.
        SceneLoadManager.Instance.SceneLoad(SceneInfo.SceneType.GameScene, () =>
        {
            // [씬 로드 완료 후 실행될 콜백 로직]
            var userCharacterData = GameMaster.Instance.dataManager.GetUserData<UserData>() as UserData;

            // A. 인게임 UI 매니저를 찾아 초기 데이터(테스트용)를 세팅합니다.
            GameMaster.Instance.uiManager.AutoUIManager
                .GetCompoent<InGameUIManager>(UIBaseData.UIType.InGameUI)
                .SetInGameData(userCharacterData.m_characterDeckList[1]);

            // B. 전체 UI 상태를 'InGame' 모드로 전환하여 필요한 UI들을 활성화합니다.
            GameMaster.Instance.uiManager.AutoUIManager.SetUIType(AutoUIManager.UIType.inGame);

        }).Forget();
    }
}