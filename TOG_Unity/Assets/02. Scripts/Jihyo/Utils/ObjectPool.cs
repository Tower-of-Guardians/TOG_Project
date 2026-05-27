using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    private GameObject prefab;
    private int initialPoolSize = 10;
    private int maxPoolSize = 50;
    private bool expandable = true;

    private Queue<GameObject> pool = new Queue<GameObject>();
    private HashSet<GameObject> activeObjects = new HashSet<GameObject>();
    private HashSet<GameObject> allInstances = new HashSet<GameObject>();

    public int PooledCount => pool.Count;
    public int ActiveCount => activeObjects.Count;
    public int TotalCount => PooledCount + ActiveCount;

    public void Initialize(GameObject prefab, int initialPoolSize, int maxPoolSize, bool expandable)
    {
        this.prefab = prefab;
        this.initialPoolSize = initialPoolSize;
        this.maxPoolSize = maxPoolSize;
        this.expandable = expandable;

        if (prefab == null)
        {
            Debug.LogError("ObjectPool: Prefab is not assigned!", this);
            return;
        }

        WarmPool();
    }

    private void WarmPool()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            CreatePooledObject();
        }
    }

    private GameObject CreatePooledObject()
    {
        GameObject obj = Instantiate(prefab, transform);
        obj.SetActive(false);
        allInstances.Add(obj);
        pool.Enqueue(obj);
        return obj;
    }

    public GameObject Get()
    {
        GameObject obj;

        if (pool.Count > 0)
        {
            obj = pool.Dequeue();
        }
        else if (expandable)
        {
            obj = Instantiate(prefab, transform);
            allInstances.Add(obj);
        }
        else
        {
            return null;
        }

        obj.SetActive(true);
        activeObjects.Add(obj);
        return obj;
    }

    public void Return(GameObject obj)
    {
        if (obj == null)
        {
            return;
        }

        if (!activeObjects.Contains(obj))
        {
            return;
        }

        activeObjects.Remove(obj);
        obj.SetActive(false);
        obj.transform.SetParent(transform);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;

        if (pool.Count < maxPoolSize)
        {
            pool.Enqueue(obj);
        }
        else
        {
            allInstances.Remove(obj);
            Destroy(obj);
        }
    }

    public bool IsInstanceFromPool(GameObject obj)
    {
        return obj != null && allInstances.Contains(obj);
    }

    public void ReturnAll()
    {
        List<GameObject> objectsToReturn = new List<GameObject>(activeObjects);
        foreach (GameObject obj in objectsToReturn)
        {
            Return(obj);
        }
    }

    public void Clear()
    {
        ReturnAll();

        while (pool.Count > 0)
        {
            GameObject obj = pool.Dequeue();
            if (obj != null)
            {
                Destroy(obj);
            }
        }

        pool.Clear();
        activeObjects.Clear();
        allInstances.Clear();
    }

    private void OnDestroy()
    {
        Clear();
    }
}
