using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectPanel : UIBase
{
    private enum CanvasGroups
    {
        PagePanel,
    }
    private UserData m_userCharacterData => GameMaster.Instance.dataManager.GetUserData<UserData>() as UserData;
    private CharacterDataList m_characterDataList => GameMaster.Instance.csvHelper.GetScripteData<CharacterDataList>();

    [SerializeField]
    private Image[] m_characterImages;
    private List<CharacterData> m_characterDatas = new();

    protected override void Awake()
    {
        Init();
    }

    public override void Init(Transform parent = null)
    {
        base.Init(parent);

        Bind<CanvasGroup>(typeof(CanvasGroups));
    }

    public override void ShowUI()
    {
        base.ShowUI();

        OnClickPage(1).Forget();
    }

    public async UniTask OnClickPage(int pageIndex)
    {
        int index = 0;
        Get<CanvasGroup>((int)CanvasGroups.PagePanel).alpha = 0;

        List<UniTask> loadList = new();
        if (m_userCharacterData.m_characterDeckList.ContainsKey(pageIndex))
        {
            foreach (var character in m_userCharacterData.m_characterDeckList[pageIndex])
            {
                var data = m_characterDataList.GetData(character.ID);
                m_characterDatas.Add(data);

                loadList.Add(data.GetCharacterSprite(targetImage:m_characterImages[index]));

                index++;
            }
        }

        for (int i = index; i < m_characterImages.Length; i++)
        {
            m_characterImages[i].sprite = null;
        }

        await UniTask.WhenAll(loadList);

        Get<CanvasGroup>((int)CanvasGroups.PagePanel).alpha = 1;
    }
}
