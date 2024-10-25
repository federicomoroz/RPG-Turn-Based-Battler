namespace Pool
{
    [System.Serializable]
    public class ClassPool<T> : Pool<T>, IPool<T> where T : IPoolable<T>, new()
    {
        #region Fields

        #endregion
        #region Properties
        public override int stock => _pooledObjects.Count;
        #endregion

        #region Constructor
        public ClassPool(int capacity) : base(capacity)
        {
            name = typeof(T).ToString();
        }
        #endregion
  
        protected override T Create() => new T().Initialize();
 
    }
}

