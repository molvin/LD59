using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : Subsystem<ObjectPool>
{
    private Dictionary<GameObject, Queue<GameObject>> pool = new();
    private Dictionary<GameObject, GameObject> poolKeys = new();

    public T Pop<T>(T prefabComponent) where T : MonoBehaviour
    {
        GameObject go = Pop(prefabComponent.gameObject);
        T component = go.GetComponent<T>();
        return component;
    }
    
    public GameObject Pop(GameObject prefab)
    {
        if (!pool.ContainsKey(prefab))
        {
            pool.Add(prefab, new());
        }

        Queue<GameObject> instancePool = pool[prefab];

        if (instancePool.Count == 0)
        {
            GameObject go = Instantiate(prefab);
            go.SetActive(false);

            poolKeys.Add(go, prefab);
            instancePool.Enqueue(go);
        }

        GameObject instance = instancePool.Dequeue();
        instance.SetActive(true);
        return instance;
    }

    public void Push(GameObject instance)
    {
        GameObject key = poolKeys[instance];
        instance.SetActive(false);
        pool[key].Enqueue(instance);
    }
}
