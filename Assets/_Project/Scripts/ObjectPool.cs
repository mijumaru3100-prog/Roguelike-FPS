using System.Collections.Generic;
using UnityEngine;
public class ObjectPool : MonoBehaviour
{
    [SerializeField] private GameObject prefab; 
    [SerializeField] private int initialSize = 20; 

    public bool isDebug = false;
    private Queue<GameObject> pool = new Queue<GameObject>();
    void Awake()
    {
        for (int i = 0; i < initialSize; i++)
        {
            CreateNewObject();
        }
    }
    private GameObject CreateNewObject()
    {
        GameObject obj = Instantiate(prefab);
        obj.transform.SetParent(this.transform); 
        obj.SetActive(false);
        pool.Enqueue(obj);
        return obj;
    }
    public GameObject Get()
    {
        if(isDebug){Debug.Log("よび出された....");}
        if (pool.Count == 0)
        {
            CreateNewObject(); 
        }
        GameObject obj = pool.Dequeue();
        obj.SetActive(true);
        return obj;
    }
   public void ReturnToPool(GameObject obj)
{
    // ★修正: 最初に非アクティブにする（これで物理判定やUpdateが即座に止まる）
    obj.SetActive(false);

    obj.transform.SetParent(transform);
    obj.transform.localPosition = Vector3.zero;
    
    PoolObject poolObject = obj.GetComponent<PoolObject>();
    if (poolObject != null) // 念のためヌルチェック
    {
        obj.transform.localScale = poolObject.defaultScale;
    }
    obj.transform.localRotation = Quaternion.identity;

    // pool.Containsのチェックを入れるとより安全です
    if (!pool.Contains(obj))
    {
        pool.Enqueue(obj);
    }
}
}


public class PoolObject : MonoBehaviour
{
    [HideInInspector]
    public Vector3 defaultScale;

    void Awake()
    {
        defaultScale = transform.localScale;
    }
}