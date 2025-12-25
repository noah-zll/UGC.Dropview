using System.Collections.Generic;
using UnityEngine;

namespace UGC.Dropview
{
    public class GameObjectPool
    {
        readonly Queue<GameObject> q = new Queue<GameObject>();
        readonly Transform parent;
        public int MaxSize = 128;
        public GameObjectPool(Transform parent)
        {
            this.parent = parent;
        }
        public GameObject Borrow(System.Func<GameObject> create)
        {
            if (q.Count > 0)
            {
                var go = q.Dequeue();
                go.SetActive(true);
                return go;
            }
            var obj = create != null ? create() : new GameObject("PooledItem");
            obj.transform.SetParent(parent, false);
            obj.SetActive(true);
            return obj;
        }
        public void Return(GameObject go)
        {
            if (go == null) return;
            go.SetActive(false);
            if (q.Count < MaxSize) q.Enqueue(go);
            else Object.Destroy(go);
        }
        public void Clear()
        {
            while (q.Count > 0)
            {
                var go = q.Dequeue();
                if (go != null) Object.Destroy(go);
            }
        }
    }
}
