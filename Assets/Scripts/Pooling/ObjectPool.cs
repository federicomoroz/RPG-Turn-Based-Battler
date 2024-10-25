using UnityEngine;

namespace Pool
{
    [System.Serializable]
    public class ObjectPool<T> : Pool<T>, IPool<T> where T : MonoBehaviour, IPoolable<T>
    {
        #region Fields
        protected Transform parent { get; private set; }
        #endregion
        #region Constructor
        public ObjectPool(int capacity) : base(capacity) { }
        #endregion
        #region Commands
        public override T Pull()
        {        
            T item = _pooledObjects.Count > 0 ? _pooledObjects.Pop() : null;

            item.gameObject.SetActive(true);

            pullObject?.Invoke(item);
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
        public override void SetObjectContainer(Transform parent) => this.parent = parent;                     
        protected override void Fill(int capacity)
        {           
            for (int i = 0; i < capacity; i++)           
                Push(Create());            
        }
        protected override T Create() 
        {
            var item = new GameObject($"o_{typeof(T)}");

            if( parent != null ) 
                item.transform.parent = parent;

            return item.AddComponent<T>();        
        }
        #endregion
    }
}