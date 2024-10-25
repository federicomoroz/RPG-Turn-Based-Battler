using UnityEngine;

namespace Pool
{
    [System.Serializable]
    public class ClassPool<T> : Pool<T>, IPool<T> where T : IPoolable<T>, new()
    {
        #region Constructor
        public ClassPool(int capacity) : base(capacity)
        {
            name = typeof(T).ToString();
        }
        #endregion
        #region Helpers / Utils
        protected override T Create() => new T().Initialize();
        #endregion
    }
}

