using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class GameMaster : LifetimeScope
{
#if UNITY_EDITOR
    public bool isAddressableLoad_Start = false;
#endif

    public SoundManager soundManager;
    public SceneLoadManager sceneLoadManager;
    public PopupManager popupManager;
    public AwakeScene awake;
    public GameObject managerParent;

    [SerializeField] private GameObject lodingObject;

    protected override void Configure(IContainerBuilder builder)
    {
        DontDestroyOnLoad(managerParent);
        builder.Register<CSVHelper>(Lifetime.Singleton);
        builder.Register<AddressableManager>(Lifetime.Singleton);
        builder.Register<UserDataManager>(Lifetime.Singleton);

        builder.Register<UIManager>(Lifetime.Singleton).WithParameter(lodingObject);

        builder.RegisterComponent<SceneLoadManager>(sceneLoadManager);
        builder.RegisterComponent<SoundManager>(soundManager);
        builder.RegisterComponent<PopupManager>(popupManager);

        builder.RegisterComponentOnNewGameObject<ObjectPoolManager>(Lifetime.Singleton);

        builder.Register<GameBootStart>(Lifetime.Singleton);

        if(awake != null)
            builder.RegisterComponent(awake);
    }
}