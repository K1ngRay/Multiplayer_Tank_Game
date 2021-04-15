using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool {
    static Dictionary<string, List<GameObject>> pool = new Dictionary<string, List<GameObject>>();
    static Dictionary<string, GameObject> parents = new Dictionary<string, GameObject>();

    public static void Create(string name) {
        if (!pool.ContainsKey(name)) {
            pool[name] = new List<GameObject>();
            parents[name] = new GameObject(name+"_pool");
        }
        else Debug.Log("已经存在" + name + "对象池");
    }

    public static void Push(string name, GameObject obj) {
        if (obj == null) {
            Debug.Log("obj为null");
            return;
        }
        if (!pool.ContainsKey(name)) {
            Create(name);
            Debug.LogWarning("不存在" + name + "对象池，已自动创建");
        }

        pool[name].Add(obj);
        obj.transform.parent = parents[name].transform;
        obj.SetActive(false);
    }

    public static GameObject Pop(string name, GameObject obj = null) {
        if (!pool.ContainsKey(name)) {
            Debug.LogWarning("不存在" + name + "对象池,已自动创建");
            Create(name);
            return Object.Instantiate(obj);
        }
        if (pool[name].Count < 1) {
            if (obj == null) {
                Debug.Log(name + "对象池长度小于1且obj为null");
                return null;
            }
            else
                return Object.Instantiate(obj);
        }
        else {
            GameObject g = pool[name][0];
            g.SetActive(true);
            pool[name].RemoveAt(0);
            return g;
        }
    }

    public static GameObject Pop(string name, GameObject obj, Vector3 position, Quaternion rotation) {
        if (!pool.ContainsKey(name)) {
            Debug.LogWarning("不存在" + name + "对象池,已自动创建");
            Create(name);
            return Object.Instantiate(obj, position, rotation);
        }
        if (pool[name].Count < 1) {
            return Object.Instantiate(obj, position, rotation);
        }
        else {
            GameObject g = pool[name][0];
            g.transform.position = position;
            g.transform.rotation = rotation;
            Rigidbody r = g.GetComponent<Rigidbody>();
            if (r != null) {
                r.isKinematic = true;
                r.isKinematic = false;
            }
            g.SetActive(true);
            pool[name].RemoveAt(0);
            return g;
        }
    }

    public static void ClearAll() {
        foreach (var dicItem in pool) {
            dicItem.Value.Clear();
        }
        pool.Clear();
    }
}
