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
        Character,
        Character_1,
        Character_2,
        Character_3,
        Character_4,
        Character_5,
        Character_6,
        Character_7,
        Character_8,
        Character_9
    }

    enum DrawTineLineCom
    {
        DrawAction,
    }

    protected override void Awake()
    {
        m_UISequence = UIManager.UISequence.ShopPanel;
        Bind<Button>(typeof(DrawButton));
        Bind<DrawTimeLine>(typeof(DrawTineLineCom));
        Bind<Image>(typeof(characterCards));
        Init();


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
        
        foreach(var objName in Enum.GetValues(typeof(characterCards)))
        {
            Get<Image>((int)objName).gameObject.SetActive(false);
        }
    }

    private void DrawCharacter(int drawCount)
    {
        List<int> ids = new();
        for (int i = 0; i < drawCount; i++)
        {
            ids.Add(Random.Range(1, 30));
        }

        int count = 0;
        List<CharacterData> data = new List<CharacterData>();

        Get<DrawTimeLine>(0).gameObject.SetActive(true);

        foreach (int id in ids)
        {
            Get<Image>(count).gameObject.SetActive(true);
            Get<Image>(count).sprite = GameMaster.Instance.csvHelper.GetScripteData<CharacterDataList>().GetData(id).GetCharacterSprite();
            count += 1;
            data.Add(GameMaster.Instance.csvHelper.GetScripteData<CharacterDataList>().GetData(id));
        }
        Get<DrawTimeLine>(0).DrawCharacter(data.ToArray());
    }
}
