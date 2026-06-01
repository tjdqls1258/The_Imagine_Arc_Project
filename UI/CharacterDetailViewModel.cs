using Cysharp.Threading.Tasks;
using MVVM;
using NetExcute;
using UnityEngine;

public class CharacterDetailViewModel
{
    // View가 구독할 속성들 (이전에 만든 BindableProperty 템플릿 사용)
    public BindableProperty<string> LevelText { get; private set; } = new();
    public BindableProperty<string> InfoText { get; private set; } = new();
    public BindableProperty<Sprite> CharacterSprite { get; private set; } = new();
    public BindableProperty<int[]> ActiveSkills { get; private set; } = new();
    public BindableProperty<int[]> PassiveSkills { get; private set; } = new();

    public async UniTask LoadDataAsync(UserCharacterData userData)
    {
        CharacterData rawData = userData.GetCharacterData();
        BaseCharacterStat characterStat = userData.GetInGameBaseStat();
        LevelText.Value = $"LV. {userData.level}";
        InfoText.Value = 
            @$"{rawData.characterName} data Not Ready
Cost : {rawData.cost}\nRating : {rawData.rating}
test Data : {characterStat.GetStat(StatType.MaxHp)}";

        ActiveSkills.Value = rawData.activeSkill;
        PassiveSkills.Value = rawData.passiveSkill;

        await rawData.GetCharacterSprite((sp) =>
        {
            CharacterSprite.Value = sp;
        });
    }
}