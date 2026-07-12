using Cysharp.Threading.Tasks;
using NetExcute;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class CharacterSelectPanel : UIBase
{
    [Inject] private readonly UserDataManager dataManager;
    [Inject] private readonly PopupManager popupManager;
    [Inject] private readonly AddressableManager addressable;
    [Inject] private readonly ICSVProvider csvHelper;
    private enum CanvasGroups
    {
        PagePanel,
    }

    private enum GameObjects
    {
        OderCharacterListPanel,
        PageList,
    }

    private enum Buttons
    {
        PageItem,
        SaveButton,
    }

    private UserData m_userCharacterData => dataManager.GetUserData<UserData>() as UserData;

    private List<Button> m_pageInteractable = new();

    [SerializeField]
    private CharacterChangeButton[] m_characterImages;
    private CharacterChangeButton m_clickTarget;
    private UserCharacterData m_targetData;
    private UserCharacterData[] m_currentDeck;

    private bool isDirtFlag = false;
    private int m_currentPage = -1;
    private int m_currentindex = -1;

    private CharacterPanelScroll m_characterListPanel => Get<CharacterPanelScroll>();

    public override void Init(Transform parent = null)
    {
        base.Init(parent);

        m_currentDeck = new UserCharacterData[UserData.MAX_CHARACTER_SETTING];

        Bind<CanvasGroup>(typeof(CanvasGroups));
        Bind<GameObject>(typeof(GameObjects));
        Bind<CharacterPanelScroll>();
        Bind<Button>(typeof(Buttons));

        SelecteButtonSetting();
        SettingContext();
        SetPageItem();

        Get<Button>((int)Buttons.SaveButton).onClick.AddListener(() =>
        {
            SavePopup().Forget();
        });

        void SelecteButtonSetting()
        {
            int index = 0;
            foreach (var item in m_characterImages)
            {
                item.Init(addressable, csvHelper, OnClickChangeCharacter, index);
                index++;
            }
        }

        void SetPageItem()
        {
            var parent = Get<GameObject>((int)GameObjects.PageList);
            for (int i = 1; i < UserData.MAX_DECKCOUNT; i++)
            {
                var page = Instantiate(Get<Button>((int)Buttons.PageItem), parent.transform);
                int buttonindex = i;
                page.onClick.AddListener(() =>
                {
                    OnClickPageAction(buttonindex);
                });
                m_pageInteractable.Add(page);
            }

            Get<Button>((int)Buttons.PageItem).onClick.AddListener(() =>
            {
                OnClickPageAction(0);
            });
            m_pageInteractable.Add(Get<Button>((int)Buttons.PageItem));
        }

        void OnClickPageAction(int pageindex)
        {
            SetInteractablePage(false);

            if (isDirtFlag && m_currentPage != pageindex)
            {
                SavePopup(() =>
                {
                    OnClickPage(pageindex).Forget();
                }).Forget();
            }
            else
            {
                OnClickPage(pageindex).Forget();
            }
        }
    }

    private void SetInteractablePage(bool active)
    {
        foreach (var i in m_pageInteractable)
            i.interactable = active;
    }

    public override void ShowUI()
    {
        base.ShowUI();

        isDirtFlag = false;
        SetInteractablePage(false);
        OnClickPage(0).Forget();
    }

    public override void OnClickClosetButton()
    {
        if (isDirtFlag)
            SavePopup(() =>
            {
                base.OnClickClosetButton();
                ResetData();
            }).Forget();
        else
        {
            base.OnClickClosetButton();
            ResetData();
        }
    }

    private void ResetData()
    {
        m_currentPage = -1;
        m_currentindex = -1;
    }

    public async UniTask OnClickPage(int pageIndex)
    {
        if (m_currentPage == pageIndex)
        {
            SetInteractablePage(true);
            return;
        }

        m_currentPage = pageIndex;
        int index = 0;

        Get<CanvasGroup>((int)CanvasGroups.PagePanel).alpha = 0;

        List<UniTask> loadList = new();
        if (m_userCharacterData.characterDeckList.ContainsKey(pageIndex))
        {
            Array.Copy(m_userCharacterData.characterDeckList[m_currentPage], m_currentDeck, m_currentDeck.Length);

            foreach (var character in m_characterImages)
            {
                loadList.Add(m_characterImages[index].SettingPrefab(m_currentDeck[index]));
                index++;
            }
        }

        await UniTask.WhenAll(loadList);

        Get<CanvasGroup>((int)CanvasGroups.PagePanel).alpha = 1;
        SetInteractablePage(true);
    }

    private void OnClickChangeCharacter(CharacterChangeButton button)
    {
        m_clickTarget = button;
        m_targetData = button.GetCharacterData();
        m_currentindex = button.prefabIndex;

        SettingContext();
        Get<GameObject>((int)GameObjects.OderCharacterListPanel).gameObject.SetActive(true);
    }

    private void OnClickChange(UserCharacterData data)
    {
        Get<GameObject>((int)GameObjects.OderCharacterListPanel).gameObject.SetActive(false);
        isDirtFlag = true;

        if (m_targetData != null && m_targetData.ID == data.ID)
        {
            m_clickTarget.SettingPrefab(null).Forget();
            m_currentDeck[m_currentindex] = null;
        }
        else if (m_currentDeck.Any(x => x != null && x.ID == data.ID))
        {
            int oldIndex = Array.IndexOf(m_currentDeck, data);
            var currentSlotData = m_currentDeck[m_currentindex];

            m_characterImages[m_currentindex].SettingPrefab(data).Forget();
            m_characterImages[oldIndex].SettingPrefab(currentSlotData).Forget();

            m_currentDeck[m_currentindex] = data;
            m_currentDeck[oldIndex] = currentSlotData;
        }
        else
        {
            m_clickTarget.SettingPrefab(data).Forget();
            m_currentDeck[m_currentindex] = data;
        }
    }

    private void SaveCurrentDeckList()
    {
        if (isDirtFlag == false) return;

        if (m_currentDeck.Any(x => x != null))
        {
            Array.Copy(m_currentDeck, m_userCharacterData.characterDeckList[m_currentPage], m_currentDeck.Length);
            isDirtFlag = false;
            return;
        }

        PopupNotSaveMessage().Forget();
        isDirtFlag = false;
    }

    private void SettingContext()
    {
        m_characterListPanel.OnCellClicked(OnClickChange, addressable, csvHelper, m_currentDeck, m_targetData);
        m_characterListPanel.UpdateContents(m_userCharacterData.oderCharacter.Values.ToList());
    }

    async UniTask SavePopup(Action closetPopupAction = null)
    {
        var popup = await popupManager.ShowPopup(PopupManager.PopupType.PopupQ) as PopupQ;

        popup.okAction = () =>
        {
            SaveCurrentDeckList();
            if (closetPopupAction != null)
                closetPopupAction.Invoke();
        };

        popup.noAction = () =>
        {
            isDirtFlag = false;
            if (closetPopupAction != null)
                closetPopupAction.Invoke();
        };

        popup.Mssage = "변경 사항이 있습니다. 저장하시겠습니까? \n(저장하지 않을 경우 데이터가 유지되지 않습니다.)";
    }

    async UniTask PopupNotSaveMessage()
    {
        var Popup = await popupManager.ShowPopup(PopupManager.PopupType.PopupMsg) as PopupMsg;
        Popup.Mssage = "최소 한 명 이상의 캐릭터가 배치되어야 저장이 가능합니다.";
    }
}