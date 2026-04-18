using UnityEngine;

public abstract class Subsystem<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;

    public static T Get()
    {
        if (instance == null)
        {
            instance = FindFirstObjectByType<T>();
            if (instance == null)
            {
                GameObject subsystemGameObject = new(typeof(T).Name)
                {
                    hideFlags = HideFlags.HideInHierarchy
                };

                instance = subsystemGameObject.AddComponent<T>();
            }
        }

        return instance;
    }

    protected virtual void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
    }
}
