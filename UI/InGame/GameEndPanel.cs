using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using VContainer;

public class GameEndPanel : UIBase
{
    enum TextMeshProUGUIs
    {
        Title,
    }

    enum GameObjects
    {
        Content,
        Reward
    }

    private GameObject RewardObject => Get<GameObject>((int)GameObjects.Reward);
    private TextMeshProUGUI Title => Get<TextMeshProUGUI>((int)TextMeshProUGUIs.Title);

    private List<RewardItem> m_itemList = new();

    protected override void Awake()
    {
        Bind<TextMeshProUGUI>(typeof(TextMeshProUGUIs));
        Bind<RewardItem>();
        Bind<GameObject>(typeof(GameObjects));

        Get<RewardItem>().gameObject.SetActive(false);
    }

    public void ResultGame(bool isWin, ItemData[] itemsCount)
    {
        gameObject.SetActive(true);

        Time.timeScale = 0;

        if (isWin)
            ResultWin(itemsCount);
        else
            ResultLose();
    }

    private void ResultWin(ItemData[] itemsCount)
    {
        Title.text = "스테이지 클리어";
        RewardObject.SetActive(true);

        ShowRewardList(itemsCount);
    }

    private void ResultLose()
    {
        Title.text = "스테이지 실패";
        RewardObject.SetActive(false);
    }

    private void ShowRewardList(ItemData[] itemsCount)
    {
        for (int i = 0; i < itemsCount.Length; i++)
        {
            if (m_itemList.Count > i)
            {
                m_itemList[i].gameObject.SetActive(true);
                m_itemList[i].SetItem(itemsCount[i]);
            }
            else
            {
                var item = Instantiate(Get<RewardItem>(), Get<GameObject>((int)GameObjects.Content).transform);
                item.gameObject.SetActive(true);
                item.SetItem(itemsCount[i]);
                m_itemList.Add(item);
            }
        }
    }

    public override void CloseUI(bool isClosetAll = false)
    {
        uiManager.GetAutoUIManager()
            .GetCompoent<InGameUIManager>(UIBaseData.UIType.InGameUI)
            .ExitGame();
    }
}