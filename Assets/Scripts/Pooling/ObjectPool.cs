using UnityEngine;

namespace Pool
{
    [System.Serializable]
    public class ObjectPool<T> : Pool<T>, IPool<T> where T : MonoBehaviour, IPoolable<T>, new()
    {
        #region Constructor
        public ObjectPool(int capacity = 0) : base(capacity) { }
        public ObjectPool(System.Action<T> pullObject, System.Action<T> pushObject, int capacity = 0) : base(capacity)
        {        
            this.pullObject = pullObject;
            this.pushObject = pushObject; 
            Fill(capacity);
        }
        #endregion
        #region Commands
        public override T Pull()
        {        
            T item = _pooledObjects.Count > 0 ? _pooledObjects.Pop() : null;

            item.gameObject.SetActive(true);

            pullObject?.Invoke(item);
            return item;
        }
        public T Pull(Vector3 position)
        {
            T item = Pull();
            item.transform.position = position;
            return item;
        }
        public T Pull(Vector3 position, Quaternion rotation)
        {
            T item = Pull(position);
            item.transform.rotation = rotation;
            return item;
        }
        public override void Push(T item)
        {
            item.gameObject.SetActive(false);
            _pooledObjects.Push(item);
            pushObject?.Invoke(item);
        }
        #endregion
        #region Setup
        protected override void Fill(int capacity)
        {
            T item;

            for (int i = 0; i < capacity; i++)
            {
                item = Create();
                Push(item);
            }
        }
        protected override T Create() 
        {
            var item = new GameObject($"o_{typeof(T)}");

            Transform parent = GameObject.Find("PoolObjects").transform;

            if (parent == null)
                parent = new GameObject("PoolObjects").transform;

            item.transform.parent = parent;

            return item.AddComponent<T>();        
        }
        #endregion
    }
}