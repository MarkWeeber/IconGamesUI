using System;
using UnityEngine;

public class SingletonBehaviour<T> : MonoBehaviour where T : SingletonBehaviour<T>
{
    private static bool? _lazyInitialization;
    private static bool lazyInitialization
    {
        get
        {
            if (_lazyInitialization == null)
            {
                object[] attributes = typeof(T).GetCustomAttributes(typeof(LazyInstatiateAttribute), true);
                _lazyInitialization = (attributes.Length > 0) ?
                    ((LazyInstatiateAttribute)attributes[0]).Enabled : true;
            }
            return _lazyInitialization.Value;
        }
    }

    private static T _instance;
    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                if (lazyInitialization)
                {
                    GameObject singletonObject = new GameObject(typeof(T).Name);
                    _instance = singletonObject.AddComponent<T>();
                    _instance.LazyInstantiate();
                }
                else
                    Debug.LogError("Singleton instance not found");
            }
            return _instance;
        }
    }

    protected bool lazyInstantiate;
    protected bool dontDestroyOnload;

    protected virtual void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            _instance = (T)this;
            Initialize();
            if (dontDestroyOnload)
            {
                DontDestroyOnLoad(gameObject);
            }
        }
    }

    protected virtual void Initialize()
    {

    }

    protected virtual void LazyInstantiate()
    {

    }
}

[AttributeUsage(AttributeTargets.Class)]
public class LazyInstatiateAttribute : Attribute
{
    public bool Enabled { get; }
    public LazyInstatiateAttribute(bool enabled = false)
    {
        Enabled = enabled;
    }
}