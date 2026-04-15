using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.UI;

public class ButtonSceneMove : UIBaseFormMaker
{
    private Button m_button;

    [Header("Scene Load Settings")]
    [SerializeField] private SceneInfo.SceneType m_loadScene;
    [SerializeField] private AutoUIManager.UIType m_uiType;

    private Action m_sceneLoadAction = null;

    protected override void Awake()
    {
        m_button = GetComponent<Button>();
        m_button.onClick.RemoveAllListeners();
        m_button.onClick.AddListener(OnClickButton);

        m_sceneLoadAction += () =>
        {
            GameMaster.Instance.uiManager.GetAutoUIManager().SetUIType(m_uiType);
        };
    }

    public void AddSceneLoadAction(Action action)
    {
        m_sceneLoadAction += action;
    }

    private void OnClickButton()
    {
        GameMaster.Instance.sceneLoadManager.SceneLoad(m_loadScene, m_sceneLoadAction).Forget();
    }
}