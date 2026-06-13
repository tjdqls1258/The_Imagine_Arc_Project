using FancyScrollView;
using NetExcute;
using System;
using UnityEngine;

public class CharacterPanelContext : FancyGridViewContext
{
    public UserCharacterData[] userCharacterDatas;

    public UserCharacterData selecteCharacterData;

    public int SelectedIndex = -1;

    public Action<UserCharacterData> OnCellClicked;
    public AddressableManager addressableManager;
    public CSVHelper csvHelper;
}

public class CharacterPanelScroll : FancyGridView<UserCharacterData, CharacterPanelContext>
{
    class CellGroup : DefaultCellGroup { }

    [Header("Cell Settings")]
    [SerializeField] CharacterCell cellPrefab = default;


    protected override void SetupCellTemplate() => Setup<CellGroup>(cellPrefab);

    public void OnCellClicked(Action<UserCharacterData> callback, AddressableManager addressableManager, CSVHelper csvHelper,
        UserCharacterData[] characterDatas = null, UserCharacterData selecteCharacterData = null)
    {
        Context.addressableManager = addressableManager;
        Context.OnCellClicked = callback;
        Context.userCharacterDatas = characterDatas;
        Context.selecteCharacterData = selecteCharacterData;
        Context.csvHelper = csvHelper;
    }
}