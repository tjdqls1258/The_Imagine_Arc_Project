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
    // ====== Main Camera Access ======

    private static Camera m_mainCamera;

    /// <summary>
    /// 게임 내 메인 카메라 인스턴스를 캐싱하여 반환합니다.
    /// 첫 접근 시 Scene에서 카메라를 검색하며, 이후에는 캐시된 인스턴스를 사용합니다.
    /// </summary>
    public static Camera mainCamera
    {
        get
        {
            // Unity에서 객체가 파괴되었는지 안전하게 확인하기 위해 IsUnityNull() 사용
            if (m_mainCamera.IsUnityNull())
                // Scene 전체에서 Camera 컴포넌트를 가진 객체를 검색합니다.
                m_mainCamera = GameObject.FindAnyObjectByType<Camera>();

            return m_mainCamera;
        }
    }

    // ====== Child Finding Utilities (Generic) ======

    /// <summary>
    /// 지정된 GameObject의 하위(Child) 계층 구조에서 특정 타입의 컴포넌트 T를 검색하여 반환합니다.
    /// </summary>
    /// <typeparam name="T">찾으려는 컴포넌트 타입 (UnityEngine.Object를 상속)</typeparam>
    /// <param name="go">검색을 시작할 부모 GameObject</param>
    /// <param name="name">찾으려는 객체의 이름 (null 또는 Empty면 이름 검사 생략)</param>
    /// <param name="recursive">true면 자식의 자식까지 모두 검색, false면 1단계 자식만 검색</param>
    /// <returns>찾은 첫 번째 T 타입 컴포넌트 또는 null</returns>
    public static T FindChild<T>(GameObject go, string name = null, bool recursive = false) where T : UnityEngine.Object
    {
        if (go == null)
            return null;

        if (recursive == false)
        {
            // [비재귀 방식] 부모의 바로 아래 자식들만 루프를 돌며 확인 (성능상 유리)
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
            // [재귀 방식] 부모를 포함한 모든 하위 깊이의 객체를 검색
            // GetComponentsInChildren(true)는 비활성화된 객체까지 포함하여 성능 부하가 있을 수 있으므로 주의해서 사용
            foreach (T component in go.GetComponentsInChildren<T>(true))
            {
                if (string.IsNullOrEmpty(name) || component.name == name)
                    return component;
            }
        }

        return null;
    }

    // ====== Child Finding Utilities (GameObject Overload) ======

    /// <summary>
    /// 지정된 GameObject의 하위 계층 구조에서 특정 이름의 GameObject를 검색하여 반환합니다.
    /// </summary>
    /// <param name="go">검색을 시작할 부모 GameObject</param>
    /// <param name="name">찾으려는 객체의 이름</param>
    /// <param name="recursive">true면 자식의 자식까지 모두 검색</param>
    /// <returns>찾은 GameObject 또는 null</returns>
    public static GameObject FindChild(GameObject go, string name = null, bool recursive = false)
    {
        // Transform은 모든 GameObject에 존재하므로, 이를 통해 검색한 뒤 .gameObject를 반환합니다.
        Transform transform = FindChild<Transform>(go, name, recursive);
        if (transform == null)
            return null;

        return transform.gameObject;
    }

    // ====== Data Conversion Utilities ======

    /// <summary>
    /// JSON 등으로 직렬화되어 저장되었던 SerializeableVector2Int 리스트를 
    /// 유니티 엔진 전용 구조체인 Vector2Int 리스트로 변환합니다.
    /// </summary>
    /// <param name="list">변환할 직렬화 데이터 리스트</param>
    /// <returns>유니티에서 즉시 사용 가능한 Vector2Int 리스트</returns>
    public static List<Vector2Int> ConvartSerializableVector2IntToVector2Int_List(List<SerializeableVector2Int> list)
    {
        List<Vector2Int> result = new();
        foreach (var vecInt in list)
        {
            // 사용자 정의 구조체(SerializeableVector2Int)의 x, y를 Vector2Int로 옮겨 담음
            result.Add(new(vecInt.x, vecInt.y));
        }

        return result;
    }
}