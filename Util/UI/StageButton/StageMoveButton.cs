using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine.UI;

public class StageMoveButton : CacheObject
{
    enum Texts
    {
        StageText
    }

    private int main;
    private int sub;
    private Button m_myButton;
    private SceneLoadManager sceneLoadManager;
    private UserDataManager dataManager;
    private UIManager uiManager;
    public void Init(int mainStage, int subStage, SceneLoadManager sceneLoadManager, UserDataManager dataManager, UIManager uiManager)
    {
        this.sceneLoadManager = sceneLoadManager;
        this.dataManager = dataManager;
        this.uiManager = uiManager;

        main = mainStage;
        sub = subStage;

        m_myButton = GetComponent<Button>();
        Bind<TextMeshProUGUI>(typeof(Texts));

        m_myButton.onClick.AddListener(SceneMove);

        Get<TextMeshProUGUI>((int)Texts.StageText).text = $"{main}-{sub}";
    }

    private void SceneMove()
    {
        GameData.Instance.MainStage = main;
        GameData.Instance.SubStage = sub;

        sceneLoadManager.SceneLoad(SceneInfo.SceneType.GameScene, async () =>
        {
            var userCharacterData = dataManager.GetUserData<UserData>() as UserData;

            await uiManager.GetAutoUIManager()
                .GetCompoent<InGameUIManager>(UIBaseData.UIType.InGameUI)
                .SetInGameData(userCharacterData.characterDeckList[0], userCharacterData.userSkillList);

        }).Forget();
    }
}