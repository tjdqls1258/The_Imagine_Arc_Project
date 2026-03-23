using UnityEngine;

public class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    protected static T _instance;

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = GameObject.FindAnyObjectByType(typeof(T), FindObjectsInactive.Exclude) as GameObject;
                if (go == null)
                {
                    go = new GameObject();
                    go.name = $"(Singleton){typeof(T).Name}";
                    _instance = go.AddComponent<T>();
                }
                else
                    _instance = go.GetComponent<T>();

                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    public virtual void Init()
    {
        DontDestroyOnLoad(gameObject);
    }

    protected virtual void Awake()
    {
        if (_instance == null)
            _instance = gameObject.GetComponent<T>();
    }
}

public class Singleton<T> where T : class, new()
{
    protected static T _instance;

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new T();
            }
            return _instance;
        }
    }

    public virtual void Init() { }
}
