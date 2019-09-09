using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 从Brackeys的视频学来的 https://www.youtube.com/watch?v=tdSmKaJvCoA
/// 有改动
/// </summary>
public class ObjectPooler : MonoBehaviour
{
    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int size;
    }

    public List<Pool> pools;
    public Dictionary<string, Queue<GameObject>> poolDictionary;
    // 注意：队列中的对象enable属性都为false
    
    // Singleton
    public static ObjectPooler Instance;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();
        foreach (Pool pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();
            for (int i = 0; i < pool.size; ++i)
            {
                GameObject obj = Instantiate(pool.prefab);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }
            poolDictionary.Add(pool.tag, objectPool);
        }
    }

    public GameObject SpawnFromPool(string poolTag, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(poolTag))
        {
            Debug.LogWarning("Pool with tag" + poolTag + " doesn't exist.'");
            return null;
        }

        if (poolDictionary[poolTag].Count == 0)
        {
            Debug.LogWarning("Pool Empty!!! Nothing to spawn. tag:" + poolTag);
            return null;
        }
        
        GameObject objectToSpawn = poolDictionary[poolTag].Dequeue();
        
        objectToSpawn.SetActive(true);
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;

        IPooledObject pooledObject = objectToSpawn.GetComponent<IPooledObject>();
        if (pooledObject != null)
        {
            pooledObject.OnObjectSpawn(); // 相当于 Start()
        }
        
        return objectToSpawn;
    }
    
    public void DestroyToPool(string poolTag, GameObject obj)
    {
        poolDictionary[poolTag].Enqueue(obj);
        obj.SetActive(false);
    }

    public IEnumerator DestroyToPoolCoroutine(string poolTag, GameObject obj, float time)
    {
        yield return new WaitForSeconds(time);
        DestroyToPool(poolTag, obj);
    }
}
