using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 유니티 기본 컴포넌트 접근 최적화 및 자식 오브젝트 자동 바인딩을 위한 베이스 클래스입니다.
/// 매번 호출되는 GetComponent나 transform 접근 오버헤드를 줄이기 위해 내부 캐싱을 수행합니다.
/// </summary>
public class CachObject : MonoBehaviour
{
    // ====== 내부 캐싱용 필드 ======
    protected GameObject m_gameObject;
    protected Transform m_transform;
    protected RectTransform m_rectTransform;

    /// <summary> 바인딩된 컴포넌트나 게임 오브젝트들을 타입별로 저장하는 저장소입니다. </summary>
    protected Dictionary<Type, UnityEngine.Object[]> _objects = new();

    // ====== 최적화된 프로퍼티 (Lazy Initialization) ======

    /// <summary> 캐싱된 GameObject를 반환합니다. </summary>
    public GameObject MyObj
    {
        get
        {
            if (m_gameObject == null)
                m_gameObject = gameObject;
            return m_gameObject;
        }
    }

    /// <summary> 캐싱된 Transform을 반환합니다. (.transform 호출 최적화) </summary>
    public Transform MyTr
    {
        get
        {
            if (m_transform == null)
                m_transform = gameObject.transform;
            return m_transform;
        }
    }

    /// <summary> 캐싱된 RectTransform을 반환합니다. (UI용) </summary>
    public RectTransform MyRT
    {
        get
        {
            if (m_rectTransform == null)
                m_rectTransform = GetComponent<RectTransform>();
            return m_rectTransform;
        }
    }

    // ----------------------------------------------------------------------
    // ## UI 및 자식 오브젝트 바인딩 시스템
    // ----------------------------------------------------------------------

    /// <summary>
    /// 지정된 Enum 타입의 이름을 기반으로 자식 오브젝트나 컴포넌트를 자동으로 찾아 연결(Bind)합니다.
    /// </summary>
    /// <typeparam name="T">찾으려는 컴포넌트 타입 (GameObject, Button, Text 등)</typeparam>
    /// <param name="type">오브젝트 이름이 정의된 Enum 타입</param>
    protected void Bind<T>(Type type) where T : UnityEngine.Object
    {
        // 1. Enum에 정의된 모든 이름들을 가져옵니다.
        string[] names = Enum.GetNames(type);
        UnityEngine.Object[] objects = new UnityEngine.Object[names.Length];

        // 2. 해당 타입(T)을 키로 하여 저장소에 배열을 등록합니다.
        _objects.Add(typeof(T), objects);

        for (int i = 0; i < names.Length; i++)
        {
            // 3. GameUtil(헬퍼 클래스)을 사용해 이름이 일치하는 자식 오브젝트를 찾습니다.
            if (typeof(T) == typeof(GameObject))
                objects[i] = GameUtil.FindChild(gameObject, names[i], true);
            else
                objects[i] = GameUtil.FindChild<T>(gameObject, names[i], true);

            // 4. 바인딩 실패 시 로그를 남겨 개발자가 즉시 확인하게 합니다.
            if (objects[i] == null)
                Logger.LogError($"Bind Fail ({names[i]})");
        }
    }

    /// <summary>
    /// Bind 메서드를 통해 저장된 오브젝트를 인덱스(Enum 순서)를 통해 가져옵니다.
    /// </summary>
    /// <typeparam name="T">가져올 컴포넌트 타입</typeparam>
    /// <param name="idx">Enum의 정수 인덱스 값</param>
    /// <returns>캐싱된 오브젝트 T</returns>
    protected T Get<T>(int idx) where T : UnityEngine.Object
    {
        UnityEngine.Object[] objects = null;

        // 해당 타입의 배열이 존재하는지 확인합니다.
        if (_objects.TryGetValue(typeof(T), out objects) == false)
            return null;

        // 인덱스에 해당하는 오브젝트를 반환합니다.
        return objects[idx] as T;
    }
}