using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


public class SettingPanel : UIBase
{
    enum Sliders
    {
        //Sound
        MasterSlider,
        BGMSlider,
        EffectSlider,
    }

    enum Buttons
    {
        SaveButton,
        CloseButton,
    }

    enum Toggles
    {
        MasterToggle,
        BGMToggle,
        EffectToggle,
    }

    enum SettingSoundType
    {
        Master = 0,
        BGM,
        Effect,
    }

    UserSettingData.UserSettingOption m_oldOption;
    UserSettingData.UserSettingOption m_newOption;
    UserSettingData SettingData => GameMaster.Instance.dataManager.GetUserData<UserSettingData>() as UserSettingData;

    public override void Init(Transform parent = null)
    {
        base.Init(parent);
        m_newOption = m_oldOption = SettingData.userSettingOption;

        Bind<Slider>(typeof(Sliders));
        Bind<Button>(typeof(Buttons));
        Bind<Toggle>(typeof(Toggles));

        SettingButton();

        SetSoundValueChanger(SettingSoundType.Master,
            m_oldOption.masterSoundValue, m_oldOption.muteMasterSound);

        SetSoundValueChanger(SettingSoundType.BGM,
            m_oldOption.bgmSoundValue, m_oldOption.muteBgmSound);

        SetSoundValueChanger(SettingSoundType.Effect,
            m_oldOption.effectSoundValue, m_oldOption.muteEffectSound);

        void SetSoundValueChanger(SettingSoundType type, float sliderValue, bool isMute)
        {
            ResetValue(type, sliderValue, isMute);

            Get<Slider>((int)type).onValueChanged.AddListener((value) =>
            {
                SoundValueChange(value, type);
            });

            Get<Toggle>((int)type).onValueChanged.AddListener((isOn) =>
            {
                SoundMuteSetting(isOn, type);
            });
        }

        void SettingButton()
        {
            Get<Button>((int)Buttons.SaveButton).onClick.RemoveListener(Save);
            Get<Button>((int)Buttons.CloseButton).onClick.RemoveListener(Cancel);

            Get<Button>((int)Buttons.SaveButton).onClick.AddListener(Save);
            Get<Button>((int)Buttons.CloseButton).onClick.AddListener(Cancel);
        }
    }

    private void SoundValueChange(float value, SettingSoundType type)
    {
        value = value <= -40 ? -80 : value;

        switch (type)
        {
            case SettingSoundType.Master:
                GameMaster.Instance.soundManager.MasterValue(value, m_newOption.muteMasterSound);
                m_newOption.masterSoundValue = value;
                break;
            case SettingSoundType.BGM:
                GameMaster.Instance.soundManager.BGMValue(value, m_newOption.muteBgmSound);
                m_newOption.bgmSoundValue = value;
                break;
            case SettingSoundType.Effect:
                GameMaster.Instance.soundManager.EffectValue(value, m_newOption.muteEffectSound);
                m_newOption.effectSoundValue = value;
                break;
        }
    }

    private void SoundMuteSetting(bool isOn, SettingSoundType type)
    {
        switch (type)
        {
            case SettingSoundType.Master:
                GameMaster.Instance.soundManager.MasterValue(m_newOption.masterSoundValue, isOn);
                m_newOption.muteMasterSound = isOn;
                break;
            case SettingSoundType.BGM:
                GameMaster.Instance.soundManager.BGMValue(m_newOption.bgmSoundValue, isOn);
                m_newOption.muteBgmSound = isOn;
                break;
            case SettingSoundType.Effect:
                GameMaster.Instance.soundManager.EffectValue(m_newOption.effectSoundValue, isOn);
                m_newOption.muteEffectSound = isOn;
                break;
        }
    }

    private void Save()
    {
        m_oldOption = m_newOption;
        SettingData.userSettingOption = m_newOption;

        SettingData.SaveData();
    }

    private void Cancel()
    {
        m_newOption = m_oldOption;
        SettingData.userSettingOption = m_oldOption;

        ResetOldValue();

        base.CloseUI();
    }

    private void ResetOldValue()
    {
        ResetValue(SettingSoundType.Master, m_oldOption.masterSoundValue, m_oldOption.muteMasterSound);
        ResetValue(SettingSoundType.BGM, m_oldOption.bgmSoundValue, m_oldOption.muteBgmSound);
        ResetValue(SettingSoundType.Effect, m_oldOption.effectSoundValue, m_oldOption.muteEffectSound);
    }

    private void ResetValue(SettingSoundType type, float sliderValue, bool isMute)
    {
        var sl = Get<Slider>((int)type);
        var to = Get<Toggle>((int)type);

        sl.value = sliderValue;
        to.isOn = isMute;
    }
}
