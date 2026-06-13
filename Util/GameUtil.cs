using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static MapData;

/// <summary>
/// 게임 전반에서 재사용 가능한 유틸리티 기능을 제공하는 정적 클래스입니다.
/// 메인 카메라 캐싱 및 GameObject 계층 구조 내에서 하위 컴포넌트나 객체를 검색하는 기능을 포함합니다.
/// </summary>
public static class GameUtil
{
    private static string _uuid = string.Empty;
    public static string UUID
    {
        get
        {
            if (_uuid == string.Empty)
            {
                _uuid = PlayerPrefs.GetString("UUID", Guid.NewGuid().ToString());
                PlayerPrefs.SetString("UUID", _uuid);
            }

            return _uuid;
        }
        set
        {
            PlayerPrefs.SetString("UUID", value);
            _uuid = value;
        }
    }


    // ====== Main Camera Access ======
    private static Camera m_mainCamera;

    public static Camera mainCamera
    {
        get
        {
            if (m_mainCamera.IsUnityNull())
                m_mainCamera = GameObject.FindAnyObjectByType<Camera>();

            return m_mainCamera;
        }
    }

    // ====== Child Finding Utilities (Generic) ======
    public static T FindChild<T>(GameObject go, string name = null, bool recursive = false) where T : UnityEngine.Object
    {
        if (go == null)
            return null;

        if (recursive == false)
        {
            for (int i = 0; i < go.transform.childCount; i++)
            {
                Transform transform = go.transform.GetChild(i);
                if (string.IsNullOrEmpty(name) || transform.name == name)
                {
                    T component = transform.GetComponent<T>();
                    if (component != null)
                        return component;
                }
            }
        }
        else
        {
            foreach (T component in go.GetComponentsInChildren<T>(true))
            {
                if (string.IsNullOrEmpty(name) || component.name == name)
                    return component;
            }
        }

        return null;
    }

    // ====== Child Finding Utilities (GameObject Overload) ======
    public static GameObject FindChild(GameObject go, string name = null, bool recursive = false)
    {
        Transform transform = FindChild<Transform>(go, name, recursive);
        if (transform == null)
            return null;

        return transform.gameObject;
    }

    // ====== Data Conversion Utilities ======

    public static List<Vector2Int> ConvartSerializableVector2IntToVector2Int_List(List<SerializeableVector2Int> list)
    {
        List<Vector2Int> result = new();
        foreach (var vecInt in list)
        {
            result.Add(new(vecInt.x, vecInt.y));
        }

        return result;
    }


    public static void InjectUtil(object obj)
    {
        var root = UnityEngine.Object.FindAnyObjectByType<GameMaster>();
        if (root != null)
        {
            root.Container.Inject(obj);
        }
    }
}