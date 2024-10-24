using System.Collections.Generic;
using UnityEngine;

namespace Pool
{
    [System.Serializable]
    public abstract class Pool
    {
        #region Fields
        [HideInInspector] 
        public string name { get; protected set; }
        [SerializeField] 
        protected int capacity;
        #endregion
        #region Properties
        public abstract int stock { get; }
        #endregion
        #region Constructor
        public Pool(int capacity) 
        {
            this.capacity = capacity;
            Fill(capacity);
        }
        #endregion
        #region Setup
        protected abstract void Fill(int capacity);
        #endregion
    }

    [System.Serializable]
    public class Pool<T> : Pool, IPool<T> where T : IPoolable<T>, new()
    {
        #region Fields
        protected Stack<T> _pooledObjects = new Stack<T>();
        #endregion
        #region Properties
        public override int stock => _pooledObjects.Count;
        #endregion
        #region Delegates
        protected
            System.Action<T>
            pullObject,
            pushObject;
        #endregion
        #region Constructor
        public Pool(int capacity) : base(capacity)
        {
            name = typeof(T).ToString();  
        }
        #endregion
        #region Commands
        public virtual T Pull()
        {
            T item = _pooledObjects.Count > 0 ? 
                _pooledObjects.Pop() : default(T);          

            pullObject?.Invoke(item);
            return item;
        }
        public virtual void Push(T item)
        {
            if (_pooledObjects.Count >= capacity)
            {
                Debug.LogWarning("Pool complete");
                return;
            }
            _pooledObjects.Push(item);

            pushObject?.Invoke(item);
        }
        #endregion
        #region Setup
        protected override void Fill(int capacity) 
        {
            for (int i = 0; i < capacity; i++)        
                _pooledObjects.Push(Create());        
        }    
        protected virtual T Create()  => new T().Initialize();
        #endregion
    }
}

