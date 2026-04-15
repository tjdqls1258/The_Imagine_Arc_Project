using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine.UI;

public class StageMoveButton : CachObject
{
    enum Texts
    {
        StageText
    }

    private int main;
    private int sub;
    private Button m_myButton;

    public void Init(int mainStage, int subStage)
    {
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

        GameMaster.Instance.sceneLoadManager.SceneLoad(SceneInfo.SceneType.GameScene, async () =>
        {
            var userCharacterData = GameMaster.Instance.dataManager.GetUserData<UserData>() as UserData;

            await GameMaster.Instance.uiManager.GetAutoUIManager()
                .GetCompoent<InGameUIManager>(UIBaseData.UIType.InGameUI)
                .SetInGameData(userCharacterData.characterDeckList[0]);

        }).Forget();
    }
}