using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance;

    [SerializeField] private List<PoolData> pools;

    private Dictionary<string, Queue<GameObject>> poolDict;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitPools();
        }
        else Destroy(gameObject);
    }

    void InitPools()
    {
        poolDict = new Dictionary<string, Queue<GameObject>>();

        foreach (var pool in pools)
        {
            Queue<GameObject> objectPool = new();

            for (int i = 0; i < pool.size; i++)
            {
                var obj = CreateObj(pool.prefab);
                objectPool.Enqueue(obj);
            }

            poolDict.Add(pool.key, objectPool);
        }
    }

    GameObject CreateObj(GameObject prefab)
    {
        var obj = Instantiate(prefab, transform);
        obj.SetActive(false);
        return obj;
    }

    // ================= SPAWN =================
    public GameObject Spawn(string key, Vector3 pos, Quaternion rot)
    {
        if (!poolDict.ContainsKey(key))
        {
            Debug.LogError($"Pool key {key} not exist");
            return null;
        }

        var pool = poolDict[key];

        GameObject obj = pool.Count > 0
            ? pool.Dequeue()
            : null;

        if (obj == null)
        {
            Debug.LogWarning("Pool empty, expanding...");
            return null;
        }

        obj.transform.SetPositionAndRotation(pos, rot);
        obj.SetActive(true);

        obj.GetComponent<IPoolable>()?.OnSpawn();
        return obj;
    }

    // ================= DESPAWN =================
    public void Despawn(string key, GameObject obj)
    {
        obj.GetComponent<IPoolable>()?.OnDespawn();
        obj.SetActive(false);
        if(!poolDict[key].Contains(obj))
        poolDict[key].Enqueue(obj);
    }
}

[System.Serializable]
public class PoolData
{
    public string key;
    public GameObject prefab;
    public int size = 10;
    public bool expandable = true;
}
