using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 버튼 클릭 시 지정된 씬으로 이동하고, 로드 완료 후 UI 상태를 자동으로 변경하는 컴포넌트입니다.
/// 로비에서 게임으로, 또는 게임에서 결과 화면으로 이동하는 등의 흐름 제어에 사용됩니다.
/// </summary>
public class ButtonSceneMove : UIBaseFormMaker
{
    // ====== UI Components ======
    private Button m_button;

    // ====== Inspector Settings ======
    [Header("Scene Load Settings")]
    [Tooltip("이동하고자 하는 타겟 씬의 타입입니다.")]
    [SerializeField] private SceneInfo.SceneType m_loadScene;

    [Tooltip("씬 로드 완료 후 적용할 UI 모드(Lobby, InGame 등)입니다.")]
    [SerializeField] private AutoUIManager.UIType m_uiType;

    /// <summary> 씬 로드가 완료된 시점에 실행될 콜백 액션 체인입니다. </summary>
    private Action m_sceneLoadAction = null;

    // ----------------------------------------------------------------------
    // ## Initialization
    // ----------------------------------------------------------------------

    protected override void Awake()
    {
        // 1. 버튼 컴포넌트 참조 및 초기화
        m_button = GetComponent<Button>();
        m_button.onClick.RemoveAllListeners();
        m_button.onClick.AddListener(OnClickButton);

        // 2. 기본 씬 로드 콜백 등록: 설정된 UI 타입으로 자동 전환
        m_sceneLoadAction += () =>
        {
            // 씬 이동 후 해당 씬에 맞는 UI 레이아웃(Lobby/InGame 등)을 활성화합니다.
            GameMaster.Instance.uiManager.AutoUIManager.SetUIType(m_uiType);
        };
    }

    /// <summary>
    /// 외부에서 씬 로드 완료 후 실행할 추가 로직(데이터 초기화 등)을 등록할 수 있습니다.
    /// </summary>
    /// <param name="action">추가할 Action 콜백</param>
    public void AddSceneLoadAction(Action action)
    {
        m_sceneLoadAction += action;
    }

    // ----------------------------------------------------------------------
    // ## Scene Transition Logic
    // ----------------------------------------------------------------------

    /// <summary>
    /// 버튼이 클릭되었을 때 실행됩니다.
    /// SceneLoadManager를 통해 비동기로 씬 전환을 시작합니다.
    /// </summary>
    private void OnClickButton()
    {
        // SceneLoadManager에 씬 타입과 완료 후 실행할 콜백 리스트를 전달합니다.
        // Forget()을 사용하여 비동기 호출을 비차단(Non-blocking) 방식으로 실행합니다.
        GameMaster.Instance.sceneLoadManager.SceneLoad(m_loadScene, m_sceneLoadAction).Forget();
    }
}