using Cysharp.Threading.Tasks;
using NetExcute;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Random = UnityEngine.Random;

public class DrawShopPanel : UIBase
{
    enum DrawButton
    {
        Draw1,
        Draw10,
    }

    enum characterCards
    {
        Character, Character_1, Character_2, Character_3, Character_4,
        Character_5, Character_6, Character_7, Character_8, Character_9
    }

    enum DrawTineLineCom
    {
        DrawAction,
    }

    private List<CharacterData> currentData;

    protected override void Awake()
    {
        base.Awake();

        Bind<Button>(typeof(DrawButton));
        Bind<DrawTimeLine>(typeof(DrawTineLineCom));
        Bind<Image>(typeof(characterCards));

        Get<Button>((int)DrawButton.Draw1).onClick.AddListener(() =>
        {
            DrawCharacter(1);
        });

        Get<Button>((int)DrawButton.Draw10).onClick.AddListener(() =>
        {
            DrawCharacter(10);
        });
    }

    public override void ShowUI()
    {
        base.ShowUI();

        foreach (var objName in Enum.GetValues(typeof(characterCards)))
        {
            Get<Image>((int)objName).gameObject.SetActive(false);
        }
    }

    private void DrawCharacter(int drawCount)
    {
        UnLoadCurrentData();
        List<int> ids = new();

        int count = 0;
        currentData = new List<CharacterData>();

        Get<DrawTimeLine>(0).gameObject.SetActive(true);

        foreach (int id in ids)
        {
            var characterDtat = GameMaster.Instance.csvHelper.GetScripteData<CharacterDataList>().GetData(id);
            currentData.Add(characterDtat);

            int currentCount = count;

            Get<Image>(currentCount).gameObject.SetActive(true);
            characterDtat.GetCharacterSprite(targetImage: Get<Image>(currentCount)).Forget();

            count += 1;
        }

        for (int i = count; i < 10; i++)
        {
            Get<Image>(i).gameObject.SetActive(false);
        }

        Get<DrawTimeLine>(0).DrawCharacter(currentData.ToArray());
    }

    public override void CloseUI(bool isClosetAll = false)
    {
        UnLoadCurrentData();
        base.CloseUI(isClosetAll);
    }

    private void UnLoadCurrentData()
    {
        if (currentData == null) return;

        foreach (var item in currentData)
        {
            item.UnloadAtlas();
        }
        currentData.Clear();
    }

#if UNITY_EDITOR
    [UnityEditor.MenuItem("Test/draw")]
    public static void TestCharacterDraw()
    {
        NetExcute.NetExcute.Instance.Requset<DrawCharacterResponse>(new DrawCharacterRequest() { userId = GameMaster.Instance.GetUUID(),count = 1 }, (res) => { }, null);
    }
#endif
}